using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators.Postgres;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.DotConnectPostgres;
using NUnit.Framework;
using NUnit.Should;
using Devart.Data.PostgreSql;

namespace FluentMigrator.Tests.Integration.Processors.DotConnectPostgres
{
    [TestFixture]
    [Category("Integration")]
    public class DotConnectPostgresSchemaTests : BaseSchemaTests
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
        public override void CallingSchemaExistsReturnsFalseIfSchemaDoesNotExist()
        {
            Processor.SchemaExists("DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingSchemaExistsReturnsTrueIfSchemaExists()
        {
            Processor.SchemaExists("public").ShouldBeTrue();
        }

    }
}