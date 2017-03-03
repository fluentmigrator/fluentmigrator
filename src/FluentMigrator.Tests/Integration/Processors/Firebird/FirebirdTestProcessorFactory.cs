using System.Data;
using System.IO;
using FirebirdSql.Data.FirebirdClient;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators.Firebird;
using FluentMigrator.Runner.Processors.Firebird;

namespace FluentMigrator.Tests.Integration.Processors.Firebird
{
    public class FirebirdTestProcessorFactory : AbstractTestProcessorFactoryOf<FirebirdProcessor>
    {
        private readonly FbConnectionStringBuilder _connectionString;

        public FirebirdTestProcessorFactory(string connectionString)
        {
            _connectionString = new FbConnectionStringBuilder(connectionString);
        }

        public override IMigrationProcessor MakeProcessor(IDbConnection connection, IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            var fbOptions = FirebirdOptions.AutoCommitBehaviour();
            return new FirebirdProcessor(connection, new FirebirdGenerator(fbOptions), announcer, options, new FirebirdDbFactory(), fbOptions);
        }

        public override IDbConnection MakeConnection()
        {
            if (string.IsNullOrEmpty(Path.GetDirectoryName(_connectionString.Database)))
                _connectionString.Database = Path.Combine(Directory.GetCurrentDirectory(), _connectionString.Database);

            var usedConnectionString = _connectionString.ToString();
            FbDatabase.CreateDatabase(usedConnectionString);

            return new FbConnection(usedConnectionString);
        }

        public override void Done()
        {
            FbDatabase.DropDatabase(_connectionString.ToString());
        }
    }
}
