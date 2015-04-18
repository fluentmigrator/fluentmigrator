namespace FluentMigrator.Runner.Processors.SQLite
{
    public class SQLiteDbFactory : ReflectionBasedDbFactory
    {
        public SQLiteDbFactory()
            : base("System.Data.SQLite", "System.Data.SQLite.SQLiteFactory")
        {
        }
    }
}