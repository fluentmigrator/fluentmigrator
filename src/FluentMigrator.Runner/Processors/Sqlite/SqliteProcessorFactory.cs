namespace FluentMigrator.Runner.Processors.Sqlite
{
    using Generators.SQLite;
    using System;
    using System.Data;

    public class SqliteProcessorFactory : MigrationProcessorFactory
    {
        public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            Func<IDbFactory> factory = () => new SqliteDbFactory();
            Func<IDbConnection> connection = () => factory().CreateConnection(connectionString);

            return new SqliteProcessor(connection, new SqliteGenerator(), announcer, options, factory);
        }
    }
}