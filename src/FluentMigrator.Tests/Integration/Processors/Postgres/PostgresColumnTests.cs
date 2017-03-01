using System;
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
    public class PostgresColumnTests : BaseColumnTests, IDisposable
    {
        public NpgsqlConnection Connection { get; set; }
        public PostgresProcessor Processor { get; set; }
        public IQuoter Quoter { get; set; }

        public PostgresColumnTests()
        {
            Connection = new NpgsqlConnection(IntegrationTestOptions.Postgres.ConnectionString);
            Processor = new PostgresProcessor(Connection, new PostgresGenerator(), new TextWriterAnnouncer(System.Console.Out), new ProcessorOptions(), new PostgresDbFactory());
            Quoter = new PostgresQuoter();
            Connection.Open();
        }

        public void Dispose()
        {
            Processor.CommitTransaction();
            Processor.Dispose();
        }

        [Fact]
        public override void CallingColumnExistsCanAcceptColumnNameWithSingleQuote()
        {
            var columnNameWithSingleQuote = Quoter.Quote("i'd");
            using (var table = new PostgresTestTable(Processor, null, columnNameWithSingleQuote + " int"))
                Processor.ColumnExists(null, table.Name, "i'd").ShouldBeTrue();
        }

        [Fact]
        public override void CallingColumnExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new PostgresTestTable("Test'Table", Processor, null, "id int"))
                Processor.ColumnExists(null, table.Name, "id").ShouldBeTrue();
        }

        [Fact]
        public override void CallingColumnExistsReturnsFalseIfColumnDoesNotExist()
        {
            using (var table = new PostgresTestTable(Processor, null, "id int"))
                Processor.ColumnExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
        }

        [Fact]
        public override void CallingColumnExistsReturnsFalseIfColumnDoesNotExistWithSchema()
        {
            using (var table = new PostgresTestTable(Processor, "TestSchema", "id int"))
                Processor.ColumnExists("TestSchema", table.Name, "DoesNotExist").ShouldBeFalse();
        }

        [Fact]
        public override void CallingColumnExistsReturnsFalseIfTableDoesNotExist()
        {
            Processor.ColumnExists(null, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Fact]
        public override void CallingColumnExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            Processor.ColumnExists("TestSchema", "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Fact]
        public override void CallingColumnExistsReturnsTrueIfColumnExists()
        {
            using (var table = new PostgresTestTable(Processor, null, "id int"))
                Processor.ColumnExists(null, table.Name, "id").ShouldBeTrue();
        }

        [Fact]
        public override void CallingColumnExistsReturnsTrueIfColumnExistsWithSchema()
        {
            using (var table = new PostgresTestTable(Processor, "TestSchema", "id int"))
                Processor.ColumnExists("TestSchema", table.Name, "id").ShouldBeTrue();
        }
    }
}
