using System.Data;
using System.Data.SqlServerCe;
using System.IO;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.Runner.Processors.SqlServer;

namespace FluentMigrator.Tests.Integration.Processors.SqlServerCe
{
    public class SqlServerCeTestFactory : AbstractTestProcessorFactoryOf<SqlServerCeProcessor>
    {
        private SqlCeConnectionStringBuilder _connectionString;

        public SqlServerCeTestFactory(string connectionString)
        {
            _connectionString = new SqlCeConnectionStringBuilder(connectionString);
        }

        public override IDbConnection MakeConnection()
        {
            if (string.IsNullOrEmpty(Path.GetDirectoryName(_connectionString.DataSource)))
                _connectionString.DataSource = Path.Combine(Directory.GetCurrentDirectory(), _connectionString.DataSource);

            RecreateDatabase();

            return new SqlCeConnection(_connectionString.ToString());
        }

        public override IMigrationProcessor MakeProcessor(IDbConnection connection, IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            return new SqlServerCeProcessor(connection, new SqlServerCeGenerator(), announcer, options, new SqlServerCeDbFactory());
        }

        private void RecreateDatabase()
        {
            if (File.Exists(_connectionString.DataSource))
            {
                File.Delete(_connectionString.DataSource);
            }

            new SqlCeEngine(_connectionString.ToString()).CreateDatabase();
        }
    }
}
