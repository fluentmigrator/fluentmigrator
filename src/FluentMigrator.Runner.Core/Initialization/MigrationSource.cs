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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FluentMigrator.Runner.Initialization
{
    /// <summary>
    /// The default implementation of a <see cref="IFilteringMigrationSource"/>.
    /// </summary>
#pragma warning disable 618
    public class MigrationSource : IFilteringMigrationSource, IMigrationSource
#pragma warning restore 618
    {
        [NotNull]
        private readonly ITypeSource _source;

        [NotNull]
        private readonly IMigrationRunnerConventions _conventions;

        [CanBeNull]
        private readonly IServiceProvider _serviceProvider;

        [NotNull]
        private readonly ConcurrentDictionary<Type, IMigration> _instanceCache = new ConcurrentDictionary<Type, IMigration>();

        [NotNull, ItemNotNull]
        private readonly IEnumerable<IMigrationSourceItem> _sourceItems;

        [NotNull]
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationSource"/> class.
        /// </summary>
        /// <param name="source">The assembly source</param>
        /// <param name="conventions">The migration runner conventions</param>
        /// <param name="serviceProvider">The service provider</param>
        /// <param name="sourceItems">The additional migration source items</param>
        /// <param name="logger">The logger for troubleshooting "No migrations found" error.</param>
        /// <param name="typeSource">
        /// The type source; when not provided, an <see cref="AssemblyTypeSource"/> wrapping
        /// <paramref name="source"/> is used, which requires unreferenced code.
        /// </param>
#if NET
        [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("When typeSource is not provided, assembly scanning uses reflection which may not be preserved in trimmed applications.")]
#endif
        public MigrationSource(
            [NotNull] IAssemblySource source,
            [NotNull] IMigrationRunnerConventions conventions,
            [NotNull] IServiceProvider serviceProvider,
            [NotNull, ItemNotNull] IEnumerable<IMigrationSourceItem> sourceItems,
            [NotNull] ILogger logger,
            [CanBeNull] ITypeSource typeSource = null)
        {
            _source = typeSource ?? new AssemblyTypeSource(source);
            _conventions = conventions;
            _serviceProvider = serviceProvider;
            _sourceItems = sourceItems;
            _logger = logger;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationSource"/> class.
        /// </summary>
        /// <param name="source">The assembly source</param>
        /// <param name="conventions">The migration runner conventions</param>
        [Obsolete]
#if NET
        [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("This type uses the AppDomain to load assemblies, which may not be preserved in trimmed applications.")]
#endif
        public MigrationSource(
            [NotNull] IAssemblySource source,
            [NotNull] IMigrationRunnerConventions conventions)
        {
            _source = new AssemblyTypeSource(source);
            _conventions = conventions;
            _sourceItems = Enumerable.Empty<IMigrationSourceItem>();
        }

        /// <inheritdoc />
        public IEnumerable<IMigration> GetMigrations()
        {
            return GetMigrations(_conventions.TypeIsMigration);
        }

        /// <inheritdoc />
        public IEnumerable<IMigration> GetMigrations(Func<Type, bool> predicate)
        {
            foreach (var type in GetMigrationTypeCandidates())
            {
                if (type.IsAbstract)
                {
                    _logger.Log(LogLevel.Trace, $"Type [{type.AssemblyQualifiedName}] is abstract. Skipping.");
                    continue;
                }

                if (!(typeof(IMigration).IsAssignableFrom(type) || typeof(MigrationBase).IsAssignableFrom(type)))
                {
                    _logger.Log(LogLevel.Trace, $"Type [{type.AssemblyQualifiedName}] is not assignable to IMigration. Skipping.");
                    continue;
                }

                if (!(predicate == null || predicate(type)))
                {
                    _logger.Log(LogLevel.Trace, $"Type [{type.AssemblyQualifiedName}] doesn't satisfy predicate. Skipping.");
                    continue;
                }

                _logger.Log(LogLevel.Trace, $"Type {type.AssemblyQualifiedName} is a migration. Adding.");

                yield return _instanceCache.GetOrAdd(type, CreateInstance);
            }
        }

        private IEnumerable<Type> GetMigrationTypeCandidates()
        {
            return _source.GetTypes()
                .Union(_sourceItems.SelectMany(i => i.MigrationTypeCandidates));
        }

        private IMigration CreateInstance(
#if NET
            [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicConstructors)]
#endif
            Type type)
        {
            if (_serviceProvider == null)
            {
                return (IMigration)Activator.CreateInstance(type);
            }

            return (IMigration)ActivatorUtilities.CreateInstance(_serviceProvider, type);
        }
    }
}
