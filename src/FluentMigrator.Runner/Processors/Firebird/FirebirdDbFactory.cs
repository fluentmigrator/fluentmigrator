using System;
using System.Data.Common;
using System.Reflection;
#if COREFX
using System.Runtime.Loader;
#endif

namespace FluentMigrator.Runner.Processors.Firebird
{
    public class FirebirdDbFactory : ReflectionBasedDbFactory
    {
        public FirebirdDbFactory()
            : base("FirebirdSql.Data.FirebirdClient", "FirebirdSql.Data.FirebirdClient.FirebirdClientFactory")
        {
        }
        
        protected override DbProviderFactory CreateFactory()
        {
#if COREFX
            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName("FirebirdSql.Data.FirebirdClient"));
#else
            var assembly = AppDomain.CurrentDomain.Load("FirebirdSql.Data.FirebirdClient");
#endif
            var type = assembly.GetType("FirebirdSql.Data.FirebirdClient.FirebirdClientFactory");
            var field = type.GetField("Instance", BindingFlags.Static | BindingFlags.Public);

            if (field == null)
            {
                return base.CreateFactory();
            }

            return (DbProviderFactory)field.GetValue(null);
        }

    }
}