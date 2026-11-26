#region License
//
// Copyright (c) 2026, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Conventions;
using FluentMigrator.Runner.Processors.SqlServer;
using FluentMigrator.Tests.Helpers;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Integration.Processors.SqlServer.SqlServer2016
{
    [TestFixture]
    [Category("Integration")]
    [Category("SqlServer2016")]
    public class SqlServerCheckConstraintIntegrationTests
    {
        private const int ConstraintViolation = 547;
        private const string PositiveQuantity = "quantity > 0";

        [Test]
        public void CreatingWithoutValidationAllowsExistingViolationsAndEnforcesNewRows()
        {
            using var services = CreateSqlServerServices();
            using var scope = services.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<SqlServer2016Processor>();
            using var table = CreateTable(processor);
            var constraint = GetConstraintName(table);
            InsertQuantity(processor, table, 0);
            var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

            runner.Up(new CheckConstraintMigration(migration => migration.Create.Check(constraint)
                .OnTable(table.Name).Condition(PositiveQuantity).WithNoCheck()));

            var exception = Should.Throw<SqlException>(() => InsertQuantity(processor, table, 0));
            exception.Number.ShouldBe(ConstraintViolation);
        }

        [Test]
        public void CreatingADisabledCheckConstraintAllowsViolations()
        {
            using var services = CreateSqlServerServices();
            using var scope = services.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<SqlServer2016Processor>();
            using var table = CreateTable(processor);
            var constraint = GetConstraintName(table);
            InsertQuantity(processor, table, 0);
            var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

            runner.Up(new CheckConstraintMigration(migration => migration.Create.Check(constraint)
                .OnTable(table.Name).Condition(PositiveQuantity).Disabled()));

            InsertQuantity(processor, table, 0);
            processor.ConstraintExists(null, table.Name, constraint).ShouldBeTrue();
        }

        [Test]
        public void DisablingAnExistingCheckConstraintAllowsNewViolations()
        {
            using var services = CreateSqlServerServices();
            using var scope = services.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<SqlServer2016Processor>();
            using var table = CreateTable(processor);
            var constraint = GetConstraintName(table);
            var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
            runner.Up(new CheckConstraintMigration(migration => migration.Create.Check(constraint)
                .OnTable(table.Name).Condition(PositiveQuantity)));

            runner.Up(new CheckConstraintMigration(migration => migration.Alter.Check(constraint).OnTable(table.Name).Disabled()));

            InsertQuantity(processor, table, 0);
            processor.ConstraintExists(null, table.Name, constraint).ShouldBeTrue();
        }

        [Test]
        public void EnablingWithoutValidationAllowsExistingViolationsAndEnforcesNewRows()
        {
            using var services = CreateSqlServerServices();
            using var scope = services.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<SqlServer2016Processor>();
            using var table = CreateTable(processor);
            var constraint = AddDisabledCheckConstraint(processor, table);
            InsertQuantity(processor, table, 0);
            var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

            runner.Up(new CheckConstraintMigration(migration => migration.Alter.Check(constraint)
                .OnTable(table.Name).Enabled().WithNoCheck()));

            var exception = Should.Throw<SqlException>(() => InsertQuantity(processor, table, 0));
            exception.Number.ShouldBe(ConstraintViolation);
        }

        [Test]
        public void EnablingValidatesExistingRows()
        {
            using var services = CreateSqlServerServices();
            using var scope = services.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<SqlServer2016Processor>();
            using var table = CreateTable(processor);
            var constraint = AddDisabledCheckConstraint(processor, table);
            InsertQuantity(processor, table, 0);
            var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

            var exception = Should.Throw<Exception>(() => runner.Up(new CheckConstraintMigration(
                migration => migration.Alter.Check(constraint).OnTable(table.Name).Enabled())));

            exception.InnerException.ShouldBeOfType<SqlException>().Number.ShouldBe(ConstraintViolation);
        }

        [Test]
        public void FailedConditionChangePreservesTheCheckConstraintWithAMigrationTransaction()
        {
            using var services = CreateSqlServerServices();
            using var scope = services.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<SqlServer2016Processor>();
            using var table = CreateTable(processor);
            var constraint = GetConstraintName(table);
            var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
            runner.Up(new CheckConstraintMigration(migration => migration.Create.Check(constraint)
                .OnTable(table.Name).Condition(PositiveQuantity)));
            InsertQuantity(processor, table, 1);

            var changeFailure = Should.Throw<Exception>(() => runner.Up(new CheckConstraintMigration(migration =>
                migration.Alter.Check(constraint).OnTable(table.Name).Condition("quantity > 1"))));
            var insertFailure = Should.Throw<SqlException>(() => InsertQuantity(processor, table, 0));

            Assert.Multiple(() =>
            {
                changeFailure.InnerException.ShouldBeOfType<SqlException>().Number.ShouldBe(ConstraintViolation);
                insertFailure.Number.ShouldBe(ConstraintViolation);
            });
        }

        private static ServiceProvider CreateSqlServerServices()
        {
            IntegrationTestOptions.SqlServer2016.IgnoreIfNotEnabled();

            return ServiceCollectionExtensions.CreateServices()
                .AddSingleton<IConventionSet>(new DefaultConventionSet(defaultSchemaName: null, workingDirectory: null))
                .ConfigureRunner(builder => builder
                    .AddSqlServer2016()
                    .WithGlobalConnectionString(IntegrationTestOptions.SqlServer2016.ConnectionString))
                .BuildServiceProvider();
        }

        private static SqlServerTestTable CreateTable(SqlServer2016Processor processor)
        {
            return new SqlServerTestTable("Items_" + Guid.NewGuid().ToString("N"), processor, null, "quantity int");
        }

        private static string GetConstraintName(SqlServerTestTable table) => "CK_" + table.Name;

        private static string AddDisabledCheckConstraint(SqlServer2016Processor processor, SqlServerTestTable table)
        {
            var constraint = GetConstraintName(table);
            var quotedConstraint = processor.Quoter.QuoteConstraintName(constraint);
            var quotedTable = processor.Quoter.QuoteTableName(table.Name, null);
            processor.Execute($"ALTER TABLE {quotedTable} ADD CONSTRAINT {quotedConstraint} CHECK ({PositiveQuantity})");
            processor.Execute($"ALTER TABLE {quotedTable} NOCHECK CONSTRAINT {quotedConstraint}");
            return constraint;
        }

        private static void InsertQuantity(SqlServer2016Processor processor, SqlServerTestTable table, int quantity)
        {
            var quotedTable = processor.Quoter.QuoteTableName(table.Name, null);
            using var command = new SqlCommand($"INSERT INTO {quotedTable} (quantity) VALUES (@quantity)", (SqlConnection)processor.Connection);
            command.Parameters.AddWithValue("quantity", quantity);
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
    }
}
