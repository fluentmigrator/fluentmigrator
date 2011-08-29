namespace FluentMigrator.Runner.Processors
{
	using System;
	using System.Data.Common;

	public interface IDbFactory
	{
		DbConnection CreateConnection(string connectionString);
		DbCommand CreateCommand(string commandText, DbConnection connection, DbTransaction transaction);
		DbDataAdapter CreateDataAdapter(DbCommand command);
		DbCommand CreateCommand(string commandText, DbConnection connection);
	}
}