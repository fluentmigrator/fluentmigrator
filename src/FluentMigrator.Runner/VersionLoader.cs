using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Versioning;
using FluentMigrator.VersionTableInfo;
using FluentMigrator.Runner.Initialization;

namespace FluentMigrator.Runner
{
    public class VersionLoader : IVersionLoader
    {
        private bool _versionSchemaMigrationAlreadyRun;
        private bool _versionMigrationAlreadyRun;
        private bool _versionUniqueMigrationAlreadyRun;
        private bool _versionDescriptionMigrationAlreadyRun;
        private IVersionInfo _versionInfo;
        private IMigrationConventions Conventions { get; set; }
        private IMigrationProcessor Processor { get; set; }
        protected Assembly Assembly { get; set; }
        public IVersionTableMetaData VersionTableMetaData { get; private set; }
        public IMigrationRunner Runner { get; set; }
        public VersionSchemaMigration VersionSchemaMigration { get; private set; }
        public IMigration VersionMigration { get; private set; }
        public IMigration VersionUniqueMigration { get; private set; }
        public IMigration VersionDescriptionMigration { get; private set; }
        
        public VersionLoader(IMigrationRunner runner, IRunnerContext runnerContext, Assembly assembly, IMigrationConventions conventions)
        {
            Runner = runner;
            Processor = runner.Processor;
            Assembly = assembly;

            Conventions = conventions;
            VersionTableMetaData = GetVersionTableMetaData(runnerContext);
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
            
            dataExpression.ExecuteWith(Processor);
        }

        public IVersionTableMetaData GetVersionTableMetaData(IRunnerContext runnerContext)
        {
            Type matchedType = TypeFinder
                .FindTypes(Assembly, runnerContext.Namespace, runnerContext.NestedNamespaces)
                .FirstOrDefault(t => Conventions.TypeIsVersionTableMetaData(t));

            if (matchedType == null)
            {
                return new DefaultVersionTableMetaData();
            }

            var versionTableMetaData = (IVersionTableMetaData)Activator.CreateInstance(matchedType);

            versionTableMetaData.ApplicationContext = runnerContext.ApplicationContext;

            return versionTableMetaData;
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
            get
            {
                return _versionInfo;
            }
            set
            {
                if (value == null)
                    throw new ArgumentException("Cannot set VersionInfo to null");

                _versionInfo = value;
            }
        }

        public bool AlreadyCreatedVersionSchema
        {
            get
            {
                return string.IsNullOrEmpty(VersionTableMetaData.SchemaName) ||
					   Processor.SchemaExists(VersionTableMetaData.SchemaName);
            }
        }

        public bool AlreadyCreatedVersionTable
        {
            get
            {
                return Processor.TableExists(VersionTableMetaData.SchemaName, VersionTableMetaData.TableName);
            }
        }

        public bool AlreadyMadeVersionUnique
        {
            get
            {
                return Processor.ColumnExists(VersionTableMetaData.SchemaName, VersionTableMetaData.TableName, VersionTableMetaData.AppliedOnColumnName);
            }
        }

        public bool AlreadyMadeVersionDescription
        {
            get
            {
                return Processor.ColumnExists(VersionTableMetaData.SchemaName, VersionTableMetaData.TableName, VersionTableMetaData.DescriptionColumnName);
            }
        }

        public bool OwnsVersionSchema
        {
            get
            {
                IVersionTableMetaDataExtended versionTableMetaDataExtended = VersionTableMetaData as IVersionTableMetaDataExtended;
                return versionTableMetaDataExtended == null || versionTableMetaDataExtended.OwnsSchema;
            }
        }

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

            var dataSet = Processor.ReadTableData(VersionTableMetaData.SchemaName, VersionTableMetaData.TableName);
            
            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                _versionInfo.AddAppliedMigration(long.Parse(row[VersionTableMetaData.ColumnName].ToString()));
            }
        }

        public void RemoveVersionTable()
        {
            var expression = new DeleteTableExpression { TableName = VersionTableMetaData.TableName, SchemaName = VersionTableMetaData.SchemaName };
            expression.ExecuteWith(Processor);

            if (OwnsVersionSchema && !string.IsNullOrEmpty(VersionTableMetaData.SchemaName))
            {
                var schemaExpression = new DeleteSchemaExpression { SchemaName = VersionTableMetaData.SchemaName };
                schemaExpression.ExecuteWith(Processor);
            }
        }

        public void DeleteVersion(long version)
        {
            var expression = new DeleteDataExpression { TableName = VersionTableMetaData.TableName, SchemaName = VersionTableMetaData.SchemaName };
            expression.Rows.Add(new DeletionDataDefinition
                                    {
                                        new KeyValuePair<string, object>(VersionTableMetaData.ColumnName, version)
                                    });
            expression.ExecuteWith(Processor);
        }
    }
}