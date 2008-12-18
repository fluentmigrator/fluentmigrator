using System;
using System.Data.SqlClient;
using FluentMigrator.Processors;
using FluentMigrator.Runner.Processors;

namespace FluentMigrator.Runner.Processors
{
	public class SqliteMigrationProcessorFactory : IMigrationProcessorFactory
	{
		public IMigrationProcessor Create(string connectionString)
		{
			var connection = new SqlConnection(connectionString);
			return new SqliteMigrationProcessor(connection);
		}
	}
}