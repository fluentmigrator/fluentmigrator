namespace FluentMigrator.Runner.Processors.Postgres
{
    using Generators.Postgres;
    using System;
    using System.Data;

    public class PostgresProcessorFactory : MigrationProcessorFactory
    {
        public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            Func<IDbFactory> factory = () => new PostgresDbFactory();
            Func<IDbConnection> connection = () => factory().CreateConnection(connectionString);
            return new PostgresProcessor(connection, new PostgresGenerator(), announcer, options, factory);
        }
    }
}