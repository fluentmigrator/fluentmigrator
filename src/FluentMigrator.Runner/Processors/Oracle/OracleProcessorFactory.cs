


namespace FluentMigrator.Runner.Processors.Oracle
{
	using Generators.Oracle;

	public class OracleProcessorFactory : MigrationProcessorFactory
	{
		public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
		{
			var factory = new OracleDbFactory();
			var connection = factory.CreateConnection(connectionString);
			return new OracleProcessor(connection, new OracleGenerator(), announcer, options, factory);
		}
	}
}