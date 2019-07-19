namespace FluentMigrator.Runner
{
    public interface IMigrationScopeHandler
    {
        IMigrationScope CurrentScope { get; set; }

        IMigrationScope BeginScope();
        IMigrationScope CreateOrWrapMigrationScope(bool transactional = true);
    }
}