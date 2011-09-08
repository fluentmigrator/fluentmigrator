namespace FluentMigrator.Runner.Processors
{
    using System.Data.Common;

	public abstract class DbFactoryBase : IDbFactory
	{
		private volatile DbProviderFactory factory;
	    private readonly object @lock = new object();

		protected DbFactoryBase(DbProviderFactory factory)
		{
			this.factory = factory;
		}

	    protected DbFactoryBase()
	    {
	    }

	    private DbProviderFactory Factory
	    {
	        get
	        {
	            if (factory == null)
	            {
	                lock (@lock)
	                {
	                    if (factory == null)
	                    {
	                        factory = CreateFactory();
	                    }
	                }
	            }
	            return factory;
	        }
	    }

	    protected abstract DbProviderFactory CreateFactory();

	    #region IDbFactory Members

		public DbConnection CreateConnection(string connectionString)
		{
			DbConnection connection = Factory.CreateConnection();
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
			DbDataAdapter dataAdapter = Factory.CreateDataAdapter();
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