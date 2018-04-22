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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.VersionTableInfo;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Extension methods for the <see cref="IMigrationRunnerBuilder"/> interface
    /// </summary>
    [CLSCompliant(false)]
    public static class MigrationRunnerBuilderExtensions
    {
        /// <summary>
        /// Sets the announcer
        /// </summary>
        /// <param name="builder">The runner builder</param>
        /// <param name="announcer">The announcer to use</param>
        /// <returns>The runner builder</returns>
        public static IMigrationRunnerBuilder WithAnnouncer(this IMigrationRunnerBuilder builder, IAnnouncer announcer)
        {
            builder.Services.AddSingleton(_ => announcer);
            return builder;
        }

        /// <summary>
        /// Sets the version table meta data
        /// </summary>
        /// <param name="builder">The runner builder</param>
        /// <param name="versionTableMetaData">The version table meta data</param>
        /// <returns>The runner builder</returns>
        public static IMigrationRunnerBuilder WithVersionTable(
            this IMigrationRunnerBuilder builder,
            IVersionTableMetaData versionTableMetaData)
        {
            builder.Services.AddSingleton(_ => versionTableMetaData);
            return builder;
        }

        /// <summary>
        /// Sets the migration runner conventions
        /// </summary>
        /// <param name="builder">The runner builder</param>
        /// <param name="conventions">The migration runner conventions</param>
        /// <returns>The runner builder</returns>
        public static IMigrationRunnerBuilder WithRunnerConventions(
            this IMigrationRunnerBuilder builder,
            IMigrationRunnerConventions conventions)
        {
            builder.Services.AddScoped(_ => conventions);
            return builder;
        }

        /// <summary>
        /// Adds the migrations
        /// </summary>
        /// <param name="builder">The runner builder</param>
        /// <param name="assemblies">The target assemblies</param>
        /// <returns>The runner builder</returns>
        public static IMigrationRunnerBuilder WithMigrationsIn(
            this IMigrationRunnerBuilder builder,
            [NotNull, ItemNotNull] params Assembly[] assemblies)
        {
            builder.Services
                .AddSingleton<IMigrationSourceItem>(new AssemblyMigrationSourceItem(assemblies))
                .AddSingleton<IMigrationSource, MigrationSourceFromItems>();
            return builder;
        }

        /// <summary>
        /// Interface to get the candidate types for <see cref="MigrationSourceFromItems"/>
        /// </summary>
        private interface IMigrationSourceItem
        {
            IEnumerable<Type> MigrationTypeCandidates { get; }
        }

        /// <summary>
        /// Implementation of <see cref="IMigrationSourceItem"/> that accepts a collection of assemnblies
        /// </summary>
        private class AssemblyMigrationSourceItem : IMigrationSourceItem
        {
            private readonly IReadOnlyCollection<Assembly> _assemblies;

            /// <summary>
            /// Initializes a new instance of the <see cref="AssemblyMigrationSourceItem"/> class.
            /// </summary>
            /// <param name="assemblies">The assemblies to get the canididate types from</param>
            public AssemblyMigrationSourceItem(IReadOnlyCollection<Assembly> assemblies)
            {
                _assemblies = assemblies;
            }

            /// <inheritdoc />
            public IEnumerable<Type> MigrationTypeCandidates => _assemblies.SelectMany(a => a.GetExportedTypes());
        }

        /// <summary>
        /// Custom implementation of <see cref="IMigrationSource"/> that works on <see cref="IMigrationSourceItem"/> elements
        /// </summary>
        private class MigrationSourceFromItems : IMigrationSource
        {
            [NotNull]
            private readonly IServiceProvider _serviceProvider;

            [NotNull]
            private readonly IMigrationRunnerConventions _conventions;

            [NotNull]
            [ItemNotNull]
            private readonly IReadOnlyCollection<IMigrationSourceItem> _sourceItems;

            [NotNull]
            private readonly ConcurrentDictionary<Type, IMigration> _instanceCache = new ConcurrentDictionary<Type, IMigration>();

            /// <summary>
            /// Initializes a new instance of the <see cref="MigrationSourceFromItems"/> class.
            /// </summary>
            /// <param name="serviceProvider">The service provider</param>
            /// <param name="conventions">The runner conventions</param>
            /// <param name="sourceItems">The items to get the candidate types from</param>
            public MigrationSourceFromItems(
                [NotNull] IServiceProvider serviceProvider,
                [NotNull] IMigrationRunnerConventions conventions,
                [NotNull, ItemNotNull] IEnumerable<IMigrationSourceItem> sourceItems)
            {
                _serviceProvider = serviceProvider;
                _conventions = conventions;
                _sourceItems = sourceItems.ToList();
            }

            /// <inheritdoc />
            public IEnumerable<IMigration> GetMigrations()
            {
                var migrationTypes = from type in _sourceItems.SelectMany(i => i.MigrationTypeCandidates)
                                     where _conventions.TypeIsMigration(type)
                                     select _instanceCache.GetOrAdd(type, CreateInstance);
                return migrationTypes;
            }

            private IMigration CreateInstance(Type type)
            {
                return (IMigration)ActivatorUtilities.CreateInstance(_serviceProvider, type);
            }
        }
    }
}
