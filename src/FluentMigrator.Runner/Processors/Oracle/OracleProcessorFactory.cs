using System.Data;
using FluentMigrator.Runner.Generators;
using Oracle.DataAccess.Client;

namespace FluentMigrator.Runner.Processors.Oracle
{
	public class OracleProcessorFactory : IMigrationProcessorFactory
	{
		public IMigrationProcessor Create(string connectionString, IAnnouncer announcer)
		{
			var connection = new OracleConnection(connectionString);
			connection.Open();
			return new OracleProcessor(connection, new OracleGenerator(), announcer);
		}

		public IMigrationProcessor Create(IDbConnection connection, IAnnouncer announcer)
		{
			return new OracleProcessor((OracleConnection)connection, new OracleGenerator(), announcer);
		}
	}
}