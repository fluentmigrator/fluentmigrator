using FirebirdSql.Data.FirebirdClient;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators.Firebird;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Firebird;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace FluentMigrator.Tests.Integration.Processors.Firebird
{
    [TestFixture]
    public class FirebirdEmbeddedTableTests
    {
        public class AutoDeleter: IDisposable
        {
            private List<string> _fileNames;

            public AutoDeleter(params string[] fileNames)
            {
                _fileNames = new List<string>();
                Add(fileNames);
            }

            public void Add(params string[] fileNames)
            {
                _fileNames.AddRange(fileNames);
            }

            public void Dispose()
            {
                lock (this)
                {
                    if (_fileNames != null)
                    {
                        foreach (var fileName in _fileNames)
                        {
                            try
                            {
                                File.Delete(fileName);
                            }
                            catch { }
                        }
                    }
                    _fileNames = null;
                }
            }
        }

        public class CreateTableMigration : Migration
        {
            public override void Up()
            {
                Create.Table("TheTable")
                    .WithColumn("Id").AsInt32().PrimaryKey()
                    .WithColumn("Name").AsString(100).Nullable();
                Insert.IntoTable("TheTable").Row(new {Id = 1});
            }

            public override void Down()
            {
            }
        }

        public class RenameTableMigration : Migration
        {
            public override void Up()
            {
                Rename.Table("TheTable").To("TheNewTable");
            }

            public override void Down()
            {
            }
        }

        [Test]
        public void RenameTable_WhenOriginalTableExistsAndContainsDataWithNulls_ShouldNotThrowException()
        {
            //---------------Set up test pack-------------------
            var tempResources = WriteOutFirebirdEmbeddedLibrariesToCurrentWorkingDirectory();
            var tempFile = Path.GetTempFileName();
            using (var deleter = new AutoDeleter(tempFile))
            {
                File.Delete(tempFile);  // Firebird will b0rk if it has to create a database where a file already exists
                deleter.Add(tempResources);
                var connectionString = GetConnectionStringToTempDatabaseAt(tempFile);

                var runnerContext = new RunnerContext(new TextWriterAnnouncer(System.Console.Out))
                                            {
                                                Namespace = "FluentMigrator.Tests.Integration.Migrations"
                                            };


                using (var connection = new FbConnection(connectionString))
                {
                    FirebirdProcessor processor;
                    var runner = CreateFirebirdEmbeddedRunnerFor(connection, runnerContext, out processor);
                    runner.Up(new CreateTableMigration());
                    //---------------Assert Precondition----------------
                    connection.Open();
                    using (var cmd = new FbCommand("select * from \"TheTable\"", connection))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            Assert.IsTrue(reader.Read());
                            Assert.IsInstanceOf<DBNull>(reader["Name"]);
                        }
                    }
                    //---------------Execute Test ----------------------
                    Exception thrown = null;
                    try
                    {
                        runner.Up(new RenameTableMigration());
                    }
                    catch (Exception ex)
                    {
                        thrown = ex;
                    }

                    //---------------Test Result -----------------------
                    Assert.IsNull(thrown);
                }
            }
        }

        private static MigrationRunner CreateFirebirdEmbeddedRunnerFor(FbConnection connection, RunnerContext runnerContext, out FirebirdProcessor processor)
        {
            var announcer = new TextWriterAnnouncer(System.Console.Out);
            announcer.ShowSql = true;
            var options = FirebirdOptions.AutoCommitBehaviour();
            processor = new FirebirdProcessor(connection, new FirebirdGenerator(options), announcer,
                new ProcessorOptions(), new FirebirdDbFactory(), options);
            var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), runnerContext, processor);
            return runner;
        }

        private static string GetConnectionStringToTempDatabaseAt(string tempFile)
        {
            var builder = new FbConnectionStringBuilder();
            builder.ServerType = FbServerType.Embedded;
            builder.UserID = "sysdba";
            builder.Password = "masterkey";
            builder.Database = tempFile;
            var connectionString = builder.ConnectionString;
            FbConnection.CreateDatabase(connectionString);
            return connectionString;
        }

        private class EmbeddedLibraryResource
        {
            public string Name { get; private set; }
            public byte[] Data { get; private set; }

            public EmbeddedLibraryResource(string name, byte[] data)
            {
                Name = name;
                Data = data;
            }
        }

        private string[] WriteOutFirebirdEmbeddedLibrariesToCurrentWorkingDirectory()
        {
            var result = new List<string>();
            lock (this)
            {
                var blobs = new[]
                {
                    new EmbeddedLibraryResource("aliases.conf", FirebirdEmbeddedLibraries.aliases),
                    new EmbeddedLibraryResource("fbembed.dll", FirebirdEmbeddedLibraries.fbembed),
                    new EmbeddedLibraryResource("firebird.conf", FirebirdEmbeddedLibraries.firebird_conf),
                    new EmbeddedLibraryResource("firebird.msg", FirebirdEmbeddedLibraries.firebird_msg),
                    new EmbeddedLibraryResource("ib_util.dll", FirebirdEmbeddedLibraries.ib_util),
                    new EmbeddedLibraryResource("icudt30.dll", FirebirdEmbeddedLibraries.icudt30),
                    new EmbeddedLibraryResource("icuin30.dll", FirebirdEmbeddedLibraries.icuin30),
                    new EmbeddedLibraryResource("icuuc30.dll", FirebirdEmbeddedLibraries.icuuc30),
                    new EmbeddedLibraryResource("msvcp80.dll", FirebirdEmbeddedLibraries.msvcp80),
                    new EmbeddedLibraryResource("msvcr80.dll", FirebirdEmbeddedLibraries.msvcp80)
                };
                foreach (var blob in blobs)
                {
                    if (!File.Exists(blob.Name))
                    {
                        File.WriteAllBytes(blob.Name, blob.Data);
                        result.Add(Path.Combine(Environment.CurrentDirectory, blob.Name));
                    }
                }
            }
            return result.ToArray();
        }

        public class MigrationWhichCreatesTableWithTextBlob : Migration
        {
            public override void Up()
            {
                Create.Table("TheTable")
                    .WithColumn("TheColumn").AsString(int.MaxValue);
            }

            public override void Down()
            {
            }
        }

        public class MigrationWhichRenamesTableWithTextBlob : Migration
        {
            public override void Up()
            {
                Rename.Table("TheTable").To("TheNewTable");
            }

            public override void Down()
            {
            }
        }

        [Test]
        public void RenamingTable_WhenTableHasTextBlobs_ShouldCreateNewTableWithTextBlobsNotBinaryBlobs()
        {
            var tempResources = WriteOutFirebirdEmbeddedLibrariesToCurrentWorkingDirectory();
            var tempFile = Path.GetTempFileName();
            using (var deleter = new AutoDeleter(tempFile))
            {
                File.Delete(tempFile);  // Firebird will b0rk if it has to create a database where a file already exists
                deleter.Add(tempResources);
                var connectionString = GetConnectionStringToTempDatabaseAt(tempFile);

                var runnerContext = new RunnerContext(new TextWriterAnnouncer(System.Console.Out))
                                            {
                                                Namespace = "FluentMigrator.Tests.Integration.Migrations"
                                            };


                using (var connection = new FbConnection(connectionString))
                {
                    FirebirdProcessor processor;
                    var runner = CreateFirebirdEmbeddedRunnerFor(connection, runnerContext, out processor);
                    runner.Up(new MigrationWhichCreatesTableWithTextBlob());
                    //---------------Assert Precondition----------------
                    var fieldName = "TheColumn";
                    var tableName = "TheTable";
                    var expectedFieldType = 261;
                    var expectedFieldSubType = 1;
                    AssertThatFieldHasCorrectTypeAndSubType(fieldName, tableName, connection, expectedFieldType, expectedFieldSubType);
                    //---------------Execute Test ----------------------
                    runner.Up(new MigrationWhichRenamesTableWithTextBlob());
                    //---------------Test Result -----------------------
                    tableName = "TheNewTable";
                    AssertThatFieldHasCorrectTypeAndSubType(fieldName, tableName, connection, expectedFieldType, expectedFieldSubType);
                }
            }
        }

        private static void AssertThatFieldHasCorrectTypeAndSubType(string fieldName, string tableName, FbConnection connection,
            int expectedFieldType, int expectedFieldSubType)
        {
            connection.Open();
            var sql =
                "select \"RDB$FIELD_TYPE\" fieldType, \"RDB$FIELD_SUB_TYPE\" subType from \"RDB$RELATION_FIELDS\" rf " +
                "inner join \"RDB$FIELDS\" f on rf.\"RDB$FIELD_SOURCE\" = f.\"RDB$FIELD_NAME\" where rf.\"RDB$FIELD_NAME\" = '" +
                fieldName + "' " +
                "and rf.\"RDB$RELATION_NAME\" = '" + tableName + "'";
            using (var cmd = new FbCommand(sql, connection))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    Assert.IsTrue(reader.Read(), "Unable to query schema for table '" + tableName + "'");
                    var fieldType = reader["fieldType"];
                    var fieldSubType = reader["subType"];
                    Assert.AreEqual(expectedFieldType, fieldType, "Field type mismatch");
                    Assert.AreEqual(expectedFieldSubType, fieldSubType, "Field subtype mismatch");
                }
            }
        }
    }
}
