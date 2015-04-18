using System;
using System.IO;
using System.Reflection;
using System.Threading;
using FirebirdSql.Data.FirebirdClient;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Initialization;
using NUnit.Framework;

namespace FluentMigrator.Tests.Integration.Processors.Firebird.EndToEnd
{
    [TestFixture]
    [Category("Integration")]
    public class TestInitialMigration
    {
        [SetUp]
        public void SetUp()
        {
            if (File.Exists("fbtest.fdb"))
                FbConnection.DropDatabase(IntegrationTestOptions.Firebird.ConnectionString);

            FbConnection.CreateDatabase(IntegrationTestOptions.Firebird.ConnectionString);
        }

        [TearDown]
        public void TearDown()
        {
            FbConnection.ClearAllPools();

            // Avoid "lock time-out on wait transaction" exception
            var retries = 5;
            while (true)
            {
                try
                {
                    FbConnection.DropDatabase(IntegrationTestOptions.Firebird.ConnectionString);
                    break;
                }
                catch
                {
                    if (--retries == 0)
                        throw;
                    else
                        Thread.Sleep(100);
                }
            }
        }

        [TestCase("SIMPLE")]
        [TestCase("testVersionTableName")]
        public void Migrate_FirstVersion_ShouldCreateTable(string tableName)
        {
            MakeTask("migrate").Execute();

            Assert.That(TableExists(tableName), Is.True, "Table {0} should have been created but it wasn't", tableName);
        }

        [TestCase("ID")]
        [TestCase("COL_STR")]
        public void Migrate_FirstVersion_ShouldCreateColumn(string columnName)
        {
            MakeTask("migrate").Execute();

            Assert.That(ColumnExists("SIMPLE", columnName), Is.True, "Column {0} should have been created but it wasn't", columnName);
        }

        [TestCase("SIMPLE")]
        [TestCase("testVersionTableName")]
        public void Rollback_FirstVersion_ShouldDropTable(string table)
        {
            MakeTask("migrate").Execute();

            MakeTask("rollback").Execute();

            Assert.That(TableExists(table), Is.False, "Table {0} should have been dropped but it wasn't", table);
        }

        private static TaskExecutor MakeTask(string task)
        {
            var announcer = new TextWriterAnnouncer(System.Console.Out);
            var runnerContext = new RunnerContext(announcer)
            {
                Database = "Firebird",
                Connection = IntegrationTestOptions.Firebird.ConnectionString,
                Targets = new[] { Assembly.GetExecutingAssembly().Location },
                Namespace = "FluentMigrator.Tests.Integration.Migrations.Firebird.FirstVersion",
                Task = task,
            };
            return new TaskExecutor(runnerContext);
        }

        private static bool TableExists(string candidate)
        {
            return IsInDatabase(cmd =>
                {
                    cmd.CommandText = "select rdb$relation_name from rdb$relations where (rdb$flags is not null) and (rdb$relation_name = @table)";
                    cmd.Parameters.AddWithValue("table", candidate.ToUpper());
                });
        }

        private static bool ColumnExists(string tableName, string candidateColumn)
        {
            return IsInDatabase(cmd =>
                {
                    cmd.CommandText = "select rdb$field_name from rdb$relation_fields where (rdb$relation_name = @table) and (rdb$field_name = @column)";
                    cmd.Parameters.AddWithValue("table", tableName);
                    cmd.Parameters.AddWithValue("column", candidateColumn);
                });
        }

        private static bool IsInDatabase(Action<FbCommand> adjustCommand)
        {
            var result = false;
            using (var connection = new FbConnection(IntegrationTestOptions.Firebird.ConnectionString))
            {
                connection.Open();
                using (var tx = connection.BeginTransaction())
                {
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.Transaction = tx;
                        adjustCommand(cmd);
                        using (var reader = cmd.ExecuteReader())
                        {
                            result = reader.Read();
                        }
                    }
                    tx.Commit();
                }
                connection.Close();
            }
            return result;
        }
    }
}
