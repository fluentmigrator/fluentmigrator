using System;
using System.Data.Common;
using System.Linq;

namespace FluentMigrator.Runner.Processors
{
    public class ReflectionBasedDbFactory : DbFactoryBase
    {
        private readonly string _assemblyName;
        private readonly string _dbProviderFactoryTypeName;

        public ReflectionBasedDbFactory(string assemblyName, string dbProviderFactoryTypeName)
        {
            this._assemblyName = assemblyName;
            this._dbProviderFactoryTypeName = dbProviderFactoryTypeName;
        }

        private Type FindTypeFromLoadedAssemblies()
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name.ToLowerInvariant() == _assemblyName.ToLowerInvariant());

            if (assembly == null)
            {
                return null;
            }

            return assembly.GetType(_dbProviderFactoryTypeName, false, true);
        }

        protected override DbProviderFactory CreateFactory()
        {
            // See if the driver is already loaded
            var dbFactoryType = FindTypeFromLoadedAssemblies();

            if (dbFactoryType == null)
            {
                // Find and Execute IDbFactoryLoader.LoadDbFactory() implementations
                var dbFactoryLoaders = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes().Where(t => t.GetInterfaces().Any(iface => iface == typeof(IDbFactoryLoader))));

                foreach (var dbFactoryLoader in dbFactoryLoaders)
                {
                    var factoryLoader = Activator.CreateInstance(dbFactoryLoader) as IDbFactoryLoader;
                    if (factoryLoader != null)
                    {
                        factoryLoader.LoadDbFactory();
                    }
                }

                dbFactoryType = FindTypeFromLoadedAssemblies();
            }

            if (dbFactoryType != null)
            {
                return (DbProviderFactory)Activator.CreateInstance(dbFactoryType);
            }

            return (DbProviderFactory)AppDomain.CurrentDomain.CreateInstanceAndUnwrap(_assemblyName, _dbProviderFactoryTypeName);
        }
    }
}