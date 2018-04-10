namespace FluentMigrator.Runner.Processors.SQLite
{
    public class SQLiteDbFactory : ReflectionBasedDbFactory
    {
        private static readonly TestEntry[] _testEntries =
        {
            new TestEntry("System.Data.SQLite", "System.Data.SQLite.SQLiteFactory"),
            new TestEntry("Mono.Data.Sqlite, Version=4.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756", "Mono.Data.Sqlite.SqliteFactory"),
            new TestEntry("Microsoft.Data.Sqlite", "Microsoft.Data.Sqlite.SqliteFactory"),
        };

        public SQLiteDbFactory()
            : base(_testEntries)
        {
        }
    }
}
