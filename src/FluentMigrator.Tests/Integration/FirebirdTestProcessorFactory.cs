using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using FirebirdSql.Data.FirebirdClient;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators.Firebird;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Firebird;

namespace FluentMigrator.Tests.Integration
{
    public class FirebirdTestProcessorFactory : TestProcessorFactory
    {
        private readonly string _connectionString;

        public FirebirdTestProcessorFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IMigrationProcessor MakeProcessor(IDbConnection connection, IAnnouncer announcer)
        {
            var options = FirebirdOptions.AutoCommitBehaviour();
            return new FirebirdProcessor(connection, new FirebirdGenerator(options), announcer, new ProcessorOptions(), new FirebirdDbFactory(), options);
        }

        public IDbConnection MakeConnection()
        {
            var connectionString = new FbConnectionStringBuilder(_connectionString);
            if (string.IsNullOrEmpty(Path.GetDirectoryName(connectionString.Database)))
                connectionString.Database = Path.Combine(Directory.GetCurrentDirectory(), connectionString.Database);

            if (!File.Exists(connectionString.Database))
            {
                FbConnection.CreateDatabase(_connectionString);
            }

            return new FbConnection(_connectionString);
        }

        public bool ProcessorTypeWithin(IEnumerable<Type> candidates)
        {
            return candidates.Any(t => typeof(FirebirdProcessor).IsAssignableFrom(t));
        }

        public void Done()
        {
            FbConnection.DropDatabase(_connectionString);
        }
    }
}
