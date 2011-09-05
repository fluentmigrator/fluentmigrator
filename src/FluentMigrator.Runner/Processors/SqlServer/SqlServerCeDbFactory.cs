namespace FluentMigrator.Runner.Processors.SqlServer
{
    public class SqlServerCeDbFactory : ReflectionBasedDbFactory
    {
        public SqlServerCeDbFactory()
            : base("System.Data.SqlServerCe", "System.Data.SqlServerCe.SqlCeProviderFactory")
        {
        }
    }
}