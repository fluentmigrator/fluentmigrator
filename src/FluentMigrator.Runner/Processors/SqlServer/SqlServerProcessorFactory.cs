using FluentMigrator.Runner.Generators.SqlServer;
using System;
using System.Data;

namespace FluentMigrator.Runner.Processors.SqlServer
{
    public class SqlServerProcessorFactory : MigrationProcessorFactory
    {
        public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            Func<IDbFactory> factory = () => new SqlServerDbFactory();
            Func<IDbConnection> connection = () => factory().CreateConnection(connectionString);
            return new SqlServerProcessor(connection, new SqlServer2008Generator(), announcer, options, factory);
        }

        public override bool IsForProvider(string provider)
        {
            return provider.ToLower().Contains("sqlclient");
        }
    }
}