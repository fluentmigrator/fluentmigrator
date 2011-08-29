namespace FluentMigrator.Runner.Processors.SqlServer
{
	using System.Data.SqlClient;

	public class SqlClientDbFactory : DbFactoryBase
	{
		public SqlClientDbFactory() : base(SqlClientFactory.Instance)
		{
		}
	}
}