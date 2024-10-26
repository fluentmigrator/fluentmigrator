namespace FluentMigrator.Runner.Versioning
{
    /// <summary>
    /// Interface to query/update migration information
    /// </summary>
    public interface IVersionInfo
    {
        /// <summary>
        /// Adds a migration version number as applied
        /// </summary>
        /// <param name="migration">The version number</param>
        void AddAppliedMigration(long migration);

        /// <summary>
        /// Gets the version numbers of all applied migrations
        /// </summary>
        /// <returns>the version numbers of all applied migrations</returns>
        System.Collections.Generic.IEnumerable<long> AppliedMigrations();

        /// <summary>
        /// Returns a value indicating whether a migration with the given version number has been applied
        /// </summary>
        /// <param name="migration">The migration version number to validate</param>
        /// <returns><c>true</c> when the migration with the given version number has been applied</returns>
        bool HasAppliedMigration(long migration);

        /// <summary>
        /// Gets the version number of the latest migration that has been applied
        /// </summary>
        /// <returns>The version number</returns>
        long Latest();
    }
}
