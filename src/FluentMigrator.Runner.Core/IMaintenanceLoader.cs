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

using System.Collections.Generic;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Defines the contract for loading maintenance migrations based on a specified migration stage.
    /// </summary>
    public interface IMaintenanceLoader
    {
        /// <summary>
        /// Loads the maintenance migrations for the specified migration stage.
        /// </summary>
        /// <param name="stage">The <see cref="MigrationStage"/> for which maintenance migrations should be loaded.</param>
        /// <returns>
        /// A list of <see cref="IMigrationInfo"/> objects representing the maintenance migrations
        /// associated with the specified <paramref name="stage"/>.
        /// </returns>
        IList<IMigrationInfo> LoadMaintenance(MigrationStage stage);
    }
}
