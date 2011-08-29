
using MySql.Data.MySqlClient;


namespace FluentMigrator.Runner.Processors.MySql
{
    using System.Data;
    using FluentMigrator.Runner.Generators.MySql;

	public class MySqlProcessorFactory : MigrationProcessorFactory
	{
		public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
		{
			var connection = new MySqlConnection(connectionString);
			return new MySqlProcessor(connection, new MySqlGenerator(), announcer, options);
		}

		public virtual IMigrationProcessor Create(IDbConnection connection, IAnnouncer announcer, IMigrationProcessorOptions options)
		{
			return new MySqlProcessor((MySqlConnection)connection, new MySqlGenerator(), announcer, options);
		}
	}
}
