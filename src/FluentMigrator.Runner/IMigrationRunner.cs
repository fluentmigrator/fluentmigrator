using System.Reflection;

namespace FluentMigrator.Runner
{
    public interface IMigrationRunner : IMigrationScopeStarter
    {
        IMigrationProcessor Processor { get; }
        Assembly MigrationAssembly { get; }
        void Up(IMigration migration);
        void Down(IMigration migration);
        void MigrateUp();
        void MigrateUp(long version);
        void MigrateUp(long startVersion, long targetVersion);
        void Rollback(int steps);
        void RollbackToVersion(long version);
        void MigrateDown(long version);
        void MigrateDown(long startVersion, long targetVersion);
        void ValidateVersionOrder();
        void ListMigrations();
    }
}