using System.Data;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.Oracle;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Oracle;
using FluentMigrator.Tests.Helpers;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors.Oracle
{
    [TestFixture]
    [Category("Integration")]
    public class OracleProcessorTests
    {
        private const string SchemaName = "test";
        public IDbConnection Connection { get; set; }
        public OracleProcessor Processor { get; set; }
        public OracleDbFactory Factory { get; set; }
        public IQuoter Quoter { get; set; }

        [SetUp]
        public void SetUp()
        {
            Factory = new OracleDbFactory();
            Connection = Factory.CreateConnection(IntegrationTestOptions.Oracle.ConnectionString);
            Quoter = new OracleQuoter();
            Processor = new OracleProcessor(() => Connection, new OracleGenerator(), new TextWriterAnnouncer(System.Console.Out), new ProcessorOptions(), () => Factory);
            Connection.Open();
        }

        [TearDown]
        public void TearDown()
        {
            Processor.Dispose();
        }

        [Test]
        public void CallingColumnExistsReturnsFalseIfColumnExistsInDifferentSchema()
        {
            using (var table = new OracleTestTable(Connection, SchemaName, Quoter.QuoteColumnName("id") + " int"))
                Processor.ColumnExists("testschema", table.Name, "id").ShouldBeFalse();
        }

        [Test]
        public void CallingConstraintExistsReturnsFalseIfConstraintExistsInDifferentSchema()
        {
            using (var table = new OracleTestTable(Connection, SchemaName, Quoter.QuoteColumnName("id") + " int"))
            {
                table.WithUniqueConstraintOn("id");
                Processor.ConstraintExists("testschema", table.Name, "UC_id").ShouldBeFalse();
            }
        }

        [Test]
        public void CallingTableExistsReturnsFalseIfTableExistsInDifferentSchema()
        {
            using (var table = new OracleTestTable(Connection, SchemaName, Quoter.QuoteColumnName("id") + " int"))
                Processor.TableExists("testschema", table.Name).ShouldBeFalse();
        }

        [Test]
        public void CallingColumnExistsWithIncorrectCaseReturnsFalseIfColumnExists()
        {
            using (var table = new OracleTestTable(Connection, null, Quoter.QuoteColumnName("id") + " int"))
                Processor.ColumnExists(null, table.Name, "ID").ShouldBeFalse();
        }

        [Test]
        public void CallingConstraintExistsWithIncorrectCaseReturnsFalseIfConstraintExists()
        {
            using (var table = new OracleTestTable(Connection, null, Quoter.QuoteColumnName("id") + " int"))
            {
                table.WithUniqueConstraintOn("id");
                Processor.ConstraintExists(null, table.Name, "UC_ID").ShouldBeFalse();
            }
        }

        [Test]
        public void CallingIndexExistsWithIncorrectCaseReturnsFalseIfIndexExist()
        {
            using (var table = new OracleTestTable(Connection, null, Quoter.QuoteColumnName("id") + " int"))
            {
                table.WithIndexOn("id");
                Processor.IndexExists(null, table.Name, "UI_ID").ShouldBeFalse();
            }
        }

        [Test]
        public void TestQuery()
        {
            string sql = "SELECT SYSDATE FROM " + Quoter.QuoteTableName("DUAL");
            var ds = new DataSet();
            using (var command = Factory.CreateCommand(sql, Connection))
            {
                var adapter = Factory.CreateDataAdapter(command);
                adapter.Fill(ds);
            }

            Assert.Greater(ds.Tables.Count, 0);
            Assert.Greater(ds.Tables[0].Columns.Count, 0);
        }
    }
}
