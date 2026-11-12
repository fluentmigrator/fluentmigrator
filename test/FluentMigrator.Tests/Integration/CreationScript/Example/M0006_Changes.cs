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

namespace FluentMigrator.Tests.Integration.CreationScript.Example
{
    /// <summary>
    /// Alter.Table, Alter.Column, Create.Column, renames, and removing a default.
    /// </summary>
    /// <remarks>
    /// Firebird alters columns and renames tables in its processor, not its generator, so a script without a
    /// connection leaves them out. The Db2 generator can't rename a column, and its ALTER COLUMN isn't valid Db2.
    /// </remarks>
    [Migration(6, "Changes")]
    public class M0006_Changes : Migration
    {
        public override void Up()
        {
            Alter.Table("Customer")
                .AddColumn("Phone").AsString(30).Nullable()
                .AddColumn("Tier").AsInt32().Nullable().SetExistingRowsTo(1);

            Create.Column("Barcode").OnTable("Product").AsAnsiString(13).Nullable();

            Alter.Table("Product").WithDescription("Things we sell");

            // Widening, with rows in the table. Nullability isn't restated: Oracle rejects MODIFY to the
            // nullability a column already has.
            IfDatabase(Db.Except(Db.Firebird, Db.Db2)).Alter.Column("Quantity").OnTable("OrderLine").AsInt64();
            IfDatabase(Db.Except(Db.Firebird, Db.Db2)).Alter.Table("OrderLine").AlterColumn("UnitPrice").AsDecimal(12, 2);

            Delete.DefaultConstraint().OnTable("Customer").OnColumn("Status");

            IfDatabase(Db.Except(Db.Db2)).Rename.Column("Notes").OnTable("Customer").To("Remarks");

            Create.Table("Draft")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey("PK_Draft")
                .WithColumn("Title").AsString(100).Nullable();

            IfDatabase(Db.Except(Db.Firebird)).Rename.Table("Draft").To("Archive");
        }

        public override void Down()
        {
            throw new NotSupportedException("The example only migrates up.");
        }
    }

    /// <summary>
    /// The Delete side of the syntax: rows, a foreign key and an index found by their columns, a unique constraint,
    /// columns, a primary key and the table. Removing a table is a breaking change.
    /// </summary>
    [Migration(7, "Retire the import table", BreakingChange = true)]
    public class M0007_RetireImport : Migration
    {
        public override void Up()
        {
            // Unnamed: the conventions name the foreign key and the index, and Delete finds them by the same names.
            Create.Table("ImportBatch")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey("PK_ImportBatch")
                .WithColumn("CustomerId").AsInt32().Nullable().ForeignKey("Customer", "Id")
                .WithColumn("Code").AsAnsiString(20).NotNullable()
                .WithColumn("Comment1").AsString(100).Nullable()
                .WithColumn("Comment2").AsString(100).Nullable();

            Create.Index().OnTable("ImportBatch").OnColumn("CustomerId").Ascending();
            Create.UniqueConstraint("UQ_ImportBatch_Code").OnTable("ImportBatch").Column("Code");

            Insert.IntoTable("ImportBatch")
                .Row(new { Id = 1, CustomerId = 1, Code = "A" })
                .Row(new { Id = 2, Code = "B" });

            Delete.FromTable("ImportBatch").AllRows();

            // MySQL won't drop an index a foreign key uses, so the key goes first.
            Delete.ForeignKey().FromTable("ImportBatch").ForeignColumn("CustomerId").ToTable("Customer").PrimaryColumn("Id");
            Delete.Index().OnTable("ImportBatch").OnColumn("CustomerId");
            Delete.UniqueConstraint("UQ_ImportBatch_Code").FromTable("ImportBatch");
            Delete.Column("Comment1").Column("Comment2").FromTable("ImportBatch");
            Delete.PrimaryKey("PK_ImportBatch").FromTable("ImportBatch");
            Delete.Table("ImportBatch");
        }

        public override void Down()
        {
        }
    }

    /// <summary>
    /// Sequences with every option. MySQL has none; Firebird's generator writes only the name.
    /// </summary>
    [Migration(8, "Sequences")]
    public class M0008_Sequences : Migration
    {
        public override void Up()
        {
            var withSequences = IfDatabase(Db.Except(Db.MySql));

            withSequences.Create.Sequence("InvoiceNumber")
                .StartWith(1000).IncrementBy(10).MinValue(1000).MaxValue(9999999).Cache(20).Cycle();

            withSequences.Create.Sequence("ScratchSequence");
            withSequences.Delete.Sequence("ScratchSequence");
        }

