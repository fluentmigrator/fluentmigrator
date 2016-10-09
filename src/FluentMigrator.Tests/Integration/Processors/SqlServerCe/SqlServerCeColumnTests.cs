using System.Data.SqlServerCe;
using System.IO;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.SqlServer;
using FluentMigrator.Tests.Helpers;
using Xunit;

namespace FluentMigrator.Tests.Integration.Processors.SqlServerCe
{
    [Trait("Category", "Integration")]
    public class SqlServerCeColumnTests : BaseColumnTests
    {
        public string DatabaseFilename { get; set; }
        public SqlCeConnection Connection { get; set; }
        public SqlServerCeProcessor Processor { get; set; }
        public IQuoter Quoter { get; set; }

        public SqlServerCeColumnTests()
        {
            DatabaseFilename = "TestDatabase.sdf";
            RecreateDatabase();
            Connection = new SqlCeConnection(IntegrationTestOptions.SqlServerCe.ConnectionString);
            Processor = new SqlServerCeProcessor(Connection, new SqlServerCeGenerator(), new TextWriterAnnouncer(System.Console.Out), new ProcessorOptions(), new SqlServerCeDbFactory());
            Quoter = new SqlServerQuoter();
            Connection.Open();
            Processor.BeginTransaction();
        }

        [TearDown]
        public void TearDown()
        {
            if (Processor == null)
                return;
            Processor.CommitTransaction();
            Processor.Dispose();
        }

        private void RecreateDatabase()
        {
            if (File.Exists(DatabaseFilename))
            {
                File.Delete(DatabaseFilename);
            }

            new SqlCeEngine(IntegrationTestOptions.SqlServerCe.ConnectionString).CreateDatabase();
        }

        [Fact]
        public override void CallingColumnExistsCanAcceptColumnNameWithSingleQuote()
        {
            using (var table = new SqlServerCeTestTable(Processor, Quoter.QuoteColumnName("i'd") + " int"))
                Processor.ColumnExists("NOTUSED", table.Name, "i'd").ShouldBeTrue();
        }

        [Fact]
        public override void CallingColumnExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new SqlServerCeTestTable("Test'Table", Processor, Quoter.QuoteColumnName("id") + " int"))
                Processor.ColumnExists("NOTUSED", table.Name, "id").ShouldBeTrue();
        }

        [Fact]
        public override void CallingColumnExistsReturnsFalseIfColumnDoesNotExist()
        {
            using (var table = new SqlServerCeTestTable(Processor, "id int"))
                Processor.ColumnExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
        }

        [Fact]
        public override void CallingColumnExistsReturnsFalseIfColumnDoesNotExistWithSchema()
        {
            using (var table = new SqlServerCeTestTable(Processor, "id int"))
                Processor.ColumnExists("NOTUSED", table.Name, "DoesNotExist").ShouldBeFalse();
        }

        [Fact]
        public override void CallingColumnExistsReturnsFalseIfTableDoesNotExist()
        {
            Processor.ColumnExists(null, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Fact]
        public override void CallingColumnExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            Processor.ColumnExists("NOTUSED", "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Fact]
        public override void CallingColumnExistsReturnsTrueIfColumnExists()
        {
            using (var table = new SqlServerCeTestTable(Processor, Quoter.QuoteColumnName("id") + " int"))
                Processor.ColumnExists(null, table.Name, "id").ShouldBeTrue();
        }

        [Fact]
        public override void CallingColumnExistsReturnsTrueIfColumnExistsWithSchema()
        {
            using (var table = new SqlServerCeTestTable(Processor, Quoter.QuoteColumnName("id") + " int"))
                Processor.ColumnExists("NOTUSED", table.Name, "id").ShouldBeTrue();
        }

    }
}

