using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using FluentMigrator.Exceptions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner.Exceptions;
using FluentMigrator.Runner.Initialization;

using JetBrains.Annotations;

namespace FluentMigrator.Runner
{
    public class DefaultMigrationInformationLoader : IMigrationInformationLoader
    {
        [CanBeNull] [ItemNotNull]
        private readonly IReadOnlyCollection<IMigration> _migrations;

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
            TagsToMatch = tagsToMatch ?? Enumerable.Empty<string>();
        }

        public DefaultMigrationInformationLoader(
            [NotNull, ItemNotNull] IEnumerable<IMigration> migrations,
            [NotNull] IMigrationRunnerConventions conventions,
            [NotNull] IRunnerContext runnerContext)
        {
            _migrations = migrations.ToList();
            Conventions = conventions;
            TagsToMatch = runnerContext.Tags ?? Enumerable.Empty<string>();
        }

        [NotNull]
        public IMigrationRunnerConventions Conventions { get; }

        [Obsolete]
        [CanBeNull]
        public IAssemblyCollection Assemblies { get; }

        [Obsolete]
        [CanBeNull]
        public string Namespace { get; }

        [Obsolete]
        public bool LoadNestedNamespaces { get; }

        [NotNull, ItemNotNull]
        public IEnumerable<string> TagsToMatch { get; }

        public SortedList<long, IMigrationInfo> LoadMigrations()
        {
            if (_migrationInfos != null)
            {
                return _migrationInfos;
            }

            _migrationInfos = new SortedList<long, IMigrationInfo>();

#pragma warning disable 612
            if (Assemblies != null)
            {
                var migrationTypes = FindMigrationTypes(Conventions, Assemblies, Namespace, LoadNestedNamespaces, TagsToMatch);
                foreach (var migrationType in migrationTypes)
                {
                    var migrationInfo = Conventions.GetMigrationInfo(migrationType);
                    if (_migrationInfos.ContainsKey(migrationInfo.Version))
                    {
                        throw new DuplicateMigrationException($"Duplicate migration version {migrationInfo.Version}.");
                    }
                    _migrationInfos.Add(migrationInfo.Version, migrationInfo);
                }
#pragma warning restore 612
            }
            else if (_migrations != null)
            {
                foreach (var migrationInfo in FindMigrations(Conventions, _migrations, TagsToMatch))
                {
                    if (_migrationInfos.ContainsKey(migrationInfo.Version))
                    {
                        throw new DuplicateMigrationException($"Duplicate migration version {migrationInfo.Version}.");
                    }

                    _migrationInfos.Add(migrationInfo.Version, migrationInfo);
                }
            }
            else
            {
                return _migrationInfos;
            }

            return _migrationInfos;
        }

        private static IEnumerable<IMigrationInfo> FindMigrations(
            IMigrationRunnerConventions conventions,
            IReadOnlyCollection<IMigration> migrations,
            IEnumerable<string> tagsToMatch)
        {
            if (migrations.Count == 0)
            {
                throw new MissingMigrationsException("No migrations found");
            }

            return
                from migration in migrations
                let type = migration.GetType()
                where conventions.TypeHasMatchingTags(type, tagsToMatch) || !conventions.TypeHasTags(type)
                select conventions.GetMigrationInfoForMigration(migration);
        }

        [Obsolete]
        private static IEnumerable<Type> FindMigrationTypes(
            IMigrationRunnerConventions conventions,
            IAssemblyCollection assemblies,
            string @namespace,
            bool loadNestedNamespaces,
            IEnumerable<string> tagsToMatch)
        {
            var migrations = assemblies.GetExportedTypes()
                .FilterByNamespace(@namespace, loadNestedNamespaces)
                .Where(t => conventions.TypeIsMigration(t))
                .ToList();
            if (migrations.Count == 0)
            {
                throw new MissingMigrationsException($"No migrations found in the namespace {@namespace}");
            }

            var tagMatchingMigrations = migrations
                .Where(t =>
                    conventions.TypeHasMatchingTags(t, tagsToMatch)
                 || !conventions.TypeHasTags(t));

            return tagMatchingMigrations;
        }
    }
}
