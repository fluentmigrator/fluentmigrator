using System;
using System.Data.Common;
using System.Reflection;

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
            var assembly = AppDomain.CurrentDomain.Load("FirebirdSql.Data.FirebirdClient");
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