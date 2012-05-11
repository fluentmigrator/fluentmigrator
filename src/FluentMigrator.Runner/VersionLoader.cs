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
        public VersionLoader(IMigrationRunner runner, Assembly assembly, IMigrationConventions conventions) :
            this(runner, assembly, conventions, "") { }

        public VersionLoader(IMigrationRunner runner, Assembly assembly, IMigrationConventions conventions, string group)
        {
            Runner = runner;
            Processor = runner.Processor;
            Assembly = assembly;
            Group = group;

            Conventions = conventions;
            VersionTableMetaData = GetVersionTableMetaData();
            VersionMigration = new VersionMigration(VersionTableMetaData);
            GroupVersionMigration = new VersionGroupMigration(VersionTableMetaData);
            VersionSchemaMigration = new VersionSchemaMigration(VersionTableMetaData);
            VersionUniqueMigration = new VersionUniqueMigration(VersionTableMetaData);

            LoadVersionInfo();
        }

        protected VersionSchemaMigration VersionSchemaMigration { get; set; }

        private IVersionInfo _versionInfo;
        public IMigrationRunner Runner { get; set; }
        protected Assembly Assembly { get; set; }
        public IVersionTableMetaData VersionTableMetaData { get; private set; }
        private IMigrationConventions Conventions { get; set; }
        private IMigrationProcessor Processor { get; set; }
        private IMigration VersionMigration { get; set; }
        private IMigration VersionUniqueMigration { get; set; }
        private IMigration GroupVersionMigration { get; set; }
        private string Group { get; set; }

        public void UpdateVersionInfo(long version)
        {
            var dataExpression = new InsertDataExpression();
            dataExpression.Rows.Add(CreateVersionInfoInsertionData(version));
            dataExpression.TableName = VersionTableMetaData.TableName;
            dataExpression.SchemaName = VersionTableMetaData.SchemaName;
            dataExpression.ExecuteWith(Processor);
        }

        public IVersionTableMetaData GetVersionTableMetaData()
        {
            Type matchedType = Assembly.GetExportedTypes().Where(t => Conventions.TypeIsVersionTableMetaData(t)).FirstOrDefault();

            if (matchedType == null)
            {
                return new DefaultVersionTableMetaData();
            }

            return (IVersionTableMetaData)Activator.CreateInstance(matchedType);
        }


        protected virtual InsertionDataDefinition CreateVersionInfoInsertionData( long version )
        {
            return new InsertionDataDefinition
                       {
                           new KeyValuePair<string, object>(VersionTableMetaData.ColumnName, version),
                           new KeyValuePair<string, object>("AppliedOn", DateTime.UtcNow),
                           new KeyValuePair<string, object>(VersionTableMetaData.GroupName, Group)
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
                return Processor.SchemaExists(VersionTableMetaData.SchemaName);
            }
        }

        public bool AlreadyCreatedVersionTable
        {
            get
            {
                return Processor.TableExists(VersionTableMetaData.SchemaName, VersionTableMetaData.TableName);
            }
        }

        public bool AlreadyAppliedGroupMigration
        {
            get
            {
                return Processor.ColumnExists(VersionTableMetaData.SchemaName, VersionTableMetaData.TableName, VersionTableMetaData.GroupName);
            }
        }

        public bool AlreadyMadeVersionUnique
        {
            get
            {
                return Processor.ColumnExists(VersionTableMetaData.SchemaName, VersionTableMetaData.TableName, "AppliedOn");
            }
        }

        public void LoadVersionInfo()
        {
            bool tableCreated = false;

            if (!AlreadyCreatedVersionSchema)
                Runner.Up(VersionSchemaMigration);

            if (!AlreadyAppliedGroupMigration)                
                Runner.Up(GroupVersionMigration);

            if (!AlreadyCreatedVersionTable)
                Runner.Up(VersionMigration);

            if (!AlreadyAppliedGroupMigration)
            {
                Runner.Up(GroupVersionMigration);
                tableCreated = true;
            }

            if (!AlreadyMadeVersionUnique)
                Runner.Up(VersionUniqueMigration);
            
            _versionInfo = new VersionInfo();

            if (tableCreated) return;

            var dataSet = Processor.ReadTableData(VersionTableMetaData.SchemaName, VersionTableMetaData.TableName);

            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                if ( string.Equals( row[ VersionTableMetaData.GroupName ], Group ) )
                    _versionInfo.AddAppliedMigration( long.Parse( row[ VersionTableMetaData.ColumnName ].ToString() ) );
            }
        }

        public void RemoveVersionTable()
        {
            var expression = new DeleteTableExpression { TableName = VersionTableMetaData.TableName, SchemaName = VersionTableMetaData.SchemaName };
            expression.ExecuteWith(Processor);

            if (!string.IsNullOrEmpty(VersionTableMetaData.SchemaName))
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
                                        new KeyValuePair<string, object>(VersionTableMetaData.ColumnName, version),
                                        new KeyValuePair<string, object>(VersionTableMetaData.GroupName, Group)
                                    });
            expression.ExecuteWith(Processor);
        }
    }
}