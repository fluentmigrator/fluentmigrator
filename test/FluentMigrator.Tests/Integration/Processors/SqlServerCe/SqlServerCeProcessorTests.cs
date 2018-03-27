using System;
using System.Data.SqlServerCe;
using System.IO;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.SqlServer;
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
        public void CallingSchemaExistsReturnsTrueAlways()
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