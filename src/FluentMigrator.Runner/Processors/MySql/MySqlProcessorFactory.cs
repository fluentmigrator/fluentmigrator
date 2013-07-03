using FluentMigrator.Runner.Generators.MySql;
using System;
using System.Data;

namespace FluentMigrator.Runner.Processors.MySql
{
    public class MySqlProcessorFactory : MigrationProcessorFactory
    {
        public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            Func<IDbFactory> factory = () => new MySqlDbFactory();
            Func<IDbConnection> connection = () => factory().CreateConnection(connectionString);
            return new MySqlProcessor(connection, new MySqlGenerator(), announcer, options, factory);
        }
    }
}