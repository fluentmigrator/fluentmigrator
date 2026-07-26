#region License
//
// Copyright (c) 2007-2024, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

#if !NETFRAMEWORK
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace FluentMigrator.Runner.Infrastructure.Hosts
{
#if NET
    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("This type uses AppDomain to load assemblies, which may not be preserved in trimmed applications.")]
#endif
    internal class NetCoreHost : IHostAbstraction
    {
        public string BaseDirectory
            => AppContext.BaseDirectory;

        public object CreateInstance(IServiceProvider serviceProvider, string assemblyName, string typeName)
        {
            var asm = GetAssembly(assemblyName);
            var type = asm.GetType(typeName, true);
            var result = serviceProvider?.GetService(type);
            if (result != null)
                return result;

            return Activator.CreateInstance(type);
        }

        public IEnumerable<Assembly> GetLoadedAssemblies()
            => AppDomain.CurrentDomain.GetAssemblies();

        private static Assembly GetAssembly(string assemblyName)
        {
            if (assemblyName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    return GetAssemblyByFileName(assemblyName);
                }
                catch
                {
                    // Ignore
                }

                assemblyName = assemblyName.Substring(0, assemblyName.Length - 4);
            }
            else
            {
                try
                {
                    return GetAssemblyByFileName(assemblyName + ".dll");
                }
                catch
                {
                    // Ignore
                }
            }

            // Last try
            return Assembly.Load(assemblyName);
        }

        private static Assembly GetAssemblyByFileName(string assemblyName)
        {
            var fileName = Path.Combine(AppContext.BaseDirectory, assemblyName);
            if (File.Exists(fileName))
                return Assembly.LoadFile(fileName);
            return Assembly.LoadFrom(fileName);
        }
    }
}
#endif
