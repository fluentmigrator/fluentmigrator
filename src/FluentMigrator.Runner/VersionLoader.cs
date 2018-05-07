#region License
//
// Copyright (c) 2007-2018, Sean Chambers <schambers80@gmail.com>
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
using System.Reflection;

using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Versioning;
using FluentMigrator.Runner.VersionTableInfo;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner.Conventions;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;

using JetBrains.Annotations;

namespace FluentMigrator.Runner
{
    public class VersionLoader : IVersionLoader
    {
        [NotNull]
        private readonly IMigrationProcessor _processor;

        private readonly IConventionSet _conventionSet;
        private bool _versionSchemaMigrationAlreadyRun;
        private bool _versionMigrationAlreadyRun;
        private bool _versionUniqueMigrationAlreadyRun;
        private bool _versionDescriptionMigrationAlreadyRun;
        private IVersionInfo _versionInfo;
        private IMigrationRunnerConventions Conventions { get; set; }

        [CanBeNull]
        [Obsolete]
        protected IAssemblyCollection Assemblies { get; set; }

        public IVersionTableMetaData VersionTableMetaData { get; }

        [NotNull]
        public IMigrationRunner Runner { get; set; }
        public VersionSchemaMigration VersionSchemaMigration { get; }
        public IMigration VersionMigration { get; }
        public IMigration VersionUniqueMigration { get; }
        public IMigration VersionDescriptionMigration { get; }

        [Obsolete]
        internal VersionLoader(
            [NotNull] IMigrationRunner runner,
            [NotNull] Assembly assembly,
            [NotNull] IConventionSet conventionSet,
            [NotNull] IMigrationRunnerConventions conventions,
            [NotNull] IRunnerContext runnerContext)
            : this(runner, new SingleAssembly(assembly), conventionSet, conventions, runnerContext)
        {
        }

        [Obsolete]
        internal VersionLoader(IMigrationRunner runner, IAssemblyCollection assemblies,
            [NotNull] IConventionSet conventionSet,
            [NotNull] IMigrationRunnerConventions conventions,
            [NotNull] IRunnerContext runnerContext,
            [CanBeNull] IVersionTableMetaData versionTableMetaData = null)
        {
            _conventionSet = conventionSet;
            _processor = runner.Processor;

            Runner = runner;
            Assemblies = assemblies;

            Conventions = conventions;
            VersionTableMetaData = versionTableMetaData ?? CreateVersionTableMetaData(runnerContext);
            VersionMigration = new VersionMigration(VersionTableMetaData);
            VersionSchemaMigration = new VersionSchemaMigration(VersionTableMetaData);
            VersionUniqueMigration = new VersionUniqueMigration(VersionTableMetaData);
            VersionDescriptionMigration = new VersionDescriptionMigration(VersionTableMetaData);

            VersionTableMetaData.ApplicationContext = runnerContext.ApplicationContext;

            LoadVersionInfo();
        }

        public VersionLoader(
            [NotNull] IProcessorAccessor processorAccessor,
            [NotNull] IConventionSet conventionSet,
            [NotNull] IMigrationRunnerConventions conventions,
            [NotNull] IVersionTableMetaData versionTableMetaData,
            [NotNull] IMigrationRunner runner)
        {
            _conventionSet = conventionSet;
            _processor = processorAccessor.Processor;

            Runner = runner;

            Conventions = conventions;
            VersionTableMetaData = versionTableMetaData;
            VersionMigration = new VersionMigration(VersionTableMetaData);
            VersionSchemaMigration = new VersionSchemaMigration(VersionTableMetaData);
            VersionUniqueMigration = new VersionUniqueMigration(VersionTableMetaData);
            VersionDescriptionMigration = new VersionDescriptionMigration(VersionTableMetaData);

            LoadVersionInfo();
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

        [NotNull]
        public IVersionTableMetaData GetVersionTableMetaData()
        {
            return VersionTableMetaData;
        }

        protected virtual InsertionDataDefinition CreateVersionInfoInsertionData(long version, string description)
        {
            return new InsertionDataDefinition
                       {
                           new KeyValuePair<string, object>(VersionTableMetaData.ColumnName, version),
                           new KeyValuePair<string, object>(VersionTableMetaData.AppliedOnColumnName, DateTime.UtcNow),
                           new KeyValuePair<string, object>(VersionTableMetaData.DescriptionColumnName, description),
                       };
        }

        public IVersionInfo VersionInfo
        {
            get => _versionInfo;
            set => _versionInfo = value ?? throw new ArgumentException("Cannot set VersionInfo to null");
        }

        public bool AlreadyCreatedVersionSchema => string.IsNullOrEmpty(VersionTableMetaData.SchemaName) ||
            _processor.SchemaExists(VersionTableMetaData.SchemaName);

        public bool AlreadyCreatedVersionTable => _processor.TableExists(VersionTableMetaData.SchemaName, VersionTableMetaData.TableName);

        public bool AlreadyMadeVersionUnique => _processor.ColumnExists(VersionTableMetaData.SchemaName, VersionTableMetaData.TableName, VersionTableMetaData.AppliedOnColumnName);

        public bool AlreadyMadeVersionDescription => _processor.ColumnExists(VersionTableMetaData.SchemaName, VersionTableMetaData.TableName, VersionTableMetaData.DescriptionColumnName);

        public bool OwnsVersionSchema => VersionTableMetaData.OwnsSchema;

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

        public void DeleteVersion(long version)
        {
            var expression = new DeleteDataExpression { TableName = VersionTableMetaData.TableName, SchemaName = VersionTableMetaData.SchemaName };
            expression.Rows.Add(new DeletionDataDefinition
                                    {
                                        new KeyValuePair<string, object>(VersionTableMetaData.ColumnName, version)
                                    });
            expression.ExecuteWith(_processor);
        }

        [Obsolete]
        [NotNull]
        private IVersionTableMetaData CreateVersionTableMetaData(IRunnerContext runnerContext)
        {
            var type = Assemblies?.Assemblies.GetVersionTableMetaDataType(Conventions, runnerContext)
             ?? typeof(DefaultVersionTableMetaData);

            var instance = (IVersionTableMetaData) Activator.CreateInstance(type);
            if (instance is ISchemaExpression schemaExpression)
            {
                _conventionSet.SchemaConvention?.Apply(schemaExpression);
            }

            return instance;
        }
    }
}
