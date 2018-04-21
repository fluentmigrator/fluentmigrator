using System;

using FluentMigrator.Runner.Generators.SqlServer;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Processors.SqlServer
{
    public class SqlServer2014ProcessorFactory : MigrationProcessorFactory
    {
        private static readonly string[] _dbTypes = {"SqlServer2014", "SqlServer"};
        private readonly IServiceProvider _serviceProvider;

        [Obsolete]
        public SqlServer2014ProcessorFactory()
            : this(serviceProvider: null)
        {
        }

        public SqlServer2014ProcessorFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        [Obsolete]
        public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            var factory = new SqlServerDbFactory();
            var connection = factory.CreateConnection(connectionString);
            return new SqlServerProcessor(_dbTypes, connection, new SqlServer2014Generator(), announcer, options, factory);
        }

        /// <inheritdoc />
        public override IMigrationProcessor Create()
        {
            if (_serviceProvider == null)
                return null;
            var factory = new SqlServerDbFactory().Factory;
            var options = _serviceProvider.GetRequiredService<IOptions<ProcessorOptions>>();
            var announcer = _serviceProvider.GetRequiredService<IAnnouncer>();
            var generator = new SqlServer2014Generator();
            return new SqlServerProcessor(_dbTypes, factory, generator, announcer, options);
        }
    }
}
