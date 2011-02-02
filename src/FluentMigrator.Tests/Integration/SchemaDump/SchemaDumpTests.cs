using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.SqlServer;
using FluentMigrator.Model;
using FluentMigrator.Tests.Helpers;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.SchemaDump {

    [TestFixture]
    public class SchemaDumpTests 
    {
        public SqlConnection Connection;
        public SqlServerProcessor Processor;

        public SchemaDumpTests() 
        {
            Connection = new SqlConnection(IntegrationTestOptions.SqlServer.ConnectionString);
            Processor = new SqlServerProcessor(Connection, new SqlServer2000Generator(), new TextWriterAnnouncer(System.Console.Out), new ProcessorOptions());
        }

        [SetUp]
        public void SetUp() 
        {
        }

        [TearDown]
        public void TearDown() 
        {
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
                ForiengKeys = new List<ForeignKeyDefinition>() { new ForeignKeyDefinition() }
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
            using (var table = new SqlServerTestTable(Processor, "id int")) {
                IList<TableDefinition> defs = Processor.ReadDbSchema();

                SchemaTestWriter testWriter = new SchemaTestWriter();
                var output = GetOutput(testWriter, defs);
                string expectedMessage = testWriter.GetMessage(1, 1, 0, 0);

                output.ShouldBe(expectedMessage);
            }
        }

        private string GetOutput(SchemaTestWriter testWriter, IList<TableDefinition> defs) 
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
