using System;
using System.Data.SqlServerCe;
using System.IO;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.SqlServer;
using FluentMigrator.Tests.Helpers;
using Xunit;

namespace FluentMigrator.Tests.Integration.Processors.SqlServerCe
{
    [Trait("Category", "Integration")]
    public class SqlServerCeConstraintTests : BaseConstraintTests, IDisposable
    {
        public string DatabaseFilename { get; set; }
        public SqlCeConnection Connection { get; set; }
        public SqlServerCeProcessor Processor { get; set; }

        public SqlServerCeConstraintTests()
        {
            DatabaseFilename = "TestDatabase.sdf";
            RecreateDatabase();
            Connection = new SqlCeConnection(IntegrationTestOptions.SqlServerCe.ConnectionString);
            Processor = new SqlServerCeProcessor(Connection, new SqlServerCeGenerator(), new TextWriterAnnouncer(System.Console.Out), new ProcessorOptions(), new SqlServerCeDbFactory());
            Connection.Open();
            Processor.BeginTransaction();
        }

        public void Dispose()
        {
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
        public override void CallingConstraintExistsCanAcceptConstraintNameWithSingleQuote()
        {
            using (var table = new SqlServerCeTestTable(Processor, "id int"))
            {
                table.WithUniqueConstraintOn("id", "UC'id");
                Processor.ConstraintExists("NOTUSED", table.Name, "UC'id").ShouldBeTrue();
            }
        }

        [Fact]
        public override void CallingConstraintExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new SqlServerCeTestTable("Test'Table", Processor, "id int"))
            {
                table.WithUniqueConstraintOn("id");
                Processor.ConstraintExists("NOTUSED", table.Name, "UC_id").ShouldBeTrue();
            }
        }

        [Fact]
        public override void CallingConstraintExistsReturnsFalseIfConstraintDoesNotExist()
        {
            using (var table = new SqlServerCeTestTable(Processor, "id int"))
            {
                table.WithUniqueConstraintOn("id");
                Processor.ConstraintExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
            }
        }

        [Fact]
        public override void CallingConstraintExistsReturnsFalseIfConstraintDoesNotExistWithSchema()
        {
            using (var table = new SqlServerCeTestTable(Processor, "id int"))
            {
                table.WithUniqueConstraintOn("id");
                Processor.ConstraintExists("NotUsed", table.Name, "DoesNotExist").ShouldBeFalse();
            }
        }

        [Fact]
        public override void CallingConstraintExistsReturnsFalseIfTableDoesNotExist()
        {
            Processor.ConstraintExists(null, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Fact]
        public override void CallingConstraintExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            Processor.ConstraintExists("NotUsed", "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Fact]
        public override void CallingConstraintExistsReturnsTrueIfConstraintExists()
        {
            using (var table = new SqlServerCeTestTable(Processor, "id int"))
            {
                table.WithUniqueConstraintOn("id");
                Processor.ConstraintExists(null, table.Name, "UC_id").ShouldBeTrue();
            }
        }


        [Fact]
        public override void CallingConstraintExistsReturnsTrueIfConstraintExistsWithSchema()
        {
            using (var table = new SqlServerCeTestTable(Processor, "id int"))
            {
                table.WithUniqueConstraintOn("id");
                Processor.ConstraintExists("NOTUSED", table.Name, "UC_id").ShouldBeTrue();
            }
        }
    }
}

