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

using FluentMigrator.Expressions;
using FluentMigrator.Generation;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Versioning;
using FluentMigrator.Runner.VersionTableInfo;

using JetBrains.Annotations;

using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// A version loader that does not require a database connection.
    /// </summary>
    public class ConnectionlessVersionLoader : IVersionLoader
    {
        [NotNull]
        private readonly IMigrationProcessor _processor;

        [NotNull]
        private readonly IMigrationInformationLoader _migrationInformationLoader;

        private readonly IQuoter _quoter;

        private bool _versionsLoaded;

        /// <inheritdoc />
        public ConnectionlessVersionLoader(
            [NotNull] IGeneratorAccessor generatorAccessor,
            [NotNull] IProcessorAccessor processorAccessor,
            [NotNull] IMigrationRunnerConventions conventions,
            [NotNull] IOptions<RunnerOptions> runnerOptions,
            [NotNull] IMigrationInformationLoader migrationInformationLoader,
            [NotNull] IVersionTableMetaData versionTableMetaData)
        {
            _processor = processorAccessor.Processor;
            _migrationInformationLoader = migrationInformationLoader;
            _quoter = generatorAccessor.Generator.Quoter;
            Conventions = conventions;
            StartVersion = runnerOptions.Value.StartVersion;
            TargetVersion = runnerOptions.Value.Version;

            VersionInfo = new VersionInfo();
            VersionTableMetaData = versionTableMetaData;
            VersionMigration = new VersionMigration(VersionTableMetaData);
            VersionSchemaMigration = new VersionSchemaMigration(VersionTableMetaData);
            VersionUniqueMigration = new VersionUniqueMigration(VersionTableMetaData);
            VersionDescriptionMigration = new VersionDescriptionMigration(VersionTableMetaData);

            LoadVersionInfo();
        }

        /// <inheritdoc />
        public IMigrationRunnerConventions Conventions { get; set; }
        /// <inheritdoc />
        public long StartVersion { get; set; }
        /// <inheritdoc />
        public long TargetVersion { get; set; }
        /// <inheritdoc />
        public VersionSchemaMigration VersionSchemaMigration { get; }
        /// <inheritdoc />
        public IMigration VersionMigration { get; }
        /// <inheritdoc />
        public IMigration VersionUniqueMigration { get; }
        /// <inheritdoc />
        public IMigration VersionDescriptionMigration { get; }

        /// <inheritdoc />
        [Obsolete]
        [CanBeNull]
        public IMigrationRunner Runner { get; set; }
        /// <inheritdoc />
        public IVersionInfo VersionInfo { get; set; }
        /// <inheritdoc />
        public IVersionTableMetaData VersionTableMetaData { get; set; }

        /// <inheritdoc />
        public bool AlreadyCreatedVersionSchema =>
            string.IsNullOrEmpty(VersionTableMetaData.SchemaName) ||
            _processor.SchemaExists(VersionTableMetaData.SchemaName);

        /// <inheritdoc />
        public bool AlreadyCreatedVersionTable => _processor.TableExists(VersionTableMetaData.SchemaName, VersionTableMetaData.TableName);

        /// <inheritdoc />
        public void DeleteVersion(long version)
        {
            var expression = new DeleteDataExpression {TableName = VersionTableMetaData.TableName, SchemaName = VersionTableMetaData.SchemaName};
            expression.Rows.Add(new DeletionDataDefinition
            {
                new KeyValuePair<string, object>(VersionTableMetaData.ColumnName, version)
            });
            expression.ExecuteWith(_processor);
        }

        /// <inheritdoc />
        public IVersionTableMetaData GetVersionTableMetaData()
        {
            return VersionTableMetaData;
        }

        /// <inheritdoc />
        public void LoadVersionInfo()
        {
            if (_versionsLoaded)
            {
                return;
            }

            foreach (var migration in _migrationInformationLoader.LoadMigrations())
            {
                if (migration.Key < StartVersion)
                {
                    VersionInfo.AddAppliedMigration(migration.Key);
                }
            }

            _versionsLoaded = true;
        }

        /// <inheritdoc />
        public void RemoveVersionTable()
        {
            var expression = new DeleteTableExpression {TableName = VersionTableMetaData.TableName, SchemaName = VersionTableMetaData.SchemaName};
            expression.ExecuteWith(_processor);

            if (!string.IsNullOrEmpty(VersionTableMetaData.SchemaName))
            {
                var schemaExpression = new DeleteSchemaExpression {SchemaName = VersionTableMetaData.SchemaName};
                schemaExpression.ExecuteWith(_processor);
            }
        }

        /// <inheritdoc />
        public void UpdateVersionInfo(long version)
        {
            UpdateVersionInfo(version, null);
        }

        /// <inheritdoc />
        public void UpdateVersionInfo(long version, string description)
        {
            var dataExpression = new InsertDataExpression();
            dataExpression.Rows.Add(CreateVersionInfoInsertionData(version, description));
            dataExpression.TableName = VersionTableMetaData.TableName;
            dataExpression.SchemaName = VersionTableMetaData.SchemaName;

            dataExpression.ExecuteWith(_processor);
        }

        /// <summary>
        /// Creates the insertion data for the version info table.
        /// </summary>
        /// <param name="version">The migration version.</param>
        /// <param name="description">The migration description.</param>
        /// <returns>The insertion data definition.</returns>
        protected virtual InsertionDataDefinition CreateVersionInfoInsertionData(long version, string description)
        {
            object appliedOnValue;

            if (_quoter is null)
            {
                appliedOnValue = DateTime.UtcNow;
            }
            else
            {
                var quotedCurrentDate = _quoter.QuoteValue(SystemMethods.CurrentUTCDateTime);

                // Default to using DateTime if no system method could be obtained
                appliedOnValue = string.IsNullOrWhiteSpace(quotedCurrentDate)
                    ? (object) DateTime.UtcNow
                    : RawSql.Insert(quotedCurrentDate);
            }

            return new InsertionDataDefinition
            {
                new KeyValuePair<string, object>(VersionTableMetaData.ColumnName, version),
                new KeyValuePair<string, object>(VersionTableMetaData.AppliedOnColumnName, appliedOnValue),
                new KeyValuePair<string, object>(VersionTableMetaData.DescriptionColumnName, description)
            };
        }
    }
}
