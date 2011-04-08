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

    public class VersionSchemaMigration : Migration
    {
        private IVersionTableMetaData _versionTableMetaData;

        public VersionSchemaMigration(IVersionTableMetaData versionTableMetaData)
		{
			_versionTableMetaData = versionTableMetaData;
		}

        public override void Up()
        {
            if(!string.IsNullOrEmpty(_versionTableMetaData.SchemaName))
                Create.Schema(_versionTableMetaData.SchemaName);
        }

        public override void Down()
        {
            if(!string.IsNullOrEmpty(_versionTableMetaData.SchemaName))
                Delete.Schema(_versionTableMetaData.SchemaName);
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
