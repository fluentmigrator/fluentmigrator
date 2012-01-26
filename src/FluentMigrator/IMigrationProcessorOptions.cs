namespace FluentMigrator
{
    /// <summary>Configuration for the migration processor.</summary>
    public interface IMigrationProcessorOptions
    {
        /// <summary>Whether to process all migrations without applying them to the database.</summary>
        bool PreviewOnly { get; }

        /// <summary>The wait time (in seconds) before terminating the attempt to execute a command and generating an error.</summary>
        int Timeout { get; }

        /// <summary>Whether to throw an exception when a SQL command is not supported by the underlying database type.</summary>
        bool StrictCompatibility { get; }

        /// <summary>Whether to imitate database support for some SQL commands that are not supported by the underlying database type.</summary>
        /// <remarks>For example, schema support can be emulated by prefixing the schema name to the table name (<c>`schema`.`table`</c> => <c>`schema_table`</c>).</remarks>
        bool EmulateCompatibility { get; }
    }
}