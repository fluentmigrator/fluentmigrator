#region License

// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
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

namespace FluentMigrator.Runner
{
    public interface IMigrationInformationLoader
    {
        SortedList<long, IMigrationInfo> LoadMigrations();
    }

    public class DefaultMigrationInformationLoader : IMigrationInformationLoader
    {
        public DefaultMigrationInformationLoader(IMigrationConventions conventions, Assembly assembly, string @namespace,
                                                 IEnumerable<string> tagsToMatch)
            : this(conventions, assembly, @namespace, false, tagsToMatch)
        {
        }

        public DefaultMigrationInformationLoader(IMigrationConventions conventions, Assembly assembly, string @namespace,
                                                 bool loadNestedNamespaces, IEnumerable<string> tagsToMatch)
        {
            Conventions = conventions;
            Assembly = assembly;
            Namespace = @namespace;
            LoadNestedNamespaces = loadNestedNamespaces;
            TagsToMatch = tagsToMatch ?? new string[] {};
        }

        public IMigrationConventions Conventions { get; private set; }
        public Assembly Assembly { get; private set; }
        public string Namespace { get; private set; }
        public bool LoadNestedNamespaces { get; private set; }
        public IEnumerable<string> TagsToMatch { get; private set; }

        public SortedList<long, IMigrationInfo> LoadMigrations()
        {
            var migrationInfos = new SortedList<long, IMigrationInfo>();

            var migrationTypes = FindMigrationTypes();

            foreach (var migrationType in migrationTypes)
            {
                IMigrationInfo migrationInfo = Conventions.GetMigrationInfo(migrationType);
                if (migrationInfos.ContainsKey(migrationInfo.Version))
                    throw new DuplicateMigrationException(String.Format("Duplicate migration version {0}.",
                                                                        migrationInfo.Version));
                migrationInfos.Add(migrationInfo.Version, migrationInfo);
            }

            return migrationInfos;
        }

        private IEnumerable<Type> FindMigrationTypes()
        {
            return TypeFinder
                .FindTypes(Assembly, Namespace, LoadNestedNamespaces)
                .Where(t => Conventions.TypeIsMigration(t)
                            &&
                            (Conventions.TypeHasMatchingTags(t, TagsToMatch) ||
                             !Conventions.TypeHasTags(t)));
        }
    }
}