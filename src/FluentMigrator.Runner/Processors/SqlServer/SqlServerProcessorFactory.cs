using System.Data;
using System.Data.SqlClient;
using FluentMigrator.Runner.Generators;

namespace FluentMigrator.Runner.Processors.SqlServer
{
	public class SqlServerProcessorFactory : MigrationProcessorFactory
	{
		public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
		{
			var connection = new SqlConnection(connectionString);
			connection.Open();
			return new SqlServerProcessor(connection, new SqlServer2008Generator(), announcer, options);
		}

		public override IMigrationProcessor Create(IDbConnection connection, IAnnouncer announcer, IMigrationProcessorOptions options)
		{
			return new SqlServerProcessor((SqlConnection)connection, new SqlServer2008Generator(), announcer, options);
		}

		public override bool IsForProvider(string provider)
		{
			return provider.ToLower().Contains("sqlclient");
		}
	}
}