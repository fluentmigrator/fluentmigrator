using System;
using System.Data.SqlServerCe;
using System.IO;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.SqlServer;
using FluentMigrator.Tests.Helpers;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors.SqlServerCe
{

    [TestFixture]
    [Category("Integration")]
    public class SqlServerCeProcessorTests
    {
        public string DatabaseFilename { get; set; }
        public SqlCeConnection Connection { get; set; }
        public SqlServerCeProcessor Processor { get; set; }

        [SetUp]
        public void SetUp()
        {
            DatabaseFilename = "TestDatabase.sdf";
            RecreateDatabase();
            Connection = new SqlCeConnection(IntegrationTestOptions.SqlServerCe.ConnectionString);
            Processor = new SqlServerCeProcessor(Connection, new SqlServerCeGenerator(), new TextWriterAnnouncer(System.Console.Out), new ProcessorOptions(), new SqlServerCeDbFactory());
            Connection.Open();
            Processor.BeginTransaction();
        }

        [TearDown]
        public void TearDown()
        {
            Processor.CommitTransaction();
            Processor.Dispose();
        }

        [Test]
        public void CallingTableExistsReturnsTrueIfTableExists()
        {
            using (var table = new SqlServerCeTestTable(Processor,  "id int"))
                Processor.TableExists(null, table.Name).ShouldBeTrue();
        }

        [Test]
        public void CallingTableExistsReturnsFalseIfTableDoesNotExist()
        {
            Processor.TableExists(null, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public void CallingColumnExistsReturnsTrueIfColumnExists()
        {
            using (var table = new SqlServerCeTestTable(Processor,  "\"id\" int"))
                Processor.ColumnExists(null, table.Name, "id").ShouldBeTrue();
        }

        [Test]
        public void CallingConstraintExistsReturnsTrueIfConstraintExists()
        {
            using (var table = new SqlServerCeTestTable(Processor, "id int"))
            {
                table.WithUniqueConstraintOn("id");
                Processor.ConstraintExists(null, table.Name, "UC_id").ShouldBeTrue();
            }
        }

        [Test]
        public void CallingIndexExistsReturnsTrueIfIndexExists()
        {
            using (var table = new SqlServerCeTestTable(Processor, "id int"))
            {
                table.WithIndexOn("id");
                Processor.IndexExists(null, table.Name, "UI_id").ShouldBeTrue();
            }
        }

        [Test]
        public void CallingColumnExistsReturnsFalseIfTableDoesNotExist()
        {
            Processor.ColumnExists(null, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public void CallingColumnExistsReturnsFalseIfColumnDoesNotExist()
        {
            using (var table = new SqlServerCeTestTable(Processor,  "id int"))
                Processor.ColumnExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public void CallingConstraintExistsReturnsFalseIfConstraintDoesNotExist()
        {
            using (var table = new SqlServerCeTestTable(Processor, "id int"))
            {
                table.WithUniqueConstraintOn("id");
                Processor.ConstraintExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
            }
        }

        [Test]
        public void CallingIndexExistsReturnsFalseIfIndexDoesNotExist()
        {
            using (var table = new SqlServerCeTestTable(Processor, "id int"))
            {
                table.WithIndexOn("id");
                Processor.IndexExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
            }
        }

        [Test]
        public void CallingTableExistsReturnsTrueIfTableExistsWithSchema()
        {
            using (var table = new SqlServerCeTestTable(Processor,  "id int"))
                Processor.TableExists("NOTUSED", table.Name).ShouldBeTrue();
        }

        [Test]
        public void CallingTableExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            Processor.TableExists("NOTUSED", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public void CallingColumnExistsReturnsTrueIfColumnExistsWithSchema()
        {
            using (var table = new SqlServerCeTestTable(Processor,  "\"id\" int"))
                Processor.ColumnExists("NOTUSED", table.Name, "id").ShouldBeTrue();
        }

        [Test]
        public void CallingConstraintExistsReturnsTrueIfConstraintExistsWithSchema()
        {
            using (var table = new SqlServerCeTestTable(Processor, "id int"))
            {
                table.WithUniqueConstraintOn("id");
                Processor.ConstraintExists("NOTUSED", table.Name, "UC_id").ShouldBeTrue();
            }
        }

        [Test]
        public void CallingIndexExistsReturnsTrueIfIndexExistsWithSchema()
        {
            using (var table = new SqlServerCeTestTable(Processor, "id int"))
            {
                table.WithIndexOn("id");
                Processor.IndexExists("NOTUSED", table.Name, "UI_id").ShouldBeTrue();
            }
        }

        [Test]
        public void CallingColumnExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            Processor.ColumnExists("NOTUSED", "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public void CallingColumnExistsReturnsFalseIfColumnDoesNotExistWithSchema()
        {
            using (var table = new SqlServerCeTestTable(Processor,  "id int"))
                Processor.ColumnExists("NOTUSED", table.Name, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public void CallingConstraintExistsReturnsFalseIfConstraintDoesNotExistWithSchema()
        {
            using (var table = new SqlServerCeTestTable(Processor, "id int"))
            {
                table.WithUniqueConstraintOn("id");
                Processor.ConstraintExists("NotUsed", table.Name, "DoesNotExist").ShouldBeFalse();
            }
        }

        [Test]
        public void CallingIndexExistsReturnsFalseIfIndexDoesNotExistWithSchema()
        {
            using (var table = new SqlServerCeTestTable(Processor, "id int"))
            {
                table.WithIndexOn("id");
                Processor.IndexExists("NOTUSED", table.Name, "DoesNotExist").ShouldBeFalse();
            }
        }

        [Test]
        public void CallingSchemaExistsReturnsTrueAllways()
        {
            Processor.SchemaExists("NOTUSED").ShouldBeTrue();
        }

        [Test]
        public void CallingExecuteWithMultilineSqlShouldExecuteInBatches()
        {
            Processor.Execute("CREATE TABLE [TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL, [TestColumn2] INT NOT NULL);" + Environment.NewLine +
                              "GO"+ Environment.NewLine +
                              "INSERT INTO TestTable1 VALUES('abc', 1);");

            Processor.TableExists("NOTUSED", "TestTable1");

            var dataset = Processor.ReadTableData("NOTUSED", "TestTable1");
            dataset.Tables[0].Rows.Count.ShouldBe(1);
        }

        [Test]
        public void CallingExecuteWithMultilineSqlAsLowercaseShouldExecuteInBatches()
        {
            Processor.Execute("create table [TestTable1] ([TestColumn1] nvarchar(255) not null, [TestColumn2] int not null);" + Environment.NewLine +
                              "go" + Environment.NewLine +
                              "insert into testtable1 values('abc', 1);");

            Processor.TableExists("NOTUSED", "TestTable1");

            var dataset = Processor.ReadTableData("NOTUSED", "TestTable1");
            dataset.Tables[0].Rows.Count.ShouldBe(1);
        }

        [Test]
        public void CallingExecuteWithMultilineSqlWithNoTrailingSemicolonShouldExecuteInBatches()
        {
            Processor.Execute("CREATE TABLE [TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL, [TestColumn2] INT NOT NULL);" + Environment.NewLine +
                              "GO" + Environment.NewLine +
                              "INSERT INTO TestTable1 VALUES('abc', 1)");

            Processor.TableExists("NOTUSED", "TestTable1");

            var dataset = Processor.ReadTableData("NOTUSED", "TestTable1");
            dataset.Tables[0].Rows.Count.ShouldBe(1);
        }

        private void RecreateDatabase()
        {
            if (File.Exists(DatabaseFilename))
            {
                File.Delete(DatabaseFilename);
            }

            new SqlCeEngine(IntegrationTestOptions.SqlServerCe.ConnectionString).CreateDatabase();
        }
    }
}