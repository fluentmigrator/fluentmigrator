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

using FluentMigrator.Infrastructure;
using FluentMigrator.Runner.Infrastructure;

namespace FluentMigrator.Runner
{
    /// <inheritdoc />
    /// <summary>
    /// Provides default conventions used by the migration runner in FluentMigrator.
    /// </summary>
    /// <remarks>
    /// This class defines a set of default behaviors and rules for identifying migrations, profiles,
    /// version table metadata, and other migration-related components. It also allows customization
    /// of these conventions by modifying its properties.
    /// </remarks>
    public class MigrationRunnerConventions : IMigrationRunnerConventions
    {
        private static readonly IMigrationRunnerConventions _default = DefaultMigrationRunnerConventions.Instance;

        /// <inheritdoc />
        public Func<Type, bool> TypeIsMigration { get; set; }
        /// <inheritdoc />
        public Func<Type, bool> TypeIsProfile { get; set; }
        /// <inheritdoc />
        public Func<Type, MigrationStage?> GetMaintenanceStage { get; set; }
        /// <inheritdoc />
        public Func<Type, bool> TypeIsVersionTableMetaData { get; set; }

        /// <inheritdoc />
        [Obsolete]
        public Func<Type, IMigrationInfo> GetMigrationInfo { get; set; }

        /// <inheritdoc />
        public Func<IMigration, IMigrationInfo> GetMigrationInfoForMigration { get; }
        /// <inheritdoc />
        public Func<Type, bool> TypeHasTags { get; set; }
        /// <inheritdoc />
        public Func<Type, IEnumerable<string>, bool> TypeHasMatchingTags { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationRunnerConventions"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor sets up default conventions for identifying migrations, version table metadata, 
        /// profiles, maintenance stages, and tag matching within the FluentMigrator framework.
        /// </remarks>
        public MigrationRunnerConventions()
        {
            TypeIsMigration = _default.TypeIsMigration;
            TypeIsVersionTableMetaData = _default.TypeIsVersionTableMetaData;
#pragma warning disable 612
            GetMigrationInfo = _default.GetMigrationInfo;
#pragma warning restore 612
            TypeIsProfile = _default.TypeIsProfile;
            GetMaintenanceStage = _default.GetMaintenanceStage;
            GetMigrationInfoForMigration = _default.GetMigrationInfoForMigration;
            TypeHasTags = _default.TypeHasTags;
            TypeHasMatchingTags = _default.TypeHasMatchingTags;
        }
    }
}
