namespace FluentMigrator.Runner.Processors.DotConnectPostgres
{
    using Generators.Postgres;

    public class DotConnectPostgresProcessorFactory : MigrationProcessorFactory
    {
        public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            var factory = new DotConnectPostgresDbFactory();
            var connection = factory.CreateConnection(connectionString);
            return new DotConnectPostgresProcessor(connection, new PostgresGenerator(), announcer, options, factory);
        }
    }
}