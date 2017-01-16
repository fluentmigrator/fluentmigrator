namespace FluentMigrator.Runner.Processors
{
    using System;
    using System.Data.Common;
    using System.Reflection;
#if COREFX
    using System.Linq;
    using System.Runtime.Loader;
#endif

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
#if COREFX
            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName(assemblyName));
            var type = assembly.GetType(dbProviderFactoryTypeName);

            var ctors = type.GetConstructors();
            if (!ctors.Any(x => x.GetParameters().Length == 0))
            {
                var instanceField = type.GetField("Instance", BindingFlags.Static | BindingFlags.Public);
                if (instanceField != null)
                    return (DbProviderFactory)instanceField.GetValue(null);

                var instanceProperty = type.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public);
                if (instanceProperty != null)
                    return (DbProviderFactory)instanceProperty.GetValue(null);
            }

            return (DbProviderFactory)Activator.CreateInstance(type);
#else
            return (DbProviderFactory)AppDomain.CurrentDomain.CreateInstanceAndUnwrap(assemblyName, dbProviderFactoryTypeName);
#endif
        }
    }
}