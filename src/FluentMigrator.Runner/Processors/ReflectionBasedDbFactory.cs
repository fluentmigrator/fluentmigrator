namespace FluentMigrator.Runner.Processors
{
    using System;
    using System.Data.Common;
    using System.Linq;

    public class ReflectionBasedDbFactory : DbFactoryBase
    {
        private readonly string assemblyName;
        private readonly string dbProviderFactoryTypeName;

        public ReflectionBasedDbFactory(string assemblyName, string dbProviderFactoryTypeName)
        {
            this.assemblyName = assemblyName;
            this.dbProviderFactoryTypeName = dbProviderFactoryTypeName;
        }

        protected override DbProviderFactory CreateFactory()
        {
#if NETSTANDARD2_0
            var factoryType = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName == assemblyName).First().GetType(dbProviderFactoryTypeName);
            return (DbProviderFactory)Activator.CreateInstance(factoryType);
#else
            return (DbProviderFactory)AppDomain.CurrentDomain.CreateInstanceAndUnwrap(assemblyName, dbProviderFactoryTypeName);
#endif
        }
    }
}