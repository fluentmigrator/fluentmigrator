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

using System.IO;
using System.Reflection;

using FluentMigrator.Runner.Infrastructure;

namespace FluentMigrator.Runner.Initialization.AssemblyLoader
{
#if NET
    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("This type uses AppDomain to load assemblies, which may not be preserved in trimmed applications.")]
#endif
    public class AssemblyLoaderFromFile : IAssemblyLoader
    {
        private readonly string _name;

        public AssemblyLoaderFromFile(string name)
        {
            _name = name;
        }

        public Assembly Load()
        {
            string fileName = _name;
            if (!Path.IsPathRooted(fileName))
            {
                fileName = Path.Combine(RuntimeHost.Current.BaseDirectory, _name);
                if (!File.Exists(fileName))
                {
                    fileName = Path.GetFullPath(_name);
                    if (!File.Exists(fileName))
                    {
                        fileName = _name;
                    }
                }
            }

            Assembly assembly = Assembly.LoadFrom(fileName);
            return assembly;
        }
    }
}
