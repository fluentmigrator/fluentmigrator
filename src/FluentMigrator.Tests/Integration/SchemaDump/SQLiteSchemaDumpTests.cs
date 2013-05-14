using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.SchemaDump.SchemaWriters;
using FluentMigrator.Model;
using FluentMigrator.SchemaDump.SchemaDumpers;
using FluentMigrator.Tests.Helpers;
using FluentMigrator.Tests.Integration.Migrations;
using NUnit.Framework;
using NUnit.Should;
using System.Linq;
using System.Data.Common;
using FluentMigrator.Runner.Processors.Sqlite;
using FluentMigrator.Runner.Generators.SQLite;
using System;

namespace FluentMigrator.Tests.Integration.SchemaDump
{
    [TestFixture]
    [Category("Integration")]
    public class SQLiteSchemaDumpTests
    {
        const string CreateTableSql = @"CREATE TABLE 'Author'  (ID  integer primary key autoincrement, Version INT not null, CreatedOn DATETIME not null, ModifiedOn DATETIME not null, Status TEXT not null, FirstName TEXT, LastName TEXT, BirthDate DATETIME, WebSiteURL TEXT,unique (FirstName, LastName))";

        public DbConnection Connection;
        public SqliteProcessor Processor;
        public SqliteSchemaDumper SchemaDumper;

        [SetUp]
        public void Setup()
        {
            Connection = new SQLiteConnection(IntegrationTestOptions.SqlLite.ConnectionString);
            Processor = new SqliteProcessor(Connection, new SqliteGenerator(), new TextWriterAnnouncer(System.Console.Out),
                                            new ProcessorOptions(), new SqliteDbFactory());

            SchemaDumper = new SqliteSchemaDumper(Processor, new TextWriterAnnouncer(System.Console.Out));
            Connection.Open();
        }

        [TearDown]
        public void TearDown()
        {
            Processor.Dispose();
        }

        [Test]
        public void CanGetTableDdlByName()
        {
            var tableName = "Author";

            using (var conn = Processor.Connection)
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = CreateTableSql;
                    cmd.ExecuteNonQuery();
                }
                var schema = new SqliteSchemaDumper(Processor, new TextWriterAnnouncer(System.Console.Out));
                var tableDDL = schema.GetTableDDL(tableName);

