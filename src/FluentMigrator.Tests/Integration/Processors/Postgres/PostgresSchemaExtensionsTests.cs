using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.Postgres;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Postgres;
using FluentMigrator.Tests.Helpers;
using NUnit.Framework;
using NUnit.Should;
using Npgsql;

namespace FluentMigrator.Tests.Integration.Processors.Postgres
{
    [TestFixture]
    [Category("Integration")]
    public class PostgresSchemaExtensionsTests : BaseSchemaExtensionsTests
    {
        public NpgsqlConnection Connection { get; set; }
        public PostgresProcessor Processor { get; set; }
        public IQuoter Quoter { get; set; }

        [SetUp]
        public void SetUp()
        {
            Connection = new NpgsqlConnection(IntegrationTestOptions.Postgres.ConnectionString);
            Processor = new PostgresProcessor(Connection, new PostgresGenerator(), new TextWriterAnnouncer(System.Console.Out), new ProcessorOptions(), new PostgresDbFactory());
            Quoter = new PostgresQuoter();
            Connection.Open();
        }

        [TearDown]
        public void TearDown()
        {
            Processor.CommitTransaction();
            Processor.Dispose();
        }

        [Test]
        public override void CallingColumnExistsCanAcceptSchemaNameWithSingleQuote()
        {
            using (var table = new PostgresTestTable(Processor, "Test'Schema", "id int"))
                Processor.ColumnExists("Test'Schema", table.Name, "id").ShouldBeTrue();
        }

        [Test]
        public override void CallingConstraintExistsCanAcceptSchemaNameWithSingleQuote()
        {
            using (var table = new PostgresTestTable(Processor, "Test'Schema", "id int", "wibble int CONSTRAINT c1 CHECK(wibble > 0)"))
                Processor.ConstraintExists("Test'Schema", table.Name, "c1").ShouldBeTrue();
        }

        [Test]
        public override void CallingIndexExistsCanAcceptSchemaNameWithSingleQuote()
        {
            using (var table = new PostgresTestTable(Processor, "Test'Schema", "id int"))
            {
                var idxName = string.Format("\"idx_{0}\"", Quoter.UnQuote(table.Name));

                var cmd = table.Connection.CreateCommand();
                cmd.Transaction = table.Transaction;
                cmd.CommandText = string.Format("CREATE INDEX {0} ON {1} (id)", idxName, table.NameWithSchema);
                cmd.ExecuteNonQuery();

                Processor.IndexExists("Test'Schema", table.Name, idxName).ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingSchemaExistsCanAcceptSchemaNameWithSingleQuote()
        {
            using (new PostgresTestTable(Processor, "Test'Schema", "id int"))
                Processor.SchemaExists("Test'Schema").ShouldBeTrue();
        }

        [Test]
        public override void CallingTableExistsCanAcceptSchemaNameWithSingleQuote()
        {
            using (var table = new PostgresTestTable(Processor, "Test'Schema", "id int"))
                Processor.TableExists("Test'Schema", table.Name).ShouldBeTrue();
        }

        [Test]
        public void CallingDefaultValueExistsCanAcceptSchemaNameWithSingleQuote()
        {
            using (var table = new PostgresTestTable(Processor, "test'schema", "id int"))
            {
                table.WithDefaultValueOn("id");
                Processor.DefaultValueExists("test'schema", table.Name, "id", 1).ShouldBeTrue();
            }
        }
    }
}