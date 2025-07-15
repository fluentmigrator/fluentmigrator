#region License
//
// Copyright (c) 2007-2024, Fluent Migrator Project
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

        public MaintenanceLoader(
            [NotNull] IAssemblySource assemblySource,
            [NotNull] IOptions<RunnerOptions> options,
            [NotNull] IMigrationRunnerConventions conventions,
            [NotNull] IServiceProvider serviceProvider,
            [CanBeNull] ITypeSource typeSource = null)
        {
            var tagsList = options.Value.Tags ?? Array.Empty<string>();

            typeSource ??= new AssemblyTypeSource(assemblySource);

#pragma warning disable IL2026
#pragma warning disable IL2072
            _maintenance = (
                from type in typeSource.GetTypes()
                let stage = conventions.GetMaintenanceStage(type)
                where stage != null
                where conventions.HasRequestedTags(type, tagsList, options.Value.IncludeUntaggedMaintenances)
                let migration = (IMigration) ActivatorUtilities.CreateInstance(serviceProvider, type)
                group migration by stage.GetValueOrDefault()
            ).ToDictionary(
                g => g.Key,
                g => (IList<IMigration>)g.OrderBy(m => m.GetType().Name).ToArray()
            );
#pragma warning disable IL2072
#pragma warning restore IL2026
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
