using System;

using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.Runner.Initialization;

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
            var quoterOptions = new OptionsWrapper<QuoterOptions>(new QuoterOptions());
            return new SqlServerProcessor(_dbTypes, connection, new SqlServer2016Generator(new SqlServer2008Quoter(quoterOptions)), announcer, options, factory);
        }

        [Obsolete]
        public override bool IsForProvider(string provider)
        {
            return provider.ToLower().Contains("sqlclient");
        }
    }
}
