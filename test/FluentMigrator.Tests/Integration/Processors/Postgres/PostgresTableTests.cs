#region License
//
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
//
#endregion

using System.Data;

using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors.Postgres;
using FluentMigrator.Tests.Helpers;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Integration.Processors.Postgres
{
    [TestFixture]
    [Category("Integration")]
    [Category("Postgres")]
    public class PostgresTableTests : BaseTableTests
    {
        private ServiceProvider ServiceProvider { get; set; }
        private IServiceScope ServiceScope { get; set; }
        private PostgresProcessor Processor { get; set; }

        [Test]
        public override void CallingTableExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new PostgresTestTable("Test'Table", Processor, null, "id int"))
                Processor.TableExists(null, table.Name).ShouldBeTrue();
        }

        [Test]
        public override void CallingTableExistsReturnsFalseIfTableDoesNotExist()
        {
            Processor.TableExists(null, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingTableExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            Processor.TableExists("TestSchema", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingTableExistsReturnsTrueIfTableExists()
        {
            using (var table = new PostgresTestTable(Processor, null, "id int"))
                Processor.TableExists(null, table.Name).ShouldBeTrue();
        }

        [Test]
        public override void CallingTableExistsReturnsTrueIfTableExistsWithSchema()
        {
            using (var table = new PostgresTestTable(Processor, "TestSchema", "id int"))
                Processor.TableExists("TestSchema", table.Name).ShouldBeTrue();
        }

        [Test]
        public void CanDropTableIfExistsInCustomSchema()
        {
            using var table = new PostgresTestTable(Processor, "TestSchema", "id int");

            Processor.TableExists("TestSchema", table.Name).ShouldBeTrue();

            Processor.Process(new DeleteTableExpression
            {
                TableName = table.Name,
                SchemaName = "TestSchema",
                IfExists = true,
            });

            Processor.TableExists("TestSchema", table.Name).ShouldBeFalse();
        }

        [Test]
        public void CreateTableIfNotExistsSkipsAnExistingTableTheRoleHasNoPrivilegesOn()
        {
            // information_schema.tables lists only the tables the current role owns or has a privilege on. A role that
            // may create tables in the schema, but has no grant on a table that is already there, would not find it
            // there, and the CREATE TABLE in the DO block would fail with "relation already exists".
            var createTable = new CreateTableExpression { SchemaName = "FmNoGrants", TableName = "Widget", IfNotExists = true };
            createTable.Columns.Add(new ColumnDefinition { Name = "Id", Type = DbType.Int32, TableName = "Widget" });
            var index = new CreateIndexExpression
            {
                Index = new IndexDefinition { Name = "IX_Widget_Id", SchemaName = "FmNoGrants", TableName = "Widget" },
            };
            index.Index.Columns.Add(new IndexColumnDefinition { Name = "Id" });
            createTable.ExpressionsForNewTable.Add(index);

            try
            {
                DropSchemaAndRoleWithoutGrants();
                Processor.Execute("CREATE SCHEMA \"FmNoGrants\"");
                Processor.Execute("CREATE TABLE \"FmNoGrants\".\"Widget\" (\"Id\" integer)");
                Processor.Execute("CREATE ROLE fm_no_grants NOLOGIN");
                Processor.Execute("GRANT USAGE, CREATE ON SCHEMA \"FmNoGrants\" TO fm_no_grants");
                Processor.Execute("SET ROLE fm_no_grants");

                Processor.Process(createTable);

                Processor.Execute("RESET ROLE");
                Processor.Exists("SELECT 1 FROM pg_indexes WHERE schemaname = 'FmNoGrants' AND indexname = 'IX_Widget_Id'")
                    .ShouldBeFalse();
            }
            finally
            {
                Processor.Execute("RESET ROLE");
                DropSchemaAndRoleWithoutGrants();
            }
        }

        private void DropSchemaAndRoleWithoutGrants()
        {
            Processor.Execute("DROP SCHEMA IF EXISTS \"FmNoGrants\" CASCADE");
            Processor.Execute("DROP ROLE IF EXISTS fm_no_grants");
        }

        [Test]
        public void CreateTableIfNotExistsInPgTempCreatesAMissingTemporaryTable()
        {
            var createTable = CreateTemporaryTableIfNotExists();

            try
            {
                Processor.Process(createTable);

                Processor.Exists("SELECT 1 FROM pg_catalog.pg_class WHERE relnamespace = pg_catalog.pg_my_temp_schema() AND relname = 'IX_FmTemp_Id'")
                    .ShouldBeTrue();
            }
            finally
            {
                Processor.Execute("DROP TABLE IF EXISTS pg_temp.\"FmTemp\"");
            }
        }

        [Test]
        public void CreateTableIfNotExistsInPgTempSkipsAnExistingTemporaryTable()
        {
            // pg_temp is an alias for the session's temporary schema, which pg_namespace lists as pg_temp_N. A check for
            // a schema named pg_temp would not find the table, and the CREATE TABLE in the DO block would fail.
            var createTable = CreateTemporaryTableIfNotExists();

            try
            {
                Processor.Execute("CREATE TEMPORARY TABLE \"FmTemp\" (\"Original\" integer)");

                Processor.Process(createTable);

                Processor.Exists("SELECT 1 FROM pg_catalog.pg_class WHERE relnamespace = pg_catalog.pg_my_temp_schema() AND relname = 'IX_FmTemp_Id'")
                    .ShouldBeFalse();
            }
            finally
            {
                Processor.Execute("DROP TABLE IF EXISTS pg_temp.\"FmTemp\"");
            }
        }

        /// <summary>
        /// <c>Create.Table("FmTemp").InSchema("pg_temp").IfNotExists().WithColumn("Id").AsInt32().Indexed("IX_FmTemp_Id")</c>.
        /// The index puts the table in a <c>DO</c> block.
        /// </summary>
        private static CreateTableExpression CreateTemporaryTableIfNotExists()
        {
            var createTable = new CreateTableExpression { SchemaName = "pg_temp", TableName = "FmTemp", IfNotExists = true };
            createTable.Columns.Add(new ColumnDefinition { Name = "Id", Type = DbType.Int32, TableName = "FmTemp" });
            var index = new CreateIndexExpression
            {
                Index = new IndexDefinition { Name = "IX_FmTemp_Id", SchemaName = "pg_temp", TableName = "FmTemp" },
            };
            index.Index.Columns.Add(new IndexColumnDefinition { Name = "Id" });
            createTable.ExpressionsForNewTable.Add(index);
            return createTable;
        }

        [OneTimeSetUp]
        public void ClassSetUp()
        {
            IntegrationTestOptions.Postgres.IgnoreIfNotEnabled();

            var serivces = ServiceCollectionExtensions.CreateServices()
                .ConfigureRunner(r => r.AddPostgres())
                .AddScoped<IConnectionStringReader>(
                    _ => new PassThroughConnectionStringReader(IntegrationTestOptions.Postgres.ConnectionString));
            ServiceProvider = serivces.BuildServiceProvider();
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
