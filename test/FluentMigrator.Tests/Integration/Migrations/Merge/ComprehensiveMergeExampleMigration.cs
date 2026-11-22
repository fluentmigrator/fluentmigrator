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
    /// Comprehensive example of Merge functionality for enumeration data
    /// </summary>
    [Migration(20240101002)]
    public class ComprehensiveMergeExampleMigration : Migration
    {
        public override void Up()
        {
            // Create enumeration tables for demonstration
            Create.Table("OrderStatus")
                .WithColumn("Id").AsInt32().PrimaryKey()
                .WithColumn("Code").AsString(20).NotNullable().Unique()
                .WithColumn("Name").AsString(100).NotNullable()
                .WithColumn("Description").AsString(500).Nullable()
                .WithColumn("IsActive").AsBoolean().NotNullable().WithDefaultValue(true)
                .WithColumn("SortOrder").AsInt32().NotNullable().WithDefaultValue(0);

            Create.Table("UserRoles")
                .WithColumn("Id").AsInt32().PrimaryKey()
                .WithColumn("Role").AsString(50).NotNullable()
                .WithColumn("Department").AsString(50).NotNullable()
                .WithColumn("Permissions").AsString(200).NotNullable()
                .WithColumn("Level").AsInt32().NotNullable();

            // Seed initial order status data using traditional Insert
            Insert.IntoTable("OrderStatus")
                .Row(new { Id = 1, Code = "NEW", Name = "New Order", Description = "Order has been created", IsActive = true, SortOrder = 1 })
                .Row(new { Id = 2, Code = "PROCESSING", Name = "Processing", Description = "Order is being processed", IsActive = true, SortOrder = 2 });

            // Now use Merge to add new statuses and update existing ones
            // This is the key benefit: you can update enumeration data without creating separate migrations
            Merge.IntoTable("OrderStatus")
                .Row(new { Id = 1, Code = "NEW", Name = "New Order", Description = "Order has been created and is awaiting processing", IsActive = true, SortOrder = 1 })
                .Row(new { Id = 2, Code = "PROCESSING", Name = "Processing Order", Description = "Order is currently being processed", IsActive = true, SortOrder = 2 })
                .Row(new { Id = 3, Code = "SHIPPED", Name = "Shipped", Description = "Order has been shipped to customer", IsActive = true, SortOrder = 3 })
                .Row(new { Id = 4, Code = "DELIVERED", Name = "Delivered", Description = "Order has been delivered", IsActive = true, SortOrder = 4 })
                .Row(new { Id = 5, Code = "CANCELLED", Name = "Cancelled", Description = "Order has been cancelled", IsActive = false, SortOrder = 99 })
                .Match("Id"); // Match on Id column

            // Example with multiple match columns - useful for composite keys
            Merge.IntoTable("UserRoles")
                .Row(new { Id = 1, Role = "Admin", Department = "IT", Permissions = "Full Access", Level = 10 })
                .Row(new { Id = 2, Role = "Manager", Department = "Sales", Permissions = "Manage Team,View Reports", Level = 8 })
                .Row(new { Id = 3, Role = "User", Department = "Sales", Permissions = "View Data", Level = 3 })
                .Row(new { Id = 4, Role = "Manager", Department = "HR", Permissions = "Manage Team,HR Functions", Level = 8 })
                .Match("Role", "Department"); // Match on combination of Role and Department

            // You could also use this in a schema
            // Merge.IntoTable("Categories").InSchema("Reference")
            //     .Row(new { Id = 1, Name = "Electronics" })
            //     .Match("Id");
        }

        public override void Down()
        {
            Delete.Table("UserRoles");
            Delete.Table("OrderStatus");
        }
    }
}