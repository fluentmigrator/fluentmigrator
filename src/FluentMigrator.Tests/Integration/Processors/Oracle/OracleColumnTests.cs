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
    public class OracleColumnTests : BaseColumnTests
    {
        private const string SchemaName = "test";
        public IDbConnection Connection { get; set; }
        public OracleProcessor Processor { get; set; }
        public OracleDbFactory Factory { get; set; }
        public IQuoter Quoter { get; set; }

        [SetUp]
        public void SetUp()
        {
            if (!IntegrationTestOptions.Oracle.IsEnabled)
            {
                Assert.Ignore("Oracle integration tests disabled in config. Tests ignored.");
            }

            Factory = new OracleDbFactory();
            Connection = Factory.CreateConnection(IntegrationTestOptions.Oracle.ConnectionString);
            Quoter = new OracleQuoter();
            Processor = new OracleProcessor(Connection, new OracleGenerator(), new TextWriterAnnouncer(System.Console.Out), new ProcessorOptions(), Factory);
            Connection.Open();
        }

        [TearDown]
        public void TearDown()
        {
            if (!IntegrationTestOptions.Oracle.IsEnabled)
            {
                return;
            }

            Processor.Dispose();
        }

        [Test]
        public override void CallingColumnExistsCanAcceptColumnNameWithSingleQuote()
        {
            using (var table = new OracleTestTable(Connection, null, Quoter.QuoteColumnName("i'd") + " int"))
                Processor.ColumnExists(null, table.Name, "i'd").ShouldBeTrue();
        }

        [Test]
        public override void CallingColumnExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new OracleTestTable("Test'Table", Connection, null, Quoter.QuoteColumnName("id") + " int"))
                Processor.ColumnExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingColumnExistsReturnsFalseIfColumnDoesNotExist()
        {
            using (var table = new OracleTestTable(Connection, null, Quoter.QuoteColumnName("id") + " int"))
                Processor.ColumnExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingColumnExistsReturnsFalseIfColumnDoesNotExistWithSchema()
        {
            using (var table = new OracleTestTable(Connection, SchemaName, Quoter.QuoteColumnName("id") + " int"))
                Processor.ColumnExists(SchemaName, table.Name, "DoesNotExist").ShouldBeFalse();
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
            using (var table = new OracleTestTable(Connection, null, Quoter.QuoteColumnName("id") + " int"))
                Processor.ColumnExists(null, table.Name, "id").ShouldBeTrue();
        }

        [Test]
        public override void CallingColumnExistsReturnsTrueIfColumnExistsWithSchema()
        {
            using (var table = new OracleTestTable(Connection, SchemaName, Quoter.QuoteColumnName("id") + " int"))
                Processor.ColumnExists(SchemaName, table.Name, "id").ShouldBeTrue();
        }
    }
}
