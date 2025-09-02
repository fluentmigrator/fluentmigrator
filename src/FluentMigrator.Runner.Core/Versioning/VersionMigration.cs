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

using FluentMigrator.Runner.VersionTableInfo;

namespace FluentMigrator.Runner.Versioning
{
    /// <summary>
    /// Represents a migration that manages the creation and deletion of the versioning table 
    /// used to track applied migrations in the database.
    /// </summary>
    /// <remarks>
    /// This class is responsible for creating the versioning table with the specified schema, 
    /// table name, and column name as defined by the <see cref="IVersionTableMetaData"/> instance.
    /// It also supports adding a primary key to the versioning table if required.
    /// </remarks>
    public class VersionMigration : Migration
    {
        private readonly IVersionTableMetaData _versionTableMetaData;

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentMigrator.Runner.Versioning.VersionMigration"/> class.
        /// </summary>
        /// <param name="versionTableMetaData">
        /// The metadata for the version table, used to manage versioning information in the database.
        /// </param>
        public VersionMigration(IVersionTableMetaData versionTableMetaData)
        {
            _versionTableMetaData = versionTableMetaData;
        }

        /// <inheritdoc />
        public override void Up()
        {
            var builder = Create.Table(_versionTableMetaData.TableName)
                .InSchema(_versionTableMetaData.SchemaName)
                .WithColumn(_versionTableMetaData.ColumnName).AsInt64().NotNullable();

            if (_versionTableMetaData.CreateWithPrimaryKey)
            {
                builder.PrimaryKey(_versionTableMetaData.UniqueIndexName);
            }
        }

        /// <inheritdoc />
        public override void Down()
        {
            Delete.Table(_versionTableMetaData.TableName).InSchema(_versionTableMetaData.SchemaName);
        }
    }

    /// <summary>
    /// Represents a migration that manages the creation and deletion of a schema
    /// for versioning purposes in a database migration process.
    /// </summary>
    /// <remarks>
    /// This class is responsible for creating or deleting the schema defined by
    /// the <see cref="IVersionTableMetaData.SchemaName"/> property of the provided
    /// <see cref="IVersionTableMetaData"/> instance during the migration process.
    /// </remarks>
    public class VersionSchemaMigration : Migration
    {
        private IVersionTableMetaData _versionTableMetaData;

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentMigrator.Runner.Versioning.VersionSchemaMigration"/> class.
        /// </summary>
        /// <param name="versionTableMetaData">
        /// The metadata for the version table, used to manage schema migrations.
        /// </param>
        /// <remarks>
        /// This constructor sets up the schema migration process by utilizing the provided
        /// <see cref="FluentMigrator.Runner.VersionTableInfo.IVersionTableMetaData"/> instance.
        /// </remarks>
        public VersionSchemaMigration(IVersionTableMetaData versionTableMetaData)
        {
            _versionTableMetaData = versionTableMetaData;
        }

        /// <inheritdoc />
        public override void Up()
        {
            if (!string.IsNullOrEmpty(_versionTableMetaData.SchemaName))
                Create.Schema(_versionTableMetaData.SchemaName);
        }

        /// <inheritdoc />
        public override void Down()
        {
            if (!string.IsNullOrEmpty(_versionTableMetaData.SchemaName))
                Delete.Schema(_versionTableMetaData.SchemaName);
        }
    }

    /// <summary>
    /// Represents a migration that ensures the uniqueness of versioning information in the database.
    /// </summary>
    /// <remarks>
    /// This migration creates a unique clustered index on the version table and adds a nullable column
    /// for storing the date and time when a version was applied. It is designed to work with the 
    /// versioning metadata provided by the <see cref="FluentMigrator.Runner.VersionTableInfo.IVersionTableMetaData"/> interface.
    /// </remarks>
    public class VersionUniqueMigration : ForwardOnlyMigration
    {
        private readonly IVersionTableMetaData _versionTableMeta;

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionUniqueMigration"/> class.
        /// </summary>
        /// <param name="versionTableMeta">
        /// The version table metadata used to define the schema, table, and column details for versioning.
        /// </param>
        /// <remarks>
        /// This constructor sets up the migration to ensure the uniqueness of versioning information
        /// in the database by utilizing the provided <paramref name="versionTableMeta"/>.
        /// </remarks>
        public VersionUniqueMigration(IVersionTableMetaData versionTableMeta)
        {
            _versionTableMeta = versionTableMeta;
        }

        /// <inheritdoc />
        public override void Up()
        {
            Create.Index(_versionTableMeta.UniqueIndexName)
                .OnTable(_versionTableMeta.TableName)
                .InSchema(_versionTableMeta.SchemaName)
                .WithOptions().Unique()
                .WithOptions().Clustered()
                .OnColumn(_versionTableMeta.ColumnName);

            Alter.Table(_versionTableMeta.TableName).InSchema(_versionTableMeta.SchemaName)
                .AddColumn(_versionTableMeta.AppliedOnColumnName).AsDateTime().Nullable();
        }
    }

    /// <summary>
    /// Represents a migration that adds or removes a description column to the version table.
    /// </summary>
    /// <remarks>
    /// This migration is responsible for modifying the version table schema by adding a nullable
    /// description column or removing it, depending on the migration direction.
    /// </remarks>
    public class VersionDescriptionMigration : Migration
    {
        private readonly IVersionTableMetaData _versionTableMeta;

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentMigrator.Runner.Versioning.VersionDescriptionMigration"/> class.
        /// </summary>
        /// <param name="versionTableMeta">
        /// The metadata for the version table, which provides details about the schema, table, and columns
        /// used to store migration version information.
        /// </param>
        /// <remarks>
        /// This constructor sets up the migration to modify the version table schema by adding or removing
        /// a description column, depending on the migration direction.
        /// </remarks>
        public VersionDescriptionMigration(IVersionTableMetaData versionTableMeta)
        {
            _versionTableMeta = versionTableMeta;
        }

        /// <inheritdoc />
        public override void Up()
        {
            Alter.Table(_versionTableMeta.TableName).InSchema(_versionTableMeta.SchemaName)
                .AddColumn(_versionTableMeta.DescriptionColumnName).AsString(1024).Nullable();
        }

        /// <inheritdoc />
        public override void Down()
        {
            Delete.Column(_versionTableMeta.DescriptionColumnName)
                .FromTable(_versionTableMeta.TableName).InSchema(_versionTableMeta.SchemaName);
        }
    }
}
