namespace FluentMigrator.Runner.Processors.SqlServer
{
	using System.Data.SqlServerCe;

	public class SqlCeDbFactory : DbFactoryBase
	{
		public SqlCeDbFactory()
			: base(SqlCeProviderFactory.Instance)
		{
		}
	}
}