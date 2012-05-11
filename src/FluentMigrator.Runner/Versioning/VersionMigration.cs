#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
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
using FluentMigrator.VersionTableInfo;

namespace FluentMigrator.Runner.Versioning
{
    public class VersionMigration : Migration
    {
        private IVersionTableMetaData _versionTableMetaData;

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

    /// <summary>
    /// Migration to extend the Version table to include a group name column.  All existing
    /// migrations are placed under the default group.
    /// </summary>
    public class VersionGroupMigration : Migration
    {
        private IVersionTableMetaData _versionTableMetaData;

        public VersionGroupMigration(IVersionTableMetaData versionTableMetaData)
		{
			_versionTableMetaData = versionTableMetaData;
		}

		public override void Up()
		{
            Alter.Table(_versionTableMetaData.TableName)
                .AddColumn(_versionTableMetaData.GroupName).AsString().NotNullable().WithDefaultValue(_versionTableMetaData.DefaultGroupName);
		}

		public override void Down()
		{
            Delete.Column(_versionTableMetaData.GroupName).FromTable(_versionTableMetaData.TableName);
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
        private readonly IVersionTableMetaData versionTableMeta;

        public VersionUniqueMigration(IVersionTableMetaData versionTableMeta)
        {
            this.versionTableMeta = versionTableMeta;
        }

        public override void Up()
        {
            Create.Index("UC_Version")
                .OnTable(versionTableMeta.TableName)
                .InSchema(versionTableMeta.SchemaName)
                .WithOptions().Unique()
                .WithOptions().Clustered()
                .OnColumn(versionTableMeta.ColumnName);

            Alter.Table(versionTableMeta.TableName).InSchema(versionTableMeta.SchemaName).AddColumn("AppliedOn").AsDateTime().Nullable();
        }

    }

    internal static class DateTimeExtensions
    {
        public static string ToISO8601(this DateTime dateTime)
        {
            return dateTime.ToString("u").Replace("Z", "");
        }
    }
}
