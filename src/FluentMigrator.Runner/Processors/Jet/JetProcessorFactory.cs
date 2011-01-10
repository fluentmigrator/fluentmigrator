using System;
using System.Data;
using System.Data.OleDb;
using FluentMigrator.Runner.Generators;

namespace FluentMigrator.Runner.Processors.Jet
{
	public class JetProcessorFactory : MigrationProcessorFactory
	{
		public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
		{
			var connection = new OleDbConnection(connectionString);
			return new JetProcessor(connection, new JetGenerator(), announcer, options);
		}

		public override IMigrationProcessor Create(IDbConnection connection, IAnnouncer announcer, IMigrationProcessorOptions options)
		{
			return new JetProcessor((OleDbConnection) connection, new JetGenerator(), announcer, options);
		}
	}
}
