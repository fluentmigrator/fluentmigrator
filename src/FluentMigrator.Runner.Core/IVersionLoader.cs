#region License
//
// Copyright (c) 2018, Fluent Migrator Project
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

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Manages the version table and the stored versions
    /// </summary>
    public interface IVersionLoader
    {
        /// <summary>
        /// Gets a value indicating whether the schema for the version table has been created (or already exited)
        /// </summary>
        bool AlreadyCreatedVersionSchema { get; }

        /// <summary>
        /// Gets a value indicating whether the version table has been created (or already exited)
        /// </summary>
        bool AlreadyCreatedVersionTable { get; }

        /// <summary>
        /// Deletes a version from the version table
        /// </summary>
        /// <param name="version">The version to delete from the version table</param>
        void DeleteVersion(long version);

        /// <summary>
        /// Get the version table metadata
        /// </summary>
        /// <returns>The version table metadata</returns>
        VersionTableInfo.IVersionTableMetaData GetVersionTableMetaData();

        /// <summary>
        /// Loads all version data stored in the version table
        /// </summary>
        void LoadVersionInfo();

        /// <summary>
        /// Removes the version table
        /// </summary>
        void RemoveVersionTable();

        /// <summary>
        /// The runner this version loader belongs to
        /// </summary>
        IMigrationRunner Runner { get; set; }

        /// <summary>
        /// Adds the version information
        /// </summary>
        /// <param name="version">The version number</param>
        void UpdateVersionInfo(long version);

        /// <summary>
        /// Adds the version information
        /// </summary>
        /// <param name="version">The version number</param>
        /// <param name="description">The version description</param>
        void UpdateVersionInfo(long version, string description);

        /// <summary>
        /// Gets an interface to query/update the status of migrations
        /// </summary>
        Versioning.IVersionInfo VersionInfo { get; set; }

        /// <summary>
        /// Gets the version table meta data
        /// </summary>
        VersionTableInfo.IVersionTableMetaData VersionTableMetaData { get; }
    }
}
