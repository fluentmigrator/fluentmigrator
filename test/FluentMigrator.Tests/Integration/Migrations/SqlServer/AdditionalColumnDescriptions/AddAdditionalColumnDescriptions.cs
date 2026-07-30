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

namespace FluentMigrator.Tests.Integration.Migrations.SqlServer.AdditionalColumnDescriptions
{
    /// <summary>
    /// Exercises <c>WithColumnAdditionalDescriptions</c> through all four fluent builders that
    /// implement it, so an end-to-end run verifies the additional descriptions are actually persisted.
    /// </summary>
    [Migration(1043_30_08_2026)]
    public class AddAdditionalColumnDescriptions : Migration
    {
        public const string TableName = "FmAdditionalColumnDescriptions";

        private static Dictionary<string, string> TwoAdditionalDescriptions => new Dictionary<string, string>
        {
            { "AdditionalColumnDescriptionKey1", "AdditionalColumnDescriptionValue1" },
            { "AdditionalColumnDescriptionKey2", "AdditionalColumnDescriptionValue2" },
        };

        public override void Up()
        {
            // 1) CreateTableExpressionBuilder.WithColumnAdditionalDescriptions (Col1)
            //    plus a plain Col4 that is altered further down.
            Create.Table(TableName)
                .WithColumn("Col1").AsInt32().Nullable()
                    .WithColumnDescription("Col1 description")
                    .WithColumnAdditionalDescriptions(TwoAdditionalDescriptions)
                .WithColumn("Col4").AsInt32().Nullable();

            // 2) CreateColumnExpressionBuilder.WithColumnAdditionalDescriptions (Col2)
            Create.Column("Col2").OnTable(TableName).AsInt32().Nullable()
                .WithColumnDescription("Col2 description")
                .WithColumnAdditionalDescriptions(TwoAdditionalDescriptions);

            // 3) AlterTableExpressionBuilder.WithColumnAdditionalDescriptions (Col3, via AddColumn)
            Alter.Table(TableName)
                .AddColumn("Col3").AsInt32().Nullable()
                    .WithColumnDescription("Col3 description")
                    .WithColumnAdditionalDescriptions(TwoAdditionalDescriptions);

            // 4) AlterColumnExpressionBuilder.WithColumnAdditionalDescriptions (Col4)
            Alter.Column("Col4").OnTable(TableName).AsInt64().Nullable()
                .WithColumnDescription("Col4 description")
                .WithColumnAdditionalDescriptions(TwoAdditionalDescriptions);
        }

        public override void Down()
        {
            Delete.Table(TableName);
        }
    }
}
