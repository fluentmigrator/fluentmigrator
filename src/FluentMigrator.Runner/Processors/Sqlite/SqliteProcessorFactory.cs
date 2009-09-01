using System.Data.SQLite;
using FluentMigrator.Runner.Generators;

namespace FluentMigrator.Runner.Processors.Sqlite
{
	public class SqliteProcessorFactory : IMigrationProcessorFactory
	{
		public IMigrationProcessor Create(string connectionString)
		{
			var connection = new SQLiteConnection(connectionString);
			return new SqliteProcessor(connection, new SqliteGenerator());
		}
	}
}