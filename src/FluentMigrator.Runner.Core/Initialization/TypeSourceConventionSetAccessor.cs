#region License
// Copyright (c) 2019, Fluent Migrator Project
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

using FluentMigrator.Runner.Conventions;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Initialization
{
    /// <summary>
    /// Provides access to a convention set by utilizing type sources and type filtering options.
    /// </summary>
    /// <remarks>
    /// This class is responsible for retrieving an implementation of <see cref="IConventionSet"/>
    /// from the provided type sources and type candidates. It uses type filtering options
    /// to determine valid types and supports dependency injection for instantiation.
    /// </remarks>
    public class TypeSourceConventionSetAccessor : IConventionSetAccessor
    {
        private readonly Lazy<IConventionSet> _lazyValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeSourceConventionSetAccessor"/> class.
        /// </summary>
        /// <param name="typeFilterOptions">The type filter options</param>
        /// <param name="sources">The sources to get type candidates</param>
        /// <param name="serviceProvider">The service provider used to instantiate the found <see cref="IConventionSet"/> implementation</param>
        /// <param name="typeSource">The type source used to search for the <see cref="IConventionSet"/> implementation</param>
        public TypeSourceConventionSetAccessor(
            [NotNull] IOptionsSnapshot<TypeFilterOptions> typeFilterOptions,
            [NotNull, ItemNotNull] IEnumerable<ITypeSourceItem<IConventionSet>> sources,
            [CanBeNull] IServiceProvider serviceProvider,
            [CanBeNull] ITypeSource typeSource = null)
        {
            var filterOptions = typeFilterOptions.Value;
            _lazyValue = new Lazy<IConventionSet>(
                () =>
                {
                    bool IsValidType(Type t)
                    {
                        return t.IsInNamespace(filterOptions.Namespace, filterOptions.NestedNamespaces);
                    }

                    var matchedType = sources.SelectMany(source => source.GetCandidates(IsValidType))
                        .Union(GetAssemblyTypes(typeSource, IsValidType))
                        .FirstOrDefault();

#pragma warning disable IL2072
                    if (matchedType != null)
                    {
                        if (serviceProvider == null)
                            return (IConventionSet)Activator.CreateInstance(matchedType);
                        return (IConventionSet)ActivatorUtilities.CreateInstance(serviceProvider, matchedType);
                    }
#pragma warning restore IL2072

                    return null;
                });
        }

        /// <inheritdoc />
        public IConventionSet GetConventionSet()
        {
            return _lazyValue.Value;
        }

        private static IEnumerable<Type> GetAssemblyTypes([CanBeNull] ITypeSource typeSource, [NotNull] Predicate<Type> predicate)
        {
            if (typeSource == null)
                return Enumerable.Empty<Type>();
            return typeSource.GetTypes()
                .Where(t => !t.IsAbstract && t.IsClass)
                .Where(t => typeof(IConventionSet).IsAssignableFrom(t))
                .Where(t => predicate(t));
        }
    }
}
