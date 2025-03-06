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

using JetBrains.Annotations;

namespace FluentMigrator.Infrastructure
{
    /// <summary>
    /// The default implementation of the <see cref="IEmbeddedResourceProvider"/> interface
    /// </summary>
    public class DefaultEmbeddedResourceProvider : IEmbeddedResourceProvider
    {
        [CanBeNull, ItemNotNull]
        private readonly IReadOnlyCollection<Assembly> _assemblies;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultEmbeddedResourceProvider"/> class.
        /// </summary>
        /// <param name="assemblies">The assemblies to be scanned for the embedded resources</param>
        public DefaultEmbeddedResourceProvider([NotNull, ItemNotNull] IEnumerable<Assembly> assemblies)
            : this(assemblies.ToArray())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultEmbeddedResourceProvider"/> class.
        /// </summary>
        /// <param name="assemblies">The assemblies to be scanned for the embedded resources</param>
        public DefaultEmbeddedResourceProvider([NotNull, ItemNotNull] params Assembly[] assemblies)
        {
            _assemblies = assemblies;
        }

        /// <inheritdoc />
        public IEnumerable<(string name, Assembly assembly)> GetEmbeddedResources()
        {
            if (_assemblies == null)
                yield break;

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
