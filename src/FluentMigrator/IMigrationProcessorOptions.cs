namespace FluentMigrator
{
    /// <summary>Configuration for the migration processor.</summary>
    public interface IMigrationProcessorOptions
    {
        /// <summary>Whether to process all migrations without applying them to the database.</summary>
        bool PreviewOnly { get; }

        /// <summary>The wait time (in seconds) before terminating the attempt to execute a command and generating an error.</summary>
        int Timeout { get; }

        /// <summary>How to handle SQL commands that are not supported by the underlying database.</summary>
        CompatibilityMode CompatibilityMode { get; }
    }
}