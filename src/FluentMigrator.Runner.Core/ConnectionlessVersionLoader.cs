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
    public class ConnectionlessVersionLoader : IVersionLoader
    {
        [NotNull]
        private readonly IMigrationProcessor _processor;

        [NotNull]
        private readonly IMigrationInformationLoader _migrationInformationLoader;

        private readonly IQuoter _quoter;

        private bool _versionsLoaded;

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
            _quoter = generatorAccessor.Generator.GetQuoter();
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

        public IMigrationRunnerConventions Conventions { get; set; }
        public long StartVersion { get; set; }
        public long TargetVersion { get; set; }
        public VersionSchemaMigration VersionSchemaMigration { get; }
        public IMigration VersionMigration { get; }
        public IMigration VersionUniqueMigration { get; }
        public IMigration VersionDescriptionMigration { get; }

        [Obsolete]
        [CanBeNull]
        public IMigrationRunner Runner { get; set; }
        public IVersionInfo VersionInfo { get; set; }
        public IVersionTableMetaData VersionTableMetaData { get; set; }

        public bool AlreadyCreatedVersionSchema
        {
            get
            {
                return string.IsNullOrEmpty(VersionTableMetaData.SchemaName) ||
                       _processor.SchemaExists(VersionTableMetaData.SchemaName);
            }
        }

        public bool AlreadyCreatedVersionTable
        {
            get { return _processor.TableExists(VersionTableMetaData.SchemaName, VersionTableMetaData.TableName); }
        }

        public void DeleteVersion(long version)
        {
            var expression = new DeleteDataExpression {TableName = VersionTableMetaData.TableName, SchemaName = VersionTableMetaData.SchemaName};
            expression.Rows.Add(new DeletionDataDefinition
            {
                new KeyValuePair<string, object>(VersionTableMetaData.ColumnName, version)
            });
            expression.ExecuteWith(_processor);
        }

        public IVersionTableMetaData GetVersionTableMetaData()
        {
            return VersionTableMetaData;
        }

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

        public void UpdateVersionInfo(long version)
        {
            UpdateVersionInfo(version, null);
        }

        public void UpdateVersionInfo(long version, string description)
        {
            var dataExpression = new InsertDataExpression();
            dataExpression.Rows.Add(CreateVersionInfoInsertionData(version, description));
            dataExpression.TableName = VersionTableMetaData.TableName;
            dataExpression.SchemaName = VersionTableMetaData.SchemaName;

            dataExpression.ExecuteWith(_processor);
        }

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
