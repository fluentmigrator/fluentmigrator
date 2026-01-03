#region License
// Copyright (c) 2018, Fluent Migrator Project
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
#endregion

using System.Collections.Generic;
using System.Linq;

using FluentMigrator.Builder.SecurityLabel.Anon;
using FluentMigrator.Builders.Create;
using FluentMigrator.Builders.Delete;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Postgres;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors.Postgres;
using FluentMigrator.Tests.Helpers;

using Microsoft.Extensions.DependencyInjection;

using Moq;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Integration.Processors.Postgres
{
    [TestFixture]
    [Category("Integration")]
    [Category("Postgres")]
    public class PostgresSecurityLabelIntegrationTests
    {
        private ServiceProvider ServiceProvider { get; set; }
        private IServiceScope ServiceScope { get; set; }
        private PostgresProcessor Processor { get; set; }

        private void InstallAnonExtension()
        {
            // Install extension if not already installed
            Processor.Execute("CREATE EXTENSION IF NOT EXISTS anon;");
            Processor.Execute("SELECT anon.init();");
        }

        [Test]
        public void CanApplySecurityLabelToTableUsingFluentApi()
        {
            using (var table = new PostgresTestTable(Processor, "test_schema", "id int"))
            {
                var (createRoot, expressions) = CreateExpressionRootWithContext();

                createRoot.SecurityLabel("anon")
                    .OnTable(table.Name)
                    .InSchema("test_schema")
                    .WithLabel("TABLESAMPLE BERNOULLI(10)");

                ExecuteExpressions(expressions);

                var hasLabel = SecurityLabelExists("pg_class", table.Name, "TABLESAMPLE BERNOULLI(10)");
                hasLabel.ShouldBeTrue();
            }
        }

        [Test]
        public void CanApplySecurityLabelToColumnUsingFluentApi()
        {
            using (var table = new PostgresTestTable(Processor, "test_schema", "id int"))
            {
                var (createRoot, expressions) = CreateExpressionRootWithContext();

                createRoot.SecurityLabel("anon")
                    .OnColumn("id")
                    .OnTable(table.Name)
                    .InSchema("test_schema")
                    .WithLabel("MASKED WITH FUNCTION anon.fake_email()");

                ExecuteExpressions(expressions);

                var hasLabel = ColumnSecurityLabelExists(table.Name, "id", "MASKED WITH FUNCTION anon.fake_email()");
                hasLabel.ShouldBeTrue();
            }
        }

        [Test]
        public void CanDeleteSecurityLabelFromTableUsingFluentApi()
        {
            using (var table = new PostgresTestTable(Processor, "test_schema", "id int"))
            {
                var (createRoot, createExpressions) = CreateExpressionRootWithContext();
                createRoot.SecurityLabel("anon")
                    .OnTable(table.Name)
                    .InSchema("test_schema")
                    .WithLabel("TABLESAMPLE BERNOULLI(10)");
                ExecuteExpressions(createExpressions);

                var hasLabel = SecurityLabelExists("pg_class", table.Name, "TABLESAMPLE BERNOULLI(10)");
                hasLabel.ShouldBeTrue();

                var (deleteRoot, deleteExpressions) = DeleteExpressionRootWithContext();
                deleteRoot.SecurityLabel("anon")
                    .FromTable(table.Name)
                    .InSchema("test_schema");
                ExecuteExpressions(deleteExpressions);

                hasLabel = SecurityLabelExists("pg_class", table.Name);
                hasLabel.ShouldBeFalse();
            }
        }

        [Test]
        public void CanApplySecurityLabelToViewUsingFluentApi()
        {
            const string viewName = "test_security_label_view";

            using (var table = new PostgresTestTable(Processor, "test_schema", "id int"))
            {
                try
                {
                    Processor.Execute($"CREATE VIEW \"test_schema\".\"{viewName}\" AS SELECT id FROM \"test_schema\".\"{table.Name}\";");

                    var (createRoot, expressions) = CreateExpressionRootWithContext();
                    createRoot.SecurityLabel("anon")
                        .OnView(viewName)
                        .InSchema("test_schema")
                        .WithLabel("TABLESAMPLE BERNOULLI(10)");
                    ExecuteExpressions(expressions);

                    var hasLabel = SecurityLabelExists("pg_class", viewName, "TABLESAMPLE BERNOULLI(10)");
                    hasLabel.ShouldBeTrue();
                }
                finally
                {
                    Processor.Execute($"DROP VIEW IF EXISTS \"test_schema\".\"{viewName}\";");
                }
            }
        }

        [Test]
        public void CanApplySecurityLabelWithTypedBuilderMaskedWithValue()
        {
            using (var table = new PostgresTestTable(Processor, "test_schema", "name varchar(100)"))
            {
                var (createRoot, expressions) = CreateExpressionRootWithContext();

                createRoot.SecurityLabel<AnonSecurityLabelBuilder>()
                    .OnColumn("name")
                    .OnTable(table.Name)
                    .InSchema("test_schema")
                    .WithLabel(label => label.MaskedWithValue("CONFIDENTIAL"));

                ExecuteExpressions(expressions);

                var hasLabel = ColumnSecurityLabelExists(table.Name, "name", "MASKED WITH VALUE 'CONFIDENTIAL'");
                hasLabel.ShouldBeTrue();
            }
        }

        [Test]
        public void CanApplySecurityLabelWithTypedBuilderMaskedWithFakeEmail()
        {
            using (var table = new PostgresTestTable(Processor, "test_schema", "email varchar(255)"))
            {
                var (createRoot, expressions) = CreateExpressionRootWithContext();

                createRoot.SecurityLabel<AnonSecurityLabelBuilder>()
                    .OnColumn("email")
                    .OnTable(table.Name)
                    .InSchema("test_schema")
                    .WithLabel(label => label.MaskedWithFakeEmail());

                ExecuteExpressions(expressions);

                var hasLabel = ColumnSecurityLabelExists(table.Name, "email", "MASKED WITH FUNCTION anon.fake_email()");
                hasLabel.ShouldBeTrue();
            }
        }

        [Test]
        public void CanApplySecurityLabelWithTypedBuilderMaskedWithDummyLastName()
        {
            using (var table = new PostgresTestTable(Processor, "test_schema", "lastname varchar(100)"))
            {
                var (createRoot, expressions) = CreateExpressionRootWithContext();

                createRoot.SecurityLabel<AnonSecurityLabelBuilder>()
                    .OnColumn("lastname")
                    .OnTable(table.Name)
                    .InSchema("test_schema")
                    .WithLabel(label => label.MaskedWithDummyLastName());

                ExecuteExpressions(expressions);

                var hasLabel = ColumnSecurityLabelExists(table.Name, "lastname", "MASKED WITH FUNCTION anon.dummy_last_name()");
                hasLabel.ShouldBeTrue();
            }
        }

        [Test]
        public void CanApplySecurityLabelWithTypedBuilderMaskedWithPseudoEmail()
        {
            using (var table = new PostgresTestTable(Processor, "test_schema", "username varchar(100), email varchar(255)"))
            {
                var (createRoot, expressions) = CreateExpressionRootWithContext();

                createRoot.SecurityLabel<AnonSecurityLabelBuilder>()
                    .OnColumn("email")
                    .OnTable(table.Name)
                    .InSchema("test_schema")
                    .WithLabel(label => label.MaskedWithPseudoEmail("username"));

                ExecuteExpressions(expressions);

                var hasLabel = ColumnSecurityLabelExists(table.Name, "email", "MASKED WITH FUNCTION anon.pseudo_email(username)");
                hasLabel.ShouldBeTrue();
            }
        }

        [Test]
        public void TypedBuilderAutomaticallySetsAnonProvider()
        {
            using (var table = new PostgresTestTable(Processor, "test_schema", "name varchar(100)"))
            {
                var (createRoot, expressions) = CreateExpressionRootWithContext();

                createRoot.SecurityLabel<AnonSecurityLabelBuilder>()
                    .OnColumn("name")
                    .OnTable(table.Name)
                    .InSchema("test_schema")
                    .WithLabel(label => label.MaskedWithFakeFirstName());

                // The expression should have been generated with the anon provider
                expressions.Count.ShouldBe(1);
                var sqlExpression = expressions[0] as ExecuteSqlStatementExpression;
                sqlExpression.ShouldNotBeNull();
                sqlExpression.SqlStatement.ShouldContain("FOR \"anon\"");
            }
        }

        [Test]
        public void CanDeleteSecurityLabelFromColumnUsingFluentApi()
        {
            using (var table = new PostgresTestTable(Processor, "test_schema", "id int"))
            {
                var (createRoot, createExpressions) = CreateExpressionRootWithContext();
                createRoot.SecurityLabel("anon")
                    .OnColumn("id")
                    .OnTable(table.Name)
                    .InSchema("test_schema")
                    .WithLabel("MASKED WITH FUNCTION anon.fake_email()");
                ExecuteExpressions(createExpressions);

                var hasLabel = ColumnSecurityLabelExists(table.Name, "id", "MASKED WITH FUNCTION anon.fake_email()");
                hasLabel.ShouldBeTrue();

                var (deleteRoot, deleteExpressions) = DeleteExpressionRootWithContext();
                deleteRoot.SecurityLabel("anon")
                    .FromColumn("id")
                    .OnTable(table.Name)
                    .InSchema("test_schema");
                ExecuteExpressions(deleteExpressions);

                hasLabel = ColumnSecurityLabelExists(table.Name, "id");
                hasLabel.ShouldBeFalse();
            }
        }

        [Test]
        public void CanDeleteSecurityLabelFromViewUsingFluentApi()
        {
            const string viewName = "test_security_label_delete_view";

            using (var table = new PostgresTestTable(Processor, "test_schema", "id int"))
            {
                try
                {
                    Processor.Execute($"CREATE VIEW \"test_schema\".\"{viewName}\" AS SELECT id FROM \"test_schema\".\"{table.Name}\";");

                    var (createRoot, createExpressions) = CreateExpressionRootWithContext();
                    createRoot.SecurityLabel("anon")
                        .OnView(viewName)
                        .InSchema("test_schema")
                        .WithLabel("TABLESAMPLE BERNOULLI(10)");
                    ExecuteExpressions(createExpressions);

                    var hasLabel = SecurityLabelExists("pg_class", viewName, "TABLESAMPLE BERNOULLI(10)");
                    hasLabel.ShouldBeTrue();

                    var (deleteRoot, deleteExpressions) = DeleteExpressionRootWithContext();
                    deleteRoot.SecurityLabel("anon")
                        .FromView(viewName)
                        .InSchema("test_schema");
                    ExecuteExpressions(deleteExpressions);

                    hasLabel = SecurityLabelExists("pg_class", viewName);
                    hasLabel.ShouldBeFalse();
                }
                finally
                {
                    Processor.Execute($"DROP VIEW IF EXISTS \"test_schema\".\"{viewName}\";");
                }
            }
        }

        [Test]
        public void CanDeleteSecurityLabelUsingTypedBuilder()
        {
            using (var table = new PostgresTestTable(Processor, "test_schema", "email varchar(255)"))
            {
                var (createRoot, createExpressions) = CreateExpressionRootWithContext();
                createRoot.SecurityLabel<AnonSecurityLabelBuilder>()
                    .OnColumn("email")
                    .OnTable(table.Name)
                    .InSchema("test_schema")
                    .WithLabel(label => label.MaskedWithFakeEmail());
                ExecuteExpressions(createExpressions);

                var hasLabel = ColumnSecurityLabelExists(table.Name, "email", "MASKED WITH FUNCTION anon.fake_email()");
                hasLabel.ShouldBeTrue();

                var (deleteRoot, deleteExpressions) = DeleteExpressionRootWithContext();
                deleteRoot.SecurityLabel<AnonSecurityLabelBuilder>()
                    .FromColumn("email")
                    .OnTable(table.Name)
                    .InSchema("test_schema");
                ExecuteExpressions(deleteExpressions);

                hasLabel = ColumnSecurityLabelExists(table.Name, "email", "MASKED WITH FUNCTION anon.fake_email()");
                hasLabel.ShouldBeFalse();
            }
        }

        private (CreateExpressionRoot, IList<IMigrationExpression>) CreateExpressionRootWithContext()
        {
            var expressions = new List<IMigrationExpression>();
            var contextMock = new Mock<IMigrationContext>();
            contextMock.SetupGet(c => c.Expressions).Returns(expressions);
            return (new CreateExpressionRoot(contextMock.Object), expressions);
        }

        private (DeleteExpressionRoot, IList<IMigrationExpression>) DeleteExpressionRootWithContext()
        {
            var expressions = new List<IMigrationExpression>();
            var contextMock = new Mock<IMigrationContext>();
            contextMock.SetupGet(c => c.Expressions).Returns(expressions);
            return (new DeleteExpressionRoot(contextMock.Object), expressions);
        }

        private void ExecuteExpressions(IList<IMigrationExpression> expressions)
        {
            foreach (var sqlExpression in expressions.OfType<ExecuteSqlStatementExpression>())
            {
                Processor.Execute(sqlExpression.SqlStatement);
            }
        }

        private bool SecurityLabelExists(string catalogTable, string objectName, string expectedLabel = null)
        {
            var escapedObjectName = EscapeSqlString(objectName);
            var escapedLabel = EscapeSqlString(expectedLabel);

            string query;

            if (expectedLabel == null)
            {
                query = $@"
                SELECT 1
                FROM pg_seclabels sl
                JOIN {catalogTable} c ON sl.objoid = c.oid
                WHERE c.{{0}} = '{escapedObjectName}'";
            }
            else
            {
                query = $@"
                SELECT 1
                FROM pg_seclabels sl
                JOIN {catalogTable} c ON sl.objoid = c.oid
                WHERE c.{{0}} = '{escapedObjectName}' AND sl.label = '{escapedLabel}'";
            }

            var columnName = catalogTable switch
            {
                "pg_class" => "relname",
                "pg_namespace" => "nspname",
                "pg_authid" => "rolname",
                _ => "name"
            };

            return Processor.Exists(string.Format(query, columnName));
        }

        private bool ColumnSecurityLabelExists(string tableName, string columnName, string expectedLabel = null)
        {
            var escapedTableName = EscapeSqlString(tableName);
            var escapedColumnName = EscapeSqlString(columnName);
            var escapedLabel = EscapeSqlString(expectedLabel);

            string query;

            if (expectedLabel == null)
            {
                query = $@"
                SELECT 1
                FROM pg_seclabels sl
                JOIN pg_class c ON sl.objoid = c.oid
                JOIN pg_attribute a ON a.attrelid = c.oid AND a.attnum = sl.objsubid
                WHERE c.relname = '{escapedTableName}'
                  AND a.attname = '{escapedColumnName}'";
            }
            else
            {
                query = $@"
                SELECT 1
                FROM pg_seclabels sl
                JOIN pg_class c ON sl.objoid = c.oid
                JOIN pg_attribute a ON a.attrelid = c.oid AND a.attnum = sl.objsubid
                WHERE c.relname = '{escapedTableName}'
                  AND a.attname = '{escapedColumnName}'
                  AND sl.label = '{escapedLabel}'";
            }

            return Processor.Exists(query);
        }

        private static string EscapeSqlString(string value)
        {
            return value?.Replace("'", "''") ?? string.Empty;
        }

        [OneTimeSetUp]
        public void ClassSetUp()
        {
            IntegrationTestOptions.Postgres.IgnoreIfNotEnabled();

            var services = ServiceCollectionExtensions.CreateServices()
                .ConfigureRunner(r => r.AddPostgres())
                .AddScoped<IConnectionStringReader>(
                    _ => new PassThroughConnectionStringReader(IntegrationTestOptions.Postgres.ConnectionString));
            ServiceProvider = services.BuildServiceProvider();
        }

        [OneTimeTearDown]
        public void ClassTearDown()
        {
            ServiceProvider?.Dispose();
        }

        [SetUp]
        public void SetUp()
        {
            ServiceScope = ServiceProvider.CreateScope();
            Processor = ServiceScope.ServiceProvider.GetRequiredService<PostgresProcessor>();

            InstallAnonExtension();
        }

        [TearDown]
        public void TearDown()
        {
            ServiceScope?.Dispose();
            Processor?.Dispose();
        }
    }
}
