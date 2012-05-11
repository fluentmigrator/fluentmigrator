using FluentMigrator.Runner.Generators.SqlServer;

namespace FluentMigrator.Runner.Processors.SqlServer
{
    public class SqlServerProcessorFactory : MigrationProcessorFactory
    {
        public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            var factory = new SqlServerDbFactory();
            var connection = factory.CreateConnection(connectionString);
            return new SqlServerProcessor(connection, new SqlServer2008Generator(), announcer, options, factory);
        }

        public override bool IsForProvider(string provider)
        {
            return provider.ToLower().Contains("sqlclient");
        }
    }
}