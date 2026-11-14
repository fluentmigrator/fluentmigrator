#region License
// Copyright (c) 2026, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Collections.Generic;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Conventions;
using FluentMigrator.Runner.Initialization;

using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Integration.Processors.Postgres
{
    /// <summary>
    /// Issue #1055: with a default schema in the convention set, <c>Schema.Table(...).Exists()</c> looked in the
    /// connection's default schema while <c>Create.Table(...)</c> created the table in the convention's, so the
    /// second of two idempotent migrations tried to create the table again.
    /// </summary>
    [TestFixture]
    [Category("Integration")]
    [Category("Postgres")]
    public class PostgresDefaultSchemaQueryTests
    {
        private string _schema;
        private readonly List<string> _schemasToDrop = new List<string>();

        [SetUp]
        public void SetUp()
        {
            IntegrationTestOptions.Postgres.IgnoreIfNotEnabled();

            _schemasToDrop.Clear();
            _schema = "fm1055_" + Guid.NewGuid().ToString("N").Substring(0, 12);
            Execute($"CREATE SCHEMA \"{_schema}\"");
            _schemasToDrop.Add(_schema);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var schema in _schemasToDrop)
            {
                Execute($"DROP SCHEMA IF EXISTS \"{schema}\" CASCADE");
            }
        }

        /// <summary>
        /// Pins "Naming a schema explicitly" in docs-website/advanced/conditional-logic.md, with a convention set
        /// that renames schemas. <c>Schema.*</c> calls aren't expressions: they are processor-level queries
        /// (<see cref="IQuerySchema"/>) that run while <c>Up()</c> builds its expressions, so the runner's
        /// conventions only reach them through <see cref="FluentMigrator.Infrastructure.IDefaultSchemaNameResolver"/>.
        /// </summary>
        [Test]
        public void ARenamingConventionAppliesToObjectChecksButNotToTheSchemaCheck()
        {
            var scenario = new ConventionRewritesSchema.Scenario(_schema);
            _schemasToDrop.AddRange(scenario.AllSchemas);

            // The convention set's default schema (App) holds VersionInfo, which the runner creates before any
            // migration runs, so it has to exist first. SetUp already created it.
            var conventionSet = ConventionSets.Create(
                new DefaultSchemaConvention(new ConventionRewritesSchema.TenantSchemaNameConvention(scenario)),
                rootPath: null);

            var services = ServiceCollectionExtensions.CreateServices(addAssemblySource: false)
                .AddSingleton<IConventionSet>(conventionSet)
                .AddSingleton(scenario)
                .Configure<TypeFilterOptions>(options => options.Namespace = typeof(ConventionRewritesSchema.CreateSchemasAndTables).Namespace)
                .ConfigureRunner(
                    builder => builder
                        .AddPostgres()
                        .WithGlobalConnectionString(IntegrationTestOptions.Postgres.ConnectionString)
                        .WithMigrationsIn(typeof(ConventionRewritesSchema.CreateSchemasAndTables).Assembly));

            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            scope.ServiceProvider.GetRequiredService<IMigrationRunner>().MigrateUp();

            // What the migration's Schema.* checks answered, after the convention.
            Assert.Multiple(
                () =>
                {
                    // Create.Schema(Reports) made Reports, not ReportsTenant: the schema check uses the name as written.
                    scenario.Checks["Schema(Reports).Exists()"].ShouldBeTrue();

                    // Create.Table("Log").InSchema(Audit) made AuditTenant.Log: object checks go through the convention.
                    scenario.Checks["Schema(Audit).Table(Log).Exists()"].ShouldBeTrue();
                    scenario.Checks["Schema(Audit).Table(Log).Column(Id).Exists()"].ShouldBeTrue();

                    // Create.Table("Gadget") made App.Gadget: no schema means the convention's default.
                    scenario.Checks["Table(Gadget).Exists()"].ShouldBeTrue();
                });

            // Where things really are, asked of the processor directly, which applies no convention.
            var processor = scope.ServiceProvider.GetRequiredService<IMigrationProcessor>();
            Assert.Multiple(
                () =>
                {
                    processor.SchemaExists(scenario.Audit).ShouldBeTrue();
                    processor.SchemaExists(scenario.AuditTenant).ShouldBeTrue();
                    processor.SchemaExists(scenario.Reports).ShouldBeTrue();
                    processor.SchemaExists(scenario.ReportsTenant).ShouldBeFalse();

                    processor.TableExists(scenario.AuditTenant, "Log").ShouldBeTrue();
                    processor.TableExists(scenario.Audit, "Log").ShouldBeFalse();
                    processor.TableExists(scenario.App, "Gadget").ShouldBeTrue();
                    processor.TableExists(null, "Gadget").ShouldBeFalse();
                });
        }

        [Test]
        public void IdempotentMigrationsFindWhatTheyCreatedInTheDefaultSchema()
        {
            var services = ServiceCollectionExtensions.CreateServices(addAssemblySource: false)
                .AddSingleton<IConventionSet>(new DefaultConventionSet(_schema, workingDirectory: null))
                .Configure<TypeFilterOptions>(options => options.Namespace = typeof(DefaultSchemaQueries.CreateWidget).Namespace)
                .ConfigureRunner(
                    builder => builder
                        .AddPostgres()
                        .WithGlobalConnectionString(IntegrationTestOptions.Postgres.ConnectionString)
                        .WithMigrationsIn(typeof(DefaultSchemaQueries.CreateWidget).Assembly));

            using (var serviceProvider = services.BuildServiceProvider())
            using (var scope = serviceProvider.CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<IMigrationRunner>().MigrateUp();
            }

            Count("SELECT count(*) FROM information_schema.tables WHERE table_schema = @schema AND table_name = 'Widget'")
                .ShouldBe(1);
            Count("SELECT count(*) FROM information_schema.columns WHERE table_schema = @schema AND table_name = 'Widget' AND column_name = 'Name'")
                .ShouldBe(1);
            Count("SELECT count(*) FROM pg_indexes WHERE schemaname = @schema AND indexname = 'IX_Widget_Name'")
                .ShouldBe(1);
            Count("SELECT count(*) FROM information_schema.sequences WHERE sequence_schema = @schema AND sequence_name = 'WidgetNumber'")
                .ShouldBe(1);
            Count($"SELECT count(*) FROM \"{_schema}\".\"VersionInfo\"").ShouldBe(3);
        }

        private void Execute(string sql)
        {
            using var connection = new NpgsqlConnection(IntegrationTestOptions.Postgres.ConnectionString);
            connection.Open();
            using var command = new NpgsqlCommand(sql, connection);
            command.ExecuteNonQuery();
        }

        private long Count(string sql)
        {
            using var connection = new NpgsqlConnection(IntegrationTestOptions.Postgres.ConnectionString);
            connection.Open();
            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("schema", _schema);
            return (long)command.ExecuteScalar();
        }
    }
}

