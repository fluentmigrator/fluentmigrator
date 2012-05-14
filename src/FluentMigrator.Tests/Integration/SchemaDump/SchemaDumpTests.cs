using System.Collections.Generic;
using System.IO;
using System.Data.SqlClient;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.SqlServer;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.SchemaDump.SchemaWriters;
using FluentMigrator.Model;
using FluentMigrator.SchemaDump.SchemaDumpers;
using FluentMigrator.Tests.Helpers;
using FluentMigrator.Tests.Integration.Migrations;
using NUnit.Framework;
using NUnit.Should;
using FluentMigrator.Runner.Generators.SqlServer;

namespace FluentMigrator.Tests.Integration.SchemaDump
{

    [TestFixture]
    public class SchemaDumpTests
    {
        public SqlConnection Connection;
        public SqlServerProcessor Processor;
        public SqlServerSchemaDumper SchemaDumper;

        public SchemaDumpTests()
        {
            Connection = new SqlConnection(IntegrationTestOptions.SqlServer2008.ConnectionString);
            Processor = new SqlServerProcessor(Connection, new SqlServer2008Generator(), new TextWriterAnnouncer(System.Console.Out), new ProcessorOptions(), new SqlServerDbFactory());
            SchemaDumper = new SqlServerSchemaDumper(Processor, new TextWriterAnnouncer(System.Console.Out));
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            if (Connection != null)
                Connection.Dispose();
        }

        [Test]
        public void TestSchemaTestWriter()
        {
            TableDefinition tableDef = new TableDefinition
            {
                SchemaName = "dbo",
                Name = "tableName",
                Columns = new List<ColumnDefinition>() { new ColumnDefinition() },
                Indexes = new List<IndexDefinition>() { new IndexDefinition() },
                ForeignKeys = new List<ForeignKeyDefinition>() { new ForeignKeyDefinition() }
            };

            List<TableDefinition> defs = new List<TableDefinition>();
            defs.Add(tableDef);

            SchemaTestWriter testWriter = new SchemaTestWriter();
            var output = GetOutput(testWriter, defs);
            string expectedMessage = testWriter.GetMessage(1, 1, 1, 1);

            output.ShouldBe(expectedMessage);
        }

        [Test]
        public void CanReadBasicSchemaInfo()
        {
            // this is the fun part.. this test should fail until the schema reading code works
            // also assume the target database contains schema described in TestMigration
            using (var table = new SqlServerTestTable(Processor, null, "id int"))
            {
                IList<TableDefinition> defs = SchemaDumper.ReadDbSchema();

                SchemaTestWriter testWriter = new SchemaTestWriter();
                var output = GetOutput(testWriter, defs);
                string expectedMessage = testWriter.GetMessage(1, 1, 0, 0);

                output.ShouldBe(expectedMessage);
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

            SchemaTestWriter testWriter = new SchemaTestWriter();
            var output = GetOutput(testWriter, defs);
            string expectedMessage = testWriter.GetMessage(4, 10, 4, 1);

            runner.Down(new TestMigration());

            //test
            output.ShouldBe(expectedMessage);
        }

        private string GetOutput(SchemaWriterBase testWriter, IList<TableDefinition> defs)
        {
            MemoryStream ms = new MemoryStream();
            StreamWriter sr = new StreamWriter(ms);
            StreamReader reader = new StreamReader(ms);
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
