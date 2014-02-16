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
    public class OracleTableTests : BaseTableTests
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
        public override void CallingTableExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new OracleTestTable("Test'Table", Connection, null, Quoter.QuoteColumnName("id") + " int"))
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
            Processor.TableExists(SchemaName, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingTableExistsReturnsTrueIfTableExists()
        {
            using (var table = new OracleTestTable(Connection, null, Quoter.QuoteColumnName("id") + " int"))
                Processor.TableExists(null, table.Name).ShouldBeTrue();
        }

        [Test]
        public override void CallingTableExistsReturnsTrueIfTableExistsWithSchema()
        {
            using (var table = new OracleTestTable(Connection, SchemaName, Quoter.QuoteColumnName("id") + " int"))
                Processor.TableExists(SchemaName, table.Name).ShouldBeTrue();
        }
    }
}
