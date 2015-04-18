using FluentMigrator.Runner.Generators.SQLite;

namespace FluentMigrator.Runner.Processors.SQLite
{
    public class SQLiteProcessorFactory : MigrationProcessorFactory
    {
        public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            var factory = new SQLiteDbFactory();
            var connection = factory.CreateConnection(connectionString);
            return new SQLiteProcessor(connection, new SQLiteGenerator(), announcer, options, factory);
        }
    }
}