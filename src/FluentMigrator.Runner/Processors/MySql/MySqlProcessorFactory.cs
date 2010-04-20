using System.Data;
using FluentMigrator.Runner.Generators;
using MySql.Data.MySqlClient;

namespace FluentMigrator.Runner.Processors.MySql
{
	public class MySqlProcessorFactory : MigrationProcessorFactory
	{
        public override IMigrationProcessor Create(string connectionString)
		{
			var connection = new MySqlConnection(connectionString);
			return new MySqlProcessor(connection, new MySqlGenerator());
		}

		public override IMigrationProcessor Create( IDbConnection connection )
		{
			return new MySqlProcessor((MySqlConnection)connection, new MySqlGenerator());
		}
	}
}
