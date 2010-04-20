using System.Data;
using System.Data.SQLite;
using FluentMigrator.Runner.Generators;

namespace FluentMigrator.Runner.Processors.Sqlite
{
	public class SqliteProcessorFactory : MigrationProcessorFactory
	{
        public override IMigrationProcessor Create(string connectionString)
		{
			var connection = new SQLiteConnection(connectionString);
			return new SqliteProcessor(connection, new SqliteGenerator());
		}

        public override IMigrationProcessor Create(IDbConnection connection)
		{
			return new SqliteProcessor((SQLiteConnection)connection, new SqliteGenerator());
		}
	}
}