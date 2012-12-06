using FluentMigrator.Runner.Generators.Oracle;
using FluentMigrator.Runner.Processors.Oracle;

namespace FluentMigrator.Runner.Processors.DotConnectOracle
{
    public class DotConnectOracleProcessorFactory : MigrationProcessorFactory
    {
        public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            var factory = new DotConnectOracleDbFactory();
            var connection = factory.CreateConnection(connectionString);
            return new DotConnectOracleProcessor(connection, new OracleGenerator(), announcer, options, factory);
        }
    }
}