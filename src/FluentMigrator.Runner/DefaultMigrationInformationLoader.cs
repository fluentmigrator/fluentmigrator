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
using System.Reflection;

using FluentMigrator.Exceptions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner.Exceptions;
using FluentMigrator.Runner.Initialization;

using JetBrains.Annotations;

using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner
{
    public class DefaultMigrationInformationLoader : IMigrationInformationLoader
    {
        [NotNull, ItemNotNull]
        private readonly IReadOnlyCollection<string> _tagsToMatch;

        [NotNull]
        private readonly IMigrationSource _source;

        [CanBeNull]
        private SortedList<long, IMigrationInfo> _migrationInfos;

        [Obsolete]
        public DefaultMigrationInformationLoader(IMigrationRunnerConventions conventions, Assembly assembly, string @namespace,
                                                   IEnumerable<string> tagsToMatch)
          : this(conventions, new SingleAssembly(assembly), @namespace, false, tagsToMatch)
        {
        }

        [Obsolete]
        public DefaultMigrationInformationLoader(IMigrationRunnerConventions conventions, IAssemblyCollection assemblies, string @namespace,
                                                 IEnumerable<string> tagsToMatch)
            : this(conventions, assemblies, @namespace, false, tagsToMatch)
        {
        }

        [Obsolete]
        public DefaultMigrationInformationLoader(IMigrationRunnerConventions conventions, Assembly assembly, string @namespace,
                                                  bool loadNestedNamespaces, IEnumerable<string> tagsToMatch)
            : this(conventions, new SingleAssembly(assembly), @namespace, loadNestedNamespaces, tagsToMatch)
        {
        }

        [Obsolete]
        public DefaultMigrationInformationLoader(IMigrationRunnerConventions conventions, IAssemblyCollection assemblies, string @namespace,
                                                 bool loadNestedNamespaces, IEnumerable<string> tagsToMatch)
        {
            Conventions = conventions;
            Assemblies = assemblies;
            Namespace = @namespace;
            LoadNestedNamespaces = loadNestedNamespaces;
            _tagsToMatch = tagsToMatch as IReadOnlyCollection<string> ?? tagsToMatch?.ToArray() ?? Array.Empty<string>();
            _source = new MigrationSource(new AssemblySource(() => assemblies), conventions);
        }

        public DefaultMigrationInformationLoader(
            [NotNull] IMigrationSource source,
            [NotNull] IOptionsSnapshot<TypeFilterOptions> filterOptions,
            [NotNull] IMigrationRunnerConventions conventions,
            [NotNull] IOptions<RunnerOptions> runnerOptions)
        {
            _source = source;
            Namespace = filterOptions.Value.Namespace;
            LoadNestedNamespaces = filterOptions.Value.NestedNamespaces;
            Conventions = conventions;
            _tagsToMatch = runnerOptions.Value.Tags ?? Array.Empty<string>();
        }

        [NotNull]
        public IMigrationRunnerConventions Conventions { get; }

        [Obsolete]
        [CanBeNull]
        public IAssemblyCollection Assemblies { get; }

        [CanBeNull]
        public string Namespace { get; }

        public bool LoadNestedNamespaces { get; }

        [NotNull, ItemNotNull]
        [Obsolete]
        public IEnumerable<string> TagsToMatch => _tagsToMatch;

        public SortedList<long, IMigrationInfo> LoadMigrations()
        {
            if (_migrationInfos != null)
            {
                if (_migrationInfos.Count == 0)
                    throw new MissingMigrationsException();
                return _migrationInfos;
            }

            _migrationInfos = new SortedList<long, IMigrationInfo>();
            var migrationInfos = FindMigrations(_source, Conventions, Namespace, LoadNestedNamespaces, _tagsToMatch);
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
            [NotNull] IMigrationSource source,
            [NotNull] IMigrationRunnerConventions conventions,
            [CanBeNull] string @namespace,
            bool loadNestedNamespaces,
            [NotNull, ItemNotNull] IReadOnlyCollection<string> tagsToMatch)
        {
            bool IsMatchingMigration(Type type)
            {
                if (!type.IsInNamespace(@namespace, loadNestedNamespaces))
                    return false;
                return conventions.TypeHasMatchingTags(type, tagsToMatch)
                 || (tagsToMatch.Count == 0 && !conventions.TypeHasTags(type))
                 || !conventions.TypeHasTags(type);
            }

            IReadOnlyCollection<IMigration> migrations;

            if (source is IFilteringMigrationSource filteringSource)
            {
                migrations = filteringSource.GetMigrations(IsMatchingMigration).ToList();
            }
            else
            {
                migrations =
                    (from migration in source.GetMigrations()
                     where IsMatchingMigration(migration.GetType())
                     select migration).ToList();
            }

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
