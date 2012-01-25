namespace FluentMigrator.Runner.Processors.Sqlite
{
    using Generators.SQLite;

    public class SqliteProcessorFactory : MigrationProcessorFactory
    {
        public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            var factory = new SqliteDbFactory();
            var connection = factory.CreateConnection(connectionString);
            var generator = this.GetGenerator<SqliteGenerator>(options);
            return new SqliteProcessor(connection, generator, announcer, options, factory);
        }
    }
}