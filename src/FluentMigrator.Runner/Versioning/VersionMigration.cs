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

using FluentMigrator.Runner.VersionTableInfo;

namespace FluentMigrator.Runner.Versioning
{
    public class VersionMigration : Migration
    {
        private readonly IVersionTableMetaData _versionTableMetaData;

        public VersionMigration(IVersionTableMetaData versionTableMetaData)
        {
            _versionTableMetaData = versionTableMetaData;
        }

        public override void Up()
        {
            Create.Table(_versionTableMetaData.TableName)
                .InSchema(_versionTableMetaData.SchemaName)
                .WithColumn(_versionTableMetaData.ColumnName).AsInt64().NotNullable();
        }

        public override void Down()
        {
            Delete.Table(_versionTableMetaData.TableName).InSchema(_versionTableMetaData.SchemaName);
        }
    }

    public class VersionSchemaMigration : Migration
    {
        private IVersionTableMetaData _versionTableMetaData;

        public VersionSchemaMigration(IVersionTableMetaData versionTableMetaData)
        {
            _versionTableMetaData = versionTableMetaData;
        }

        public override void Up()
        {
            if (!string.IsNullOrEmpty(_versionTableMetaData.SchemaName))
                Create.Schema(_versionTableMetaData.SchemaName);
        }

        public override void Down()
        {
            if (!string.IsNullOrEmpty(_versionTableMetaData.SchemaName))
                Delete.Schema(_versionTableMetaData.SchemaName);
        }
    }

    public class VersionUniqueMigration : ForwardOnlyMigration
    {
        private readonly IVersionTableMetaData _versionTableMeta;

        public VersionUniqueMigration(IVersionTableMetaData versionTableMeta)
        {
            _versionTableMeta = versionTableMeta;
        }

        public override void Up()
        {
            Create.Index(_versionTableMeta.UniqueIndexName)
                .OnTable(_versionTableMeta.TableName)
                .InSchema(_versionTableMeta.SchemaName)
                .WithOptions().Unique()
                .WithOptions().Clustered()
                .OnColumn(_versionTableMeta.ColumnName);

            Alter.Table(_versionTableMeta.TableName).InSchema(_versionTableMeta.SchemaName).AddColumn(_versionTableMeta.AppliedOnColumnName).AsDateTime().Nullable();
        }

    }

    public class VersionDescriptionMigration : Migration
    {
        private readonly IVersionTableMetaData _versionTableMeta;

        public VersionDescriptionMigration(IVersionTableMetaData versionTableMeta)
        {
            _versionTableMeta = versionTableMeta;
        }

        public override void Up()
        {
            Alter.Table(_versionTableMeta.TableName).InSchema(_versionTableMeta.SchemaName)
                .AddColumn(_versionTableMeta.DescriptionColumnName).AsString(1024).Nullable();
        }

        public override void Down()
        {
            Delete.Column(_versionTableMeta.DescriptionColumnName)
                  .FromTable(_versionTableMeta.TableName).InSchema(_versionTableMeta.SchemaName);
        }
    }
}
