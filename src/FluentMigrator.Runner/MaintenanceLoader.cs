#region License
//
// Copyright (c) 2007-2018, Sean Chambers <schambers80@gmail.com>
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

using FluentMigrator.Infrastructure;
using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Runner.Initialization;

namespace FluentMigrator.Runner
{
    public class MaintenanceLoader : IMaintenanceLoader
    {
        private readonly IDictionary<MigrationStage, IList<IMigration>> _maintenance;

        [Obsolete]
        public MaintenanceLoader(IAssemblyCollection assemblyCollection, IEnumerable<string> tags, IMigrationRunnerConventions conventions)
        {
            var tagsList = tags?.ToArray() ?? new string[0];
            var requireTags = tagsList.Length != 0;

            _maintenance = (
                from a in assemblyCollection.Assemblies
                    from type in a.GetExportedTypes()
                    let stage = conventions.GetMaintenanceStage(type)
                    where stage != null
                where (requireTags && conventions.TypeHasMatchingTags(type, tagsList)) || (!requireTags && !conventions.TypeHasTags(type))
                let migration = (IMigration)Activator.CreateInstance(type)
                group migration by stage.GetValueOrDefault()
            ).ToDictionary(
                g => g.Key,
                g => (IList<IMigration>)g.OrderBy(m => m.GetType().Name).ToArray()
            );
        }

        public MaintenanceLoader(IEnumerable<IMigration> migrations, IRunnerContext runnerContext, IMigrationRunnerConventions conventions)
        {
            var tags = runnerContext.Tags?.ToList() ?? new List<string>();
            var requireTags = tags.Count != 0;

            _maintenance = (
                from migration in migrations
                let type = migration.GetType()
                let stage = conventions.GetMaintenanceStage(type)
                where stage != null
                where (requireTags && conventions.TypeHasMatchingTags(type, tags)) || (!requireTags && !conventions.TypeHasTags(type))
                group migration by stage.GetValueOrDefault()
            ).ToDictionary(
                g => g.Key,
                g => (IList<IMigration>)g.OrderBy(m => m.GetType().Name).ToArray()
            );
        }

        public IList<IMigrationInfo> LoadMaintenance(MigrationStage stage)
        {
            IList<IMigrationInfo> migrationInfos = new List<IMigrationInfo>();
            if (!_maintenance.TryGetValue(stage, out var migrations))
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
