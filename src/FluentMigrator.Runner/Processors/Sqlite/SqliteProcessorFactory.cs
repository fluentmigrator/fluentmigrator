namespace FluentMigrator.Runner.Processors.Sqlite
{
    using Generators.SQLite;

    public class SqliteProcessorFactory : MigrationProcessorFactory
    {
        public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            var factory = new SqliteDbFactory();
            var connection = factory.CreateConnection(connectionString);
            return new SqliteProcessor(connection, new SqliteGenerator(), announcer, options, factory);
        }
    }
}