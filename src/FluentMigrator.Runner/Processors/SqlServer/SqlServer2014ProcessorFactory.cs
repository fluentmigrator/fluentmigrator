using FluentMigrator.Runner.Generators.SqlServer;

namespace FluentMigrator.Runner.Processors.SqlServer
{
    public class SqlServer2014ProcessorFactory : MigrationProcessorFactory
    {
        public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            var factory = new SqlServerDbFactory();
            var connection = factory.CreateConnection(connectionString);
            return new SqlServerProcessor(connection, new SqlServer2014Generator(), announcer, options, factory);
        }
    }
}