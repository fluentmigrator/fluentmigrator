using System;

using FluentMigrator.Runner.Generators.SqlServer;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Processors.SqlServer
{
    [Obsolete]
    public class SqlServer2014ProcessorFactory : MigrationProcessorFactory
    {
        private static readonly string[] _dbTypes = {"SqlServer2014", "SqlServer"};

        [Obsolete]
        public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            var factory = new SqlServerDbFactory();
            var connection = factory.CreateConnection(connectionString);
            return new SqlServerProcessor(_dbTypes, connection, new SqlServer2014Generator(), announcer, options, factory);
        }
    }
}