        public override void Down()
        {
            IfDatabase(Db.Except(Db.MySql)).Delete.Sequence("InvoiceNumber");
        }
    }

    /// <summary>
    /// InSchema across the syntax, and moving a table between schemas. MySQL's schemas are databases, and Oracle's
    /// are users, so neither generator creates one; Firebird has none.
    /// </summary>
    [Migration(9, "Schemas")]
    public class M0009_Schemas : Migration
    {
        public override void Up()
        {
            var withSchemas = IfDatabase(Db.SqlServer, Db.Postgres, Db.Db2);

            withSchemas.Create.Schema("reporting");

            withSchemas.Create.Table("DailySales").InSchema("reporting")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey("PK_DailySales")
                .WithColumn("SalesDate").AsDate().NotNullable()
                .WithColumn("ProductId").AsInt32().NotNullable()
                .WithColumn("Amount").AsDecimal(12, 2).NotNullable();

            // From the reporting schema to the default one.
            withSchemas.Create.ForeignKey("FK_DailySales_Product")
                .FromTable("DailySales").InSchema("reporting").ForeignColumn("ProductId")
                .ToTable("Product").PrimaryColumn("Id");

            withSchemas.Create.Index("IX_DailySales_Date").OnTable("DailySales").InSchema("reporting")
                .OnColumn("SalesDate").Descending();
            withSchemas.Create.UniqueConstraint("UQ_DailySales_Day").OnTable("DailySales").WithSchema("reporting")
                .Columns("SalesDate", "ProductId");
            withSchemas.Create.Sequence("ReportRun").InSchema("reporting").StartWith(1);

            withSchemas.Alter.Table("DailySales").InSchema("reporting").AddColumn("Units").AsInt32().Nullable();

            withSchemas.Insert.IntoTable("DailySales").InSchema("reporting")
                .Row(new { Id = 1, SalesDate = new DateTime(2026, 1, 5), ProductId = 1, Amount = 99.50m, Units = 1 })
                .Row(new { Id = 2, SalesDate = new DateTime(2026, 1, 5), ProductId = 2, Amount = 250m, Units = 1 })
                .Row(new { Id = 3, SalesDate = new DateTime(2026, 2, 7), ProductId = 3, Amount = 10m, Units = 1 });
            withSchemas.Update.Table("DailySales").InSchema("reporting").Set(new { Units = 5 }).Where(new { Id = 1 });
            withSchemas.Delete.FromTable("DailySales").InSchema("reporting").Row(new { Id = 3 });

            // Db2's generator can't rename a column or move a table to another schema.
            var withSchemaMoves = IfDatabase(Db.SqlServer, Db.Postgres);

            withSchemaMoves.Rename.Column("Amount").OnTable("DailySales").InSchema("reporting").To("Revenue");
            withSchemaMoves.Create.Schema("staging");
            withSchemaMoves.Create.Table("Feed").InSchema("staging")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey("PK_Feed");
            withSchemaMoves.Alter.Table("Feed").InSchema("staging").ToSchema("reporting");
            withSchemaMoves.Rename.Table("Feed").InSchema("reporting").To("Feeds");
            withSchemaMoves.Delete.Schema("staging");
        }

        public override void Down()
        {
            throw new NotSupportedException("The example only migrates up.");
        }
    }

    /// <summary>
    /// Identity columns, with rows the database numbers.
    /// </summary>
    /// <remarks>
    /// Firebird's identity is a sequence and a trigger that its processor creates, so a script without a connection
    /// has neither, and an insert without an id fails.
    /// </remarks>
    [Migration(10, "Identity")]
    public class M0010_Identity : Migration
    {
        public override void Up()
        {
            var withIdentity = IfDatabase(Db.Except(Db.Firebird));

            withIdentity.Create.Table("AuditEntry")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey("PK_AuditEntry").Identity()
                .WithColumn("EventName").AsAnsiString(50).NotNullable()
                .WithColumn("RecordedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime);

            // One insert per row: Oracle writes a multi-row insert as INSERT ALL, which gives every row the same
            // identity value.
            withIdentity.Insert.IntoTable("AuditEntry").Row(new { EventName = "created" });
            withIdentity.Insert.IntoTable("AuditEntry").Row(new { EventName = "updated" });
        }

        public override void Down()
        {
            IfDatabase(Db.Except(Db.Firebird)).Delete.Table("AuditEntry");
        }
    }
}
