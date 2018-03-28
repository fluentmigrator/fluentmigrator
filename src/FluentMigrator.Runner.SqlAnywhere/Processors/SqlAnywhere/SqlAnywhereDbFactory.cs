#region License
// Copyright (c) 2007-2018, FluentMigrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Data.Common;
using System.IO;
using System.Reflection;

namespace FluentMigrator.Runner.Processors.SqlAnywhere
{
    public class SqlAnywhereDbFactory : DbFactoryBase
    {
        protected override DbProviderFactory CreateFactory()
        {
            Assembly assembly = GetLatestSqlAnywhereAssembly();
            Type factoryType = assembly.GetType("iAnywhere.Data.SQLAnywhere.SAFactory");

            if (factoryType == null)
                throw new Exception($"Type iAnywhere.Data.SQLAnywhere.SAFactory was not found in assembly {assembly.Location}.");

            return (DbProviderFactory)factoryType.InvokeMember("Instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.GetField, null, null, null);
        }

        private Assembly GetLatestSqlAnywhereAssembly()
        {
            if (!TryLoadAssemblyFromCurrentDomain("iAnywhere.Data.SQLAnywhere.v4.5", out var assembly))
                if (!TryLoadAssemblyFromCurrentDomain("iAnywhere.Data.SQLAnywhere.EF6", out assembly))
                    if (!TryLoadAssemblyFromCurrentDomain("iAnywhere.Data.SQLAnywhere.v4.0", out assembly))
                        if (!TryLoadAssemblyFromCurrentDomain("iAnywhere.Data.SQLAnywhere.v3.5", out assembly))
                            throw new FileNotFoundException("Unable to load driver for SQLAnywhere. Attempted to load iAnywhere.Data.SQLAnywhere.v4.5.dll, EF6.dll, 4.0.dll or 3.5.dll from current app domain.");

            return assembly;
        }

        private bool TryLoadAssemblyFromCurrentDomain(string assemblyName, out Assembly assembly)
        {
            try
            {
                assembly = AppDomain.CurrentDomain.Load(new AssemblyName(assemblyName));
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
