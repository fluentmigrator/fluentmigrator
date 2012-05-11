using FluentMigrator.Runner.Generators.MySql;

namespace FluentMigrator.Runner.Processors.MySql
{
    public class MySqlProcessorFactory : MigrationProcessorFactory
    {
        public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            var factory = new MySqlDbFactory();
            var connection = factory.CreateConnection(connectionString);
            return new MySqlProcessor(connection, new MySqlGenerator(), announcer, options, factory);
        }
    }
}