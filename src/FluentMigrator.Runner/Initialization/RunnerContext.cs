using FluentMigrator.Runner.Processors;

namespace FluentMigrator.Runner.Initialization
{
	public class RunnerContext : IRunnerContext
	{
		public string Database { get; set; }
		public string Connection { get; set; }
		public string Target { get; set; }
		public bool LoggingEnabled { get; set; }
		public string Namespace { get; set; }
		public string Task { get; set; }
		public long Version { get; set; }
		public int Steps { get; set; }
		public string WorkingDirectory { get; set; }

		private IMigrationProcessor _processor;

		public IMigrationProcessor Processor
		{
			get
			{
				if (_processor != null)
					return _processor;

				IMigrationProcessorFactory processorFactory = ProcessorFactory.GetFactory(Database);
				_processor = processorFactory.Create(Connection);

				return _processor;
			}
		}
	}
}