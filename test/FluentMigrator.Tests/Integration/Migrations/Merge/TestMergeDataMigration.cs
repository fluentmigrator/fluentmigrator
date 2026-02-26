#region License
//
// Copyright (c) 2007-2024, Fluent Migrator Project
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

namespace FluentMigrator.Tests.Integration.Migrations.Merge
{
    /// <summary>
    /// Migration demonstrating Merge functionality
    /// </summary>
    [Migration(20240101001)]
    public class TestMergeDataMigration : Migration
    {
        public override void Up()
        {
            // Create a test table first
            Create.Table("TestMergeTable")
                .WithColumn("Id").AsInt32().PrimaryKey()
                .WithColumn("Code").AsString(10).NotNullable()
                .WithColumn("Name").AsString(50).NotNullable()
                .WithColumn("Description").AsString(200).Nullable();

            // Insert some initial data
            Insert.IntoTable("TestMergeTable")
                .Row(new { Id = 1, Code = "ACTIVE", Name = "Active Status", Description = "Active record" })
                .Row(new { Id = 2, Code = "INACTIVE", Name = "Inactive Status", Description = "Inactive record" });

            // Now use Merge to update existing and insert new
            Merge.IntoTable("TestMergeTable")
                .Row(new { Id = 1, Code = "ACTIVE", Name = "Active Status Updated", Description = "Updated active record" })
                .Row(new { Id = 3, Code = "PENDING", Name = "Pending Status", Description = "New pending record" })
                .Match("Id");
        }

        public override void Down()
        {
            Delete.Table("TestMergeTable");
        }
    }
}