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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FluentMigrator.Infrastructure
{
    /// <summary>
    /// An assembly collection for multiple assemblies
    /// </summary>
    [Obsolete]
    public class AssemblyCollection : IAssemblyCollection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyCollection"/> class.
        /// </summary>
        /// <param name="assemblies">The assemblies for the collection</param>
        public AssemblyCollection(IEnumerable<Assembly> assemblies)
        {
            if (assemblies == null)
                throw new ArgumentNullException(nameof(assemblies));
            Assemblies = assemblies.ToArray();
        }

        /// <inheritdoc />
        public Type[] GetExportedTypes()
        {
            var result = new List<Type>();

            foreach (var assembly in Assemblies)
            {
                try
                {
                    result.AddRange(assembly.GetExportedTypes());
                }
                catch
                {
                    // Ignore assemblies that couldn't be loaded
                }
            }

            return result.ToArray();
        }

        /// <inheritdoc />
        public Assembly[] Assemblies { get; }

        /// <inheritdoc />
        public ManifestResourceNameWithAssembly[] GetManifestResourceNames()
        {
            return Assemblies.SelectMany(a => a.GetManifestResourceNames().Select(name =>
                new ManifestResourceNameWithAssembly(name, a))).ToArray();
        }
    }
}
