#region License
// Copyright (c) 2018, FluentMigrator Project
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

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using FluentMigrator.Infrastructure;

using JetBrains.Annotations;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// The default implementation of the <see cref="IEmbeddedResourceProvider"/> interface
    /// </summary>
    public class DefaultEmbeddedResourceProvider : IEmbeddedResourceProvider
    {
        [NotNull, ItemNotNull]
        private readonly IReadOnlyCollection<Assembly> _assemblies;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultEmbeddedResourceProvider"/> class.
        /// </summary>
        /// <param name="migrations">The migrations used as hint for the assemblies to be scanned for the embedded resources</param>
        public DefaultEmbeddedResourceProvider([NotNull, ItemNotNull] IEnumerable<IMigration> migrations)
        {
            _assemblies = migrations.Select(m => m.GetType().Assembly).Distinct().ToList();
        }

        /// <inheritdoc />
        public IEnumerable<(string name, Assembly assembly)> GetEmbeddedResources()
        {
            foreach (var assembly in _assemblies)
            {
                foreach (var resourceName in assembly.GetManifestResourceNames())
                {
                    yield return (resourceName, assembly);
                }
            }
        }
    }
}
