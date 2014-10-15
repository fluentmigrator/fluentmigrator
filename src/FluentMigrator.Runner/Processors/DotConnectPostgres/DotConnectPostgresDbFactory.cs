using System;

namespace FluentMigrator.Runner.Processors.DotConnectPostgres
{
    public class DotConnectPostgresDbFactory : ReflectionBasedDbFactory
    {
        public DotConnectPostgresDbFactory()
            : base("Devart.Data.PostgreSql", "Devart.Data.PostgreSql.PgSqlProviderFactory")
        {
        }
    }
}