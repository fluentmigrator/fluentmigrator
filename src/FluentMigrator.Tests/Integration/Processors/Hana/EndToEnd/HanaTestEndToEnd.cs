using System.Reflection;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Initialization;
using NUnit.Framework;

namespace FluentMigrator.Tests.Integration.Processors.Hana.EndToEnd
{
    [Category("Integration")]
    [Category("Hana")]
    public class HanaEndToEndFixture
    {
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
                Database = "Hana",
                Connection = IntegrationTestOptions.Hana.ConnectionString,
                Targets = new[] { Assembly.GetExecutingAssembly().Location },
                Namespace = migrationsNamespace,
                Task = task
            };
            return new TaskExecutor(runnerContext);
        }
        
    }
}
