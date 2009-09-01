namespace FluentMigrator.Runner.Processors
{
	public interface IMigrationProcessorFactory
	{
		IMigrationProcessor Create(string connectionString);
	}
}