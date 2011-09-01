using System;
using System.Configuration;
using System.IO;
using System.Linq;
using FluentMigrator.Runner.Processors;

namespace FluentMigrator.Runner.Initialization
{
    public class RunnerContext : IRunnerContext
    {
        public RunnerContext(IAnnouncer announcer)
        {
            Announcer = announcer;
            StopWatch = new StopWatch();
        }

        private IMigrationProcessor _processor;

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
            set;
        }

        public IStopWatch StopWatch
        {
            get;
            private set;
        }

        public IMigrationProcessor Processor
        {
            get
            {
                if (_processor != null)
                {
                    return _processor;
                }

                var manager = new NetConfigManager(Connection, ConnectionStringConfigPath, Target, Database);

                manager.LoadConnectionString();

                if (Timeout == 0)
                {
                    Timeout = 30; // Set default timeout for command
                }

                var processorFactory = ProcessorFactory.GetFactory(Database);
                _processor = processorFactory.Create(manager.ConnectionString, Announcer, new ProcessorOptions
                                                                                    {
                                                                                        PreviewOnly = PreviewOnly,
                                                                                        Timeout = Timeout
                                                                                    });

                return _processor;
            }
        }

    }
}