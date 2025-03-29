using System;

namespace FluentMigrator
{
    public static class ProcessorIdConstants
    {
        public const string DB2 = nameof(DB2);
        public const string IbmDb2 = "IBM DB2";
        public const string IbmDb2ISeries = "IBM DB2 iSeries";
        public const string Db2ISeries = "DB2 iSeries";
        public const string Firebird = nameof(Firebird);
        public const string Hana = nameof(Hana);
        public const string Jet = nameof(Jet);
        public const string MariaDB = nameof(MariaDB);
        public const string MySql = nameof(MySql);
        public const string MySql4 = nameof(MySql4);
        public const string MySql_4 = "MySQL 4";
        public const string MySql5 = nameof(MySql5);
        public const string MySql8 = nameof(MySql8);
        public const string Oracle = nameof(Oracle);
        public const string OracleManaged = nameof(OracleManaged);
        public const string Oracle12c = nameof(Oracle12c);
        public const string Postgres = nameof(Postgres);
        public const string PostgreSQL = nameof(PostgreSQL);
        public const string Postgres92 = nameof(Postgres92);
        public const string PostgreSQL92 = nameof(PostgreSQL92);
        public const string PostgreSQL10_0 = nameof(PostgreSQL10_0);
        public const string PostgreSQL11_0 = nameof(PostgreSQL11_0);
        public const string PostgreSQL15_0 = nameof(PostgreSQL15_0);
        public const string Redshift = nameof(Redshift);
        public const string Snowflake = nameof(Snowflake);
        public const string SQLite = nameof(SQLite);
        public const string SqlServer = nameof(SqlServer);
        public const string SqlServer2000 = nameof(SqlServer2000);
        public const string SqlServer2005 = nameof(SqlServer2005);
        public const string SqlServer2008 = nameof(SqlServer2008);
        public const string SqlServer2012 = nameof(SqlServer2012);
        public const string SqlServer2014 = nameof(SqlServer2014);
        public const string SqlServer2016 = nameof(SqlServer2016);
    }

    [Obsolete("This has been renamed ProcessorIdConstants. Upcoming FluentMigrator 8.0 will remove this old class name.")]
    public static class ProcessorId
    {
        public const string DB2 = nameof(DB2);
        public const string Db2ISeries = nameof(Db2ISeries);
        public const string Firebird = nameof(Firebird);
        public const string Hana = nameof(Hana);
        public const string Jet = nameof(Jet);
        public const string MariaDB = nameof(MariaDB);
        public const string MySql = nameof(MySql);
        public const string MySql4 = nameof(MySql4);
        public const string MySql5 = nameof(MySql5);
        public const string MySql8 = nameof(MySql8);
        public const string Oracle = nameof(Oracle);
        public const string OracleManaged = nameof(OracleManaged);
        public const string Oracle12c = nameof(Oracle12c);
        public const string Postgres = nameof(Postgres);
        public const string PostgreSQL = nameof(PostgreSQL);
        public const string PostgreSQL10_0 = nameof(PostgreSQL10_0);
        public const string PostgreSQL11_0 = nameof(PostgreSQL11_0);
        public const string PostgreSQL15_0 = nameof(PostgreSQL15_0);
        public const string Redshift = nameof(Redshift);
        public const string Snowflake = nameof(Snowflake);
        public const string SQLite = nameof(SQLite);
        public const string SqlServer = nameof(SqlServer);
        public const string SqlServer2000 = nameof(SqlServer2000);
        public const string SqlServer2005 = nameof(SqlServer2005);
        public const string SqlServer2008 = nameof(SqlServer2008);
        public const string SqlServer2012 = nameof(SqlServer2012);
        public const string SqlServer2014 = nameof(SqlServer2014);
        public const string SqlServer2016 = nameof(SqlServer2016);
    }
}
