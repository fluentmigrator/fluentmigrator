using System.Collections.Generic;

namespace FluentMigrator.Runner.Initialization
{
    public class RunnerContext : IRunnerContext
    {
        public RunnerContext(IAnnouncer announcer)
        {
            Announcer = announcer;
            StopWatch = new StopWatch();
        }

        public string Database { get; set; }
        public string Connection { get; set; }
        public string[] Targets { get; set; }
        public bool PreviewOnly { get; set; }
        public string Namespace { get; set; }
        public bool NestedNamespaces { get; set; }
        public string Task { get; set; }
        public long Version { get; set; }
        public long StartVersion { get; set; }
        public bool NoConnection { get; set; }
        public int Steps { get; set; }
        public string WorkingDirectory { get; set; }
        public string Profile { get; set; }
        public int Timeout { get; set; }
        public string ConnectionStringConfigPath { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public bool TransactionPerSession { get; set; }
        public string ProviderSwitches { get; set; }

        public IAnnouncer Announcer
        {
            get;
            private set;
        }

        public IStopWatch StopWatch
        {
            get;
            private set;
        }

        /// <summary>The arbitrary application context passed to the task runner.</summary>
        public object ApplicationContext { get; set; }
    }
}
