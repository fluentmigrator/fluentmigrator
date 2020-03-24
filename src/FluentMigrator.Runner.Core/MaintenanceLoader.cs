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

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner
{
    public class MaintenanceLoader : IMaintenanceLoader
    {
        private readonly IDictionary<MigrationStage, IList<IMigration>> _maintenance;

        [Obsolete]
        public MaintenanceLoader(IAssemblyCollection assemblyCollection, IEnumerable<string> tags, IMigrationRunnerConventions conventions)
        {
            var tagsList = tags?.ToArray() ?? new string[0];

            _maintenance = (
                from a in assemblyCollection.Assemblies
                    from type in a.GetExportedTypes()
                    let stage = conventions.GetMaintenanceStage(type)
                    where stage != null
                where conventions.HasRequestedTags(type, tagsList, false)
                let migration = (IMigration)Activator.CreateInstance(type)
                group migration by stage.GetValueOrDefault()
            ).ToDictionary(
                g => g.Key,
                g => (IList<IMigration>)g.OrderBy(m => m.GetType().Name).ToArray()
            );
        }

        public MaintenanceLoader(
            [NotNull] IAssemblySource assemblySource,
            [NotNull] IOptions<RunnerOptions> options,
            [NotNull] IMigrationRunnerConventions conventions,
            [NotNull] IServiceProvider serviceProvider)
        {
            var tagsList = options.Value.Tags ?? new string[0];

            var types = assemblySource.Assemblies.SelectMany(a => a.ExportedTypes).ToList();

            _maintenance = (
                from type in types
                let stage = conventions.GetMaintenanceStage(type)
                where stage != null
                where conventions.HasRequestedTags(type, tagsList, options.Value.IncludeUntaggedMaintenances)
                let migration = (IMigration) ActivatorUtilities.CreateInstance(serviceProvider, type)
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
