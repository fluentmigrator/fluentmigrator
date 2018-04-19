using FluentMigrator.Runner.Initialization;

namespace FluentMigrator.Runner.Processors
{
    public class ProcessorOptions : IMigrationProcessorOptions
    {
        public ProcessorOptions()
        {
        }

        public ProcessorOptions(IRunnerContext runnerContext)
        {
            PreviewOnly = runnerContext.PreviewOnly;
            Timeout = runnerContext.Timeout;
            ProviderSwitches = runnerContext.ProviderSwitches;
        }

        public bool PreviewOnly { get; set; }
        public int? Timeout { get; set; }
        public string ProviderSwitches  { get; set; }
    }
}
