#region License
// Copyright (c) 2018, Fluent Migrator Project
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
using System.IO;

using FluentMigrator.Runner.Generators.Oracle;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors.Oracle;
using FluentMigrator.Tests.Helpers;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using Oracle.ManagedDataAccess.Client;

using Shouldly;

namespace FluentMigrator.Tests.Integration.Processors.Oracle
{
    [Category("Integration")]
    public abstract class OracleProcessorTestsBase
    {
        // Oracle Schemas are different from other RDBMS
        private string SchemaName =>  new OracleConnectionStringBuilder(Processor.Connection.ConnectionString).UserID;

        private ServiceProvider ServiceProvider { get; set; }
        private IServiceScope ServiceScope { get; set; }
        private OracleProcessorBase Processor { get; set; }
        private OracleQuoterBase Quoter { get; set; }

        [Test]
        public void CallingColumnExistsReturnsFalseIfColumnExistsInDifferentSchema()
        {
            using (var table = new OracleTestTable(Processor, SchemaName, "id int"))
            {
                Processor.ColumnExists("UnknownSchema", table.Name, "ID").ShouldBeFalse();
            }
        }

        [Test]
        public void CallingConstraintExistsReturnsFalseIfConstraintExistsInDifferentSchema()
        {
            using (var table = new OracleTestTable(Processor, SchemaName, "id int"))
            {
                table.WithUniqueConstraintOn("ID");
                Processor.ConstraintExists("UnknownSchema", table.Name, "UC_id").ShouldBeFalse();
            }
        }

        [Test]
        public void CallingTableExistsReturnsFalseIfTableExistsInDifferentSchema()
        {
            using (var table = new OracleTestTable(Processor, SchemaName, "id int"))
            {
                Processor.TableExists("UnknownSchema", table.Name).ShouldBeFalse();
            }
        }

        [Test]
        public void CallingColumnExistsWithIncorrectCaseReturnsTrueIfColumnExists()
        {
            //the ColumnExisits() function is'nt case sensitive
            using (var table = new OracleTestTable(Processor, null, "id int"))
            {
                Processor.ColumnExists(null, table.Name, "Id").ShouldBeTrue();
            }
        }

        [Test]
        public void CallingConstraintExistsWithIncorrectCaseReturnsTrueIfConstraintExists()
        {
            //the ConstraintExists() function is'nt case sensitive
            using (var table = new OracleTestTable(Processor, null, "id int"))
            {
                table.WithUniqueConstraintOn("ID", "uc_id");
                Processor.ConstraintExists(null, table.Name, "Uc_Id").ShouldBeTrue();
            }
        }

        [Test]
        public void CallingIndexExistsWithIncorrectCaseReturnsFalseIfIndexExist()
        {
            //the IndexExists() function is'nt case sensitive
            using (var table = new OracleTestTable(Processor, null, "id int"))
            {
                table.WithIndexOn("ID", "ui_id");
                Processor.IndexExists(null, table.Name, "Ui_Id").ShouldBeTrue();
            }
        }

        [Test]
        public void TestQuery()
        {
            var sql = "SELECT SYSDATE FROM " + Quoter.QuoteTableName("DUAL");
            var ds = Processor.Read(sql);
            Assert.That(ds.Tables, Is.Not.Empty);
            Assert.That(ds.Tables[0].Columns, Is.Not.Empty);
        }

        private ServiceProvider CreateProcessorServices([CanBeNull] Action<IServiceCollection> initAction)
        {
            IntegrationTestOptions.Oracle.IgnoreIfNotEnabled();

            var services = AddOracleServices(ServiceCollectionExtensions.CreateServices())
                .AddScoped<IConnectionStringReader>(
                    _ => new PassThroughConnectionStringReader(IntegrationTestOptions.Oracle.ConnectionString));

            initAction?.Invoke(services);

            return services.BuildServiceProvider();
        }

        [OneTimeSetUp]
        public void ClassSetUp()
        {
            ServiceProvider = CreateProcessorServices(null);
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
            Quoter = ServiceScope.ServiceProvider.GetRequiredService<OracleQuoterBase>();

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
