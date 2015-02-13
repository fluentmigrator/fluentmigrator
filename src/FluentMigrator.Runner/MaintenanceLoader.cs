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

namespace FluentMigrator.Runner
{
    public class MaintenanceLoader : IMaintenanceLoader
    {
        private readonly IMigrationRunner _runner;
        private readonly IDictionary<MigrationStage, IList<IMigration>> _maintenance;

        public MaintenanceLoader(IMigrationRunner runner, IMigrationConventions conventions)
        {
            _runner = runner;
            _maintenance = (
                from a in runner.MigrationAssemblies.Assemblies
                from type in a.GetExportedTypes()
                let stage = conventions.GetMaintenanceStage(type)
                where stage != null
                let migration = (IMigration)Activator.CreateInstance(type)
                group migration by stage
            ).ToDictionary(
                g => g.Key.Value,
                g => (IList<IMigration>)g.OrderBy(m => m.GetType().Name).ToArray()
            );
        }
        
        public void ApplyMaintenance(MigrationStage stage)
        {
            IList<IMigration> migrations;
            if (!_maintenance.TryGetValue(stage, out migrations))
                return;

            foreach (var migration in migrations)
            {
                _runner.Up(migration);
            }
        }
    }
}