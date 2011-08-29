

namespace FluentMigrator.Runner.Processors.Sqlite
{
    using System.Data;
    using System.Data.SQLite;
    using FluentMigrator.Runner.Generators.SQLite;

    public class SqliteProcessorFactory : MigrationProcessorFactory
	{
		public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
		{
			var connection = new SQLiteConnection(connectionString);
			return new SqliteProcessor(connection, new SqliteGenerator(), announcer, options);
		}

		public virtual IMigrationProcessor Create(IDbConnection connection, IAnnouncer announcer, IMigrationProcessorOptions options)
		{
			return new SqliteProcessor((SQLiteConnection)connection, new SqliteGenerator(), announcer, options);
		}
	}
}