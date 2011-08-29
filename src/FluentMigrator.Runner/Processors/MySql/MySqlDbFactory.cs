namespace FluentMigrator.Runner.Processors.MySql
{
	using global::MySql.Data.MySqlClient;

	public class MySqlDbFactory : DbFactoryBase
	{
		public MySqlDbFactory() : base(MySqlClientFactory.Instance)
		{
		}
	}
}