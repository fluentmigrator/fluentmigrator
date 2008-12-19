using FluentMigrator.Runner.Processors;

namespace FluentMigrator.Runner
{
	public interface IMigrationProcessorFactory
	{
		IMigrationProcessor Create(string connectionString);
	}
}