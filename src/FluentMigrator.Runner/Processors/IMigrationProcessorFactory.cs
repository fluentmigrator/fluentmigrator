using System.Data;

namespace FluentMigrator.Runner.Processors
{
	public interface IMigrationProcessorFactory
	{
		IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options);
		IMigrationProcessor Create(IDbConnection connection, IAnnouncer announcer, IMigrationProcessorOptions options);

	    bool IsForProvider(string provider);

        string Name { get; }
	}
}