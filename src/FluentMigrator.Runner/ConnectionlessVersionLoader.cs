using System;
using System.Collections.Generic;
using System.Linq;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;
using FluentMigrator.Runner.Versioning;
using FluentMigrator.VersionTableInfo;

namespace FluentMigrator.Runner
{
    public class ConnectionlessVersionLoader : IVersionLoader
    {
        private bool _versionsLoaded;

        public ConnectionlessVersionLoader(IMigrationRunner runner, IAssemblyCollection assemblies, IMigrationConventions conventions, long startVersion, long targetVersion)
        {
            Runner = runner;
            Assemblies = assemblies;
            Conventions = conventions;
            StartVersion = startVersion;
            TargetVersion = targetVersion;

            Processor = Runner.Processor;

            VersionInfo = new VersionInfo();
            VersionTableMetaData = GetVersionTableMetaData();
            VersionMigration = new VersionMigration(VersionTableMetaData);
            VersionSchemaMigration = new VersionSchemaMigration(VersionTableMetaData);
            VersionUniqueMigration = new VersionUniqueMigration(VersionTableMetaData);
            VersionDescriptionMigration = new VersionDescriptionMigration(VersionTableMetaData);

            LoadVersionInfo();
        }

        private IMigrationProcessor Processor { get; set; }
        protected IAssemblyCollection Assemblies { get; set; }
        public IMigrationConventions Conventions { get; set; }
        public long StartVersion { get; set; }
        public long TargetVersion { get; set; }
        public VersionSchemaMigration VersionSchemaMigration { get; private set; }
        public IMigration VersionMigration { get; private set; }
        public IMigration VersionUniqueMigration { get; private set; }
        public IMigration VersionDescriptionMigration { get; private set; }
        public IMigrationRunner Runner { get; set; }
        public IVersionInfo VersionInfo { get; set; }
        public IVersionTableMetaData VersionTableMetaData { get; set; }

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
            get { return Processor.TableExists(VersionTableMetaData.SchemaName, VersionTableMetaData.TableName); }
        }

        public void DeleteVersion(long version)
        {
            var expression = new DeleteDataExpression {TableName = VersionTableMetaData.TableName, SchemaName = VersionTableMetaData.SchemaName};
            expression.Rows.Add(new DeletionDataDefinition
            {
                new KeyValuePair<string, object>(VersionTableMetaData.ColumnName, version)
            });
            expression.ExecuteWith(Processor);
        }

        public IVersionTableMetaData GetVersionTableMetaData()
        {
            var matchedType = Assemblies.GetExportedTypes()
                .FilterByNamespace(Runner.RunnerContext.Namespace, Runner.RunnerContext.NestedNamespaces)
                .FirstOrDefault(t => Conventions.TypeIsVersionTableMetaData(t));

            if (matchedType == null)
            {
                return new DefaultVersionTableMetaData();
            }

            return (IVersionTableMetaData) Activator.CreateInstance(matchedType);
        }

        public void LoadVersionInfo()
        {
            if (_versionsLoaded)
            {
                return;
            }

            foreach (var migration in Runner.MigrationLoader.LoadMigrations())
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
            expression.ExecuteWith(Processor);

            if (!string.IsNullOrEmpty(VersionTableMetaData.SchemaName))
            {
                var schemaExpression = new DeleteSchemaExpression {SchemaName = VersionTableMetaData.SchemaName};
                schemaExpression.ExecuteWith(Processor);
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

            dataExpression.ExecuteWith(Processor);
        }

        protected virtual InsertionDataDefinition CreateVersionInfoInsertionData(long version, string description)
        {
            return new InsertionDataDefinition
            {
                new KeyValuePair<string, object>(VersionTableMetaData.ColumnName, version),
                new KeyValuePair<string, object>(VersionTableMetaData.AppliedOnColumnName, DateTime.UtcNow),
                new KeyValuePair<string, object>(VersionTableMetaData.DescriptionColumnName, description)
            };
        }
    }
}