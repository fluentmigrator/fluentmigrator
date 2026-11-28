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

namespace FluentMigrator.Tests.SyntaxRoots
{
    /// <summary>
    /// A fluent syntax root that can set a table or column description (issue #2351).
    /// </summary>
    public enum DescriptionRoot
    {
        CreateTableWithDescription,
        CreateTableWithColumn,
        CreateTableIfNotExists,
        CreateColumn,
        AlterTableAddColumn,
        AlterTableAlterColumn,
        AlterColumn,
        AlterTableWithDescription,
    }

    /// <summary>
    /// One way to set a description: a migration written with one fluent syntax root, and the descriptions it sets.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Each root has its own builder, which maps its syntax to expressions and from there to SQL. Two roots that build
    /// the same expression type, such as <c>Create.Column(...)</c> and <c>Alter.Table(...).AddColumn(...)</c>, which
    /// both build a <c>CreateColumnExpression</c>, still go through different code, so each one is a scenario.
    /// </para>
    /// <para>
    /// The generator contract (<c>BaseDescriptionContractTests</c>) runs these through the runner without a database
    /// and checks the SQL. The processor integration tests (<c>BaseDescriptionTests</c>) run them on a real database and
    /// read the descriptions back from its catalog.
    /// </para>
    /// </remarks>
    public sealed class DescriptionScenario
    {
        public const string TableDescription = "Things the shop sells";

        public const string Column = "Name";

        public const string ColumnDescription = "What the thing is called";

        public const string AdditionalDescriptionName = "Owner";

        public const string AdditionalDescription = "Catalog team";

        private DescriptionScenario(
            DescriptionRoot root,
            string syntax,
            string tableName,
            bool describesTable,
            bool describesColumn,
            bool arrangeWithColumn,
            Action<Migration, string> act)
        {
            Root = root;
            Syntax = syntax;
            TableName = tableName;
            DescribesTable = describesTable;
            DescribesColumn = describesColumn;
            ArrangeWithColumn = arrangeWithColumn;
            _act = act;
        }

        private readonly Action<Migration, string> _act;

        public DescriptionRoot Root { get; }

        /// <summary>
        /// The syntax as a migration author writes it.
        /// </summary>
        public string Syntax { get; }

        /// <summary>
        /// A table name of this scenario's own, at most 30 characters for Oracle.
        /// </summary>
        public string TableName { get; }

        public bool DescribesTable { get; }

        /// <summary>
        /// Whether the scenario sets a description and an additional description on <see cref="Column"/>.
        /// </summary>
        public bool DescribesColumn { get; }

        /// <summary>
        /// Whether the table the scenario starts from already has <see cref="Column"/>, nullable, for the roots
        /// that alter it to <c>NOT NULL</c>.
        /// </summary>
        public bool ArrangeWithColumn { get; }

        /// <summary>
        /// Whether the scenario starts from an existing table. The <c>Create.Table</c> roots create it.
        /// </summary>
        public bool NeedsExistingTable => Root != DescriptionRoot.CreateTableWithDescription
            && Root != DescriptionRoot.CreateTableWithColumn
            && Root != DescriptionRoot.CreateTableIfNotExists;

        /// <summary>
        /// Every description text the scenario sets.
        /// </summary>
        public IEnumerable<string> ExpectedTexts
        {
            get
            {
                if (DescribesTable)
                {
                    yield return TableDescription;
                }

                if (DescribesColumn)
                {
                    yield return ColumnDescription;
                    yield return AdditionalDescription;
                }
            }
        }

        /// <summary>
        /// The table the scenario starts from, created without descriptions.
        /// </summary>
        public IMigration Arrange() => new DelegateMigration(
            m =>
            {
                var table = m.Create.Table(TableName)
                    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey("PK_" + TableName);
                if (ArrangeWithColumn)
                {
                    // Nullable, and altered to NOT NULL, so the alter changes it. Oracle rejects a MODIFY that restates
                    // the column's nullability (ORA-01451, ORA-01442), and Firebird rejects the SET NULL that FluentMigrator
                    // writes to make a column nullable; SET NOT NULL works on both.
                    table.WithColumn(Column).AsString(100).Nullable();
                }
            });

