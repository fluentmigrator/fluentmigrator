using System.Linq;
using System.Collections.Generic;
using System;

using FluentMigrator.Runner.Generators.Oracle;

namespace FluentMigrator.Runner.Processors.Oracle
{
    public class OracleManagedProcessorFactory : MigrationProcessorFactory
    {
        public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            var factory = new OracleManagedDbFactory();
            var connection = factory.CreateConnection(connectionString);
            return new OracleProcessor(connection, new OracleGenerator(this.Quoted(options.ProviderSwitches)), announcer, options, factory);
        }

        private bool Quoted(string options)
        {
            return !string.IsNullOrEmpty(options) && 
                options.ToUpper().Contains("QUOTEDIDENTIFIERS=TRUE");
        }
    }
}