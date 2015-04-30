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
using FluentMigrator.Infrastructure;
using FluentMigrator.Infrastructure.Extensions;

namespace FluentMigrator.Runner
{
    public class MaintenanceLoader : IMaintenanceLoader
    {
        private readonly IDictionary<MigrationStage, IList<IMigration>> _maintenance;

        public MaintenanceLoader(IAssemblyCollection assemblyCollection, IEnumerable<string> tags, IMigrationConventions conventions)
        {
            tags = tags ?? new string[] {};
            var requireTags = tags.Any();

            _maintenance = (
                from a in assemblyCollection.Assemblies
                    from type in a.GetExportedTypes()
                    let stage = conventions.GetMaintenanceStage(type)
                    where stage != null
                where (requireTags && conventions.TypeHasMatchingTags(type, tags)) || (!requireTags && !conventions.TypeHasTags(type))
                let migration = (IMigration)Activator.CreateInstance(type)
                group migration by stage
            ).ToDictionary(
                g => g.Key.Value,
                g => (IList<IMigration>)g.OrderBy(m => m.GetType().Name).ToArray()
            );
        }

        public IList<IMigrationInfo> LoadMaintenance(MigrationStage stage)
        {
            IList<IMigration> migrations;
            IList<IMigrationInfo> migrationInfos = new List<IMigrationInfo>();
            if (!_maintenance.TryGetValue(stage, out migrations))
                return migrationInfos;

            foreach (var migration in migrations)
            {
                var transactionBehavior = migration.GetType().GetOneAttribute<MaintenanceAttribute>().TransactionBehavior;
                migrationInfos.Add(new NonAttributedMigrationToMigrationInfoAdapter(migration, transactionBehavior));
            }

            return migrationInfos;
        }
    }
}