        /// <summary>
        /// The migration written with this scenario's syntax root.
        /// </summary>
        public IMigration Act() => new DelegateMigration(m => _act(m, TableName));

        public override string ToString() => Root.ToString();

        public static IReadOnlyList<DescriptionScenario> All { get; } = new[]
        {
            new DescriptionScenario(
                DescriptionRoot.CreateTableWithDescription,
                "Create.Table(...).WithDescription(...)",
                "FmDescTable",
                describesTable: true,
                describesColumn: false,
                arrangeWithColumn: false,
                (m, t) => m.Create.Table(t).WithDescription(TableDescription)
                    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey("PK_" + t)),
            new DescriptionScenario(
                DescriptionRoot.CreateTableWithColumn,
                "Create.Table(...).WithColumn(...).WithColumnDescription(...)",
                "FmDescTableColumn",
                describesTable: false,
                describesColumn: true,
                arrangeWithColumn: false,
                (m, t) => m.Create.Table(t)
                    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey("PK_" + t)
                    .WithColumn(Column).AsString(100).Nullable()
                    .WithColumnDescription(ColumnDescription)
                    .WithColumnAdditionalDescription(AdditionalDescriptionName, AdditionalDescription)),
            new DescriptionScenario(
                DescriptionRoot.CreateTableIfNotExists,
                "Create.Table(...).IfNotExists().WithDescription(...), with a column description",
                "FmDescIfNotExists",
                describesTable: true,
                describesColumn: true,
                arrangeWithColumn: false,
                (m, t) => m.Create.Table(t).IfNotExists().WithDescription(TableDescription)
                    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey("PK_" + t)
                    .WithColumn(Column).AsString(100).Nullable()
                    .WithColumnDescription(ColumnDescription)
                    .WithColumnAdditionalDescription(AdditionalDescriptionName, AdditionalDescription)),
            new DescriptionScenario(
                DescriptionRoot.CreateColumn,
                "Create.Column(...).OnTable(...)",
                "FmDescCreateColumn",
                describesTable: false,
                describesColumn: true,
                arrangeWithColumn: false,
                (m, t) => m.Create.Column(Column).OnTable(t).AsString(100).Nullable()
                    .WithColumnDescription(ColumnDescription)
                    .WithColumnAdditionalDescription(AdditionalDescriptionName, AdditionalDescription)),
            new DescriptionScenario(
                DescriptionRoot.AlterTableAddColumn,
                "Alter.Table(...).AddColumn(...)",
                "FmDescAddColumn",
                describesTable: false,
                describesColumn: true,
                arrangeWithColumn: false,
                (m, t) => m.Alter.Table(t).AddColumn(Column).AsString(100).Nullable()
                    .WithColumnDescription(ColumnDescription)
                    .WithColumnAdditionalDescription(AdditionalDescriptionName, AdditionalDescription)),
            new DescriptionScenario(
                DescriptionRoot.AlterTableAlterColumn,
                "Alter.Table(...).AlterColumn(...)",
                "FmDescAlterTableColumn",
                describesTable: false,
                describesColumn: true,
                arrangeWithColumn: true,
                (m, t) => m.Alter.Table(t).AlterColumn(Column).AsString(100).NotNullable()
                    .WithColumnDescription(ColumnDescription)
                    .WithColumnAdditionalDescription(AdditionalDescriptionName, AdditionalDescription)),
            new DescriptionScenario(
                DescriptionRoot.AlterColumn,
                "Alter.Column(...).OnTable(...)",
                "FmDescAlterColumn",
                describesTable: false,
                describesColumn: true,
                arrangeWithColumn: true,
                (m, t) => m.Alter.Column(Column).OnTable(t).AsString(100).NotNullable()
                    .WithColumnDescription(ColumnDescription)
                    .WithColumnAdditionalDescription(AdditionalDescriptionName, AdditionalDescription)),
            new DescriptionScenario(
                DescriptionRoot.AlterTableWithDescription,
                "Alter.Table(...).WithDescription(...)",
                "FmDescAlterTable",
                describesTable: true,
                describesColumn: false,
                arrangeWithColumn: false,
                (m, t) => m.Alter.Table(t).WithDescription(TableDescription)),
        };
    }
}
