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
    public class OracleIndexTests : BaseIndexTests
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
        public override void CallingIndexExistsCanAcceptIndexNameWithSingleQuote()
        {
            using (var table = new OracleTestTable(Connection, null, Quoter.QuoteColumnName("id") + " int"))
            {
                table.WithIndexOn("id", "UI'id");
                Processor.IndexExists(null, table.Name, "UI'id").ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingIndexExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new OracleTestTable("Test'Table", Connection, null, Quoter.QuoteColumnName("id") + " int"))
            {
                table.WithIndexOn("id");
                Processor.IndexExists(null, table.Name, "UI_id").ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingIndexExistsReturnsFalseIfIndexDoesNotExist()
        {
            using (var table = new OracleTestTable(Connection, null, Quoter.QuoteColumnName("id") + " int"))
            {
                table.WithIndexOn("id");
                Processor.IndexExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
            }
        }

        [Test]
        public override void CallingIndexExistsReturnsFalseIfIndexDoesNotExistWithSchema()
        {
            using (var table = new OracleTestTable(Connection, SchemaName, Quoter.QuoteColumnName("id") + " int"))
            {
                table.WithIndexOn("id");
                Processor.IndexExists(SchemaName, table.Name, "DoesNotExist").ShouldBeFalse();
            }
        }

        [Test]
        public override void CallingIndexExistsReturnsFalseIfTableDoesNotExist()
        {
            Processor.IndexExists(null, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingIndexExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            Processor.IndexExists(SchemaName, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingIndexExistsReturnsTrueIfIndexExists()
        {
            using (var table = new OracleTestTable(Connection, null, Quoter.QuoteColumnName("id") + " int"))
            {
                table.WithIndexOn("id");
                Processor.IndexExists(null, table.Name, "UI_id").ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingIndexExistsReturnsTrueIfIndexExistsWithSchema()
        {
            using (var table = new OracleTestTable(Connection, SchemaName, Quoter.QuoteColumnName("id") + " int"))
            {
                table.WithIndexOn("id");
                Processor.IndexExists(SchemaName, table.Name, "UI_id").ShouldBeTrue();
            }
        }
    }
}
