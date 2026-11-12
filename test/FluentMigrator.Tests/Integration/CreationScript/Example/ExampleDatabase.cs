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
using System.Data;
using System.Linq;
using System.Reflection;

using NUnit.Framework;

namespace FluentMigrator.Tests.Integration.CreationScript.Example
{
    /// <summary>
    /// An example database built with every part of FluentMigrator's fluent syntax that a creation script can hold,
    /// for <see cref="CreationScriptTests"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Where a database can't do something, the migration says so with <c>IfDatabase</c> and a comment, so the
    /// example also documents what each database supports. The migrations are numbered in the order they run.
    /// </para>
    /// <para>
    /// Left out, because a script can't hold them:
    /// <list type="bullet">
    /// <item><c>Schema.*.Exists()</c>: it reads the database while the migration is built; a run without a
    /// connection throws.</item>
    /// <item><c>Execute.WithConnection</c>: it runs .NET code against a connection; a preview logs that it would
    /// and leaves it out.</item>
    /// <item><c>MigrationConstraint</c> attributes: they read the version table when the run starts, which is
    /// empty for a creation script, so the script and a later live run would disagree.</item>
    /// <item><c>Down()</c>: a creation script only migrates up.</item>
    /// </list>
    /// </para>
    /// </remarks>
    public static class ExampleDatabase
    {
        public const string Profile = "Example";

        public static string[] Tags { get; } = ["Example"];

        /// <summary>
        /// Every migration in the example except the one whose tag isn't selected.
        /// </summary>
        public static IReadOnlyList<long> Versions { get; } = typeof(ExampleDatabase).Assembly.GetTypes()
            .Where(t => t.Namespace == typeof(ExampleDatabase).Namespace && t != typeof(M0016_NotSelectedByTag))
            .Select(t => t.GetCustomAttribute<MigrationAttribute>()?.Version)
            .Where(v => v != null)
            .Select(v => v.Value)
            .OrderBy(v => v)
            .ToList();

        /// <summary>
        /// The rows <c>note</c> ends up with: one per way of running raw SQL, migration kind and maintenance stage.
        /// </summary>
        public static IReadOnlyList<long> NoteIds { get; } = Enumerable.Range(1, 12).Select(i => (long)i).ToList();

