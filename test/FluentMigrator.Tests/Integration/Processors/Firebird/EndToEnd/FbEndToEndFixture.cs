using System;
using System.Diagnostics;
using System.Reflection;
using FirebirdSql.Data.FirebirdClient;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

namespace FluentMigrator.Tests.Integration.Processors.Firebird.EndToEnd
{
    [Category("Integration")]
    [Category("Firebird")]
    public class FbEndToEndFixture
    {
        private static readonly FirebirdLibraryProber _firebirdLibraryProber = new FirebirdLibraryProber();
        private TemporaryDatabase _temporaryDatabase;

        protected string ConnectionString => _temporaryDatabase.ConnectionString;

        [SetUp]
        public void SetUp()
        {
            if (!IntegrationTestOptions.Firebird.IsEnabled)
                Assert.Ignore();
            _temporaryDatabase = new TemporaryDatabase(
                IntegrationTestOptions.Firebird,
                _firebirdLibraryProber);
        }

        [TearDown]
        public void TearDown()
        {
            if (_temporaryDatabase == null)
                return;

            FbDatabase.DropDatabase(_temporaryDatabase.ConnectionString);
            _temporaryDatabase.Dispose();
            _temporaryDatabase = null;
        }

        protected void Migrate(string migrationsNamespace)
        {
            MakeTask("migrate", migrationsNamespace).Execute();
        }

        protected void Rollback(string migrationsNamespace)
        {
            MakeTask("rollback", migrationsNamespace).Execute();
        }

        protected TaskExecutor MakeTask(string task, string migrationsNamespace, Action<ProcessorOptions> configureOptions = null)
        {
            var consoleAnnouncer = new TextWriterAnnouncer(TestContext.Out) { ShowSql = true };
            var debugAnnouncer = new TextWriterAnnouncer(msg => Debug.WriteLine(msg));
            var announcer = new CompositeAnnouncer(consoleAnnouncer, debugAnnouncer);

            var services = new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(builder => builder
                    .WithAnnouncer(new TextWriterAnnouncer(TestContext.Out) { ShowSql = true })
                    .AddFirebird())
                .Configure<RunnerOptions>(opt => opt.AllowBreakingChange = true)
                .AddSingleton<IAnnouncer>(announcer)
                .AddScoped<IConnectionStringReader>(_ => new PassThroughConnectionStringReader(ConnectionString))
                .WithMigrationsIn(migrationsNamespace)
                .Configure<RunnerOptions>(opt => opt.Task = task)
                .AddScoped<TaskExecutor>();

            var serviceBuilder = services.BuildServiceProvider();
            return serviceBuilder.GetRequiredService<TaskExecutor>();
        }

        protected bool TableExists(string candidate)
        {
            return IsInDatabase(cmd =>
                {
                    cmd.CommandText = "select rdb$relation_name from rdb$relations where (rdb$flags is not null) and (rdb$relation_name = @table)";
                    cmd.Parameters.AddWithValue("table", candidate.ToUpper());
                });
        }

        protected bool ColumnExists(string tableName, string candidateColumn)
        {
            return IsInDatabase(cmd =>
                {
                    cmd.CommandText = "select rdb$field_name from rdb$relation_fields where (rdb$relation_name = @table) and (rdb$field_name = @column)";
                    cmd.Parameters.AddWithValue("table", tableName.ToUpper());
                    cmd.Parameters.AddWithValue("column", candidateColumn.ToUpper());
                });
        }

        protected bool IsInDatabase(Action<FbCommand> adjustCommand)
        {
            bool result;
            using (var connection = new FbConnection(ConnectionString))
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
            }

            return result;
        }
    }
}
