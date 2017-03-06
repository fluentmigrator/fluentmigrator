using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentMigrator.Model;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors.SqlServer;
using FluentMigrator.SchemaDump.SchemaDumpers;
using FluentMigrator.SchemaDump.SchemaWriters;
using FluentMigrator.Tests.Helpers;
using FluentMigrator.Tests.Integration.Migrations;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.SchemaDump
{

    [TestFixture]
    [Category("Integration")]
    public class SchemaDumpTests : IntegrationTestBase
    {
        public SqlServerProcessor Processor;
        public SqlServerSchemaDumper SchemaDumper;

        [SetUp]
        public void Setup()
        {
            if (ConfiguredProcessor.IsAssignableFrom(typeof(SqlServerProcessor)))
            {
                Processor = CreateProcessor() as SqlServerProcessor;
                SchemaDumper = new SqlServerSchemaDumper(Processor, new TextWriterAnnouncer(System.Console.Out));
                Processor.Connection.Open();
            }
            else
                Assert.Ignore("Test is intended to run against SqlServer. Current configuration: {0}", ConfiguredDbEngine);
        }

        [TearDown]
        public void TearDown()
        {
            if (ConfiguredProcessor.IsAssignableFrom(typeof(SqlServerProcessor)))
                Processor.Dispose();
        }

        [Test]
        public void TestSchemaTestWriter()
        {
            var tableDef = new TableDefinition
            {
                SchemaName = "dbo",
                Name = "tableName",
                Columns = new List<ColumnDefinition> { new ColumnDefinition() },
                Indexes = new List<IndexDefinition> { new IndexDefinition() },
                ForeignKeys = new List<ForeignKeyDefinition> { new ForeignKeyDefinition() }
            };

            var defs = new List<TableDefinition> { tableDef };

            var testWriter = new SchemaTestWriter();
            var output = GetOutput(testWriter, defs);
            string expectedMessage = testWriter.GetMessage(1, 1, 1, 1);

            output.ShouldBe(expectedMessage);
        }

        [Test]
        public void CanReadBasicSchemaInfo()
        {
            // this is the fun part.. this test should fail until the schema reading code works
            // also assume the target database contains schema described in TestMigration
            using (new SqlServerTestTable(Processor, null, "id int"))
            {
                IList<TableDefinition> defs = SchemaDumper.ReadDbSchema();

                var testWriter = new SchemaTestWriter();
                var output = GetOutput(testWriter, defs);
                string expectedMessage = testWriter.GetMessage(1, 1, 0, 0);

                output.ShouldBe(expectedMessage);
            }
        }

        [Test]
        public void CanReadSchemaInfoWithIdentity()
        {
            using (new SqlServerTestTable(Processor, null, "id int IDENTITY(1,1) NOT NULL"))
            {
                IList<TableDefinition> defs = SchemaDumper.ReadDbSchema();

                var identityColumn = defs[0].Columns.First();
                identityColumn.Name.ShouldBe("id");
                identityColumn.IsNullable.ShouldBe(false);
                identityColumn.IsIdentity.ShouldBe(true);
            }
        }

        [Test]
        public void CanReadSchemaInfoWithNullable()
        {
            using (new SqlServerTestTable(Processor, null, "id int NULL"))
            {
                IList<TableDefinition> defs = SchemaDumper.ReadDbSchema();

                var identityColumn = defs[0].Columns.First();
                identityColumn.Name.ShouldBe("id");
                identityColumn.IsNullable.ShouldBe(true);
            }
        }

        [Test]
        public void VerifyTestMigrationSchema()
        {
            //run TestMigration migration, read, then remove...
            var runnerContext = new RunnerContext(new TextWriterAnnouncer(System.Console.Out))
            {
                Namespace = typeof(TestMigration).Namespace
            };

            var runner = new MigrationRunner(typeof(TestMigration).Assembly, runnerContext, Processor);
            runner.Up(new TestMigration());

            //read schema here
            IList<TableDefinition> defs = SchemaDumper.ReadDbSchema();

            var testWriter = new SchemaTestWriter();
            var output = GetOutput(testWriter, defs);
            string expectedMessage = testWriter.GetMessage(4, 11, 4, 1);

            runner.Down(new TestMigration());
            runner.VersionLoader.RemoveVersionTable();

            //test
            output.ShouldBe(expectedMessage);
        }

        private string GetOutput(SchemaWriterBase testWriter, IList<TableDefinition> defs)
        {
            var ms = new MemoryStream();
            var sr = new StreamWriter(ms);
            var reader = new StreamReader(ms);
            testWriter.WriteToStream(defs, sr);
            sr.Flush();
            ms.Seek(0, SeekOrigin.Begin); //goto beginning
            var output = reader.ReadToEnd();

            sr.Close();
            reader.Close();
            ms.Close();

            return output;
        }
    }
}
