using System.Data;
using System.Data.SqlClient;
using FluentMigrator.Runner.Generators;

namespace FluentMigrator.Runner.Processors.SqlServer
{
	public class SqlServerProcessorFactory : MigrationProcessorFactory
	{
        public override IMigrationProcessor Create(string connectionString)
		{
			var connection = new SqlConnection(connectionString);
			connection.Open();
			return new SqlServerProcessor(connection, new SqlServerGenerator());
		}

        public override IMigrationProcessor Create(IDbConnection connection)
		{
			return new SqlServerProcessor((SqlConnection)connection, new SqliteGenerator());
		}

        public override bool IsForProvider(string provider)
        {
	        return provider.ToLower().Contains("sqlclient");
	    }
	}
}