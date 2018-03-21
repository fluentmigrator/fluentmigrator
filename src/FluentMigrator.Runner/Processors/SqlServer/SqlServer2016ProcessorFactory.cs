using FluentMigrator.Runner.Generators.SqlServer;

namespace FluentMigrator.Runner.Processors.SqlServer
{
    public class SqlServer2016ProcessorFactory : MigrationProcessorFactory
    {
        public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            var factory = new SqlServerDbFactory();
            var connection = factory.CreateConnection(connectionString);
            return new SqlServerProcessor(connection, new SqlServer2016Generator(), announcer, options, factory);
        }
    }
}