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
    public class FbEndToEndFixture
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

        protected void Migrate(string migrationsNamespace)
        {
            MakeTask("migrate", migrationsNamespace).Execute();
        }

        protected void Rollback(string migrationsNamespace)
        {
            MakeTask("rollback", migrationsNamespace).Execute();
        }

        protected TaskExecutor MakeTask(string task, string migrationsNamespace)
        {
            var announcer = new TextWriterAnnouncer(System.Console.Out);
            var runnerContext = new RunnerContext(announcer)
            {
                Database = "Firebird",
                Connection = IntegrationTestOptions.Firebird.ConnectionString,
                Targets = new[] { Assembly.GetExecutingAssembly().Location },
                Namespace = migrationsNamespace,
                Task = task
            };
            return new TaskExecutor(runnerContext);
        }

        protected static bool TableExists(string candidate)
        {
            return IsInDatabase(cmd =>
                {
                    cmd.CommandText = "select rdb$relation_name from rdb$relations where (rdb$flags is not null) and (rdb$relation_name = @table)";
                    cmd.Parameters.AddWithValue("table", candidate.ToUpper());
                });
        }

        protected static bool ColumnExists(string tableName, string candidateColumn)
        {
            return IsInDatabase(cmd =>
                {
                    cmd.CommandText = "select rdb$field_name from rdb$relation_fields where (rdb$relation_name = @table) and (rdb$field_name = @column)";
                    cmd.Parameters.AddWithValue("table", tableName.ToUpper());
                    cmd.Parameters.AddWithValue("column", candidateColumn.ToUpper());
                });
        }

        protected static bool IsInDatabase(Action<FbCommand> adjustCommand)
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
