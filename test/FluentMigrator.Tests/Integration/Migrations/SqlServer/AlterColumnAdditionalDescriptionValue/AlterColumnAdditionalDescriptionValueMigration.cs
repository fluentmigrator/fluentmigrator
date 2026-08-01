#region License
// Copyright (c) 2018, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System.Collections.Generic;

namespace FluentMigrator.Tests.Integration.Migrations.SqlServer.AlterColumnAdditionalDescriptionValue
{
    [Migration(1145_28_07_2026)]
    public class AlterColumnAdditionalDescriptionValueMigration : Migration
    {
        public const string TableName = "FmAlterColumnAdditionalDescriptionValue";
        public const string ColumnName = "Data";
        public const string MainDescription = "Main column description";
        public const string Key1 = "AdditionalColumnDescriptionKey1";
        public const string Value1 = "AdditionalColumnDescriptionValue1";
        public const string Key2 = "AdditionalColumnDescriptionKey2";
        public const string Value2 = "AdditionalColumnDescriptionValue2";

        public override void Up()
        {
            Create.Table(TableName)
                .WithColumn(ColumnName).AsInt32().Nullable();

            Alter.Column(ColumnName).OnTable(TableName).AsInt64().Nullable()
                .WithColumnDescription(MainDescription)
                .WithColumnAdditionalDescriptions(new Dictionary<string, string>
                {
                    { Key1, Value1 },
                    { Key2, Value2 },
                });
        }

        public override void Down()
        {
            Delete.Table(TableName);
        }
    }
}
