namespace FluentMigrator.Runner.Processors.Postgres
{
	using Generators.Postgres;
	using Npgsql;

	public class PostgresProcessorFactory : MigrationProcessorFactory
	{
		public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
		{
			var factory = new PostgresDbFactory();
			var connection = factory.CreateConnection(connectionString);
			return new PostgresProcessor(connection, new PostgresGenerator(), announcer, options, factory);
		}
	}

	public class PostgresDbFactory : DbFactoryBase
	{
		public PostgresDbFactory()
			: base(NpgsqlFactory.Instance)
		{
		}
	}
}