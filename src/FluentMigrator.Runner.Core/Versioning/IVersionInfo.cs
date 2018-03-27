namespace FluentMigrator.Runner.Versioning
{
    public interface IVersionInfo
    {
        void AddAppliedMigration(long migration);
        System.Collections.Generic.IEnumerable<long> AppliedMigrations();
        bool HasAppliedMigration(long migration);
        long Latest();
    }
}
