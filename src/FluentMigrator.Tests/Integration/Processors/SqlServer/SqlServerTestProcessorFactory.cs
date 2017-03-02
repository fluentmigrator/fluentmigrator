using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Processors.SqlServer;

namespace FluentMigrator.Tests.Integration.Processors.SqlServer
{
    public class SqlServerTestProcessorFactory : TestProcessorFactory
    {
        private readonly SqlConnectionStringBuilder _connectionString;
        private readonly IMigrationGenerator _generator;

        public SqlServerTestProcessorFactory(string connectionString, IMigrationGenerator generator)
        {
            _connectionString = new SqlConnectionStringBuilder(connectionString);
            _generator = generator;
        }

        public void Done()
        {
        }

        public IDbConnection MakeConnection()
        {
            return new SqlConnection(_connectionString.ToString());
        }

        public IMigrationProcessor MakeProcessor(IDbConnection connection, IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            return new SqlServerProcessor(connection, _generator, announcer, options, new SqlServerDbFactory()); ;
        }

        public bool ProcessorTypeWithin(IEnumerable<Type> candidates)
        {
            return candidates.Any(t => typeof(SqlServerProcessor).IsAssignableFrom(t));
        }
    }
}
