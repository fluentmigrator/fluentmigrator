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
    /// <summary>
    /// Provides functionality to load and organize maintenance migrations based on specified stages and conventions.
    /// </summary>
    public class MaintenanceLoader : IMaintenanceLoader
    {
        private readonly IDictionary<MigrationStage, IList<IMigration>> _maintenance;

        /// <summary>
        /// Initializes a new instance of the <see cref="MaintenanceLoader"/> class.
        /// </summary>
        /// <param name="assemblySource">
        /// The source of assemblies containing migration and maintenance classes.
        /// </param>
        /// <param name="options">
        /// The options for configuring the migration runner.
        /// </param>
        /// <param name="conventions">
        /// The conventions used to identify and process migrations and maintenance stages.
        /// </param>
        /// <param name="serviceProvider">
        /// The service provider used to resolve dependencies for migration and maintenance instances.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if any of the required parameters are <c>null</c>.
        /// </exception>
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

        /// <summary>
        /// Loads maintenance migrations for the specified migration stage.
        /// </summary>
        /// <param name="stage">The migration stage for which to load maintenance migrations.</param>
        /// <returns>
        /// A list of <see cref="IMigrationInfo"/> objects representing the maintenance migrations
        /// associated with the specified <paramref name="stage"/>.
        /// </returns>
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
