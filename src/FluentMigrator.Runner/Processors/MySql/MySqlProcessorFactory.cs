using System.Data;
using FluentMigrator.Runner.Generators;
using MySql.Data.MySqlClient;

namespace FluentMigrator.Runner.Processors.MySql
{
	public class MySqlProcessorFactory : MigrationProcessorFactory
	{
		public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
		{
			var connection = new MySqlConnection(connectionString);
			return new MySqlProcessor(connection, new MySqlGenerator(), announcer, options);
		}

		public override IMigrationProcessor Create(IDbConnection connection, IAnnouncer announcer, IMigrationProcessorOptions options)
		{
			return new MySqlProcessor((MySqlConnection)connection, new MySqlGenerator(), announcer, options);
		}
	}
}
