using System.Data;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators.SQLite;
using FluentMigrator.Runner.Processors.SQLite;

namespace FluentMigrator.Tests.Integration.Processors.SQLite
{
    public class SQLiteTestProcessorFactory : AbstractTestProcessorFactoryOf<SQLiteProcessor>
    {
        private readonly string _connectionString;
        private SQLiteDbFactory _factory;

        public SQLiteTestProcessorFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public override void Done()
        {
        }

        public override IDbConnection MakeConnection()
        {
            _factory = new SQLiteDbFactory();
            return _factory.CreateConnection(_connectionString);
        }

        public override IMigrationProcessor MakeProcessor(IDbConnection connection, IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            return new SQLiteProcessor(connection, new SQLiteGenerator(), announcer, options, _factory);
        }
    }
}
