namespace FluentMigrator.Runner.Processors
{
    public class ProcessorOptions : IMigrationProcessorOptions
    { 
        public bool PreviewOnly { get; set; }
        public int Timeout { get; set; }
        public bool UseTrans { get; set; }
    }
}