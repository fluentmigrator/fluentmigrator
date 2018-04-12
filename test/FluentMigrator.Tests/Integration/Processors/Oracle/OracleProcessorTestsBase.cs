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

using System.Data;

using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.Oracle;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Oracle;
using FluentMigrator.Tests.Helpers;
using FluentMigrator.Tests.Unit;

using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors.Oracle {
    [Category("Integration")]
    [Category("Oracle")]
    public abstract class OracleProcessorTestsBase
    {
        private const string SchemaName = "test";
        private IDbConnection Connection { get; set; }
        private OracleProcessor Processor { get; set; }
        private IDbFactory Factory { get; set; }
        private IQuoter Quoter { get { return this.Processor.Quoter; } }

        protected void SetUp(IDbFactory dbFactory)
        {
            if (!IntegrationTestOptions.Oracle.IsEnabled)
                Assert.Ignore();
            this.Factory = dbFactory;
            this.Connection = this.Factory.CreateConnection(IntegrationTestOptions.Oracle.ConnectionString);
            this.Processor = new OracleProcessor(this.Connection, new OracleGenerator(), new TextWriterAnnouncer(TestContext.Out), new TestMigrationProcessorOptions(), this.Factory);
            this.Connection.Open();
        }

        [TearDown]
        public void TearDown()
        {
            this.Processor?.Dispose();
        }

        [Test]
        public void CallingColumnExistsReturnsFalseIfColumnExistsInDifferentSchema()
        {
            using (var table = new OracleTestTable(Processor, SchemaName, "id int"))
                this.Processor.ColumnExists("testschema", table.Name, "ID").ShouldBeFalse();
        }

        [Test]
        public void CallingConstraintExistsReturnsFalseIfConstraintExistsInDifferentSchema()
        {
            using (var table = new OracleTestTable(Processor, SchemaName, "id int"))
            {
                table.WithUniqueConstraintOn("ID");
                this.Processor.ConstraintExists("testschema", table.Name, "UC_id").ShouldBeFalse();
            }
        }

        [Test]
        public void CallingTableExistsReturnsFalseIfTableExistsInDifferentSchema()
        {
            using (var table = new OracleTestTable(Processor, SchemaName, "id int"))
                this.Processor.TableExists("testschema", table.Name).ShouldBeFalse();
        }

        [Test]
        public void CallingColumnExistsWithIncorrectCaseReturnsTrueIfColumnExists()
        {
            //the ColumnExisits() function is'nt case sensitive
            using (var table = new OracleTestTable(Processor, null, "id int"))
                this.Processor.ColumnExists(null, table.Name, "Id").ShouldBeTrue();
        }

        [Test]
        public void CallingConstraintExistsWithIncorrectCaseReturnsTrueIfConstraintExists()
        {
            //the ConstraintExists() function is'nt case sensitive
            using (var table = new OracleTestTable(Processor, null, "id int"))
            {
                table.WithUniqueConstraintOn("ID", "uc_id");
                this.Processor.ConstraintExists(null, table.Name, "Uc_Id").ShouldBeTrue();
            }
        }

        [Test]
        public void CallingIndexExistsWithIncorrectCaseReturnsFalseIfIndexExist()
        {
            //the IndexExists() function is'nt case sensitive
            using (var table = new OracleTestTable(Processor, null, "id int"))
            {
                table.WithIndexOn("ID", "ui_id");
                this.Processor.IndexExists(null, table.Name, "Ui_Id").ShouldBeTrue();
            }
        }

        [Test]
        public void TestQuery()
        {
            string sql = "SELECT SYSDATE FROM " + this.Quoter.QuoteTableName("DUAL");
            using (var command = this.Factory.CreateCommand(sql, Processor.Connection, Processor.Transaction, Processor.Options))
            using (var reader = command.ExecuteReader())
            {
                var ds = reader.ReadDataSet();
                Assert.Greater(ds.Tables.Count, 0);
                Assert.Greater(ds.Tables[0].Columns.Count, 0);
            }
        }
    }
}
