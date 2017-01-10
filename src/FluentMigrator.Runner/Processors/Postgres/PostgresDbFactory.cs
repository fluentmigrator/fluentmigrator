using System;
using System.Data.Common;
using System.Reflection;
#if COREFX
using System.Runtime.Loader;
#endif

namespace FluentMigrator.Runner.Processors.Postgres
{
    public class PostgresDbFactory : ReflectionBasedDbFactory
    {
        public PostgresDbFactory()
            : base("Npgsql", "Npgsql.NpgsqlFactory")
        {
        }

        protected override DbProviderFactory CreateFactory()
        {
#if COREFX
            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName("Npgsql"));
#else
            var assembly = AppDomain.CurrentDomain.Load("Npgsql");
#endif
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