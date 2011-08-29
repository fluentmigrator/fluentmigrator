namespace FluentMigrator.Runner.Processors.Oracle
{
	using System.Data.Common;

	public class OracleDbFactory : DbFactoryBase
	{
		public OracleDbFactory()
			: base(DbProviderFactories.GetFactory("Oracle.DataAccess.Client"))
		{
		}
	}
}