namespace FluentMigrator.Tests.Integration.Processors.Postgres.DefaultSchemaQueries
{
    // Each migration checks before it creates, as an idempotent migration does. None names a schema.

    [Migration(1)]
    public class CreateWidget : ForwardOnlyMigration
    {
        public override void Up()
        {
            if (!Schema.Table("Widget").Exists())
            {
                Create.Table("Widget").WithColumn("Id").AsInt32().NotNullable().PrimaryKey();
            }
        }
    }

    [Migration(2)]
    public class AddNameAndNumber : ForwardOnlyMigration
    {
        public override void Up()
        {
            // Migration 1 created Widget; a check that looks in the wrong schema creates it again and fails.
            if (!Schema.Table("Widget").Exists())
            {
                Create.Table("Widget").WithColumn("Id").AsInt32().NotNullable().PrimaryKey();
            }

            if (!Schema.Table("Widget").Column("Name").Exists())
            {
                Alter.Table("Widget").AddColumn("Name").AsString(50).Nullable();
            }

            if (!Schema.Table("Widget").Index("IX_Widget_Name").Exists())
            {
                Create.Index("IX_Widget_Name").OnTable("Widget").OnColumn("Name");
            }

            if (!Schema.Sequence("WidgetNumber").Exists())
            {
                Create.Sequence("WidgetNumber");
            }
        }
    }

