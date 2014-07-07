using System.Data;
using System.Diagnostics;
using System.Threading;
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
    //[Category("Integration")]
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
            builder.Pooling = false;
            var connectionString = builder.ConnectionString;
            if (!File.Exists(tempFile))
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
            switch (Environment.OSVersion.Platform)
            {
		case PlatformID.Unix:
		    return new string[] { };
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                    return WriteOutFirebirdEmbeddedLibraries();
                default:
                    throw new PlatformUnsupportedException(Environment.OSVersion.Platform);
            }
        }

        private string[] WriteOutFirebirdEmbeddedLibraries()
        {
            var result = new List<string>();
            lock (this)
            {
                var blobs = new[]
                            {
                                new EmbeddedLibraryResource("aliases.conf", FirebirdEmbeddedLibrariesForWindows.aliases),
                                new EmbeddedLibraryResource("fbembed.dll", FirebirdEmbeddedLibrariesForWindows.fbembed),
                                new EmbeddedLibraryResource("firebird.conf", FirebirdEmbeddedLibrariesForWindows.firebird_conf),
                                new EmbeddedLibraryResource("firebird.msg", FirebirdEmbeddedLibrariesForWindows.firebird_msg),
                                new EmbeddedLibraryResource("ib_util.dll", FirebirdEmbeddedLibrariesForWindows.ib_util),
                                new EmbeddedLibraryResource("icudt30.dll", FirebirdEmbeddedLibrariesForWindows.icudt30),
                                new EmbeddedLibraryResource("icuin30.dll", FirebirdEmbeddedLibrariesForWindows.icuin30),
                                new EmbeddedLibraryResource("icuuc30.dll", FirebirdEmbeddedLibrariesForWindows.icuuc30),
                                new EmbeddedLibraryResource("msvcp80.dll", FirebirdEmbeddedLibrariesForWindows.msvcp80),
                                new EmbeddedLibraryResource("msvcr80.dll", FirebirdEmbeddedLibrariesForWindows.msvcp80)
                            };
                foreach (var blob in blobs)
                {
                    var path = Path.Combine(Environment.CurrentDirectory, blob.Name);
                    if (!File.Exists(path))
                    {
                        System.Console.WriteLine("-- " + path);
                        File.WriteAllBytes(path, blob.Data);
                        result.Add(path);
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

        public class MigrationWhichCreatesTwoRelatedTables : Migration
        {
            public const string ForeignKeyName = "FK_table2_table1";
            public override void Up()
            {
                Create.Table("table1")
                        .WithColumn("id").AsInt32().PrimaryKey().Identity();
                Create.Table("table2")
                        .WithColumn("table1_id").AsInt32();
                Create.ForeignKey(ForeignKeyName)
                        .FromTable("table2").ForeignColumn("table1_id")
                        .ToTable("table1").PrimaryColumn("id"); 
            }

            public override void Down()
            {
            }
        }

        public class MigrationWhichAltersTableWithFK : Migration
        {
            public override void Up()
            {
                Alter.Table("table1").AddColumn("Value").AsDouble().Nullable();
            }

            public override void Down()
            {
            }
        }



        [Test]
        public void AlterTable_MigrationRequiresAutomaticDelete_AndProcessorHasUndoDisabled_ShouldNotThrow()
        {
            // this test was originally created to investigate an issue with foreign key names but in the process,
            // I found that FirebirdProcessor.CreateSequenceForIdentity doesn't respect FBOptions.UndoEnabled. Since
            // Undo isn't implemented for Firebird, a migration runner always has to turn it off, but even with it off,
            // the migrations below will fail unless CreateSequenceForIdentity is appropriately altered
            var tempResources = WriteOutFirebirdEmbeddedLibrariesToCurrentWorkingDirectory();
            var tempFile = Path.GetTempFileName();
            using (var deleter = new AutoDeleter(tempFile))
            {
                File.Delete(tempFile);
                deleter.Add(tempResources);
                var connectionString = GetConnectionStringToTempDatabaseAt(tempFile);

                var runnerContext = new RunnerContext(new TextWriterAnnouncer(System.Console.Out))
                                            {
                                                Namespace = "FluentMigrator.Tests.Integration.Migrations"
                                            };


                try
                {
                    using (var connection = new FbConnection(connectionString))
                    {
                        FirebirdProcessor processor;
                        var announcer = new TextWriterAnnouncer(System.Console.Out);
                        announcer.ShowSql = true;
                        var options = FirebirdOptions.AutoCommitBehaviour();
                        options.TruncateLongNames = false;
                        processor = new FirebirdProcessor(connection, new FirebirdGenerator(options), announcer,
                            new ProcessorOptions(), new FirebirdDbFactory(), options);
                        processor.FBOptions.UndoEnabled = false;
                        var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), runnerContext, processor);
                        runner.Up(new MigrationWhichCreatesTwoRelatedTables());
                        processor.CommitTransaction();
                        FbConnection.ClearPool(connection);
                    }
                    //---------------Assert Precondition----------------
                    Assert.IsTrue(ForeignKeyExists(connectionString, MigrationWhichCreatesTwoRelatedTables.ForeignKeyName),
                        "Foreign key does not exist after first migration");
                    using (var connection = new FbConnection(connectionString))
                    {
                        FirebirdProcessor processor;
                        var announcer = new TextWriterAnnouncer(System.Console.Out);
                        announcer.ShowSql = true;
                        var options = FirebirdOptions.AutoCommitBehaviour();
                        processor = new FirebirdProcessor(connection, new FirebirdGenerator(options), announcer,
                            new ProcessorOptions(), new FirebirdDbFactory(), options);
                        processor.FBOptions.UndoEnabled = false;
                        var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), runnerContext, processor);
                        runner.Up(new MigrationWhichAltersTableWithFK());
                        processor.CommitTransaction();

                    }
                    Assert.IsTrue(ForeignKeyExists(connectionString, MigrationWhichCreatesTwoRelatedTables.ForeignKeyName),
                        "Foreign key does not exist after second migration");

                }
                catch (Exception ex)
                {
                    try { File.Copy(tempFile, "C:\\tmp\\fm_tests.fdb", true); }
                    catch { }
                    throw ex;

                }
            }
        }

        private bool ForeignKeyExists(string connectionString, string withName)
        {
            using (var connection = new FbConnection(connectionString))
            {
                connection.Open();
                var keyQuery = String.Format(@"
SELECT rc.RDB$CONSTRAINT_NAME AS constraint_name,
i.RDB$RELATION_NAME AS table_name,
s.RDB$FIELD_NAME AS field_name,
i.RDB$DESCRIPTION AS description,
rc.RDB$DEFERRABLE AS is_deferrable,
rc.RDB$INITIALLY_DEFERRED AS is_deferred,
refc.RDB$UPDATE_RULE AS on_update,
refc.RDB$DELETE_RULE AS on_delete,
refc.RDB$MATCH_OPTION AS match_type,
i2.RDB$RELATION_NAME AS references_table,
s2.RDB$FIELD_NAME AS references_field,
(s.RDB$FIELD_POSITION + 1) AS field_position
FROM RDB$INDEX_SEGMENTS s
LEFT JOIN RDB$INDICES i ON i.RDB$INDEX_NAME = s.RDB$INDEX_NAME
LEFT JOIN RDB$RELATION_CONSTRAINTS rc ON rc.RDB$INDEX_NAME = s.RDB$INDEX_NAME
LEFT JOIN RDB$REF_CONSTRAINTS refc ON rc.RDB$CONSTRAINT_NAME = refc.RDB$CONSTRAINT_NAME
LEFT JOIN RDB$RELATION_CONSTRAINTS rc2 ON rc2.RDB$CONSTRAINT_NAME = refc.RDB$CONST_NAME_UQ
LEFT JOIN RDB$INDICES i2 ON i2.RDB$INDEX_NAME = rc2.RDB$INDEX_NAME
LEFT JOIN RDB$INDEX_SEGMENTS s2 ON i2.RDB$INDEX_NAME = s2.RDB$INDEX_NAME
WHERE rc.RDB$CONSTRAINT_TYPE = 'FOREIGN KEY'
ORDER BY s.RDB$FIELD_POSITION", MigrationWhichCreatesTwoRelatedTables.ForeignKeyName);
                var cmd = connection.CreateCommand();
                cmd.CommandText = keyQuery;
                var result = false;
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        try
                        {
                            var constraintName = rdr["CONSTRAINT_NAME"];
                            if (constraintName == null) continue;
                            if (constraintName is DBNull) continue;
                            if (constraintName.ToString().Trim() == withName)
                            {
                                result = true;
                                break;
                            }
                        }
                        catch { }
                    }
                }
                connection.Close();
                FbConnection.ClearPool(connection);
                return result;
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

    public class PlatformUnsupportedException : Exception
    {
        public PlatformID PlatFormID { get; private set; }
        public PlatformUnsupportedException(PlatformID platformID): base("The current platform is not supported: " + platformID.ToString())
        {
            PlatFormID = platformID;
        }
    }
}
