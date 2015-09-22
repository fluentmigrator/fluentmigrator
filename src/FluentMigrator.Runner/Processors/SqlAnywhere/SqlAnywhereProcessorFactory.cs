using FluentMigrator.Runner.Generators.SqlAnywhere;

namespace FluentMigrator.Runner.Processors.SqlAnywhere
{
    public class SqlAnywhereProcessorFactory : MigrationProcessorFactory
    {
        public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            var factory = new SqlAnywhereDbFactory();
            var connection = factory.CreateConnection(connectionString);
            return new SqlAnywhereProcessor(connection, new SqlAnywhere16Generator(), announcer, options, factory);
        }

        public override bool IsForProvider(string provider)
        {
            return provider.ToLower().Contains("ianywhere");
        }
    }
}