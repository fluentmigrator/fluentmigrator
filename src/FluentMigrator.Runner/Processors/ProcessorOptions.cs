namespace FluentMigrator.Runner.Processors
{
    public class ProcessorOptions : IMigrationProcessorOptions
    {
        public bool PreviewOnly { get; set; }
        public int Timeout { get; set; }

        /// <summary>How to handle SQL commands that are not supported by the underlying database.</summary>
        public CompatibilityMode CompatibilityMode { get; set; }
    }
}