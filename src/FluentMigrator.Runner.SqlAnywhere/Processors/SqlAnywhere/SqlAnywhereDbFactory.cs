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
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;

using FluentMigrator.Runner.Infrastructure;

namespace FluentMigrator.Runner.Processors.SqlAnywhere
{
    public class SqlAnywhereDbFactory : DbFactoryBase
    {
        private static readonly string[] _assemblyNames =
        {
            "iAnywhere.Data.SQLAnywhere.v4.5",
            "iAnywhere.Data.SQLAnywhere.EF6",
            "iAnywhere.Data.SQLAnywhere.v4.0",
            "iAnywhere.Data.SQLAnywhere.v3.5",
        };

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
            foreach (var name in _assemblyNames)
            {
                if (TryLoadAssemblyFromCurrentDomain(name, out var assembly))
                    return assembly;
            }

            var asmNames = FindAssembliesInGac(_assemblyNames).ToList();
            var asmName = asmNames.OrderByDescending(n => n.Version).First();

            if (asmName == null)
                throw new FileNotFoundException("Unable to load driver for SQLAnywhere. Attempted to load iAnywhere.Data.SQLAnywhere.v4.5.dll, EF6.dll, 4.0.dll or 3.5.dll from current app domain.");

            return Assembly.Load(asmName);
        }

        private static bool TryLoadAssemblyFromCurrentDomain(string assemblyName, out Assembly assembly)
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

        private static IEnumerable<AssemblyName> FindAssembliesInGac(params string[] names)
        {
            foreach (var name in names)
            {
                foreach (var assemblyName in RuntimeHost.FindAssemblies(name))
                {
                    yield return assemblyName;
                }
            }
        }
    }
}
