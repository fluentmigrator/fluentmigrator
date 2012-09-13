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
using System.Reflection;
using FluentMigrator.Infrastructure;
using System.Linq;

namespace FluentMigrator.Runner
{
    public class MigrationLoader : IMigrationLoader
    {
        public IMigrationConventions Conventions { get; private set; }
        public ICollection<MigrationAssemblyInfo> Assemblies { get; private set; }
        public bool LoadNestedNamespaces { get; private set; }
        public SortedList<long, IMigration> Migrations { get; private set; }
        public IEnumerable<string> TagsToMatch { get; set; }

        public MigrationLoader(IMigrationConventions conventions, Assembly assembly, string @namespace, IEnumerable<string> tagsToMatch)
            : this(
                conventions,
                new List<MigrationAssemblyInfo>() { new MigrationAssemblyInfo() { Assembly = assembly, Namespace = @namespace } },
                false,
                tagsToMatch ?? new string[] { })
        {

        }
        
        public MigrationLoader(IMigrationConventions conventions, Assembly assembly, string @namespace, bool loadNestedNamespaces)
            : this( 
                conventions, 
                new List<MigrationAssemblyInfo>() {new MigrationAssemblyInfo() { Assembly = assembly, Namespace = @namespace } }, 
                loadNestedNamespaces, 
                new string[] { })
        {

        }

        public MigrationLoader(IMigrationConventions conventions, ICollection<MigrationAssemblyInfo> assemblies, bool loadNestedNamespaces , IEnumerable<string> tagsToMatch)
        {
            Conventions = conventions;
            Assemblies = assemblies;
            LoadNestedNamespaces = loadNestedNamespaces;
            TagsToMatch = tagsToMatch;

            Initialize();
        }

        private void Initialize()
        {
            Migrations = new SortedList<long, IMigration>();

            IEnumerable<MigrationMetadata> migrationList = FindMigrations();

            if (migrationList == null)
                return;

            foreach (var migrationMetadata in migrationList)
            {
                if (Migrations.ContainsKey(migrationMetadata.Version))
                    throw new Exception(String.Format("Duplicate migration version {0}.", migrationMetadata.Version));

                var migration = (IMigration)migrationMetadata.Type.Assembly.CreateInstance(migrationMetadata.Type.FullName);
                Migrations.Add(migrationMetadata.Version, new MigrationWithMetaDataAdapter(migration, migrationMetadata));
            }
        }

        public IEnumerable<MigrationMetadata> FindMigrations()
        {
            foreach (var migrationAssemblyInfo in Assemblies)
            {
                IEnumerable<Type> matchedTypes = migrationAssemblyInfo.Assembly.GetExportedTypes().Where(t => Conventions.TypeIsMigration(t)
                && (Conventions.TypeHasMatchingTags(t, TagsToMatch) || !Conventions.TypeHasTags(t)));

                if (!string.IsNullOrEmpty(migrationAssemblyInfo.Namespace))
                {
                    Func<Type, bool> shouldInclude = t => t.Namespace == migrationAssemblyInfo.Namespace;
                    if (LoadNestedNamespaces)
                    {
                        string matchNested = migrationAssemblyInfo.Namespace + ".";
                        shouldInclude = t => t.Namespace == migrationAssemblyInfo.Namespace || t.Namespace.StartsWith(matchNested);
                    }

                    matchedTypes = matchedTypes.Where(shouldInclude);
                }

                foreach (Type type in matchedTypes)
                    yield return Conventions.GetMetadataForMigration(type);
            }
            
        }
    }
}