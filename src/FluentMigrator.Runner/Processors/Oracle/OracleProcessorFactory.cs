using System;
using System.Data;
using FluentMigrator.Runner.Generators;
using Oracle.DataAccess.Client;

namespace FluentMigrator.Runner.Processors.Oracle
{
    public class OracleProcessorFactory : MigrationProcessorFactory
	{
		public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
		{
			var connection = new OracleConnection(connectionString);
			connection.Open();
			return new OracleProcessor(connection, new OracleGenerator(), announcer, options);
		}

		public override IMigrationProcessor Create(IDbConnection connection, IAnnouncer announcer, IMigrationProcessorOptions options)
		{
			return new OracleProcessor((OracleConnection)connection, new OracleGenerator(), announcer, options);
		}
	}
}