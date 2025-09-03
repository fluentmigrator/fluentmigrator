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
using System.Data;

using FluentMigrator.Expressions;
using FluentMigrator.Generation;
using FluentMigrator.Model;
using FluentMigrator.Runner.Versioning;
using FluentMigrator.Runner.VersionTableInfo;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner.Conventions;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;

using JetBrains.Annotations;

namespace FluentMigrator.Runner
{
    /// <inheritdoc />
    public class VersionLoader : IVersionLoader
    {
        [NotNull]
        private readonly IMigrationProcessor _processor;

        private readonly IConventionSet _conventionSet;
        private readonly IQuoter _quoter;
        private bool _versionSchemaMigrationAlreadyRun;
        private bool _versionMigrationAlreadyRun;
        private bool _versionUniqueMigrationAlreadyRun;
        private bool _versionDescriptionMigrationAlreadyRun;
        private IVersionInfo _versionInfo;
        private IMigrationRunnerConventions Conventions { get; set; }

        /// <inheritdoc />
        public IVersionTableMetaData VersionTableMetaData { get; }

        /// <inheritdoc />
        [NotNull]
        public IMigrationRunner Runner { get; set; }
        /// <inheritdoc />
        public VersionSchemaMigration VersionSchemaMigration { get; }
        /// <inheritdoc />
        public IMigration VersionMigration { get; }
        /// <inheritdoc />
        public IMigration VersionUniqueMigration { get; }
        /// <inheritdoc />
        public IMigration VersionDescriptionMigration { get; }

        /// <inheritdoc />
        public VersionLoader(
            [NotNull] IProcessorAccessor processorAccessor,
            [NotNull] IGeneratorAccessor generatorAccessor,
            [NotNull] IConventionSet conventionSet,
            [NotNull] IMigrationRunnerConventions conventions,
            [NotNull] IVersionTableMetaData versionTableMetaData,
            [NotNull] IMigrationRunner runner)
        {
            _conventionSet = conventionSet;
            _processor = processorAccessor.Processor;
            _quoter = generatorAccessor.Generator.GetQuoter();

            Runner = runner;

            Conventions = conventions;
            VersionTableMetaData = versionTableMetaData;
            VersionMigration = new VersionMigration(VersionTableMetaData);
            VersionSchemaMigration = new VersionSchemaMigration(VersionTableMetaData);
            VersionUniqueMigration = new VersionUniqueMigration(VersionTableMetaData);
            VersionDescriptionMigration = new VersionDescriptionMigration(VersionTableMetaData);

            LoadVersionInfo();
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

        /// <inheritdoc />
        [NotNull]
        public IVersionTableMetaData GetVersionTableMetaData()
        {
            return VersionTableMetaData;
        }

        /// <inheritdoc />
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
                           new KeyValuePair<string, object>(VersionTableMetaData.DescriptionColumnName, description),
                       };
        }

        /// <inheritdoc />
        public IVersionInfo VersionInfo
        {
            get => _versionInfo;
            set => _versionInfo = value ?? throw new ArgumentException("Cannot set VersionInfo to null");
        }

        /// <inheritdoc />
        public bool AlreadyCreatedVersionSchema => string.IsNullOrEmpty(VersionTableMetaData.SchemaName) ||
            _processor.SchemaExists(VersionTableMetaData.SchemaName);

        /// <inheritdoc />
        public bool AlreadyCreatedVersionTable => _processor.TableExists(VersionTableMetaData.SchemaName, VersionTableMetaData.TableName);

        /// <inheritdoc />
        public bool AlreadyMadeVersionUnique => _processor.ColumnExists(VersionTableMetaData.SchemaName, VersionTableMetaData.TableName, VersionTableMetaData.AppliedOnColumnName);

        /// <inheritdoc />
        public bool AlreadyMadeVersionDescription => _processor.ColumnExists(VersionTableMetaData.SchemaName, VersionTableMetaData.TableName, VersionTableMetaData.DescriptionColumnName);

        /// <inheritdoc />
        public bool OwnsVersionSchema => VersionTableMetaData.OwnsSchema;

        /// <inheritdoc />
        public void LoadVersionInfo()
        {
            if (!AlreadyCreatedVersionSchema && !_versionSchemaMigrationAlreadyRun)
            {
                Runner.Up(VersionSchemaMigration);
                _versionSchemaMigrationAlreadyRun = true;
            }

            if (!AlreadyCreatedVersionTable && !_versionMigrationAlreadyRun)
            {
                Runner.Up(VersionMigration);
                _versionMigrationAlreadyRun = true;
            }

            if (!AlreadyMadeVersionUnique && !_versionUniqueMigrationAlreadyRun)
            {
                Runner.Up(VersionUniqueMigration);
                _versionUniqueMigrationAlreadyRun = true;
            }

            if (!AlreadyMadeVersionDescription && !_versionDescriptionMigrationAlreadyRun)
            {
                Runner.Up(VersionDescriptionMigration);
                _versionDescriptionMigrationAlreadyRun = true;
            }

            _versionInfo = new VersionInfo();

            if (!AlreadyCreatedVersionTable) return;

            var dataSet = _processor.ReadTableData(VersionTableMetaData.SchemaName, VersionTableMetaData.TableName);
            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                _versionInfo.AddAppliedMigration(long.Parse(row[VersionTableMetaData.ColumnName].ToString()));
            }
        }

        /// <inheritdoc />
        public void RemoveVersionTable()
        {
            var expression = new DeleteTableExpression { TableName = VersionTableMetaData.TableName, SchemaName = VersionTableMetaData.SchemaName };
            expression.ExecuteWith(_processor);

            if (OwnsVersionSchema && !string.IsNullOrEmpty(VersionTableMetaData.SchemaName))
            {
                var schemaExpression = new DeleteSchemaExpression { SchemaName = VersionTableMetaData.SchemaName };
                schemaExpression.ExecuteWith(_processor);
            }
        }

        /// <inheritdoc />
        public void DeleteVersion(long version)
        {
            var expression = new DeleteDataExpression { TableName = VersionTableMetaData.TableName, SchemaName = VersionTableMetaData.SchemaName };
            expression.Rows.Add(new DeletionDataDefinition
                                    {
                                        new KeyValuePair<string, object>(VersionTableMetaData.ColumnName, version)
                                    });
            expression.ExecuteWith(_processor);
        }
    }
}
