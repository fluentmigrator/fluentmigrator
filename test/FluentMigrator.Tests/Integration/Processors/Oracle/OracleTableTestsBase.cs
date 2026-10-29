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

using FluentMigrator.Expressions;
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
    public abstract class OracleTableTestsBase : BaseTableTests
    {
        // Oracle Schemas are different from other RDBMS
        private string SchemaName =>  new OracleConnectionStringBuilder(Processor.Connection.ConnectionString).UserID;

        private ServiceProvider ServiceProvider { get; set; }
        private IServiceScope ServiceScope { get; set; }
        private OracleProcessorBase Processor { get; set; }

        [Test]
        public override void CallingTableExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new OracleTestTable("Test'Table", Processor, null, "id int"))
            {
                Processor.TableExists(null, table.Name).ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingTableExistsReturnsFalseIfTableDoesNotExist()
        {
            Processor.TableExists(null, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingTableExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            Processor.TableExists(SchemaName, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingTableExistsReturnsTrueIfTableExists()
        {
            using (var table = new OracleTestTable(Processor, null, "id int"))
            {
                Processor.TableExists(null, table.Name).ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingTableExistsReturnsTrueIfTableExistsWithSchema()
        {
            using (var table = new OracleTestTable(Processor, SchemaName, "id int"))
            {
                Processor.TableExists(SchemaName, table.Name).ShouldBeTrue();
            }
        }

        [Test]
        public void CanCreateTableWithTableAndColumnDescriptions()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithTableDescriptionAndColumnDescriptions();
            expression.TableName = "TableWithDescriptions";

            try
            {
                Processor.Process(expression);

                Processor.TableExists(null, expression.TableName).ShouldBeTrue();
                Processor.Read(
                        "SELECT COMMENTS FROM USER_TAB_COMMENTS WHERE UPPER(TABLE_NAME) = UPPER('{0}')",
                        expression.TableName)
                    .Tables[0].Rows[0][0].ShouldBe(GeneratorTestHelper.TestTableDescription);
                Processor.Read(
                        "SELECT COMMENTS FROM USER_COL_COMMENTS WHERE UPPER(TABLE_NAME) = UPPER('{0}') AND UPPER(COLUMN_NAME) = UPPER('{1}')",
                        expression.TableName,
                        GeneratorTestHelper.TestColumnName1)
                    .Tables[0].Rows[0][0].ShouldBe(GeneratorTestHelper.TestColumn1Description);
            }
            finally
            {
                if (Processor.TableExists(null, expression.TableName))
                {
                    Processor.Process(new DeleteTableExpression { TableName = expression.TableName });
                }
            }
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

        /// <summary>
        /// Hook that needs to be implemented to add the Oracle services to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection</returns>
        protected abstract IServiceCollection AddOracleServices(IServiceCollection services);
    }
}
