using FluentMigrator.Runner.Generators.Hana;

namespace FluentMigrator.Runner.Processors.Hana
{
    public class HanaProcessorFactory : MigrationProcessorFactory
    {
        public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            var factory = new HanaDbFactory();
            
            var connection = factory.CreateConnection(connectionString);

            return new HanaProcessor(connection, new HanaGenerator(), announcer, options, factory);
        }
    }
}