using System.Data.SQLite;

namespace FluentMigrator.Runner.Processors
{
	public class SqliteProcessorFactory : IMigrationProcessorFactory
	{
		public IMigrationProcessor Create(string connectionString)
		{
			var connection = new SQLiteConnection(connectionString);
			return new SqliteProcessor(connection);
		}
	}
}