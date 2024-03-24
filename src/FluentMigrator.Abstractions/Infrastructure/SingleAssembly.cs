#region License
// Copyright (c) 2018, Fluent Migrator Project
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
using System.Linq;
using System.Reflection;

namespace FluentMigrator.Infrastructure
{
    /// <summary>
    /// A simple wrapper which is equivalent to a collection with a single Assembly
    /// </summary>
    [Obsolete]
    public class SingleAssembly : IAssemblyCollection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SingleAssembly"/> class.
        /// </summary>
        /// <param name="assembly">The assembly</param>
        public SingleAssembly(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));
            Assemblies = new[] { assembly };
        }

        /// <inheritdoc />
        public Type[] GetExportedTypes()
        {
            try
            {
                return Assemblies[0].GetExportedTypes();
            }
            catch
            {
                // Ignore assemblies that couldn't be loaded
                return Type.EmptyTypes;
            }
        }

        /// <inheritdoc />
        public Assembly[] Assemblies { get; }

        /// <inheritdoc />
        public ManifestResourceNameWithAssembly[] GetManifestResourceNames()
        {
            return Assemblies[0].GetManifestResourceNames().Select(name => new ManifestResourceNameWithAssembly(name, Assemblies[0])).ToArray();
        }
    }
}
