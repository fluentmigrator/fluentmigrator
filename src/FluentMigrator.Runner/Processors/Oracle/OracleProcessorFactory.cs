


namespace FluentMigrator.Runner.Processors.Oracle
{
    using System.Data;
    using FluentMigrator.Runner.Generators.Oracle;

	public class OracleProcessorFactory : MigrationProcessorFactory
	{
		public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
		{
			var connection = OracleFactory.GetOpenConnection(connectionString);
			
			return new OracleProcessor(connection, new OracleGenerator(), announcer, options);
		}

		public override IMigrationProcessor Create(IDbConnection connection, IAnnouncer announcer, IMigrationProcessorOptions options)
		{
			return new OracleProcessor(connection, new OracleGenerator(), announcer, options);
		}
	}
}