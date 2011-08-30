namespace FluentMigrator.Runner.Processors.SqlServer
{
	using System.Data.SqlClient;

	public class SqlServerDbFactory : DbFactoryBase
	{
		public SqlServerDbFactory() : base(SqlClientFactory.Instance)
		{
		}
	}
}