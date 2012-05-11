namespace FluentMigrator.Runner.Processors.SqlServer
{
    using System.Data.Common;
    using System.Data.SqlClient;

    public class SqlServerDbFactory : DbFactoryBase
    {
        public SqlServerDbFactory()
            : base(SqlClientFactory.Instance)
        {
        }

        protected override DbProviderFactory CreateFactory()
        {
            return SqlClientFactory.Instance;
        }
    }
}