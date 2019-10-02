namespace FluentMigrator.Runner
{
    /// <summary>
    /// Handler for <see cref="IMigrationScope"/>
    /// </summary>
    public interface IMigrationScopeManager
    {
        /// <summary>
        /// Gets migration scope for the runner
        /// </summary>
        IMigrationScope CurrentScope { get; }

        /// <summary>
        /// Creates new migration scope
        /// </summary>
        /// <returns>Newly created scope</returns>
        IMigrationScope BeginScope();

        /// <summary>
        /// Creates new migrations scope or reuses existing one
        /// </summary>
        /// <param name="transactional">Defines if transactions should be used</param>
        /// <returns>Migration scope</returns>
        IMigrationScope CreateOrWrapMigrationScope(bool transactional = true);
    }
}
