using System.Reflection;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner.Initialization;

namespace FluentMigrator.Runner
{
    public interface IMigrationRunner : IMigrationScopeStarter
    {
        IMigrationProcessor Processor { get; }
        IMigrationInformationLoader MigrationLoader { get; set; }
        IAssemblyCollection MigrationAssemblies { get; }
        IRunnerContext RunnerContext { get; }
        void Up(IMigration migration);
        void Down(IMigration migration);
        void MigrateUp();
        void MigrateUp(long version);
        void Rollback(int steps);
        void RollbackToVersion(long version);
        void MigrateDown(long version);
        void ValidateVersionOrder();
        void ListMigrations();
    }
}