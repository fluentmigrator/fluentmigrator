using FluentMigrator.Runner.Generators.Oracle;
using FluentMigrator.Runner.Processors.Oracle;
using System;
using System.Data;

namespace FluentMigrator.Runner.Processors.DotConnectOracle
{
    public class DotConnectOracleProcessorFactory : MigrationProcessorFactory
    {
        public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            Func<IDbFactory> factory = () => new DotConnectOracleDbFactory();
            Func<IDbConnection> connection = () => factory().CreateConnection(connectionString);
            return new DotConnectOracleProcessor(connection, new OracleGenerator(), announcer, options, factory);
        }
    }
}