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
using FluentMigrator.Runner.Processors.SqlServer;
using FluentMigrator.Tests.Helpers;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Integration.Processors.SqlServer.SqlServer2016
{
    [TestFixture]
    [Category("Integration")]
    [Category("SqlServer2016")]
    public class SqlServerTableTests : BaseTableTests
    {
        private ServiceProvider ServiceProvider { get; set; }
        private IServiceScope ServiceScope { get; set; }
        private SqlServer2016Processor Processor { get; set; }

        [Test]
        public override void CallingTableExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new SqlServerTestTable("Test'Table", Processor, null, "id int"))
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
            Processor.TableExists("test_schema", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingTableExistsReturnsTrueIfTableExists()
        {
            using (var table = new SqlServerTestTable(Processor, null, "id int"))
                Processor.TableExists(null, table.Name).ShouldBeTrue();
        }

        [Test]
        public override void CallingTableExistsReturnsTrueIfTableExistsWithSchema()
        {
            using (var table = new SqlServerTestTable(Processor, "test_schema", "id int"))
                Processor.TableExists("test_schema", table.Name).ShouldBeTrue();
        }

        [Test]
        public void CreateTableIfNotExistsCreatesAMissingTemporaryTable()
        {
            var createTable = CreateTemporaryTableIfNotExists();

            try
            {
                Processor.Process(createTable);

                Processor.Exists("SELECT 1 FROM tempdb.sys.columns WHERE object_id = OBJECT_ID(N'tempdb..#FmWork') AND name = 'Id'")
                    .ShouldBeTrue();
            }
            finally
            {
                Processor.Execute("IF OBJECT_ID(N'tempdb..#FmWork') IS NOT NULL DROP TABLE #FmWork");
            }
        }

        [Test]
        public void CreateTableIfNotExistsSkipsAnExistingTemporaryTable()
        {
            // A temporary table is in tempdb, so a check of the current database would not find it, and the CREATE TABLE
            // would fail because #FmWork already exists.
            var createTable = CreateTemporaryTableIfNotExists();

            try
            {
                Processor.Execute("CREATE TABLE #FmWork (Original INT)");

                Processor.Process(createTable);

                Processor.Exists("SELECT 1 FROM tempdb.sys.columns WHERE object_id = OBJECT_ID(N'tempdb..#FmWork') AND name = 'Original'")
                    .ShouldBeTrue();
            }
            finally
            {
                Processor.Execute("IF OBJECT_ID(N'tempdb..#FmWork') IS NOT NULL DROP TABLE #FmWork");
            }
        }

        private static CreateTableExpression CreateTemporaryTableIfNotExists()
        {
            var createTable = new CreateTableExpression { TableName = "#FmWork", IfNotExists = true };
            createTable.Columns.Add(new ColumnDefinition { Name = "Id", Type = DbType.Int32, TableName = "#FmWork" });
            return createTable;
        }

        [OneTimeSetUp]
        public void ClassSetUp()
        {
            IntegrationTestOptions.SqlServer2016.IgnoreIfNotEnabled();

            var serivces = ServiceCollectionExtensions.CreateServices()
                .ConfigureRunner(r => r.AddSqlServer2016())
                .AddScoped<IConnectionStringReader>(
                    _ => new PassThroughConnectionStringReader(IntegrationTestOptions.SqlServer2016.ConnectionString));
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
            Processor = ServiceScope.ServiceProvider.GetRequiredService<SqlServer2016Processor>();
        }

        [TearDown]
        public void TearDown()
        {
            ServiceScope?.Dispose();
            Processor?.Dispose();
        }
    }
}
