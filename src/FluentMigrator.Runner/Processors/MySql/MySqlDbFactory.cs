namespace FluentMigrator.Runner.Processors.MySql
{
    public class MySqlDbFactory : ReflectionBasedDbFactory
    {
        public MySqlDbFactory()
            : base("MySql.Data", "MySql.Data.MySqlClient.MySqlClientFactory")
        {
        }
    }
}