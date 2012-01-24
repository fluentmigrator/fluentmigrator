using System.Data;
using FluentMigrator.Runner.Generators.Oracle;

namespace FluentMigrator.Runner.Processors.Oracle
{
    public class OracleProcessorFactory : MigrationProcessorFactory
    {
        public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            var factory = new OracleDbFactory();
            var connection = factory.CreateConnection(connectionString);
            var generator = this.GetGenerator<OracleGenerator>(options);
            return new OracleProcessor(connection, generator, announcer, options, factory);
        }
    }
}