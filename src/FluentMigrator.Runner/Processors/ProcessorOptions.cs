namespace FluentMigrator.Runner.Processors
{
    public class ProcessorOptions : IMigrationProcessorOptions
    {
        public bool PreviewOnly { get; set; }
        public int Timeout { get; set; }

        /// <summary>Whether to throw exceptions if a SQL command is not recognized by the underlying database.</summary>
        /// <remarks>The default value is <c>false</c>.</remarks>
        public bool StrictCompatibility { get; set; }

        /// <summary>Whether to emulate support for incompatible SQL commands.</summary>
        /// <example>For example, generators that don't support schemas (like SQLite) might substitute the schema name into the table name (i.e., `schema`.`table` => `schema_table`).</example>
        public bool EmulateCompatibility { get; set; }
    }
}