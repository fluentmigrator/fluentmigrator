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
            IntegrationTestOptions.Postgres.IgnoreIfNotEnabled();

            using var serviceProvider = ServiceCollectionExtensions.CreateServices()
                .AddSingleton<IConventionSet>(new DefaultConventionSet(defaultSchemaName, workingDirectory: null))
                .ConfigureRunner(builder => builder
                    .AddPostgres()
                    .WithGlobalConnectionString(IntegrationTestOptions.Postgres.ConnectionString))
                .BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<PostgresProcessor>();
            using var table = new PostgresTestTable(processor, schemaName ?? defaultSchemaName, "code text");

            var runner = (MigrationRunner)scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
            runner.Up(new AddCodeConstraint(table.Name, schemaName));

            using var insertValid = new NpgsqlCommand($"INSERT INTO {table.NameWithSchema} (code) VALUES ('abc')", table.Connection);
            insertValid.ExecuteNonQuery();

            using var insertInvalid = new NpgsqlCommand($"INSERT INTO {table.NameWithSchema} (code) VALUES ('a')", table.Connection);
            var exception = Should.Throw<PostgresException>(() => insertInvalid.ExecuteNonQuery());
            Assert.Multiple(() =>
            {
                exception.SqlState.ShouldBe(PostgresErrorCodes.CheckViolation);
                exception.ConstraintName.ShouldBe(ConstraintName);
            });
        }

        [Test]
        public void CheckConstraintUsesTheConfiguredIdentifierQuoting()
        {
            IntegrationTestOptions.Postgres.IgnoreIfNotEnabled();

            using var serviceProvider = ServiceCollectionExtensions.CreateServices()
                .AddSingleton<IConventionSet>(new DefaultConventionSet(defaultSchemaName: null, workingDirectory: null))
                .Configure<ProcessorOptions>(options => options.ProviderSwitches = "Force Quote=false")
                .ConfigureRunner(builder => builder
                    .AddPostgres()
                    .WithGlobalConnectionString(IntegrationTestOptions.Postgres.ConnectionString))
                .BuildServiceProvider();
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
            IntegrationTestOptions.Postgres.IgnoreIfNotEnabled();

            using var serviceProvider = ServiceCollectionExtensions.CreateServices()
                .ConfigureRunner(builder => builder
                    .AddPostgres()
                    .WithGlobalConnectionString(IntegrationTestOptions.Postgres.ConnectionString))
                .BuildServiceProvider();
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
