#region License
// Copyright (c) 2007-2024, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;

using FluentMigrator.Runner.Initialization;

using JetBrains.Annotations;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// The migration runner
    /// </summary>
    public interface IMigrationRunner : IMigrationScopeStarter
    {
        /// <summary>
        /// Gets the migration processor used by this runner
        /// </summary>
        [NotNull]
        IMigrationProcessor Processor { get; }

        /// <summary>
        /// Gets or sets the migration loader to be used by this migration runner
        /// </summary>
        [NotNull]
        IMigrationInformationLoader MigrationLoader { get; }

        /// <summary>
        /// Executes an <c>Up</c> migration
        /// </summary>
        /// <param name="migration">The migration to execute</param>
        void Up([NotNull] IMigration migration);

        /// <summary>
        /// Executes an <c>Down</c> migration
        /// </summary>
        /// <param name="migration">The migration to execute</param>
        void Down([NotNull] IMigration migration);

        /// <summary>
        /// Executes all found (and unapplied) migrations
        /// </summary>
        void MigrateUp();

        /// <summary>
        /// Executes all found (and unapplied) migrations up to the given version
        /// </summary>
        /// <param name="version">The target version to migrate to (inclusive)</param>
        void MigrateUp(long version);

        /// <summary>
        /// Rollback the given number of steps
        /// </summary>
        /// <param name="steps">The number of steps to rollback</param>
        void Rollback(int steps);

        /// <summary>
        /// Rollback to a given version
        /// </summary>
        /// <param name="version">The target version to rollback to</param>
        void RollbackToVersion(long version);

        /// <summary>
        /// Migrate down to the given version
        /// </summary>
        /// <param name="version">The version to migrate down to</param>
        void MigrateDown(long version);

        /// <summary>
        /// Validate that there were no missing migration versions
        /// </summary>
        /// <remarks>
        /// Throws an exception if a missing migration was found.
        /// </remarks>
        void ValidateVersionOrder();

        /// <summary>
        /// List all migrations to the logger
        /// </summary>
        void ListMigrations();

        /// <summary>
        /// Returns <c>true</c> when there are migrations to apply
        /// </summary>
        /// <param name="version">The target version (or <c>null</c> if the last one should be used)</param>
        /// <returns><c>true</c> when there are migrations to apply</returns>
        bool HasMigrationsToApplyUp(long? version = null);

        /// <summary>
        /// Returns <c>true</c> when there are migrations to revert
        /// </summary>
        /// <param name="version">The target version</param>
        /// <returns><c>true</c> when there are migrations to revert</returns>
        bool HasMigrationsToApplyDown(long version);

        /// <summary>
        /// Are there migrations available for a rollback?
        /// </summary>
        /// <returns><c>true</c> when there are migrations available for a rollback</returns>
        bool HasMigrationsToApplyRollback();

        /// <summary>
        /// Creates the required version info database tables
        /// </summary>
        /// <returns><c>true</c> if the table had to be created or <c>false</c> when it already existed</returns>
        bool LoadVersionInfoIfRequired();
    }
}
