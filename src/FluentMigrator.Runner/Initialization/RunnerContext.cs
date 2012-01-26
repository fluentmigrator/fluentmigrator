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
        public string Target { get; set; }
        public bool PreviewOnly { get; set; }
        public string Namespace { get; set; }
        public string Task { get; set; }
        public long Version { get; set; }
        public int Steps { get; set; }
        public string WorkingDirectory { get; set; }
        public string Profile { get; set; }
        public int Timeout { get; set; }
        public string ConnectionStringConfigPath { get; set; }

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

        /// <summary>Whether to throw an exception when a SQL command is not supported by the underlying database type.</summary>
        public bool StrictCompatibility { get; set; }

        /// <summary>Whether to imitate database support for some SQL commands that are not supported by the underlying database type.</summary>
        /// <remarks>For example, schema support can be emulated by prefixing the schema name to the table name (<c>`schema`.`table`</c> => <c>`schema_table`</c>).</remarks>
        public bool EmulateCompatibility { get; set; }
    }
}