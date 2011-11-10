namespace FluentMigrator.Runner.Processors.Postgres
{
    using System;
    using System.Data.Common;
    using System.Reflection;

    public class PostgresDbFactory : ReflectionBasedDbFactory
    {
        public PostgresDbFactory()
            : base("Npgsql", "Npgsql.NpgsqlFactory")
        {
        }

        protected override DbProviderFactory CreateFactory()
        {
            var assembly = AppDomain.CurrentDomain.Load("Npgsql");
            var type = assembly.GetType("Npgsql.NpgsqlFactory");
            var field = type.GetField("Instance", BindingFlags.Static | BindingFlags.Public);

            if (field == null)
            {
                return base.CreateFactory();
            }

            return (DbProviderFactory)field.GetValue(null);
        }
    }
}