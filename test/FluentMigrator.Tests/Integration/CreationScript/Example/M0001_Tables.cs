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

using System.Data;

namespace FluentMigrator.Tests.Integration.CreationScript.Example
{
    // Names stay within 31 characters: Firebird's limit, which FluentMigrator applies to Firebird 4 as well.

    /// <summary>
    /// Create.Table with descriptions, the column options, and a named primary key, unique constraint and default.
    /// </summary>
    [Migration(1, "Customers")]
    public class M0001_Customers : Migration
    {
        public override void Up()
        {
            // Descriptions: SQL Server extended properties, MySQL COMMENT, Oracle COMMENT ON. The PostgreSQL, Firebird
            // and Db2 generators leave them out.
            Create.Table("Customer")
                .WithDescription("People and companies that place orders")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey("PK_Customer")
                .WithColumn("Email").AsString(200).NotNullable().Unique("UQ_Customer_Email")
                .WithColumn("DisplayName").AsString(100).NotNullable().WithColumnDescription("Printed on invoices")
                .WithColumn("Status").AsAnsiString(20).NotNullable().WithDefaultValue("active")
                .WithColumn("Notes").AsString(2000).Nullable()
                .WithColumn("TaxNumber").AsAnsiString(20).Nullable()
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime);

            // A unique index that allows several NULLs. Oracle has no such option and Db2 only the opposite.
            IfDatabase(Db.SqlServer, Db.Postgres, Db.MySql, Db.Firebird)
                .Create.Index("UX_Customer_TaxNumber").OnTable("Customer")
                .OnColumn("TaxNumber").Unique().NullsDistinct();
        }

