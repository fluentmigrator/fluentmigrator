using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators.SQLite;
using FluentMigrator.Runner.Processors.SQLite;

namespace FluentMigrator.Tests.Integration.Processors.SQLite
{
    public class SQLiteTestProcessorFactory : TestProcessorFactory
    {
        private readonly string _connectionString;
        private SQLiteDbFactory _factory;

        public SQLiteTestProcessorFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Done()
        {
        }

        public IDbConnection MakeConnection()
        {
            _factory = new SQLiteDbFactory();
            return _factory.CreateConnection(_connectionString);
        }

        public IMigrationProcessor MakeProcessor(IDbConnection connection, IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            return new SQLiteProcessor(connection, new SQLiteGenerator(), announcer, options, _factory);
        }

        public bool ProcessorTypeWithin(IEnumerable<Type> candidates)
        {
            return candidates.Any(t => typeof(SQLiteProcessor).IsAssignableFrom(t));
        }
    }
}
