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

using FluentMigrator.Tests.Unit.Generators.Oracle;

namespace FluentMigrator.Tests.Integration.Migrations.Oracle.Issue1196
{
    [Migration(100)]
    // ReSharper disable once InconsistentNaming
    public class Migration_v100_AddSimpleTable : Migration
    {
        private const string TableName = "SimpleTable";

        public override void Up()
        {
            Create.Table(TableName)
                .WithColumn("Id")
                    .AsInt32()
                    .NotNullable()
                    .PrimaryKey()
                .WithColumn("LargeUnicodeString")
                    .AsString(int.MaxValue)
                    .NotNullable()
                .WithColumn("LargeString")
                    .AsAnsiString(int.MaxValue)
                    .NotNullable()
                .WithColumn("ShortString")
                    .AsString(50)
                    .NotNullable();

            Insert.IntoTable(TableName).Row(
                new
                {
                    Id = 1,
                    LargeUnicodeString = OracleDataTests.LargeString,
                    LargeString = OracleDataTests.LargeString,
                    ShortString = "meow"
                });
        }

        public override void Down()
        {
            Delete.Table(TableName);
        }
    }
}
