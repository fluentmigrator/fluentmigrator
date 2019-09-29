namespace FluentMigrator.Runner
{
    /// <summary>
    /// Handler for <see cref="IMigrationScope"/>
    /// </summary>
    public interface IMigrationScopeHandler
    {
        IMigrationScope CurrentScope { get; set; }

        IMigrationScope BeginScope();
        IMigrationScope CreateOrWrapMigrationScope(bool transactional = true);
    }
}
