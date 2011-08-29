namespace FluentMigrator.Runner.Processors
{
	using System;
	using System.Data.Common;

	public class DbFactoryBase : IDbFactory
	{
		private readonly DbProviderFactory factory;

		protected DbFactoryBase(DbProviderFactory factory)
		{
			this.factory = factory;
		}

		#region IDbFactory Members

		public DbConnection CreateConnection(string connectionString)
		{
			DbConnection connection = factory.CreateConnection();
			connection.ConnectionString = connectionString;
			return connection;
		}

		public DbCommand CreateCommand(string commandText, DbConnection connection, DbTransaction transaction)
		{
			DbCommand command = connection.CreateCommand();
			command.CommandText = commandText;
			command.Transaction = transaction;
			return command;
		}

		public DbDataAdapter CreateDataAdapter(DbCommand command)
		{
			DbDataAdapter dataAdapter = factory.CreateDataAdapter();
			dataAdapter.SelectCommand = command;
			return dataAdapter;
		}

		public DbCommand CreateCommand(string commandText, DbConnection connection)
		{
			DbCommand command = connection.CreateCommand();
			command.CommandText = commandText;
			return command;
		}

		#endregion
	}
}