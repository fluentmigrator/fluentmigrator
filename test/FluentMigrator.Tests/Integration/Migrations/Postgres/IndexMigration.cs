#region License
// Copyright (c) 2020, FluentMigrator Project
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

using FluentMigrator.Postgres;

namespace FluentMigrator.Tests.Integration.Migrations.Postgres
{
    [Migration(20201127080600)]
    public class IndexMigration : Migration
    {
        /// <inheritdoc />
        public override void Up()
        {
            Create.Table("Test")
                .WithColumn("Id").AsInt32().Identity().PrimaryKey().Unique()
                .WithColumn("Name").AsString(200).NotNullable()
                .WithColumn("ExternalId").AsGuid().NotNullable();

            Create.Index("IX_TEST_INDEX_Only")
                .OnTable("Test")
                .OnColumn("ExternalId").Ascending()
                .WithOptions()
                .AsOnly()
                .AsConcurrently()
                .Include("Name");
        }

        /// <inheritdoc />
        public override void Down()
        {
            Delete.Table("Test");
        }
    }
}
