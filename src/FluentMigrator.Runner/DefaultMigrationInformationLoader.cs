using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using FluentMigrator.Exceptions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner.Exceptions;

namespace FluentMigrator.Runner
{
    public class DefaultMigrationInformationLoader : IMigrationInformationLoader
    {
        private SortedList<long, IMigrationInfo> _migrationInfos;

        public DefaultMigrationInformationLoader(IMigrationConventions conventions, Assembly assembly, string @namespace,
                                                   IEnumerable<string> tagsToMatch)
          : this(conventions, new SingleAssembly(assembly), @namespace, false, tagsToMatch)
        {
        }



        public DefaultMigrationInformationLoader(IMigrationConventions conventions, IAssemblyCollection assemblies, string @namespace,
                                                 IEnumerable<string> tagsToMatch)
            : this(conventions, assemblies, @namespace, false, tagsToMatch)
        {
        }

        public DefaultMigrationInformationLoader(IMigrationConventions conventions, Assembly assembly, string @namespace,
                                                  bool loadNestedNamespaces, IEnumerable<string> tagsToMatch)
            : this(conventions, new SingleAssembly(assembly), @namespace, loadNestedNamespaces, tagsToMatch)
        {
        }

        public DefaultMigrationInformationLoader(IMigrationConventions conventions, IAssemblyCollection assemblies, string @namespace,
                                                 bool loadNestedNamespaces, IEnumerable<string> tagsToMatch)
        {
            Conventions = conventions;
            Assemblies = assemblies;
            Namespace = @namespace;
            LoadNestedNamespaces = loadNestedNamespaces;
            TagsToMatch = tagsToMatch ?? new string[] { };
        }

        public IMigrationConventions Conventions { get; private set; }
        public IAssemblyCollection Assemblies { get; private set; }
        public string Namespace { get; private set; }
        public bool LoadNestedNamespaces { get; private set; }
        public IEnumerable<string> TagsToMatch { get; private set; }

        public SortedList<long, IMigrationInfo> LoadMigrations()
        {
            if (_migrationInfos != null)
            {
                return _migrationInfos;
            }

            _migrationInfos = new SortedList<long, IMigrationInfo>();

            var migrationTypes = FindMigrationTypes();

            foreach (var migrationType in migrationTypes)
            {
                var migrationInfo = Conventions.GetMigrationInfo(migrationType);
                if (_migrationInfos.ContainsKey(migrationInfo.Version))
                {
                    throw new DuplicateMigrationException(String.Format("Duplicate migration version {0}.", migrationInfo.Version));
                }
                _migrationInfos.Add(migrationInfo.Version, migrationInfo);
            }

            return _migrationInfos;
        }

        private IEnumerable<Type> FindMigrationTypes()
        {
            var migrations = Assemblies.GetExportedTypes()
                .FilterByNamespace(Namespace, LoadNestedNamespaces)
                .Where(t => Conventions.TypeIsMigration(t))
                .ToList();
            if (migrations.Count == 0)
            {
                throw new MissingMigrationsException($"No migrations found in the namespace {Namespace}");
            }

            var tagMatchingMigrations = migrations
                .Where(t =>
                    Conventions.TypeHasMatchingTags(t, TagsToMatch)
                 || !Conventions.TypeHasTags(t));

            return tagMatchingMigrations;
        }
    }
}
