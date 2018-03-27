using System;

namespace FluentMigrator.Runner.Processors.SQLite
{
    public class SQLiteDbFactory : ReflectionBasedDbFactory
    {
        public SQLiteDbFactory()
            : base(DefaultAssemblyName, DefaultFactoryClassName)
        {
        }

        private static string DefaultAssemblyName
        {
            get
            {
                if (Environment.OSVersion.Platform == PlatformID.Unix)
                    return "Mono.Data.Sqlite, Version=4.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756";
                return "System.Data.SQLite";
            }
        }

        private static string DefaultFactoryClassName
        {
            get
            {
                if (Environment.OSVersion.Platform == PlatformID.Unix)
                    return "Mono.Data.Sqlite.SqliteFactory";
                return "System.Data.SQLite.SQLiteFactory";
            }
        }
    }
}
