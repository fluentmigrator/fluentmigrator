using System.Data;
using System.Data.SqlClient;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Processors.SqlServer;

namespace FluentMigrator.Tests.Integration.Processors.SqlServer
{
    public class SqlServerTestProcessorFactory : AbstractTestProcessorFactoryOf<SqlServerProcessor>
    {
        private readonly SqlConnectionStringBuilder _connectionString;
        private readonly IMigrationGenerator _generator;

        public SqlServerTestProcessorFactory(string connectionString, IMigrationGenerator generator)
        {
            _connectionString = new SqlConnectionStringBuilder(connectionString);
            _generator = generator;
        }

        public override IMigrationProcessor MakeProcessor(IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            return new SqlServerProcessor(MakeConnection(), _generator, announcer, options, new SqlServerDbFactory()); ;
        }

        public override string ConnectionString
        {
            get { return _connectionString.ToString(); }
        }

        private IDbConnection MakeConnection()
        {
            return new SqlConnection(_connectionString.ToString());
        }
    }
}
