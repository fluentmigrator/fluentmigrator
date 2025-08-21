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

namespace FluentMigrator.Tests.IssueTests.GH2063.Migrations
{
    [Migration(version: 1)]
    public class TestMigration : Migration
    {
        /// <inheritdoc />
        public override void Up()
        {
            Create.Table("NamingConfig")
                .WithColumn("Id").AsGuid().PrimaryKey()
                .WithColumn("RenameEpisodes").AsInt32();

            Update.Table("NamingConfig")
                .Set(new { RenameEpisodes = 1 })
                .Where(new { RenameEpisodes = -1 });
        }

        /// <inheritdoc />
        public override void Down()
        {
            Delete.Table("NamingConfig");
        }
    }
}
