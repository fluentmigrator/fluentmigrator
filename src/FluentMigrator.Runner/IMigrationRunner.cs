using System.Reflection;

namespace FluentMigrator.Runner
{
    public interface IMigrationRunner
    {
        IMigrationProcessor Processor { get; }
        Assembly MigrationAssembly { get; }
        void Up(IMigration migration);
        void MigrateUp();
        void MigrateUp(long version);
        void Rollback(int steps);
        void RollbackToVersion(long version);
        void MigrateDown(long version);
        void ValidateVersionOrder();
    }
}