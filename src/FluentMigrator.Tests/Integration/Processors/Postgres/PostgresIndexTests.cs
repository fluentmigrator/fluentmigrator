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
    public class PostgresIndexTests : BaseIndexTests, IDisposable
    {
        public NpgsqlConnection Connection { get; set; }
        public PostgresProcessor Processor { get; set; }
        public IQuoter Quoter { get; set; }

        public PostgresIndexTests()
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
        public override void CallingIndexExistsCanAcceptIndexNameWithSingleQuote()
        {
            using (var table = new PostgresTestTable(Processor, null, "id int"))
            {
                var idxName = string.Format("\"id'x_{0}\"", Quoter.UnQuote(table.Name));

                var cmd = table.Connection.CreateCommand();
                cmd.Transaction = table.Transaction;
                cmd.CommandText = string.Format("CREATE INDEX {0} ON {1} (id)", idxName, table.Name);
                cmd.ExecuteNonQuery();

                Processor.IndexExists(null, table.Name, idxName).ShouldBeTrue();
            }
        }

        [Fact]
        public override void CallingIndexExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new PostgresTestTable("Test'Table", Processor, null, "id int"))
            {
                var idxName = string.Format("\"idx_{0}\"", Quoter.UnQuote(table.Name));

                var cmd = table.Connection.CreateCommand();
                cmd.Transaction = table.Transaction;
                cmd.CommandText = string.Format("CREATE INDEX {0} ON {1} (id)", idxName, table.Name);
                cmd.ExecuteNonQuery();

                Processor.IndexExists(null, table.Name, idxName).ShouldBeTrue();
            }
        }

        [Fact]
        public override void CallingIndexExistsReturnsFalseIfIndexDoesNotExist()
        {
            using (var table = new PostgresTestTable(Processor, null, "id int"))
                Processor.IndexExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
        }

        [Fact]
        public override void CallingIndexExistsReturnsFalseIfIndexDoesNotExistWithSchema()
        {
            using (var table = new PostgresTestTable(Processor, "TestSchema", "id int"))
                Processor.IndexExists("TestSchema", table.Name, "DoesNotExist").ShouldBeFalse();
        }

        [Fact]
        public override void CallingIndexExistsReturnsFalseIfTableDoesNotExist()
        {
            Processor.IndexExists(null, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Fact]
        public override void CallingIndexExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            Processor.IndexExists("TestSchema", "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Fact]
        public override void CallingIndexExistsReturnsTrueIfIndexExists()
        {
            using (var table = new PostgresTestTable(Processor, null, "id int"))
            {
                var idxName = string.Format("\"idx_{0}\"", Quoter.UnQuote(table.Name));

                var cmd = table.Connection.CreateCommand();
                cmd.Transaction = table.Transaction;
                cmd.CommandText = string.Format("CREATE INDEX {0} ON {1} (id)", idxName, table.Name);
                cmd.ExecuteNonQuery();

                Processor.IndexExists(null, table.Name, idxName).ShouldBeTrue();
            }
        }

        [Fact]
        public override void CallingIndexExistsReturnsTrueIfIndexExistsWithSchema()
        {
            using (var table = new PostgresTestTable(Processor, "TestSchema", "id int"))
            {
                var idxName = string.Format("\"idx_{0}\"", Quoter.UnQuote(table.Name));

                var cmd = table.Connection.CreateCommand();
                cmd.Transaction = table.Transaction;
                cmd.CommandText = string.Format("CREATE INDEX {0} ON {1} (id)", idxName, table.NameWithSchema);
                cmd.ExecuteNonQuery();

                Processor.IndexExists("TestSchema", table.Name, idxName).ShouldBeTrue();
            }
        }
    }
}
