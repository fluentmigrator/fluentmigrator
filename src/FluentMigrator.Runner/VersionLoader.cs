using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Versioning;
using FluentMigrator.VersionTableInfo;

namespace FluentMigrator.Runner
{
    public class VersionLoader : IVersionLoader
    {
        private bool _versionSchemaMigrationAlreadyRun;
        private bool _versionMigrationAlreadyRun;
        private bool _versionUniqueMigrationAlreadyRun;
        private IVersionInfo _versionInfo;
        private IVersionTableMetaData _versionTableMetaData;
        private IMigration _versionMigration;
        private VersionSchemaMigration _versionSchemaMigration;
        private IMigration _versionUniqueMigration;

        private IMigrationConventions Conventions { get; set; }
        private IMigrationProcessor Processor { get; set; }
        protected Assembly Assembly { get; set; }

        public IMigrationRunner Runner { get; set; }
        
        public IVersionTableMetaData VersionTableMetaData
        {
            get { return _versionTableMetaData ?? (_versionTableMetaData = this.GetVersionTableMetaData()); }
        }

        public VersionSchemaMigration VersionSchemaMigration
        {
            get { return _versionSchemaMigration ?? (_versionSchemaMigration = new VersionSchemaMigration(VersionTableMetaData)); }
        }

        public IMigration VersionMigration
        {
            get { return _versionMigration ?? (_versionMigration = new VersionMigration(VersionTableMetaData)); }
        }
        
        public IMigration VersionUniqueMigration
        {
            get { return _versionUniqueMigration ?? (_versionUniqueMigration = new VersionUniqueMigration(VersionTableMetaData)); }
        }
        
        public VersionLoader(IMigrationRunner runner, Assembly assembly, IMigrationConventions conventions)
        {
            Runner = runner;
            Processor = runner.Processor;
            Assembly = assembly;
            Conventions = conventions;
        }

        public virtual void UpdateVersionInfo(long version)
        {
            var dataExpression = new InsertDataExpression();
            dataExpression.Rows.Add(CreateVersionInfoInsertionData(version));
            dataExpression.TableName = VersionTableMetaData.TableName;
            dataExpression.SchemaName = VersionTableMetaData.SchemaName;
            dataExpression.ExecuteWith(Processor);
        }

        public virtual IVersionTableMetaData GetVersionTableMetaData()
        {
            Type matchedType = Assembly.GetExportedTypes().FirstOrDefault(t => Conventions.TypeIsVersionTableMetaData(t));

            if (matchedType == null)
            {
                return new DefaultVersionTableMetaData();
            }

            return (IVersionTableMetaData)Activator.CreateInstance(matchedType);
        }

        protected virtual InsertionDataDefinition CreateVersionInfoInsertionData(long version)
        {
            return new InsertionDataDefinition
                       {
                           new KeyValuePair<string, object>(VersionTableMetaData.ColumnName, version),
                           new KeyValuePair<string, object>("AppliedOn", DateTime.UtcNow)
                       };
        }

        public IVersionInfo VersionInfo
        {
            get
            {
                if(_versionInfo == null)
                    LoadVersionInfo();
                return _versionInfo;
            }
            set
            {
                if (value == null)
                    throw new ArgumentException("Cannot set VersionInfo to null");

                _versionInfo = value;
            }
        }

        public virtual bool AlreadyCreatedVersionSchema
        {
            get
            {
                return Processor.SchemaExists(VersionTableMetaData.SchemaName);
            }
        }

        public virtual bool AlreadyCreatedVersionTable
        {
            get
            {
                return Processor.TableExists(VersionTableMetaData.SchemaName, VersionTableMetaData.TableName);
            }
        }

        public virtual bool AlreadyMadeVersionUnique
        {
            get
            {
                return Processor.ColumnExists(VersionTableMetaData.SchemaName, VersionTableMetaData.TableName, "AppliedOn");
            }
        }

        public virtual void LoadVersionInfo()
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

            _versionInfo = new VersionInfo();

            if (!AlreadyCreatedVersionTable) return;

            var dataSet = Processor.ReadTableData(VersionTableMetaData.SchemaName, VersionTableMetaData.TableName);
            
            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                _versionInfo.AddAppliedMigration(long.Parse(row[0].ToString()));
            }
        }
        
        public virtual void RemoveVersionTable()
        {
            var expression = new DeleteTableExpression { TableName = VersionTableMetaData.TableName, SchemaName = VersionTableMetaData.SchemaName };
            expression.ExecuteWith(Processor);

            if (!string.IsNullOrEmpty(VersionTableMetaData.SchemaName))
            {
                var schemaExpression = new DeleteSchemaExpression { SchemaName = VersionTableMetaData.SchemaName };
                schemaExpression.ExecuteWith(Processor);
            }
        }

        public virtual void DeleteVersion(long version)
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