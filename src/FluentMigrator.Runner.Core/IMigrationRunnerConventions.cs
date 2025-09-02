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
using FluentMigrator.Runner.VersionTableInfo;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Defines a set of conventions used by the migration runner to identify and process migrations,
    /// profiles, maintenance stages, version table metadata, and tags.
    /// </summary>
    public interface IMigrationRunnerConventions
    {
        /// <summary>
        /// Gets a function that determines whether a given <see cref="Type"/> represents a migration.
        /// </summary>
        /// <remarks>
        /// A type is considered a migration if it extends the <see cref="IMigration"/> interface
        /// and optionally satisfies additional criteria defined by the implementation.
        /// </remarks>
        Func<Type, bool> TypeIsMigration { get; }

        /// <summary>
        /// Gets a function that determines whether a given <see cref="Type"/> represents a profile.
        /// </summary>
        /// <remarks>
        /// A profile is typically a class annotated with the <see cref="ProfileAttribute"/> and is used
        /// to group migrations or define specific behaviors for a migration run.
        /// </remarks>
        Func<Type, bool> TypeIsProfile { get; }

        /// <summary>
        /// Gets a function that determines the <see cref="MigrationStage"/> for a given migration type.
        /// </summary>
        /// <remarks>
        /// The returned function takes a <see cref="Type"/> as input and returns the corresponding
        /// <see cref="MigrationStage"/> if applicable, or <c>null</c> if the type does not correspond
        /// to a maintenance stage.
        /// </remarks>
        Func<Type, MigrationStage?> GetMaintenanceStage { get; }

        /// <summary>
        /// Gets a function that determines whether a given <see cref="Type"/> represents
        /// a version table metadata class.
        /// </summary>
        /// <remarks>
        /// A type is considered a version table metadata class if it implements the
        /// <see cref="IVersionTableMetaData"/> interface and is decorated with the
        /// <see cref="VersionTableMetaDataAttribute"/> attribute.
        /// </remarks>
        Func<Type, bool> TypeIsVersionTableMetaData { get; }

        /// <summary>
        /// Gets a function that retrieves migration information for a given migration type.
        /// </summary>
        /// <remarks>
        /// This property is marked as <see cref="ObsoleteAttribute"/> and may be removed in future versions.
        /// It is recommended to use <see cref="IMigrationRunnerConventions.GetMigrationInfoForMigration"/> instead.
        /// </remarks>
        /// <value>
        /// A delegate that takes a <see cref="Type"/> representing a migration and returns an <see cref="IMigrationInfo"/> object.
        /// </value>
        [Obsolete]
        Func<Type, IMigrationInfo> GetMigrationInfo { get; }

        /// <summary>
        /// Create an <see cref="IMigrationInfo"/> instance for a given <see cref="IMigration"/>
        /// </summary>
        Func<IMigration, IMigrationInfo> GetMigrationInfoForMigration { get; }

        /// <summary>
        /// Determines whether the specified type is associated with tags.
        /// </summary>
        /// <remarks>
        /// This property is used to check if a given type has tags, typically by inspecting
        /// attributes or metadata associated with the type. It is commonly utilized in scenarios
        /// where migrations or other components need to be filtered or categorized based on tags.
        /// </remarks>
        /// <value>
        /// A function that takes a <see cref="Type"/> as input and returns <c>true</c> if the type
        /// has tags; otherwise, <c>false</c>.
        /// </value>
        Func<Type, bool> TypeHasTags { get; }

        /// <summary>
        /// Determines whether a given type has tags that match a specified set of tags.
        /// </summary>
        /// <remarks>
        /// This property is used to evaluate if a migration type is associated with a specific set of tags,
        /// which can be useful for filtering migrations based on their tagging.
        /// </remarks>
        /// <value>
        /// A function that takes a <see cref="Type"/> and an <see cref="IEnumerable{T}"/> of <see cref="string"/> 
        /// representing the tags to match, and returns <c>true</c> if the type has matching tags; otherwise, <c>false</c>.
        /// </value>
        Func<Type, IEnumerable<string>, bool> TypeHasMatchingTags { get; }
    }
}
