using System;
using System.Data.SqlClient;
using FluentMigrator.Runner.Dialects;

namespace FluentMigrator.Runner.Processors
{
	public class SqlServerProcessorFactory : IMigrationProcessorFactory
	{
		public IMigrationProcessor Create(string connectionString)
		{
			var connection = new SqlConnection(connectionString);
			var dialect = new SqlServer2005Dialect();
			return new SqlServerProcessor(connection, dialect);
		}
	}
}