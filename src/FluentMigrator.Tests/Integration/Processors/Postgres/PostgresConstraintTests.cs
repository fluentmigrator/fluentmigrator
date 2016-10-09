using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.Postgres;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Postgres;
using FluentMigrator.Tests.Helpers;
using Xunit;
using Npgsql;

namespace FluentMigrator.Tests.Integration.Processors.Postgres
{
    [Trait("Category", "Integration")]
    public class PostgresConstraintTests : BaseConstraintTests
    {
        public NpgsqlConnection Connection { get; set; }
        public PostgresProcessor Processor { get; set; }
        public IQuoter Quoter { get; set; }

        public PostgresConstraintTests()
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

        [Fact]
        public override void CallingConstraintExistsCanAcceptConstraintNameWithSingleQuote()
        {
            using (var table = new PostgresTestTable(Processor, null, "id int", string.Format("wibble int CONSTRAINT {0} CHECK(wibble > 0)", Quoter.QuoteConstraintName("c'1"))))
                Processor.ConstraintExists(null, table.Name, "c'1").ShouldBeTrue();
        }

        [Fact]
        public override void CallingConstraintExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new PostgresTestTable("Test'Table", Processor, null, "id int", "wibble int CONSTRAINT c1 CHECK(wibble > 0)"))
                Processor.ConstraintExists(null, table.Name, "c1").ShouldBeTrue();
        }

        [Fact]
        public override void CallingConstraintExistsReturnsFalseIfConstraintDoesNotExist()
        {
            using (var table = new PostgresTestTable(Processor, null, "id int"))
                Processor.ConstraintExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
        }

        [Fact]
        public override void CallingConstraintExistsReturnsFalseIfConstraintDoesNotExistWithSchema()
        {
            using (var table = new PostgresTestTable(Processor, "TestSchema", "id int"))
                Processor.ConstraintExists("TestSchema", table.Name, "DoesNotExist").ShouldBeFalse();
        }

        [Fact]
        public override void CallingConstraintExistsReturnsFalseIfTableDoesNotExist()
        {
            Processor.ConstraintExists(null, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Fact]
        public override void CallingConstraintExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            Processor.ConstraintExists("TestSchema", "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Fact]
        public override void CallingConstraintExistsReturnsTrueIfConstraintExists()
        {
            using (var table = new PostgresTestTable(Processor, null, "id int", "wibble int CONSTRAINT c1 CHECK(wibble > 0)"))
                Processor.ConstraintExists(null, table.Name, "c1").ShouldBeTrue();
        }

        [Fact]
        public override void CallingConstraintExistsReturnsTrueIfConstraintExistsWithSchema()
        {
            using (var table = new PostgresTestTable(Processor, "TestSchema", "id int", "wibble int CONSTRAINT c1 CHECK(wibble > 0)"))
                Processor.ConstraintExists("TestSchema", table.Name, "c1").ShouldBeTrue();
        }

    }
}