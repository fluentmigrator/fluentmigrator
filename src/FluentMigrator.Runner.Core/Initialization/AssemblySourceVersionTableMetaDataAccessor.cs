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

using FluentMigrator.Runner.VersionTableInfo;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Initialization
{
    /// <summary>
    /// Scans the given source assemblies and returns a found <see cref="IVersionTableMetaData"/> implementation
    /// </summary>
    public class AssemblySourceVersionTableMetaDataAccessor : IVersionTableMetaDataAccessor
    {
        private readonly Lazy<IVersionTableMetaData> _lazyValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblySourceVersionTableMetaDataAccessor"/> class.
        /// </summary>
        /// <param name="typeFilterOptions">The type filter options</param>
        /// <param name="sources">The sources to get type candidates</param>
        /// <param name="serviceProvider">The service provider used to instantiate the found <see cref="IVersionTableMetaData"/> implementation</param>
        /// <param name="assemblySource">The assemblies used to search for the <see cref="IVersionTableMetaData"/> implementation</param>
        public AssemblySourceVersionTableMetaDataAccessor(
            [NotNull] IOptionsSnapshot<TypeFilterOptions> typeFilterOptions,
            [NotNull, ItemNotNull] IEnumerable<IVersionTableMetaDataSourceItem> sources,
            [CanBeNull] IServiceProvider serviceProvider,
            [CanBeNull] IAssemblySource assemblySource = null)
        {
            var filterOptions = typeFilterOptions.Value;
            _lazyValue = new Lazy<IVersionTableMetaData>(
                () =>
                {
                    bool IsValidType(Type t)
                    {
                        return t.IsInNamespace(filterOptions.Namespace, filterOptions.NestedNamespaces);
                    }

                    var matchedType = sources.SelectMany(source => source.GetCandidates(IsValidType))
                        .Union(GetAssemblyTypes(assemblySource, IsValidType))
                        .FirstOrDefault();

                    if (matchedType != null)
                    {
#pragma warning disable IL2072 // Public constructor
                        if (serviceProvider == null)
                            return (IVersionTableMetaData)Activator.CreateInstance(matchedType);
                        return (IVersionTableMetaData)ActivatorUtilities.CreateInstance(serviceProvider, matchedType);
#pragma warning restore IL2072
                    }

                    return null;
                });
        }

        /// <inheritdoc />
        public IVersionTableMetaData VersionTableMetaData => _lazyValue.Value;

        private static IEnumerable<Type> GetAssemblyTypes([CanBeNull] IAssemblySource assemblySource, [NotNull] Predicate<Type> predicate)
        {
#if NET
            if (!System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported)
            {
                return Enumerable.Empty<Type>();
            }
#endif

            if (assemblySource == null)
                return Enumerable.Empty<Type>();

#pragma warning disable IL2026 // Requires unreferenced code
            return assemblySource.Assemblies.SelectMany(a => a.GetExportedTypes())
                .Where(t => !t.IsAbstract && t.IsClass)
                .Where(t => typeof(IVersionTableMetaData).IsAssignableFrom(t))
                .Where(t => predicate(t));
#pragma warning restore IL2026
        }
    }
}