                Assert.IsFalse(string.IsNullOrEmpty(tableDDL));
                Assert.IsTrue(SqliteSchemaDumper.HasValidSimpleTableDDL(ref tableDDL));
            }
        }

        [Test]
        public void CanGenerateColumnDefinitionsFromTableDDL()
        {
            List<ColumnDefinition> schema = SqliteSchemaDumper.GetColumnDefinitionsForTableDDL(CreateTableSql);

            Assert.IsNotNull(schema, "ColumnDefinition can't be null");
            Assert.IsNotEmpty(schema, "ColumnDefinition must have elements");
            Assert.AreEqual(schema.Count, 9, "Schema must have 9 columns");
        }

        [Test]
        public void CanGetIndexeColumnsFromIndexDDLComplex()
        {
            var indexDDL1 = @"[FirstName]  ASC,[LastName]  DESC";
            var columns11 = SqliteSchemaDumper.GetIndexColumnsFromIndexDDL(indexDDL1);
            Assert.IsNotEmpty(columns11);
            Assert.AreEqual(columns11.Count, 2, "Should have 2 index column!");

            var index1 = columns11.First();
            Assert.AreEqual(index1.Name, "FirstName");
            Assert.AreEqual(index1.Direction, Direction.Ascending);

            var index2 = columns11.Last();
            Assert.AreEqual(index2.Name, "LastName");
            Assert.AreEqual(index2.Direction, Direction.Descending);
        }

        [Test]
        public void CanGetIndexeColumnsFromIndexDDLSimple()
        {
            //var indexDDL1 = @"CREATE INDEX [IDX_USERS_LastLogin] ON [Users]([LastLogin]  DESC)";
            var indexDDL1 = @"[LastLogin]  DESC";
            var columns11 = SqliteSchemaDumper.GetIndexColumnsFromIndexDDL(indexDDL1);
            Assert.IsNotEmpty(columns11);
            columns11.Count.ShouldBe(1);
            var index1 = columns11.First();
            index1.Name.ShouldBe("LastLogin");
            index1.Direction.ShouldBe(Direction.Descending);
        }

        [Test]
        public void CanGetProperTextBetweenBrackets()
        {
            var text1 = @"CREATE INDEX [IDX_USERS_LastLogin] ON [Users]([LastLogin]  DESC)";
            var result1 = SqliteSchemaDumper.GetTextBeteenBrackets(text1);

            Assert.AreEqual(result1, "[LastLogin]  DESC");
        }

        [Test]
        public void CanReadBasicSchemaInfo()
        {
            // this is the fun part.. this test should fail until the schema reading code works
            // also assume the target database contains schema described in TestMigration
            using (new SqliteTestTable(Processor, null, "id int"))
            {
                try
                {
                    var defs = SchemaDumper.ReadDbSchema();

                    var testWriter = new SchemaTestWriter();
                    var output = GetOutput(testWriter, defs);
                    var expectedMessage = testWriter.GetMessage(1, 1, 0, 0);

                    output.ShouldBe(expectedMessage);
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine("CanReadBasicSchemaInfo exception :" + ex.StackTrace);
                    throw;
                }
            }
        }

        [Test]
        public void CanReadSchemaInfoWithIdentity()
        {
            using (new SqliteTestTable(Processor, null, "id INTEGER  PRIMARY KEY AUTOINCREMENT NOT NULL"))
            {
                var defs = SchemaDumper.ReadDbSchema();

                var identityColumn = defs[0].Columns.First();
                identityColumn.Name.ShouldBe("id");
                identityColumn.IsNullable.ShouldBe(false);
                identityColumn.IsIdentity.ShouldBe(true);
            }
        }

        [Test]
        public void CanReadSchemaInfoWithNullable()
        {
            using (new SqliteTestTable(Processor, null, "id int NULL"))
            {
                var defs = SchemaDumper.ReadDbSchema();

                var identityColumn = defs[0].Columns.First();
                identityColumn.Name.ShouldBe("id");
                identityColumn.IsNullable.ShouldBe(true);
            }
        }

        [Test]
        public void TestSchemaTestWriter()
        {
            var tableDef = new TableDefinition
            {
                Name = "tableName",
                Columns = new List<ColumnDefinition> { new ColumnDefinition() },
                Indexes = new List<IndexDefinition> { new IndexDefinition() },
                ForeignKeys = new List<ForeignKeyDefinition> { new ForeignKeyDefinition() }
            };

            var defs = new List<TableDefinition> { tableDef };

            var testWriter = new SchemaTestWriter();
            var output = GetOutput(testWriter, defs);
            var expectedMessage = testWriter.GetMessage(1, 1, 1, 1);

            output.ShouldBe(expectedMessage);
        }

        [Test]
        public void VerifyTestMigrationSchema()
        {
            //run TestMigration migration, read, then remove...
            var runnerContext = new RunnerContext(new TextWriterAnnouncer(System.Console.Out))
            {
                Namespace =
                    typeof(TestMigration).Namespace
            };

            var runner = new MigrationRunner(typeof(TestMigration).Assembly, runnerContext, Processor);
            runner.Up(new TestMigration());

            //read schema here
            var defs = SchemaDumper.ReadDbSchema();

            var testWriter = new SchemaTestWriter();
            var output = GetOutput(testWriter, defs);
            // extra table testversion with 1 index
            var expectedMessage = testWriter.GetMessage(4, 10, 2, 0);

            runner.Down(new TestMigration());
            runner.VersionLoader.RemoveVersionTable();

            //test
            output.ShouldBe(expectedMessage);
        }

        private string GetOutput(SchemaWriterBase testWriter, IList<TableDefinition> defs)
        {
            string output;
            using (var ms = new MemoryStream())
            {
                using (var reader = new StreamReader(ms))
                {
                    using (var sr = new StreamWriter(ms))
                    {
                        testWriter.WriteToStream(defs, sr);
                        sr.Flush();
                        ms.Seek(0, SeekOrigin.Begin); //goto beginning
                        output = reader.ReadToEnd();
                    }
                }
            }
            return output;
        }
    }
}