#region License
// Copyright (c) 2026, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;

using FluentMigrator.Infrastructure;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Conventions;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Postgres;
using FluentMigrator.Tests.Helpers;

using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Integration.Processors.Postgres
{
    [TestFixture]
    [Category("Integration")]
    [Category("Postgres")]
    public class PostgresCheckConstraintTests
    {
        private const string ConstraintName = "valid \"item code\"";

        [TestCase(null, null)]
        [TestCase("", null)]
        [TestCase("Check Constraint Schema", "Check Constraint Default Schema")]
        [TestCase(null, "Check Constraint Default Schema")]
        public void CheckConstraintRejectsRowsThatFailTheCondition(string schemaName, string defaultSchemaName)
        {
            using var serviceProvider = CreatePostgresServices(defaultSchemaName);
            using var scope = serviceProvider.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<PostgresProcessor>();
            using var table = new PostgresTestTable(processor, schemaName ?? defaultSchemaName, "code text");

            var runner = (MigrationRunner)scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
            runner.Up(new AddCodeConstraint(table.Name, schemaName));

            InsertCode(table, "abc");
            var exception = Should.Throw<PostgresException>(() => InsertCode(table, "a"));
            Assert.Multiple(() =>
            {
                exception.SqlState.ShouldBe(PostgresErrorCodes.CheckViolation);
                exception.ConstraintName.ShouldBe(ConstraintName);
            });
        }

        [Test]
        public void CheckConstraintUsesTheConfiguredIdentifierQuoting()
        {
            using var serviceProvider = CreatePostgresServices(providerSwitches: "Force Quote=false");
            using var scope = serviceProvider.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<PostgresProcessor>();
            using var table = new PostgresTestTable("items_" + Guid.NewGuid().ToString("N"), processor, null, "code text");
            const string constraintName = "ck_item_code";

            var runner = (MigrationRunner)scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
            runner.Up(new AddCodeConstraint(table.Name.ToUpperInvariant(), null, constraintName));

            processor.ConstraintExists(null, table.Name, constraintName).ShouldBeTrue();
        }

        [Test]
        public void ReversingACheckConstraintDropsItFromTheSpecifiedSchema()
        {
            using var serviceProvider = CreatePostgresServices();
            using var scope = serviceProvider.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<PostgresProcessor>();
            var schemaName = "Check Rollback " + Guid.NewGuid().ToString("N");
            using var table = new PostgresTestTable(processor, schemaName, "code text");
            using var publicTable = new PostgresTestTable(table.Name, processor, null, "code text");
            var runner = (MigrationRunner)scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
            var migration = new AddCodeConstraint(table.Name, schemaName);
            runner.Up(new AddCodeConstraint(table.Name, null));
            runner.Up(migration);

            runner.Down(migration);

            Assert.Multiple(() =>
            {
                processor.ConstraintExists(schemaName, table.Name, ConstraintName).ShouldBeFalse();
                processor.ConstraintExists(null, publicTable.Name, ConstraintName).ShouldBeTrue();
            });
        }

        [Test]
        public void CheckConstraintWithoutValidationSkipsExistingRowsAndEnforcesNewRows()
        {
            using var serviceProvider = CreatePostgresServices();
            using var scope = serviceProvider.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<PostgresProcessor>();
            using var table = new PostgresTestTable(processor, null, "code text");
            InsertCode(table, "a");
            var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

            runner.Up(new CheckConstraintMigration(migration => migration.Create
                .Check(ConstraintName).OnTable(table.Name).Condition("length(code) = 3").WithNoCheck()));

            var exception = Should.Throw<PostgresException>(() => InsertCode(table, "a"));
            exception.SqlState.ShouldBe(PostgresErrorCodes.CheckViolation);
        }

        [Test]
        public void EnablingAnUnvalidatedCheckConstraintChecksExistingRows()
        {
            using var serviceProvider = CreatePostgresServices();
            using var scope = serviceProvider.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<PostgresProcessor>();
            using var table = new PostgresTestTable(processor, null, "code text");
            InsertCode(table, "a");
            var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
            runner.Up(new CheckConstraintMigration(migration => migration.Create
                .Check(ConstraintName).OnTable(table.Name).Condition("length(code) = 3").WithNoCheck()));

            var exception = Should.Throw<Exception>(() => runner.Up(new CheckConstraintMigration(
                migration => migration.Alter.Check(ConstraintName).OnTable(table.Name).Enabled())));

            exception.InnerException.ShouldBeOfType<PostgresException>().SqlState.ShouldBe(PostgresErrorCodes.CheckViolation);
        }

        [Test]
        public void ChangingAConditionReplacesTheCheckConstraint()
        {
            using var serviceProvider = CreatePostgresServices();
            using var scope = serviceProvider.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<PostgresProcessor>();
            using var table = new PostgresTestTable(processor, null, "code text");
            var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
            runner.Up(new AddCodeConstraint(table.Name, null));

            runner.Up(new CheckConstraintMigration(migration => migration.Alter
                .Check(ConstraintName).OnTable(table.Name).Condition("length(code) = 1")));

            InsertCode(table, "a");
            var exception = Should.Throw<PostgresException>(() => InsertCode(table, "abc"));
            exception.SqlState.ShouldBe(PostgresErrorCodes.CheckViolation);
        }

        [Test]
        public void FailedConditionChangePreservesTheCheckConstraintWithoutAMigrationTransaction()
        {
            using var serviceProvider = CreatePostgresServices();
            using var scope = serviceProvider.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<PostgresProcessor>();
            using var table = new PostgresTestTable(processor, null, "code text");
            var runner = (MigrationRunner)scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
            runner.Up(new AddCodeConstraint(table.Name, null));
            InsertCode(table, "abc");
            var migration = new CheckConstraintMigration(step => step.Alter
                .Check(ConstraintName).OnTable(table.Name).Condition("length(code) = 1"));
            var migrationInfo = new NonAttributedMigrationToMigrationInfoAdapter(migration, TransactionBehavior.None);

            var changeFailure = Should.Throw<Exception>(() => runner.ApplyMigrationUp(migrationInfo, useTransaction: false));
            var insertFailure = Should.Throw<PostgresException>(() => InsertCode(table, "a"));

            Assert.Multiple(() =>
            {
                changeFailure.InnerException.ShouldBeOfType<PostgresException>().SqlState.ShouldBe(PostgresErrorCodes.CheckViolation);
                insertFailure.SqlState.ShouldBe(PostgresErrorCodes.CheckViolation);
            });
        }

        private static ServiceProvider CreatePostgresServices(string defaultSchemaName = null, string providerSwitches = null)
        {
            IntegrationTestOptions.Postgres.IgnoreIfNotEnabled();

            return ServiceCollectionExtensions.CreateServices()
                .AddSingleton<IConventionSet>(new DefaultConventionSet(defaultSchemaName, workingDirectory: null))
                .Configure<ProcessorOptions>(options => options.ProviderSwitches = providerSwitches)
                .ConfigureRunner(builder => builder
                    .AddPostgres()
                    .WithGlobalConnectionString(IntegrationTestOptions.Postgres.ConnectionString))
                .BuildServiceProvider();
        }

        private static void InsertCode(PostgresTestTable table, string code)
        {
            using var command = new NpgsqlCommand($"INSERT INTO {table.NameWithSchema} (code) VALUES (@code)", table.Connection);
            command.Parameters.AddWithValue("code", code);
            command.ExecuteNonQuery();
        }

        private sealed class CheckConstraintMigration : Migration
        {
            private readonly Action<Migration> _up;

            public CheckConstraintMigration(Action<Migration> up)
            {
                _up = up;
            }

            public override void Up() => _up(this);

            public override void Down() => throw new NotSupportedException();
        }

        private sealed class AddCodeConstraint : AutoReversingMigration
        {
            private readonly string _tableName;
            private readonly string _schemaName;
            private readonly string _constraintName;

            public AddCodeConstraint(string tableName, string schemaName, string constraintName = ConstraintName)
            {
                _tableName = tableName;
                _schemaName = schemaName;
                _constraintName = constraintName;
            }

            public override void Up()
            {
                var constraint = Create.Check(_constraintName).OnTable(_tableName);
                var condition = _schemaName == null ? constraint : constraint.InSchema(_schemaName);
                condition.Condition("code ~ '^[a-z]{3}$'");
            }
        }
    }
}
