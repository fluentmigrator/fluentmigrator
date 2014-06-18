namespace FluentMigrator.Runner.Processors.SQLite
{
    public class SqliteDbFactory : ReflectionBasedDbFactory
    {
        public SqliteDbFactory()
            : base("System.Data.SQLite", "System.Data.SQLite.SQLiteFactory")
        {
        }
    }
}