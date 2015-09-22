namespace FluentMigrator.Runner.Processors.SqlAnywhere
{
    using System;
    using System.Data.Common;
    using System.Linq;
    using System.Reflection;

    public class SqlAnywhereDbFactory : ReflectionBasedDbFactory
    {
        public SqlAnywhereDbFactory()
            : base("iAnywhere.Data.SQLAnywhere.v3.5", "iAnywhere.Data.SQLAnywhere.SAFactory")
        {
        }

        protected override DbProviderFactory CreateFactory()
        {
            Assembly assembly = AppDomain.CurrentDomain.Load(new System.Reflection.AssemblyName(assemblyName));
            Type factoryType = assembly.GetType(dbProviderFactoryTypeName);
            //MemberInfo instanceMember = factoryType.GetMember("Instance", BindingFlags.Static | BindingFlags.Public).FirstOrDefault();
            return (DbProviderFactory)factoryType.InvokeMember("Instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.GetField, null, null, null);
        }
    }
}