    [Migration(3)]
    public class RunTheSameChecksAgain : ForwardOnlyMigration
    {
        public override void Up()
        {
            // Everything exists now, so each check must find it and nothing is created twice.
            if (!Schema.Table("Widget").Column("Name").Exists())
            {
                Alter.Table("Widget").AddColumn("Name").AsString(50).Nullable();
            }

            if (!Schema.Table("Widget").Index("IX_Widget_Name").Exists())
            {
                Create.Index("IX_Widget_Name").OnTable("Widget").OnColumn("Name");
            }

            if (!Schema.Sequence("WidgetNumber").Exists())
            {
                Create.Sequence("WidgetNumber");
            }
        }
    }
}

namespace FluentMigrator.Tests.Integration.Processors.Postgres.ConventionRewritesSchema
{
    /// <summary>
    /// Per-run schema names, and what the migration's checks answered.
    /// </summary>
    public class Scenario
    {
        public Scenario(string app)
        {
            App = app;
            Audit = app + "_audit";
            AuditTenant = app + "_audit_t";
            Reports = app + "_reports";
            ReportsTenant = app + "_reports_t";
        }

        /// <summary>The convention set's default schema.</summary>
        public string App { get; }

        /// <summary>A schema the migrations name, which the convention renames to <see cref="AuditTenant"/>.</summary>
        public string Audit { get; }

        public string AuditTenant { get; }

        /// <summary>A schema the migrations name, which the convention renames to <see cref="ReportsTenant"/>.</summary>
        public string Reports { get; }

        /// <summary>Never created.</summary>
        public string ReportsTenant { get; }

        public IEnumerable<string> AllSchemas => new[] { App, Audit, AuditTenant, Reports, ReportsTenant };

        public Dictionary<string, bool> Checks { get; } = new Dictionary<string, bool>();
    }

    /// <summary>
    /// Renames two schemas the migrations name, as a multi-tenant setup might, and gives everything without a
    /// schema the default. Any other name, the renamed ones included, is left alone.
    /// </summary>
    public class TenantSchemaNameConvention : IDefaultSchemaNameConvention
    {
        private readonly Scenario _scenario;

        public TenantSchemaNameConvention(Scenario scenario)
        {
            _scenario = scenario;
        }

        public string GetSchemaName(string originalSchemaName)
        {
            if (string.IsNullOrEmpty(originalSchemaName))
                return _scenario.App;
            if (originalSchemaName == _scenario.Audit)
                return _scenario.AuditTenant;
            if (originalSchemaName == _scenario.Reports)
                return _scenario.ReportsTenant;
            return originalSchemaName;
        }
    }

    [Migration(1)]
    public class CreateSchemasAndTables : ForwardOnlyMigration
    {
        private readonly Scenario _scenario;

        public CreateSchemasAndTables(Scenario scenario)
        {
            _scenario = scenario;
        }

        public override void Up()
        {
            // Create.Schema doesn't go through the schema convention: these are created as written.
            Create.Schema(_scenario.Audit);
            Create.Schema(_scenario.AuditTenant);
            Create.Schema(_scenario.Reports);

            // Create.Table does: Log goes to AuditTenant, Gadget to App.
            Create.Table("Log").InSchema(_scenario.Audit)
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey();
            Create.Table("Gadget")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey();
        }
    }

    [Migration(2)]
    public class RecordWhatTheChecksFind : ForwardOnlyMigration
    {
        private readonly Scenario _scenario;

        public RecordWhatTheChecksFind(Scenario scenario)
        {
            _scenario = scenario;
        }

        public override void Up()
        {
            var checks = _scenario.Checks;
            checks["Schema(Reports).Exists()"] = Schema.Schema(_scenario.Reports).Exists();
            checks["Schema(Audit).Table(Log).Exists()"] = Schema.Schema(_scenario.Audit).Table("Log").Exists();
            checks["Schema(Audit).Table(Log).Column(Id).Exists()"] = Schema.Schema(_scenario.Audit).Table("Log").Column("Id").Exists();
            checks["Table(Gadget).Exists()"] = Schema.Table("Gadget").Exists();
        }
    }
}
