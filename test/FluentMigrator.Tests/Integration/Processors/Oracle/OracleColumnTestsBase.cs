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

using System;
using System.IO;

using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors.Oracle;
using FluentMigrator.Tests.Helpers;
using FluentMigrator.Tests.Unit.Generators;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using Oracle.ManagedDataAccess.Client;

using Shouldly;

namespace FluentMigrator.Tests.Integration.Processors.Oracle
{
    [Category("Integration")]
    public abstract class OracleColumnTestsBase : BaseColumnTests
    {
        // Oracle Schemas are different from other RDBMS
        private string SchemaName =>  new OracleConnectionStringBuilder(Processor.Connection.ConnectionString).UserID;

        private ServiceProvider ServiceProvider { get; set; }
        private IServiceScope ServiceScope { get; set; }
        private OracleProcessorBase Processor { get; set; }

        [Test]
        public override void CallingColumnExistsCanAcceptColumnNameWithSingleQuote()
        {
            using (var table = new OracleTestTable(Processor, null, "\"i'd\" int"))
            {
                Processor.ColumnExists(null, table.Name, "i'd").ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingColumnExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new OracleTestTable("Test'Table", Processor, null, "id int"))
            {
                Processor.ColumnExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
            }
        }

        [Test]
        public override void CallingColumnExistsReturnsFalseIfColumnDoesNotExist()
        {
            using (var table = new OracleTestTable(Processor, null, "id int"))
            {
                Processor.ColumnExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
            }
        }

        [Test]
        public override void CallingColumnExistsReturnsFalseIfColumnDoesNotExistWithSchema()
        {
            using (var table = new OracleTestTable(Processor, SchemaName, "id int"))
            {
                Processor.ColumnExists(SchemaName, table.Name, "DoesNotExist").ShouldBeFalse();
            }
        }

        [Test]
        public override void CallingColumnExistsReturnsFalseIfTableDoesNotExist()
        {
            Processor.ColumnExists(null, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingColumnExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            Processor.ColumnExists(SchemaName, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingColumnExistsReturnsTrueIfColumnExists()
        {
            using (var table = new OracleTestTable(Processor, null, "id int"))
            {
                Processor.ColumnExists(null, table.Name, "id").ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingColumnExistsReturnsTrueIfColumnExistsWithSchema()
        {
            using (var table = new OracleTestTable(Processor, SchemaName, "id int"))
            {
                Processor.ColumnExists(SchemaName, table.Name, "id").ShouldBeTrue();
            }
        }

        [Test]
        public void CanCreateColumnWithDescription()
        {
            using (var table = new OracleTestTable("COLUMN_DESCRIPTIONS", Processor, null, "id int"))
            {
                var expression = GeneratorTestHelper.GetCreateColumnExpressionWithDescription();
                expression.TableName = table.Name;

                Processor.Process(expression);

                Processor.ColumnExists(null, table.Name, expression.Column.Name).ShouldBeTrue();
                ReadColumnComment(table.Name, expression.Column.Name).ShouldBe(GeneratorTestHelper.TestColumn1Description);
            }
        }

        [Test]
        public void CanAlterColumnWithDescription()
        {
            using (var table = new OracleTestTable("COLUMN_DESCRIPTIONS", Processor, null, "id int, TestColumn1 NVARCHAR2(5)"))
            {
                var expression = GeneratorTestHelper.GetAlterColumnExpressionWithDescription();
                expression.TableName = table.Name;

                Processor.Process(expression);

                ReadColumnComment(table.Name, expression.Column.Name).ShouldBe(GeneratorTestHelper.TestColumn1Description);
            }
        }

        private object ReadColumnComment(string tableName, string columnName)
        {
            return Processor.Read(
                    "SELECT COMMENTS FROM USER_COL_COMMENTS WHERE UPPER(TABLE_NAME) = UPPER('{0}') AND UPPER(COLUMN_NAME) = UPPER('{1}')",
                    tableName,
                    columnName)
                .Tables[0].Rows[0][0];
        }

        [OneTimeSetUp]
        public void ClassSetUp()
        {
            IntegrationTestOptions.Oracle.IgnoreIfNotEnabled();

            var serivces = AddOracleServices(ServiceCollectionExtensions.CreateServices())
                .AddScoped<IConnectionStringReader>(
                    _ => new PassThroughConnectionStringReader(IntegrationTestOptions.Oracle.ConnectionString));
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
            Processor = ServiceScope.ServiceProvider.GetRequiredService<OracleProcessorBase>();

            OracleTestUtils.CheckNativeOracleDataAccess(Processor);
        }

        [TearDown]
        public void TearDown()
        {
            ServiceScope?.Dispose();
            Processor?.Dispose();
        }

        protected abstract IServiceCollection AddOracleServices(IServiceCollection services);
    }
}
