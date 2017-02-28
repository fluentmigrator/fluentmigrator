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

namespace FluentMigrator.Tests.Integration.Processors.Firebird
{
    public class FirebirdTestProcessorFactory : TestProcessorFactory
    {
        private readonly FbConnectionStringBuilder _connectionString;

        public FirebirdTestProcessorFactory(string connectionString)
        {
            _connectionString = new FbConnectionStringBuilder(connectionString);
        }

        public IMigrationProcessor MakeProcessor(IDbConnection connection, IAnnouncer announcer)
        {
            var options = FirebirdOptions.AutoCommitBehaviour();
            return new FirebirdProcessor(connection, new FirebirdGenerator(options), announcer, new ProcessorOptions(), new FirebirdDbFactory(), options);
        }

        public IDbConnection MakeConnection()
        {
            if (string.IsNullOrEmpty(Path.GetDirectoryName(_connectionString.Database)))
                _connectionString.Database = Path.Combine(Directory.GetCurrentDirectory(), _connectionString.Database);

            var usedConnectionString = _connectionString.ToString();
            FbDatabase.CreateDatabase(usedConnectionString);

            return new FbConnection(usedConnectionString);
        }

        public bool ProcessorTypeWithin(IEnumerable<Type> candidates)
        {
            return candidates.Any(t => typeof(FirebirdProcessor).IsAssignableFrom(t));
        }

        public void Done()
        {
            FbDatabase.DropDatabase(_connectionString.ToString());
        }
    }
}
