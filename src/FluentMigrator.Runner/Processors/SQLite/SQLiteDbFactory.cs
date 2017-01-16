namespace FluentMigrator.Runner.Processors.SQLite
{
    public class SQLiteDbFactory : ReflectionBasedDbFactory
    {
        public SQLiteDbFactory()
#if COREFX
            : base("Microsoft.Data.Sqlite", "Microsoft.Data.Sqlite.SqliteFactory")
#else
            : base("System.Data.SQLite", "System.Data.SQLite.SQLiteFactory")
#endif
        {
        }
    }
}