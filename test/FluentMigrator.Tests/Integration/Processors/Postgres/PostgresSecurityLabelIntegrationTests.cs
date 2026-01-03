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

        [Test]
        public void CanApplySecurityLabelToTableUsingFluentApi()
        {
            using (var table = new PostgresTestTable(Processor, "public", "id int"))
            {
                var (createRoot, expressions) = CreateExpressionRootWithContext();

                createRoot.SecurityLabel("foo")
                    .OnTable(table.Name)
                    .InSchema("public")
                    .WithLabel("test label");

                ExecuteExpressions(expressions);

                var hasLabel = SecurityLabelExists("pg_class", table.Name, "test label");
                hasLabel.ShouldBeTrue();
            }
        }

        [Test]
        public void CanApplySecurityLabelToTableWithProviderUsingFluentApi()
        {
            using (var table = new PostgresTestTable(Processor, "public", "id int"))
            {
                var (createRoot, expressions) = CreateExpressionRootWithContext();

                createRoot.SecurityLabel("foo")
                    .OnTable(table.Name)
                    .InSchema("public")
                    .WithLabel("test label with provider");

                ExecuteExpressions(expressions);

                var hasLabel = SecurityLabelExists("pg_class", table.Name, "test label with provider");
                hasLabel.ShouldBeTrue();
            }
        }

        [Test]
        public void CanApplySecurityLabelToColumnUsingFluentApi()
        {
            using (var table = new PostgresTestTable(Processor, "public", "id int"))
            {
                var (createRoot, expressions) = CreateExpressionRootWithContext();

                createRoot.SecurityLabel("foo")
                    .OnColumn("id")
                    .OnTable(table.Name)
                    .InSchema("public")
                    .WithLabel("column label");

                ExecuteExpressions(expressions);

                var hasLabel = ColumnSecurityLabelExists(table.Name, "id", "column label");
                hasLabel.ShouldBeTrue();
            }
        }

        [Test]
        public void CanDeleteSecurityLabelFromTableUsingFluentApi()
        {
            using (var table = new PostgresTestTable(Processor, "public", "id int"))
            {
                var (createRoot, createExpressions) = CreateExpressionRootWithContext();
                createRoot.SecurityLabel("foo")
                    .OnTable(table.Name)
                    .InSchema("public")
                    .WithLabel("to be deleted");
                ExecuteExpressions(createExpressions);

                var hasLabel = SecurityLabelExists("pg_class", table.Name, "to be deleted");
                hasLabel.ShouldBeTrue();

                var (deleteRoot, deleteExpressions) = DeleteExpressionRootWithContext();
                deleteRoot.SecurityLabel("foo")
                    .FromTable(table.Name)
                    .InSchema("public");
                ExecuteExpressions(deleteExpressions);

                hasLabel = SecurityLabelExists("pg_class", table.Name, "to be deleted");
                hasLabel.ShouldBeFalse();
            }
        }

        [Test]
        public void CanApplySecurityLabelToSchemaUsingFluentApi()
        {
            const string schemaName = "test_security_label_schema";

            try
            {
                Processor.Execute($"CREATE SCHEMA IF NOT EXISTS \"{schemaName}\";");

                var (createRoot, expressions) = CreateExpressionRootWithContext();
                createRoot.SecurityLabel("foo")
                    .OnSchema(schemaName)
                    .WithLabel("schema label");
                ExecuteExpressions(expressions);

                var hasLabel = SecurityLabelExists("pg_namespace", schemaName, "schema label");
                hasLabel.ShouldBeTrue();
            }
            finally
            {
                Processor.Execute($"DROP SCHEMA IF EXISTS \"{schemaName}\" CASCADE;");
            }
        }

        [Test]
        public void CanApplySecurityLabelToRoleUsingFluentApi()
        {
            const string roleName = "test_security_label_role";

            try
            {
                Processor.Execute($"CREATE ROLE \"{roleName}\";");

                var (createRoot, expressions) = CreateExpressionRootWithContext();
                createRoot.SecurityLabel("foo")
                    .OnRole(roleName)
                    .WithLabel("role label");
                ExecuteExpressions(expressions);

                var hasLabel = SecurityLabelExists("pg_authid", roleName, "role label");
                hasLabel.ShouldBeTrue();
            }
            finally
            {
                Processor.Execute($"DROP ROLE IF EXISTS \"{roleName}\";");
            }
        }

        [Test]
        public void CanApplySecurityLabelToViewUsingFluentApi()
        {
            const string viewName = "test_security_label_view";

            using (var table = new PostgresTestTable(Processor, "public", "id int"))
            {
                try
                {
                    Processor.Execute($"CREATE VIEW \"public\".\"{viewName}\" AS SELECT id FROM \"public\".\"{table.Name}\";");

                    var (createRoot, expressions) = CreateExpressionRootWithContext();
                    createRoot.SecurityLabel("foo")
                        .OnView(viewName)
                        .InSchema("public")
                        .WithLabel("view label");
                    ExecuteExpressions(expressions);

                    var hasLabel = SecurityLabelExists("pg_class", viewName, "view label");
                    hasLabel.ShouldBeTrue();
                }
                finally
                {
                    Processor.Execute($"DROP VIEW IF EXISTS \"public\".\"{viewName}\";");
                }
            }
        }

        [Test]
        public void CanApplySecurityLabelWithTypedBuilderMaskedWithValue()
        {
            using (var table = new PostgresTestTable(Processor, "public", "name varchar(100)"))
            {
                var (createRoot, expressions) = CreateExpressionRootWithContext();

                createRoot.SecurityLabel<AnonSecurityLabelBuilder>()
                    .OnColumn("name")
                    .OnTable(table.Name)
                    .InSchema("public")
                    .WithLabel(label => label.MaskedWithValue("CONFIDENTIAL"));

                ExecuteExpressions(expressions);

                var hasLabel = ColumnSecurityLabelExists(table.Name, "name", "MASKED WITH VALUE ''CONFIDENTIAL''");
                hasLabel.ShouldBeTrue();
            }
        }

        [Test]
        public void CanApplySecurityLabelWithTypedBuilderMaskedWithFakeEmail()
        {
            using (var table = new PostgresTestTable(Processor, "public", "email varchar(255)"))
            {
                var (createRoot, expressions) = CreateExpressionRootWithContext();

                createRoot.SecurityLabel<AnonSecurityLabelBuilder>()
                    .OnColumn("email")
                    .OnTable(table.Name)
                    .InSchema("public")
                    .WithLabel(label => label.MaskedWithFakeEmail());

                ExecuteExpressions(expressions);

                var hasLabel = ColumnSecurityLabelExists(table.Name, "email", "MASKED WITH FUNCTION anon.fake_email()");
                hasLabel.ShouldBeTrue();
            }
        }

        [Test]
        public void CanApplySecurityLabelWithTypedBuilderMaskedWithDummyLastName()
        {
            using (var table = new PostgresTestTable(Processor, "public", "lastname varchar(100)"))
            {
                var (createRoot, expressions) = CreateExpressionRootWithContext();

                createRoot.SecurityLabel<AnonSecurityLabelBuilder>()
                    .OnColumn("lastname")
                    .OnTable(table.Name)
                    .InSchema("public")
                    .WithLabel(label => label.MaskedWithDummyLastName());

                ExecuteExpressions(expressions);

                var hasLabel = ColumnSecurityLabelExists(table.Name, "lastname", "MASKED WITH FUNCTION anon.dummy_last_name()");
                hasLabel.ShouldBeTrue();
            }
        }

        [Test]
        public void CanApplySecurityLabelWithTypedBuilderMaskedWithPseudoEmail()
        {
            using (var table = new PostgresTestTable(Processor, "public", "username varchar(100), email varchar(255)"))
            {
                var (createRoot, expressions) = CreateExpressionRootWithContext();

                createRoot.SecurityLabel<AnonSecurityLabelBuilder>()
                    .OnColumn("email")
                    .OnTable(table.Name)
                    .InSchema("public")
                    .WithLabel(label => label.MaskedWithPseudoEmail("username"));

                ExecuteExpressions(expressions);

                var hasLabel = ColumnSecurityLabelExists(table.Name, "email", "MASKED WITH FUNCTION anon.pseudo_email(username)");
                hasLabel.ShouldBeTrue();
            }
        }

        [Test]
        public void TypedBuilderAutomaticallySetsAnonProvider()
        {
            using (var table = new PostgresTestTable(Processor, "public", "name varchar(100)"))
            {
                var (createRoot, expressions) = CreateExpressionRootWithContext();

                createRoot.SecurityLabel<AnonSecurityLabelBuilder>()
                    .OnColumn("name")
                    .OnTable(table.Name)
                    .InSchema("public")
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
            using (var table = new PostgresTestTable(Processor, "public", "id int"))
            {
                var (createRoot, createExpressions) = CreateExpressionRootWithContext();
                createRoot.SecurityLabel("foo")
                    .OnColumn("id")
                    .OnTable(table.Name)
                    .InSchema("public")
                    .WithLabel("column label to delete");
                ExecuteExpressions(createExpressions);

                var hasLabel = ColumnSecurityLabelExists(table.Name, "id", "column label to delete");
                hasLabel.ShouldBeTrue();

                var (deleteRoot, deleteExpressions) = DeleteExpressionRootWithContext();
                deleteRoot.SecurityLabel("foo")
                    .FromColumn("id")
                    .OnTable(table.Name)
                    .InSchema("public");
                ExecuteExpressions(deleteExpressions);

                hasLabel = ColumnSecurityLabelExists(table.Name, "id", "column label to delete");
                hasLabel.ShouldBeFalse();
            }
        }

        [Test]
        public void CanDeleteSecurityLabelFromSchemaUsingFluentApi()
        {
            const string schemaName = "test_security_label_delete_schema";

            try
            {
                Processor.Execute($"CREATE SCHEMA IF NOT EXISTS \"{schemaName}\";");

                var (createRoot, createExpressions) = CreateExpressionRootWithContext();
                createRoot.SecurityLabel("foo")
                    .OnSchema(schemaName)
                    .WithLabel("schema label to delete");
                ExecuteExpressions(createExpressions);

                var hasLabel = SecurityLabelExists("pg_namespace", schemaName, "schema label to delete");
                hasLabel.ShouldBeTrue();

                var (deleteRoot, deleteExpressions) = DeleteExpressionRootWithContext();
                deleteRoot.SecurityLabel("foo")
                    .FromSchema(schemaName);
                ExecuteExpressions(deleteExpressions);

                hasLabel = SecurityLabelExists("pg_namespace", schemaName, "schema label to delete");
                hasLabel.ShouldBeFalse();
            }
            finally
            {
                Processor.Execute($"DROP SCHEMA IF EXISTS \"{schemaName}\" CASCADE;");
            }
        }

        [Test]
        public void CanDeleteSecurityLabelFromRoleUsingFluentApi()
        {
            const string roleName = "test_security_label_delete_role";

            try
            {
                Processor.Execute($"CREATE ROLE \"{roleName}\";");

                var (createRoot, createExpressions) = CreateExpressionRootWithContext();
                createRoot.SecurityLabel("foo")
                    .OnRole(roleName)
                    .WithLabel("role label to delete");
                ExecuteExpressions(createExpressions);

                var hasLabel = SecurityLabelExists("pg_authid", roleName, "role label to delete");
                hasLabel.ShouldBeTrue();

                var (deleteRoot, deleteExpressions) = DeleteExpressionRootWithContext();
                deleteRoot.SecurityLabel("foo")
                    .FromRole(roleName);
                ExecuteExpressions(deleteExpressions);

                hasLabel = SecurityLabelExists("pg_authid", roleName, "role label to delete");
                hasLabel.ShouldBeFalse();
            }
            finally
            {
                Processor.Execute($"DROP ROLE IF EXISTS \"{roleName}\";");
            }
        }

        [Test]
        public void CanDeleteSecurityLabelFromViewUsingFluentApi()
        {
            const string viewName = "test_security_label_delete_view";

            using (var table = new PostgresTestTable(Processor, "public", "id int"))
            {
                try
                {
                    Processor.Execute($"CREATE VIEW \"public\".\"{viewName}\" AS SELECT id FROM \"public\".\"{table.Name}\";");

                    var (createRoot, createExpressions) = CreateExpressionRootWithContext();
                    createRoot.SecurityLabel("foo")
                        .OnView(viewName)
                        .InSchema("public")
                        .WithLabel("view label to delete");
                    ExecuteExpressions(createExpressions);

                    var hasLabel = SecurityLabelExists("pg_class", viewName, "view label to delete");
                    hasLabel.ShouldBeTrue();

                    var (deleteRoot, deleteExpressions) = DeleteExpressionRootWithContext();
                    deleteRoot.SecurityLabel("foo")
                        .FromView(viewName)
                        .InSchema("public");
                    ExecuteExpressions(deleteExpressions);

                    hasLabel = SecurityLabelExists("pg_class", viewName, "view label to delete");
                    hasLabel.ShouldBeFalse();
                }
                finally
                {
                    Processor.Execute($"DROP VIEW IF EXISTS \"public\".\"{viewName}\";");
                }
            }
        }

        [Test]
        public void CanDeleteSecurityLabelUsingTypedBuilder()
        {
            using (var table = new PostgresTestTable(Processor, "public", "email varchar(255)"))
            {
                var (createRoot, createExpressions) = CreateExpressionRootWithContext();
                createRoot.SecurityLabel<AnonSecurityLabelBuilder>()
                    .OnColumn("email")
                    .OnTable(table.Name)
                    .InSchema("public")
                    .WithLabel(label => label.MaskedWithFakeEmail());
                ExecuteExpressions(createExpressions);

                var hasLabel = ColumnSecurityLabelExists(table.Name, "email", "MASKED WITH FUNCTION anon.fake_email()");
                hasLabel.ShouldBeTrue();

                var (deleteRoot, deleteExpressions) = DeleteExpressionRootWithContext();
                deleteRoot.SecurityLabel<AnonSecurityLabelBuilder>()
                    .FromColumn("email")
                    .OnTable(table.Name)
                    .InSchema("public");
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

        private bool SecurityLabelExists(string catalogTable, string objectName, string expectedLabel)
        {
            var escapedObjectName = EscapeSqlString(objectName);
            var escapedLabel = EscapeSqlString(expectedLabel);

            var query = $@"
                SELECT 1
                FROM pg_seclabels sl
                JOIN {catalogTable} c ON sl.objoid = c.oid
                WHERE c.{{0}} = '{escapedObjectName}' AND sl.label = '{escapedLabel}'";

            var columnName = catalogTable switch
            {
                "pg_class" => "relname",
                "pg_namespace" => "nspname",
                "pg_authid" => "rolname",
                _ => "name"
            };

            return Processor.Exists(string.Format(query, columnName));
        }

        private bool ColumnSecurityLabelExists(string tableName, string columnName, string expectedLabel)
        {
            var escapedTableName = EscapeSqlString(tableName);
            var escapedColumnName = EscapeSqlString(columnName);
            var escapedLabel = EscapeSqlString(expectedLabel);

            var query = $@"
                SELECT 1
                FROM pg_seclabels sl
                JOIN pg_class c ON sl.objoid = c.oid
                JOIN pg_attribute a ON a.attrelid = c.oid AND a.attnum = sl.objsubid
                WHERE c.relname = '{escapedTableName}'
                  AND a.attname = '{escapedColumnName}'
                  AND sl.label = '{escapedLabel}'";

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
        }

        [TearDown]
        public void TearDown()
        {
            ServiceScope?.Dispose();
            Processor?.Dispose();
        }
    }
}
