namespace FluentMigrator.Runner.Processors.SqlServer
{
	using System.Data.SqlServerCe;

	public class SqlServerCeDbFactory : DbFactoryBase
	{
		public SqlServerCeDbFactory()
			: base(SqlCeProviderFactory.Instance)
		{
		}
	}
}