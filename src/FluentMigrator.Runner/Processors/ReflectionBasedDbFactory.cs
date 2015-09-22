namespace FluentMigrator.Runner.Processors
{
    using System;
    using System.Data.Common;

    public class ReflectionBasedDbFactory : DbFactoryBase
    {
        protected readonly string assemblyName;
        protected readonly string dbProviderFactoryTypeName;

        public ReflectionBasedDbFactory(string assemblyName, string dbProviderFactoryTypeName)
        {
            this.assemblyName = assemblyName;
            this.dbProviderFactoryTypeName = dbProviderFactoryTypeName;
        }

        protected override DbProviderFactory CreateFactory()
        {
            return (DbProviderFactory)AppDomain.CurrentDomain.CreateInstanceAndUnwrap(assemblyName, dbProviderFactoryTypeName);
        }
    }
}