using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators.Postgres;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.DotConnectPostgres;
using FluentMigrator.Tests.Helpers;
using NUnit.Framework;
using NUnit.Should;
using Devart.Data.PostgreSql;

namespace FluentMigrator.Tests.Integration.Processors.Postgres
{
    [TestFixture]
    [Category("Integration")]
    public class DotConnectPostgresTableTests : BaseTableTests
    {
        public PgSqlConnection Connection { get; set; }
        public DotConnectPostgresProcessor Processor { get; set; }

        [SetUp]
        public void SetUp()
        {
            Connection = new PgSqlConnection(IntegrationTestOptions.DotConnectPostgres.ConnectionString);
            Processor = new DotConnectPostgresProcessor(Connection, new PostgresGenerator(), new TextWriterAnnouncer(System.Console.Out), new ProcessorOptions(), new DotConnectPostgresDbFactory());
            Connection.Open();
        }

        [TearDown]
        public void TearDown()
        {
            Processor.CommitTransaction();
            Processor.Dispose();
        }

        [Test]
        public override void CallingTableExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new DotConnectPostgresTestTable("Test'Table", Processor, null, "id int"))
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
            Processor.TableExists("TestSchema", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingTableExistsReturnsTrueIfTableExists()
        {
            using (var table = new DotConnectPostgresTestTable(Processor, null, "id int"))
                Processor.TableExists(null, table.Name).ShouldBeTrue();
        }

        [Test]
        public override void CallingTableExistsReturnsTrueIfTableExistsWithSchema()
        {
            using (var table = new DotConnectPostgresTestTable(Processor, "TestSchema", "id int"))
                Processor.TableExists("TestSchema", table.Name).ShouldBeTrue();
        }
    }
}