using System;
using System.Data;
using System.Data.Common;

namespace FluentMigrator.Runner.Processors.Oracle
{
	public class OracleFactory
	{
		public static DbProviderFactory GetProvider()
		{
		   try
		   {
            // Try the ODP.Net connection first as this should be more recent and is supported by Oracle
            return DbProviderFactories.GetFactory("Oracle.DataAccess.Client");
		   }
		   catch (Exception ex)
		   {
            // Ok its failed two cases we handle
            // First you have Oracle.DataAccess.Client but it has an incorrect version of OCI installed
            // ... Second the Oracle ODP.Net client is not installed at all
            if (ex.ToString().Contains("The provider is not compatible with the version of Oracle client")
               || ex.ToString().Contains("Unable to find the requested .Net Framework Data Provider"))
            {
               // Fall back to using the Microsoft Oracle provider as this requires less dependancies to be run
               // Note: The Microsoft Oracle provider has been depricated in .Net 4.0
               return DbProviderFactories.GetFactory("System.Data.OracleClient");

               // if this fails, do we need to think about supporting other Oracle providers Post .Net 4.0 [Grant, 20110524]
            }
		      throw;
		   }
         
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
