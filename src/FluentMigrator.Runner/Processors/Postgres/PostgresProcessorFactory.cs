namespace FluentMigrator.Runner.Processors.Postgres
{
    using Generators.Postgres;

    public class PostgresProcessorFactory : MigrationProcessorFactory
    {
        public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            var factory = new PostgresDbFactory();
            var connection = factory.CreateConnection(connectionString);
            var generator = this.GetGenerator<PostgresGenerator>(options);
            return new PostgresProcessor(connection, generator, announcer, options, factory);
        }
    }
}