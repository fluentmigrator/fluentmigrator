#region License
//
// Copyright (c) 2026, Fluent Migrator Project
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
using System.Collections.Generic;
using System.IO;

namespace FluentMigrator.Tests.Integration.CreationScript.Example
{
    /// <summary>
    /// Insert, Update and Delete, with rows as anonymous objects and as dictionaries.
    /// </summary>
    [Migration(4, "Sample data")]
    public class M0004_Data : Migration
    {
        public override void Up()
        {
            Insert.IntoTable("Customer")
                .Row(new { Id = 1, Email = "ada@example.com", DisplayName = "Ada Lovelace", Status = "active" })
                .Row(new { Id = 2, Email = "grace@example.com", DisplayName = "Grace Hopper", Status = "active", TaxNumber = "GH-1906" })
                .Row(new Dictionary<string, object>
                {
                    ["Id"] = 3,
                    ["Email"] = "alan@example.com",
                    ["DisplayName"] = "Alan Turing",
                    ["Status"] = "pending",
                });

            Insert.IntoTable("Supplier").Rows(
                new { Id = 1, Name = "Analytical Works" },
                new { Id = 2, Name = "Harvard Mark" });

            Insert.IntoTable("Category").Row(new { Id = 1, Name = "Engines" });

            Insert.IntoTable("Product")
                .Row(new
                {
                    Id = 1,
                    Sku = "SKU-0001",
                    Name = "Difference Engine",
                    SupplierId = 1,
                    CategoryId = 1,
                    Price = 99.50m,
                    Quantity = 1,
                    Stock = 10,
                    IsActive = true,
                    ReleasedOn = new DateTime(1991, 8, 25),
                    CountryCode = "GB",
                    Thumbnail = new byte[] { 1, 2, 3 },
                })
                .Row(new { Id = 2, Sku = "SKU-0002", Name = "Analytical Engine", SupplierId = 1, Price = 250m, Weight = 1.5, Ratio = 0.25f })
                .Row(new { Id = 3, Sku = "SKU-0003", Name = "Mark I", SupplierId = 2, Price = 10m, Shelf = 4, UpdatedAt = new DateTime(2026, 1, 2, 3, 4, 5) });

            Insert.IntoTable("CustomerOrder")
                .Row(new { Id = 1, CustomerId = 1, OrderedOn = new DateTime(2026, 1, 5), Total = 349.50m })
                .Row(new { Id = 2, CustomerId = 2, OrderedOn = new DateTime(2026, 2, 7), Total = 10m });

            Insert.IntoTable("OrderLine")
                .Row(new { OrderId = 1, LineNumber = 1, ProductId = 1, Quantity = 1, UnitPrice = 99.50m })
                .Row(new { OrderId = 1, LineNumber = 2, ProductId = 2, Quantity = 1, UnitPrice = 250m })
                .Row(new { OrderId = 2, LineNumber = 1, ProductId = 3, Quantity = 1, UnitPrice = 10m })
                .Row(new { OrderId = 2, LineNumber = 2, Quantity = 1, UnitPrice = 0m });

            Insert.IntoTable("Shipment")
                .Row(new { Id = 1, OrderId = 1, LineNumber = 1, ShippedOn = new DateTime(2026, 1, 6), Carrier = "Royal Mail" })
                .Row(new { Id = 2, OrderId = 2, LineNumber = 1, ShippedOn = new DateTime(2026, 2, 8), Carrier = "Pony Express" });

            Update.Table("Customer").Set(new { Status = "vip" }).Where(new { Id = 1 });
            Update.Table("Product").Set(new { IsActive = false, Summary = "Retired" }).Where(new { Sku = "SKU-0003" });
            Update.Table("OrderLine").Set(new { Quantity = 2 }).AllRows();

            Delete.FromTable("OrderLine").IsNull("ProductId");
            Delete.FromTable("Customer").Row(new { Id = 3 });
        }

        public override void Down()
        {
            Delete.FromTable("Shipment").AllRows();
            Delete.FromTable("OrderLine").AllRows();
            Delete.FromTable("CustomerOrder").AllRows();
            Delete.FromTable("Product").AllRows();
            Delete.FromTable("Category").AllRows();
            Delete.FromTable("Supplier").AllRows();
            Delete.FromTable("Customer").AllRows();
        }
    }

    /// <summary>
    /// Every way to run raw SQL that a script can hold.
    /// </summary>
    /// <remarks>
    /// The table is all lower case, so the same unquoted SQL names it on every database: PostgreSQL keeps the case
    /// FluentMigrator quoted, and Oracle, Firebird and Db2 fold unquoted names to upper case. Each statement ends with
    /// <c>;</c>: a script runs statements one after another, and FluentMigrator writes raw SQL as it is.
    /// </remarks>
    [Migration(5, "Raw SQL")]
    public class M0005_RawSql : Migration
    {
        public override void Up()
        {
            Create.Table("note")
                .WithColumn("id").AsInt32().NotNullable().PrimaryKey("pk_note")
                .WithColumn("body").AsString(200).NotNullable();

            Execute.Sql("INSERT INTO note (id, body) VALUES (1, 'Execute.Sql');", "A statement with a description");

            // $[name] writes the value as a literal of the target database.
            Execute.Sql(
                "INSERT INTO note (id, body) VALUES (2, $[body]);",
                new Dictionary<string, object> { ["body"] = "Execute.Sql with a value token" });

            // $(name) writes the text as it is.
            Execute.EmbeddedScript(
                "CreationScriptExampleNote.sql",
                new Dictionary<string, string> { ["source"] = "a text token" });

            Execute.Script(Path.Combine(AppContext.BaseDirectory, "Integration", "CreationScript", "Example", "ExecuteScriptNote.sql"));
        }

        public override void Down()
        {
            Delete.Table("note");
        }
    }
}
