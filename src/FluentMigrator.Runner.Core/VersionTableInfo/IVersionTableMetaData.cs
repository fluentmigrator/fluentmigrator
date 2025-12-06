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

namespace FluentMigrator.Runner.VersionTableInfo
{
    /// <summary>
    /// Defines metadata for a version table used to track database migrations.
    /// </summary>
    /// <remarks>
    /// Implementations of this interface provide details about the schema, table, and columns
    /// used to store migration version information. This metadata is utilized by the migration
    /// runner to manage and apply migrations.
    /// </remarks>
    public interface IVersionTableMetaData
    {
        /// <summary>
        /// Gets a value indicating whether the schema associated with the version table
        /// is owned by the implementation of <see cref="IVersionTableMetaData"/>.
        /// </summary>
        /// <value>
        /// <c>true</c> if the schema is owned by the implementation; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// When <c>true</c>, the migration runner assumes responsibility for creating
        /// and managing the schema. If <c>false</c>, the schema is expected to exist
        /// and be managed externally.
        /// </remarks>
        bool OwnsSchema { get; }
        /// <summary>
        /// Gets the name of the schema where the version table is stored.
        /// </summary>
        /// <remarks>
        /// This property specifies the schema name used to organize the version table within the database.
        /// If <see langword="null" /> or an empty string is returned, the default schema for the database will be used.
        /// </remarks>
        string SchemaName { get; }
        /// <summary>
        /// Gets the name of the table used to store migration version information.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> representing the name of the version table.
        /// </value>
        /// <remarks>
        /// The version table tracks the applied migrations in the database. 
        /// Implementations may provide a default table name or allow customization.
        /// </remarks>
        string TableName { get; }
        /// <summary>
        /// Gets the name of the column used to store version information in the version table.
        /// </summary>
        /// <remarks>
        /// This property specifies the column name that is used to track migration versions.
        /// Implementations may provide a default value or allow customization of the column name.
        /// </remarks>
        string ColumnName { get; }
        /// <summary>
        /// Gets the name of the column used to store descriptions of migrations in the version table.
        /// </summary>
        /// <remarks>
        /// This property specifies the column name where additional details or descriptions
        /// about each migration are stored. It is typically used to provide context or
        /// metadata about the purpose or content of a migration.
        /// </remarks>
        string DescriptionColumnName { get; }
        /// <summary>
        /// Gets the name of the unique index used in the version table.
        /// </summary>
        /// <remarks>
        /// This property specifies the name of the unique index that ensures the uniqueness
        /// of version entries in the version table. It is used when creating or managing
        /// the version table during database migrations.
        /// </remarks>
        string UniqueIndexName { get; }
        /// <summary>
        /// Gets the name of the column that stores the timestamp indicating when a migration was applied.
        /// </summary>
        /// <remarks>
        /// This column is used to track the date and time a specific migration was executed.
        /// It is typically nullable to accommodate migrations applied before the column was introduced.
        /// </remarks>
        string AppliedOnColumnName { get; }
        /// <summary>
        /// Gets a value indicating whether the version table should be created with a primary key.
        /// </summary>
        /// <value>
        /// <c>true</c> if the version table should include a primary key; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// This property determines if the version table used for tracking migrations will have a primary key
        /// defined on the version column. Implementations can override this behavior based on specific requirements.
        /// </remarks>
        bool CreateWithPrimaryKey { get; }
    }
}
