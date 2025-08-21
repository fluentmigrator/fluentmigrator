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

namespace FluentMigrator.Tests.Integration.Migrations
{
    [Migration(8)]
    public class ComputedColumnMigration : Migration
    {
        public override void Up()
        {
            Create.Table("Products")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("Price").AsDecimal(10, 2).NotNullable()
                .WithColumn("Quantity").AsInt32().NotNullable()
                .WithColumn("Total").AsDecimal(10, 2).Computed("Price * Quantity");
                
            Alter.Table("Products")
                .AddColumn("TotalWithTax").AsDecimal(10, 2).Computed("Total * 1.1", true);
        }
        
        public override void Down()
        {
            Delete.Table("Products");
        }
    }
}
