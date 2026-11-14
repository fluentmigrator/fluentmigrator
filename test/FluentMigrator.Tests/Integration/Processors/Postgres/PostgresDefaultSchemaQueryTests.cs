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

        [SetUp]
        public void SetUp()
        {
            IntegrationTestOptions.Postgres.IgnoreIfNotEnabled();

            _schema = "fm1055_" + Guid.NewGuid().ToString("N").Substring(0, 12);
            Execute($"CREATE SCHEMA \"{_schema}\"");
        }

        [TearDown]
        public void TearDown()
        {
            if (_schema != null)
            {
                Execute($"DROP SCHEMA IF EXISTS \"{_schema}\" CASCADE");
            }
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
