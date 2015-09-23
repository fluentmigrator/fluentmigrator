namespace FluentMigrator.Runner.Processors.SqlAnywhere
{
    using System;
    using System.Data.Common;
    using System.IO;
    using System.Reflection;

    public class SqlAnywhereDbFactory : DbFactoryBase
    {
        public SqlAnywhereDbFactory()
            : base()
        {
        }

        protected override DbProviderFactory CreateFactory()
        {
            Assembly assembly = this.GetLatestSqlAnywhereAssembly();
            Type factoryType = assembly.GetType("iAnywhere.Data.SQLAnywhere.SAFactory");

            if (factoryType == null)
                throw new Exception(string.Format("Type iAnywhere.Data.SQLAnywhere.SAFactory was not found in assembly {0}.", assembly.Location));

            return (DbProviderFactory)factoryType.InvokeMember("Instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.GetField, null, null, null);
        }

        private Assembly GetLatestSqlAnywhereAssembly()
        {
            Assembly assembly = null;

            if (!this.TryLoadAssemblyFromCurrentDomain("iAnywhere.Data.SQLAnywhere.v4.5", out assembly))
                if (!this.TryLoadAssemblyFromCurrentDomain("iAnywhere.Data.SQLAnywhere.v4.0", out assembly))
                    if (!this.TryLoadAssemblyFromCurrentDomain("iAnywhere.Data.SQLAnywhere.v3.5", out assembly))
                        throw new FileNotFoundException("Unable to load driver for SQLAnywhere. Attempted to load iAnywhere.Data.SQLAnywhere.v4.5.dll, 4.0.dll or 3.5.dll from current app domain.");

            return assembly;
        }

        private bool TryLoadAssemblyFromCurrentDomain(string assemblyName, out Assembly assembly)
        {
            try
            {
                assembly = AppDomain.CurrentDomain.Load(new System.Reflection.AssemblyName(assemblyName));

                if (assembly == null)
                    return false;
                else
                    return true;
            }
            catch
            {
                assembly = null;
                return false;
            }
        }
    }
}