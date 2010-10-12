using System.Data;
using System.Data.Common;

namespace FluentMigrator.Runner.Processors.Oracle
{
	public class OracleFactory
	{
		public static DbProviderFactory GetProvider()
		{
			return DbProviderFactories.GetFactory("Oracle.DataAccess.Client");
		}

		public static DbConnection GetOpenConnection(string connectionString)
		{
			DbConnection connection = GetProvider().CreateConnection();
			connection.ConnectionString = connectionString;
			connection.Open();
			return connection;
		}

		public static DbCommand GetCommand(IDbConnection connection, string commandText)
		{
			DbCommand command = GetProvider().CreateCommand();
			command.Connection = (DbConnection) connection;
			command.CommandText = commandText;
			return command;
		}

		public static DbDataAdapter GetDataAdapter(DbCommand selectCommand)
		{
			DbDataAdapter adapter = GetProvider().CreateDataAdapter();
			adapter.SelectCommand = selectCommand;
			return adapter;
		}
	}
}
