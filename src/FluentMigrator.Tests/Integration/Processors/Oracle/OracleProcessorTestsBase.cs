using System.Data;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Oracle;
using FluentMigrator.Tests.Helpers;

using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors.Oracle
{
    [Category("Integration")]
    public abstract class OracleProcessorTestsBase : IntegrationTestBase
    {
        private const string SchemaName = "test";
        private IDbConnection Connection { get; set; }
        private OracleProcessor Processor { get; set; }
        private IDbFactory Factory { get; set; }
        private IQuoter Quoter { get { return this.Processor.Quoter; } }

        protected void SetUp(IDbFactory dbFactory)
        {
            if (ConfiguredProcessor.IsAssignableFrom(typeof(OracleProcessor)))
            {
                this.Processor = CreateProcessor() as OracleProcessor;
                this.Connection = Processor.Connection;
                this.Factory = Processor.Factory;
                this.Connection.Open();
            }
            else
                Assert.Ignore("Test is intended to run against Oracle. Current configuration: {0}", ConfiguredDbEngine);
        }

        [TearDown]
        public void TearDown()
        {
            if (ConfiguredProcessor.IsAssignableFrom(typeof(OracleProcessor)))
                this.Processor.Dispose();
        }

        [Test]
        public void CallingColumnExistsReturnsFalseIfColumnExistsInDifferentSchema()
        {
            using (var table = new OracleTestTable(this.Connection, SchemaName, this.Factory, "id int"))
                this.Processor.ColumnExists("testschema", table.Name, "ID").ShouldBeFalse();
        }

        [Test]
        public void CallingConstraintExistsReturnsFalseIfConstraintExistsInDifferentSchema()
        {
            using (var table = new OracleTestTable(this.Connection, SchemaName, this.Factory, "id int"))
            {
                table.WithUniqueConstraintOn("ID");
                this.Processor.ConstraintExists("testschema", table.Name, "UC_id").ShouldBeFalse();
            }
        }

        [Test]
        public void CallingTableExistsReturnsFalseIfTableExistsInDifferentSchema()
        {
            using (var table = new OracleTestTable(this.Connection, SchemaName, this.Factory, "id int"))
                this.Processor.TableExists("testschema", table.Name).ShouldBeFalse();
        }

        [Test]
        public void CallingColumnExistsWithIncorrectCaseReturnsTrueIfColumnExists()
        {
            //the ColumnExisits() function is'nt case sensitive
            using (var table = new OracleTestTable(this.Connection, null, this.Factory, "id int"))
                this.Processor.ColumnExists(null, table.Name, "Id").ShouldBeTrue();
        }

        [Test]
        public void CallingConstraintExistsWithIncorrectCaseReturnsTrueIfConstraintExists()
        {
            //the ConstraintExists() function is'nt case sensitive
            using (var table = new OracleTestTable(this.Connection, null, this.Factory, "id int"))
            {
                table.WithUniqueConstraintOn("ID", "uc_id");
                this.Processor.ConstraintExists(null, table.Name, "Uc_Id").ShouldBeTrue();
            }
        }

        [Test]
        public void CallingIndexExistsWithIncorrectCaseReturnsFalseIfIndexExist()
        {
            //the IndexExists() function is'nt case sensitive
            using (var table = new OracleTestTable(this.Connection, null, this.Factory, "id int"))
            {
                table.WithIndexOn("ID", "ui_id");
                this.Processor.IndexExists(null, table.Name, "Ui_Id").ShouldBeTrue();
            }
        }

        [Test]
        public void TestQuery()
        {
            string sql = "SELECT SYSDATE FROM " + this.Quoter.QuoteTableName("DUAL");
            var ds = new DataSet();
            using (var command = this.Factory.CreateCommand(sql, this.Connection))
            {
                var adapter = this.Factory.CreateDataAdapter(command);
                adapter.Fill(ds);
            }

            Assert.Greater(ds.Tables.Count, 0);
            Assert.Greater(ds.Tables[0].Columns.Count, 0);
        }
    }
}