        /// <summary>
        /// Checks the objects and data the example creates, after the script ran.
        /// </summary>
        public static void Verify(IMigrationProcessor processor)
        {
            // The same names IfDatabase matches.
            var names = new HashSet<string>(processor.DatabaseTypeAliases, StringComparer.OrdinalIgnoreCase) { processor.DatabaseType };
            bool On(params string[] databases) => databases.Any(names.Contains);

            void Table(string table, bool exists = true, string schema = null) =>
                Assert.That(processor.TableExists(schema, table), Is.EqualTo(exists), $"table {schema}.{table}");

            void Column(string table, string column, bool exists = true) =>
                Assert.That(processor.ColumnExists(null, table, column), Is.EqualTo(exists), $"column {table}.{column}");

            void Index(string table, string index) =>
                Assert.That(processor.IndexExists(null, table, index), Is.True, $"index {index} on {table}");

            void Constraint(string table, string constraint) =>
                Assert.That(processor.ConstraintExists(null, table, constraint), Is.True, $"constraint {constraint} on {table}");

            Assert.Multiple(
                () =>
                {
                    // 1-3: tables, keys, indexes
                    foreach (var table in new[] { "Customer", "Supplier", "Product", "Category", "CustomerOrder", "OrderLine", "Shipment", "note", "Tag" })
                    {
                        Table(table);
                    }

                    Constraint("Customer", "UQ_Customer_Email");
                    Constraint("Supplier", "UQ_Supplier_Name");
                    Constraint("CustomerOrder", "FK_Order_Customer");
                    Constraint("OrderLine", "FK_OrderLine_Order");
                    Constraint("Product", "FK_Product_Category");
                    Constraint("Shipment", "UQ_Shipment_Line_Date");
                    Index("Product", "IX_Product_Name");
                    Index("CustomerOrder", "IX_Order_Customer_Date");
                    Index("OrderLine", "IX_OrderLine_Product");
                    Index("Shipment", "UX_Shipment_Carrier");
                    Index("Tag", "IX_Tag_Label");

                    Column("Product", "Body", exists: !On(Db.Db2));
                    Column("Product", "Spec", exists: On(Db.SqlServer, Db.Postgres, Db.Oracle, Db.Db2));
                    Column("Product", "PublishedAt", exists: On(Db.SqlServer, Db.Postgres, Db.Oracle));
                    Column("Product", "StockValue", exists: !On(Db.Db2));

                    // 6: alterations and renames
                    Column("Customer", "Phone");
                    Column("Customer", "Tier");
                    Column("Product", "Barcode");
                    Column("Customer", "Remarks", exists: !On(Db.Db2));
                    Column("Customer", "Notes", exists: On(Db.Db2));
                    Table("Archive", exists: !On(Db.Firebird));
                    Table("Draft", exists: On(Db.Firebird));

                    // 7: everything it creates, it removes
                    Table("ImportBatch", exists: false);

                    // 8: sequences
                    if (!On(Db.MySql))
                    {
                        Assert.That(processor.SequenceExists(null, "InvoiceNumber"), Is.True, "sequence InvoiceNumber");
                        Assert.That(processor.SequenceExists(null, "ScratchSequence"), Is.False, "sequence ScratchSequence");
                    }

                    // 9: schemas
                    if (On(Db.SqlServer, Db.Postgres, Db.Db2))
                    {
                        Assert.That(processor.SchemaExists("reporting"), Is.True, "schema reporting");
                        Table("DailySales", schema: "reporting");
                    }

                    if (On(Db.SqlServer, Db.Postgres))
                    {
                        Assert.That(processor.SchemaExists("staging"), Is.False, "schema staging");
                        Table("Feeds", schema: "reporting");
                    }

                    // 10: identity
                    Table("AuditEntry", exists: !On(Db.Firebird));

                    // 16: not selected by its tag
                    Table("Excluded", exists: false);

                    // 20-23: provider extensions
                    Table("Document", exists: On(Db.SqlServer), schema: On(Db.SqlServer) ? "audit" : null);
                    Table("Event", exists: On(Db.Postgres));
                    Table("Ticket", exists: On(Db.Oracle));
                    Table("Session", exists: On(Db.MySql));
                });

            // 4: data, after the inserts, updates and deletes
            var customers = Rows(processor, "Customer");
            Assert.That(customers.Select(r => Long(r, "Id")), Is.EquivalentTo(new long[] { 1, 2 }), "Customer ids");
            Assert.That(customers.Single(r => Long(r, "Id") == 1)["Status"].ToString().Trim(), Is.EqualTo("vip"));
            Assert.That(customers.Select(r => Long(r, "Tier")), Is.All.EqualTo(1), "Customer.Tier set on existing rows");

            var lines = Rows(processor, "OrderLine");
            Assert.That(lines, Has.Count.EqualTo(3), "OrderLine rows, after deleting the one without a product");
            Assert.That(lines.Select(r => Long(r, "Quantity")), Is.All.EqualTo(2), "OrderLine.Quantity after updating all rows");

            var retired = Rows(processor, "Product").Single(r => r["Sku"].ToString().Trim() == "SKU-0003");
            Assert.That(Long(retired, "IsActive"), Is.EqualTo(0), "SKU-0003 retired");

            Assert.That(Rows(processor, "note").Select(r => Long(r, "id")), Is.EquivalentTo(NoteIds), "note ids");

            if (!On(Db.Firebird))
            {
                Assert.That(Rows(processor, "AuditEntry"), Has.Count.EqualTo(2), "AuditEntry rows numbered by the database");
            }

            var versionOne = Rows(processor, "VersionInfo").Single(r => Long(r, "Version") == 1);
            Assert.That(versionOne["Description"], Is.EqualTo("Customers"), "VersionInfo.Description");
        }

        private static List<DataRow> Rows(IMigrationProcessor processor, string table) =>
            processor.ReadTableData(null, table).Tables[0].Rows.Cast<DataRow>().ToList();

        // Each provider has its own types for numbers and booleans: decimal on Oracle, a string on Db2.
        private static long Long(DataRow row, string column)
        {
            var value = row[column];
            return value is bool b ? (b ? 1 : 0) : Convert.ToInt64(value);
        }
    }

    /// <summary>
    /// The names <c>IfDatabase</c> matches: every processor lists its family among its aliases, whether the run has
    /// a connection or not.
    /// </summary>
    public static class Db
    {
        public const string SqlServer = "SqlServer";
        public const string Postgres = "Postgres";
        public const string MySql = "MySql";
        public const string Oracle = "Oracle";
        public const string Firebird = "Firebird";
        public const string Db2 = "DB2";

        public static string[] All { get; } = [SqlServer, Postgres, MySql, Oracle, Firebird, Db2];

        public static string[] Except(params string[] databases) => All.Except(databases).ToArray();
    }
}
