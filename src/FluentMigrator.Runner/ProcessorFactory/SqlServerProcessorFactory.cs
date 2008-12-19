using System.Data.SqlClient;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Processors;

namespace FluentMigrator.Runner.Processors
{
	public class SqlServerProcessorFactory : IMigrationProcessorFactory
	{
		public IMigrationProcessor Create(string connectionString)
		{
			var connection = new SqlConnection(connectionString);
			return new SqlServerProcessor(connection, new SqlServerGenerator());
		}
	}
}