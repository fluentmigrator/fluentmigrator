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
using System.Text;

using FluentMigrator.Infrastructure;

using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Initialization
{
    /// <summary>
    /// Provides access to delay-loaded assemblies
    /// </summary>
#if NET
    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("This type gets the exported types from assemblies, which may not be preserved in trimmed applications.")]
#endif
    public class AssemblySource : IAssemblySource
    {
        private readonly Lazy<IReadOnlyCollection<Assembly>> _assemblies;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblySource"/> class.
        /// </summary>
        /// <param name="options">The options</param>
        /// <param name="loadEngines">The assembly load engines</param>
        /// <param name="sourceItems">The additional source items</param>
        public AssemblySource(IOptions<AssemblySourceOptions> options, IEnumerable<IAssemblyLoadEngine> loadEngines, IEnumerable<IAssemblySourceItem> sourceItems)
        {
            _assemblies = new Lazy<IReadOnlyCollection<Assembly>>(
                () => LoadAssemblies(options.Value, loadEngines.ToList())
                    .Union(sourceItems.SelectMany(i => i.Assemblies)).ToList());
        }

        /// <inheritdoc />
        public IReadOnlyCollection<Assembly> Assemblies => _assemblies.Value;

        private static IReadOnlyCollection<Assembly> LoadAssemblies(AssemblySourceOptions options, IReadOnlyCollection<IAssemblyLoadEngine> loadEngines)
        {
            var assemblyNames = options.AssemblyNames;
            if (assemblyNames == null || assemblyNames.Length == 0)
            {
                // Replace with current assemblies loaded in AppDomain
                return Array.Empty<Assembly>();
            }

            var result = new List<Assembly>();
            foreach (var assemblyName in assemblyNames)
            {
                var exceptions = new List<Exception>();
                var added = false;
                foreach (var loadEngine in loadEngines)
                {
                    if (loadEngine.TryLoad(assemblyName, exceptions, out var assembly))
                    {
                        result.Add(assembly);
                        added = true;
                        break;
                    }
                }

                if (!added)
                {
                    var errorMessage = new StringBuilder($"Failed to load assembly with name {assemblyName}:").AppendLine();
                    foreach (var exception in exceptions)
                    {
                        errorMessage.AppendLine(exception.Message);
                    }

                    throw new AggregateException(errorMessage.ToString(), exceptions);
                }
            }

            return result;
        }
    }
}
