using System.Data;

namespace FluentMigrator.Runner.Processors
{
	public interface IMigrationProcessorFactory
	{
		IMigrationProcessor Create(string connectionString);
		IMigrationProcessor Create(IDbConnection connection);
	}
}