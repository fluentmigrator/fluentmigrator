namespace FluentMigrator.Runner.Initialization
{
	public interface IRunnerContext
	{
		string Database { get; set; }
		string Connection { get; set; }
		string Target { get; set; }
		bool LoggingEnabled { get; set; }
		string Namespace { get; set; }
		string Task { get; set; }
		long Version { get; set; }
		int Steps { get; set; }
		string WorkingDirectory { get; set; }
		IMigrationProcessor Processor { get; }
	}
}