using System;
using System.Collections.Generic;
using FluentMigrator.Runner.Processors.DB2;
using FluentMigrator.Runner.Processors.Firebird;
using FluentMigrator.Runner.Processors.Hana;
using FluentMigrator.Runner.Processors.Jet;
using FluentMigrator.Runner.Processors.MySql;
using FluentMigrator.Runner.Processors.Oracle;
using FluentMigrator.Runner.Processors.Postgres;
using FluentMigrator.Runner.Processors.SQLite;
using FluentMigrator.Runner.Processors.SqlServer;

namespace FluentMigrator.Tests.Integration
{
    public class Procesors
    {
        public static IEnumerable<Type> All()
        {
            yield return typeof(Db2Processor);
            yield return typeof(FirebirdProcessor);
            yield return typeof(HanaProcessor);
            yield return typeof(JetProcessor);
            yield return typeof(MySqlProcessor);
            yield return typeof(OracleProcessor);
            yield return typeof(PostgresProcessor);
            yield return typeof(SQLiteProcessor);
            yield return typeof(SqlServerProcessor);

        }
    }
}