        public override void Down()
        {
            Delete.Table("Customer");
        }
    }

    /// <summary>
    /// Every column type, defaults from SystemMethods, a computed column, and the column-level unique, index and
    /// foreign key options.
    /// </summary>
    [Migration(2, "Products")]
    public class M0002_Products : Migration
    {
        public override void Up()
        {
            Create.Table("Supplier")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey("PK_Supplier")
                .WithColumn("Name").AsString(100).NotNullable();

            Create.UniqueConstraint("UQ_Supplier_Name").OnTable("Supplier").Column("Name");

            // The types every database has. Sized strings: on Db2 an unsized one is a CLOB, which can't be indexed.
            Create.Table("Product")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey("PK_Product")
                .WithColumn("Sku").AsFixedLengthAnsiString(12).NotNullable().Unique()
                .WithColumn("Name").AsString(150).NotNullable().Indexed("IX_Product_Name")
                .WithColumn("SupplierId").AsInt32().Nullable()
                    .ForeignKey("FK_Product_Supplier", "Supplier", "Id").OnDelete(Rule.SetNull)
                .WithColumn("CategoryId").AsInt32().Nullable()
                .WithColumn("Summary").AsAnsiString(500).Nullable()
                .WithColumn("Price").AsDecimal(10, 2).NotNullable().WithDefaultValue(0)
                .WithColumn("Weight").AsDouble().Nullable()
                .WithColumn("Ratio").AsFloat().Nullable()
                .WithColumn("Quantity").AsInt16().NotNullable().WithDefaultValue(0)
                .WithColumn("Stock").AsInt64().NotNullable().WithDefaultValue(0)
                .WithColumn("Shelf").AsByte().Nullable()
                .WithColumn("IsActive").AsBoolean().NotNullable().WithDefaultValue(true)
                .WithColumn("ReleasedOn").AsDate().Nullable()
                .WithColumn("ReleasedAt").AsTime().Nullable()
                .WithColumn("UpdatedAt").AsDateTime2().Nullable()
                .WithColumn("CountryCode").AsFixedLengthString(2).Nullable()
                .WithColumn("Thumbnail").AsBinary(256).Nullable()
                .WithColumn("Picture").AsBinary().Nullable()
                .WithColumn("Code").AsCustom("VARCHAR(10)").Nullable();

            // Create.Column, for the types and defaults only some databases have. Db2 has no money type, no type for
            // a string of any length, no GUID type and no NEWID(); MySQL, Firebird and Db2 have no time-zone type
            // FluentMigrator maps to, nor XML (except Db2).
            IfDatabase(Db.Except(Db.Db2)).Create.Column("ListPrice").OnTable("Product").AsCurrency().Nullable();
            IfDatabase(Db.Except(Db.Db2)).Create.Column("Body").OnTable("Product").AsString(int.MaxValue).Nullable();
            IfDatabase(Db.Except(Db.Db2)).Create.Column("ExternalId").OnTable("Product").AsGuid().Nullable();
            IfDatabase(Db.SqlServer, Db.Postgres, Db.Oracle).Create.Column("PublishedAt").OnTable("Product").AsDateTimeOffset().Nullable();
            IfDatabase(Db.SqlServer, Db.Postgres, Db.Oracle, Db.Db2).Create.Column("Spec").OnTable("Product").AsXml().Nullable();

            // PostgreSQL's NewGuid needs the uuid-ossp extension. MySQL rejects ADD COLUMN with a UUID() default
            // while binary logging is on (error 1674), so its NewGuid default is in a CREATE TABLE (M0023). Firebird
            // allows only literals and context variables as defaults, so its gen_uuid() is rejected (#2440). MySQL has no
            // CURRENT_USER default; Db2's UTC expression isn't allowed as a default.
            IfDatabase(Db.SqlServer, Db.Oracle)
                .Create.Column("TraceId").OnTable("Product").AsGuid().NotNullable().WithDefault(SystemMethods.NewGuid);
            IfDatabase(Db.SqlServer, Db.Postgres, Db.Oracle)
                .Create.Column("CheckedAt").OnTable("Product").AsDateTimeOffset().NotNullable().WithDefault(SystemMethods.CurrentDateTimeOffset);
            IfDatabase(Db.Except(Db.Db2))
                .Create.Column("SyncedAt").OnTable("Product").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);
            IfDatabase(Db.Except(Db.MySql))
                .Create.Column("CreatedBy").OnTable("Product").AsString(128).NotNullable().WithDefault(SystemMethods.CurrentUser);

            // A computed column. PostgreSQL quotes the names FluentMigrator created, so its expression must too. The
            // Db2 generator writes STORED, which Db2 doesn't accept.
            IfDatabase(Db.SqlServer, Db.MySql, Db.Oracle, Db.Firebird)
                .Create.Column("StockValue").OnTable("Product").AsDecimal(19, 2).Computed("Price * Stock", stored: true);
            IfDatabase(Db.Postgres)
                .Create.Column("StockValue").OnTable("Product").AsDecimal(19, 2).Computed("\"Price\" * \"Stock\"", stored: true);
        }

        public override void Down()
        {
            Delete.Table("Product");
            Delete.Table("Supplier");
        }
    }

    /// <summary>
    /// Foreign keys created three ways, composite keys, cascade rules, and index and unique-constraint options.
    /// </summary>
    [Migration(3, "Orders")]
    public class M0003_Orders : Migration
    {
        public override void Up()
        {
            // ReferencedBy: the foreign key from a table that already exists to this one.
            Create.Table("Category")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey("PK_Category")
                    .ReferencedBy("FK_Product_Category", "Product", "CategoryId")
                .WithColumn("Name").AsString(100).NotNullable();

            Create.Table("CustomerOrder")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey("PK_CustomerOrder")
                .WithColumn("CustomerId").AsInt32().NotNullable()
                    .ForeignKey("FK_Order_Customer", "Customer", "Id").OnDelete(Rule.Cascade)
                .WithColumn("OrderedOn").AsDate().NotNullable()
                .WithColumn("Total").AsDecimal(12, 2).NotNullable().WithDefaultValue(0);

            Create.Index("IX_Order_Customer_Date").OnTable("CustomerOrder")
                .OnColumn("CustomerId").Ascending()
                .OnColumn("OrderedOn").Descending();

            // A table without a primary key in Create.Table; Create.PrimaryKey adds a composite one.
            Create.Table("OrderLine")
                .WithColumn("OrderId").AsInt32().NotNullable()
                .WithColumn("LineNumber").AsInt16().NotNullable()
                .WithColumn("ProductId").AsInt32().Nullable()
                .WithColumn("Quantity").AsInt32().NotNullable()
                .WithColumn("UnitPrice").AsDecimal(10, 2).NotNullable();

            Create.PrimaryKey("PK_OrderLine").OnTable("OrderLine").Columns("OrderId", "LineNumber");

            Create.ForeignKey("FK_OrderLine_Order")
                .FromTable("OrderLine").ForeignColumn("OrderId")
                .ToTable("CustomerOrder").PrimaryColumn("Id")
                .OnDelete(Rule.Cascade);

            Create.ForeignKey("FK_OrderLine_Product")
                .FromTable("OrderLine").ForeignColumn("ProductId")
                .ToTable("Product").PrimaryColumn("Id")
                .OnDelete(Rule.SetNull);

            Create.Index("IX_OrderLine_Product").OnTable("OrderLine")
                .OnColumn("ProductId").Ascending()
                .WithOptions().NonClustered();

            Create.Table("Shipment")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey("PK_Shipment")
                .WithColumn("OrderId").AsInt32().NotNullable()
                .WithColumn("LineNumber").AsInt16().NotNullable()
                .WithColumn("ShippedOn").AsDate().NotNullable()
                .WithColumn("Carrier").AsAnsiString(30).Nullable();

            // A composite foreign key. Oracle and Db2 have no ON UPDATE.
            IfDatabase(Db.SqlServer, Db.Postgres, Db.MySql, Db.Firebird)
                .Create.ForeignKey("FK_Shipment_OrderLine")
                .FromTable("Shipment").ForeignColumns("OrderId", "LineNumber")
                .ToTable("OrderLine").PrimaryColumns("OrderId", "LineNumber")
                .OnDeleteOrUpdate(Rule.Cascade);
            IfDatabase(Db.Oracle, Db.Db2)
                .Create.ForeignKey("FK_Shipment_OrderLine")
                .FromTable("Shipment").ForeignColumns("OrderId", "LineNumber")
                .ToTable("OrderLine").PrimaryColumns("OrderId", "LineNumber")
                .OnDelete(Rule.Cascade);

            Create.UniqueConstraint("UQ_Shipment_Line_Date").OnTable("Shipment")
                .Columns("OrderId", "LineNumber", "ShippedOn");

            Create.Index("UX_Shipment_Carrier").OnTable("Shipment")
                .WithOptions().Unique()
                .OnColumn("Carrier").Ascending()
                .OnColumn("Id").Ascending();
        }

        public override void Down()
        {
            Delete.Table("Shipment");
            Delete.Table("OrderLine");
            Delete.Table("CustomerOrder");
            Delete.ForeignKey("FK_Product_Category").OnTable("Product");
            Delete.Table("Category");
        }
    }
}
