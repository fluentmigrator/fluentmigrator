#region License
//
// Copyright (c) 2018, Fluent Migrator Project
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

using System;
using System.Collections.Generic;
using System.Linq;

using FluentMigrator.Exceptions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner.Exceptions;
using FluentMigrator.Runner.Initialization;

using JetBrains.Annotations;

using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Loads migration information from a migration source.
    /// </summary>
    public class DefaultMigrationInformationLoader : IMigrationInformationLoader
    {
        [NotNull, ItemNotNull]
        private readonly string[] _tagsToMatch;

        private readonly bool _includeUntaggedMigrations;

        [NotNull]
#pragma warning disable 618
        private readonly IFilteringMigrationSource _source;
#pragma warning restore 618

        [CanBeNull]
        private SortedList<long, IMigrationInfo> _migrationInfos;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultMigrationInformationLoader"/> class.
        /// </summary>
        /// <param name="source">
        /// The migration source that provides migrations, filtered by the specified criteria.
        /// </param>
        /// <param name="filterOptions">
        /// The options used to filter migrations by namespace and nested namespaces.
        /// </param>
        /// <param name="conventions">
        /// The conventions used by the migration runner to identify and process migrations.
        /// </param>
        /// <param name="runnerOptions">
        /// The options for configuring the migration runner, including tags and untagged migration handling.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if any of the required parameters (<paramref name="source"/>, <paramref name="filterOptions"/>, 
        /// <paramref name="conventions"/>, or <paramref name="runnerOptions"/>) are <c>null</c>.
        /// </exception>
        public DefaultMigrationInformationLoader(
#pragma warning disable 618
            [NotNull] IFilteringMigrationSource source,
#pragma warning restore 618
            [NotNull] IOptionsSnapshot<TypeFilterOptions> filterOptions,
            [NotNull] IMigrationRunnerConventions conventions,
            [NotNull] IOptions<RunnerOptions> runnerOptions)
        {
            _source = source;
            Namespace = filterOptions.Value.Namespace;
            LoadNestedNamespaces = filterOptions.Value.NestedNamespaces;
            Conventions = conventions;
            _tagsToMatch = runnerOptions.Value.Tags ?? Array.Empty<string>();
            _includeUntaggedMigrations = runnerOptions.Value.IncludeUntaggedMigrations;
        }

        /// <summary>
        /// Gets the conventions used by the migration runner to identify and process migrations,
        /// profiles, maintenance stages, version table metadata, and tags.
        /// </summary>
        /// <value>
        /// An instance of <see cref="IMigrationRunnerConventions"/> that defines the conventions
        /// for the migration runner.
        /// </value>
        [NotNull]
        public IMigrationRunnerConventions Conventions { get; }

        /// <summary>
        /// Gets the namespace used to filter migrations.
        /// </summary>
        /// <remarks>
        /// This property is typically set based on the <see cref="TypeFilterOptions.Namespace"/> value.
        /// It is used to restrict the migrations loaded to those within the specified namespace.
        /// </remarks>
        [CanBeNull]
        public string Namespace { get; }

        /// <summary>
        /// Gets a value indicating whether migrations from nested namespaces should be included
        /// when loading migration information.
        /// </summary>
        /// <value>
        /// <c>true</c> if migrations from nested namespaces should be included; otherwise, <c>false</c>.
        /// </value>
        public bool LoadNestedNamespaces { get; }

        /// <inheritdoc />
        public SortedList<long, IMigrationInfo> LoadMigrations()
        {
            if (_migrationInfos != null)
            {
                if (_migrationInfos.Count == 0)
                    throw new MissingMigrationsException();
                return _migrationInfos;
            }

            _migrationInfos = new SortedList<long, IMigrationInfo>();
            var migrationInfos = FindMigrations(
                _source,
                Conventions,
                Namespace,
                LoadNestedNamespaces,
                _tagsToMatch,
                _includeUntaggedMigrations);
            foreach (var migrationInfo in migrationInfos)
            {
                if (_migrationInfos.ContainsKey(migrationInfo.Version))
                {
                    throw new DuplicateMigrationException($"Duplicate migration version {migrationInfo.Version}.");
                }

                _migrationInfos.Add(migrationInfo.Version, migrationInfo);
            }

            if (_migrationInfos.Count == 0)
                throw new MissingMigrationsException();

            return _migrationInfos;
        }

        [NotNull, ItemNotNull]
        private static IEnumerable<IMigrationInfo> FindMigrations(
#pragma warning disable 618
            [NotNull] IFilteringMigrationSource source,
#pragma warning restore 618
            [NotNull] IMigrationRunnerConventions conventions,
            [CanBeNull] string @namespace,
            bool loadNestedNamespaces,
            [NotNull, ItemNotNull] string[] tagsToMatch,
            bool includeUntagged)
        {
            bool IsMatchingMigration(Type type)
            {
                if (!type.IsInNamespace(@namespace, loadNestedNamespaces))
                    return false;
                if (!conventions.TypeIsMigration(type))
                    return false;
                return conventions.HasRequestedTags(type, tagsToMatch, includeUntagged);
            }

            IReadOnlyCollection<IMigration> migrations = source.GetMigrations(IsMatchingMigration).ToList();

            if (migrations.Count == 0)
            {
                throw new MissingMigrationsException("No migrations found");
            }

            var migrationInfos = migrations
                .Select(conventions.GetMigrationInfoForMigration)
                .ToList();

            return migrationInfos;
        }
    }
}
