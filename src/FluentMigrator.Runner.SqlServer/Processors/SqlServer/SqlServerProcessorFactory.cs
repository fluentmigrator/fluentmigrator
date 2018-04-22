using System;

using FluentMigrator.Runner.Generators.SqlServer;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Processors.SqlServer
{
    [Obsolete]
    public class SqlServerProcessorFactory : MigrationProcessorFactory
    {
        private static readonly string[] _dbTypes = {"SqlServer"};

        [Obsolete]
        public SqlServerProcessorFactory()
        {
        }

        [Obsolete]
        public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            var factory = new SqlServerDbFactory();
            var connection = factory.CreateConnection(connectionString);
            return new SqlServerProcessor(_dbTypes, connection, new SqlServer2016Generator(), announcer, options, factory);
        }

        [Obsolete]
        public override bool IsForProvider(string provider)
        {
            return provider.ToLower().Contains("sqlclient");
        }
    }
}
