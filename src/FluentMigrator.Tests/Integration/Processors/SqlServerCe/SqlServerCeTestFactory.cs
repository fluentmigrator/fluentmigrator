using System.Data;
using System.Data.SqlServerCe;
using System.IO;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.Runner.Processors.SqlServer;

namespace FluentMigrator.Tests.Integration.Processors.SqlServerCe
{
    public class SqlServerCeTestFactory : AbstractTestProcessorFactoryOf<SqlServerCeProcessor>, TestCleaner
    {
        private SqlCeConnectionStringBuilder _connectionString;

        public SqlServerCeTestFactory(string connectionString)
        {
            _connectionString = new SqlCeConnectionStringBuilder(connectionString);
            if (string.IsNullOrEmpty(Path.GetDirectoryName(_connectionString.DataSource)))
                _connectionString.DataSource = Path.Combine(Directory.GetCurrentDirectory(), _connectionString.DataSource);
        }

        public void CleanUp()
        {
            if (File.Exists(_connectionString.DataSource))
            {
                File.Delete(_connectionString.DataSource);
            }
        }

        public override IMigrationProcessor MakeProcessor(IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            return new SqlServerCeProcessor(MakeConnection(), new SqlServerCeGenerator(), announcer, options, new SqlServerCeDbFactory());
        }

        public override string ConnectionString
        {
            get { return _connectionString.ToString(); }
        }

        private void RecreateDatabase()
        {
            if (!File.Exists(_connectionString.DataSource))
                new SqlCeEngine(_connectionString.ToString()).CreateDatabase();
        }

        private IDbConnection MakeConnection()
        {
            RecreateDatabase();

            return new SqlCeConnection(_connectionString.ToString());
        }
    }
}
