using System;
using System.Data.SqlServerCe;
using System.IO;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.SqlServer;
using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Integration.Processors.SqlServerCe
{
    [TestFixture]
    [Category("Integration")]
    [Category("SqlServerCe")]
    public class SqlServerCeProcessorTests
    {
        private string _tempDataDirectory;

        public string DatabaseFilename { get; set; }
        public SqlCeConnection Connection { get; set; }
        public SqlServerCeProcessor Processor { get; set; }

        [SetUp]
        public void SetUp()
        {
            if (!IntegrationTestOptions.SqlServerCe.IsEnabled)
                Assert.Ignore();

            if (!HostUtilities.ProbeSqlServerCeBehavior())
            {
                Assert.Ignore("SQL Server CE binaries not found");
            }

            _tempDataDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDataDirectory);
            AppDomain.CurrentDomain.SetData("DataDirectory", _tempDataDirectory);

            var csb = new SqlCeConnectionStringBuilder(IntegrationTestOptions.SqlServerCe.ConnectionString);
            DatabaseFilename = HostUtilities.ReplaceDataDirectory(csb.DataSource);
            RecreateDatabase();
            Connection = new SqlCeConnection(IntegrationTestOptions.SqlServerCe.ConnectionString);
            Processor = new SqlServerCeProcessor(Connection, new SqlServerCeGenerator(), new TextWriterAnnouncer(TestContext.Out), new ProcessorOptions(), new SqlServerCeDbFactory(serviceProvider: null));
            Connection.Open();
            Processor.BeginTransaction();
        }

        [TearDown]
        public void TearDown()
        {
            Processor?.CommitTransaction();
            Processor?.Dispose();

            if (!string.IsNullOrEmpty(_tempDataDirectory) && Directory.Exists(_tempDataDirectory))
            {
                Directory.Delete(_tempDataDirectory, true);
            }
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
