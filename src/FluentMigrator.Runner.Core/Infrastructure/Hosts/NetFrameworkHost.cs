#region License
//
// Copyright (c) 2007-2018, Sean Chambers <schambers80@gmail.com>
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

#if NETFRAMEWORK

using System;
using System.Collections.Generic;
using System.Reflection;

namespace FluentMigrator.Runner.Infrastructure.Hosts
{
    internal class NetFrameworkHost : IHostAbstraction
    {
        public string BaseDirectory
            => AppDomain.CurrentDomain.BaseDirectory;

        public object CreateInstance(IServiceProvider serviceProvider, string assemblyName, string typeName)
        {
            if (serviceProvider != null)
            {
                try
                {
                    var asm = AppDomain.CurrentDomain.Load(new AssemblyName(assemblyName));
                    var type = asm.GetType(typeName, true);
                    var result = serviceProvider.GetService(type);
                    if (result != null)
                        return result;
                }
                catch
                {
                    // Ignore, fall back to legacy method
                }
            }

            return AppDomain.CurrentDomain.CreateInstanceAndUnwrap(assemblyName, typeName);
        }

        public IEnumerable<Assembly> GetLoadedAssemblies()
            => AppDomain.CurrentDomain.GetAssemblies();
    }
}
#endif
