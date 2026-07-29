#region License
//
// Copyright (c) 2007-2024, Fluent Migrator Project
// Copyright (c) 2010, Nathan Brown
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

using System.Collections.Generic;
using System.Linq;

using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Generic;
using FluentMigrator.SQLite;

using JetBrains.Annotations;

using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Generators.SQLite
{
    /// <summary>
    /// The SQLite SQL generator for FluentMigrator.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class SQLiteGenerator : GenericGenerator
    {
        /// <inheritdoc />
        public SQLiteGenerator()
            : this(new SQLiteQuoter())
        {
        }

        /// <inheritdoc />
        public SQLiteGenerator(
            [NotNull] SQLiteQuoter quoter)
            : this(quoter, new OptionsWrapper<GeneratorOptions>(new GeneratorOptions()))
        {
        }

        /// <inheritdoc />
        public SQLiteGenerator(
            [NotNull] SQLiteQuoter quoter,
            [NotNull] ISQLiteTypeMap typeMap)
            : this(quoter, typeMap, new OptionsWrapper<GeneratorOptions>(new GeneratorOptions()))
        {

        }

        /// <inheritdoc />
        public SQLiteGenerator(
            [NotNull] SQLiteQuoter quoter,
            [NotNull] IOptions<GeneratorOptions> generatorOptions)
            // ReSharper disable once RedundantArgumentDefaultValue
            : this(quoter, new SQLiteTypeMap(false), generatorOptions)
        {
        }

        /// <inheritdoc />
        public SQLiteGenerator(
            [NotNull] SQLiteQuoter quoter,
            [NotNull] ISQLiteTypeMap typeMap,
            [NotNull] IOptions<GeneratorOptions> generatorOptions)
            : base(new SQLiteColumn(quoter, typeMap), quoter, new EmptyDescriptionGenerator(), generatorOptions)
        {
            CompatibilityMode = generatorOptions.Value.CompatibilityMode ?? CompatibilityMode.STRICT;
        }

        /// <inheritdoc />
        public override string RenameTable => "ALTER TABLE {0} RENAME TO {1}";

        /// <inheritdoc />
        public override string GeneratorId => GeneratorIdConstants.SQLite;

        /// <inheritdoc />
        public override List<string> GeneratorIdAliases => new List<string> { GeneratorIdConstants.SQLite };

        /// <inheritdoc />
        public override string Generate(CreateTableExpression expression)
        {
            var withoutRowId = expression.AdditionalFeatures.TryGetValue(SQLiteExtensions.WithoutRowIdTable, out var value)
                && value is true;

            if (withoutRowId)
            {
                if (!expression.Columns.Any(column => column.IsPrimaryKey))
                {
                    return CompatibilityMode.HandleCompatibility(
                        $"SQLite requires a PRIMARY KEY on tables created WITHOUT ROWID. Table '{expression.TableName}' is marked WITHOUT ROWID but defines no primary key.");
                }

                var identityColumns = expression.Columns
                    .Where(column => column.IsIdentity)
                    .Select(column => column.Name)
                    .ToList();

                if (identityColumns.Count > 0)
                {
                    return CompatibilityMode.HandleCompatibility(
                        $"SQLite does not allow AUTOINCREMENT (identity) columns on tables created WITHOUT ROWID. Table '{expression.TableName}' defines identity column(s): {string.Join(", ", identityColumns)}.");
                }
            }

            var createTable = base.Generate(expression);

            if (!withoutRowId || string.IsNullOrEmpty(createTable))
            {
                return createTable;
            }

            // Remove trailing semicolon (if any) before appending WITHOUT ROWID
            var trimmed = createTable.TrimEnd();
            if (trimmed.EndsWith(";"))
            {
                trimmed = trimmed.Substring(0, trimmed.Length - 1);
            }

            return trimmed + " WITHOUT ROWID;";
        }

        /// <inheritdoc />
        public override string Generate(AlterColumnExpression expression)
        {
            return CompatibilityMode.HandleCompatibility("SQLite does not support alter column");
        }

        /// <inheritdoc />
        public override string Generate(AlterDefaultConstraintExpression expression)
        {
            return CompatibilityMode.HandleCompatibility("SQLite does not support altering of default constraints");
        }

        /// <inheritdoc />
        /// <remarks>
        /// SQLite accepts foreign keys only as part of a <c>CREATE TABLE</c> statement - there is no
        /// <c>ALTER TABLE ... ADD CONSTRAINT</c>. Declaring the key on the column when the table is
        /// created does work and is generated inline; see <see cref="SQLiteColumn"/>.
        /// </remarks>
        public override string Generate(CreateForeignKeyExpression expression)
        {
            // If a FK name starts with $$IGNORE$$_ then it means it was handled by the CREATE TABLE
            // routine and we know it's been handled so we should just not bother erroring.
            if (expression.ForeignKey.Name.StartsWith("$$IGNORE$$_"))
                return string.Empty;

            return CompatibilityMode.HandleCompatibility(DescribeUnsupportedForeignKey(expression.ForeignKey));
        }

        /// <inheritdoc />
        /// <remarks>
        /// SQLite has no <c>ALTER TABLE ... DROP CONSTRAINT</c>, so an existing foreign key can only
        /// be removed by rebuilding the table without it.
        /// </remarks>
        public override string Generate(DeleteForeignKeyExpression expression)
        {
            var foreignKey = expression.ForeignKey;
            var name = string.IsNullOrEmpty(foreignKey?.Name) ? "The foreign key" : $"Foreign key '{foreignKey.Name}'";

            return CompatibilityMode.HandleCompatibility(
                $"{name} cannot be dropped: SQLite has no ALTER TABLE ... DROP CONSTRAINT. " +
                "Removing a foreign key requires rebuilding the table without it - create a replacement table, " +
                "copy the rows across, drop the original and rename. " +
                CompatibilityModeHint);
        }

        /// <summary>
        /// Explains why a standalone foreign key cannot be created on SQLite, and what to do instead.
        /// </summary>
        /// <param name="foreignKey">The foreign key that could not be generated</param>
        /// <returns>The message</returns>
        /// <remarks>
        /// The original message was just "Foreign keys are not supported in SQLite", which reads as
        /// though SQLite has no foreign keys at all. It does - they simply have to be declared when
        /// the table is created. Reporters of #1784 and #2117 both concluded FluentMigrator could not
        /// do foreign keys on SQLite when the working form was one call away.
        /// </remarks>
        private static string DescribeUnsupportedForeignKey(ForeignKeyDefinition foreignKey)
        {
            var name = string.IsNullOrEmpty(foreignKey?.Name) ? null : $"'{foreignKey.Name}' ";

            var foreignColumns = foreignKey?.ForeignColumns?.ToList() ?? new List<string>();
            var primaryColumns = foreignKey?.PrimaryColumns?.ToList() ?? new List<string>();

            // Only a single-column key can be shown as a concrete one-liner. A composite key needs
            // the table-level syntax, which FluentMigrator has no fluent equivalent for yet (#2090),
            // so the generic example is used rather than one that would not compile.
            var example = foreignColumns.Count == 1 && primaryColumns.Count == 1
                ? $"Create.Table(\"{foreignKey.ForeignTable}\").WithColumn(\"{foreignColumns[0]}\").AsInt32()" +
                  $".ForeignKey(\"{foreignKey.PrimaryTable}\", \"{primaryColumns[0]}\")"
                : "Create.Table(\"Child\").WithColumn(\"ParentId\").AsInt32().ForeignKey(\"Parent\", \"Id\")";

            return
                $"Foreign key {name}cannot be created with Create.ForeignKey on SQLite. SQLite accepts foreign keys " +
                "only inside a CREATE TABLE statement; it has no ALTER TABLE ... ADD CONSTRAINT. " +
                $"Declare the key on the column when the table is created - {example} - which FluentMigrator " +
                "generates inline and SQLite accepts. To add one to a table that already exists, rebuild the table: " +
                "create a replacement with the key declared, copy the rows across, drop the original and rename. " +
                CompatibilityModeHint;
        }

        /// <summary>
        /// The trailing sentence pointing at LOOSE compatibility mode, which downgrades these
        /// failures to a warning.
        /// </summary>
        private const string CompatibilityModeHint =
            "If you would rather these statements be skipped than fail, register SQLite with " +
            "AddSQLite(compatibilityMode: CompatibilityMode.LOOSE).";

        /// <inheritdoc />
        public override string Generate(CreateSequenceExpression expression)
        {
            return CompatibilityMode.HandleCompatibility("Sequences are not supported in SQLite");
        }

        /// <inheritdoc />
        public override string Generate(DeleteSequenceExpression expression)
        {
            return CompatibilityMode.HandleCompatibility("Sequences are not supported in SQLite");
        }

        /// <inheritdoc />
        public override string Generate(DeleteDefaultConstraintExpression expression)
        {
            return CompatibilityMode.HandleCompatibility("Default constraints are not supported in SQLite");
        }

        /// <inheritdoc />
        public override string Generate(CreateConstraintExpression expression)
        {
            if (!(expression.Constraint.IsUniqueConstraint || expression.Constraint.IsPrimaryKeyConstraint))
            {
                return CompatibilityMode.HandleCompatibility("Only creating UNIQUE and PRIMARY KEY constraints are supported in SQLite");
            }

            if (expression.Constraint.IsUniqueConstraint)
            {
                // Convert the constraint into a UNIQUE index
                var idx = new CreateIndexExpression();
                idx.Index.Name = expression.Constraint.ConstraintName;
                idx.Index.TableName = expression.Constraint.TableName;
                idx.Index.SchemaName = expression.Constraint.SchemaName;
                idx.Index.IsUnique = true;

                foreach (var col in expression.Constraint.Columns)
                    idx.Index.Columns.Add(new IndexColumnDefinition { Name = col });

                return Generate(idx);
            }

            return base.Generate(expression);
        }

        /// <inheritdoc />
        public override string Generate(DeleteConstraintExpression expression)
        {
            if (!expression.Constraint.IsUniqueConstraint)
                return CompatibilityMode.HandleCompatibility("Only deleting UNIQUE constraints are supported in SQLite");

            // Convert the constraint into a drop UNIQUE index
            var idx = new DeleteIndexExpression();
            idx.Index.Name = expression.Constraint.ConstraintName;
            idx.Index.SchemaName = expression.Constraint.SchemaName;

            return Generate(idx);
        }

        /// <inheritdoc />
        public override string Generate(CreateIndexExpression expression)
        {
            // SQLite prefixes the index name, rather than the table name with the schema

            var indexColumns = new string[expression.Index.Columns.Count];
            IndexColumnDefinition columnDef;

            for (var i = 0; i < expression.Index.Columns.Count; i++)
            {
                columnDef = expression.Index.Columns.ElementAt(i);
                if (columnDef.Direction == Direction.Ascending)
                {
                    indexColumns[i] = Quoter.QuoteColumnName(columnDef.Name) + " ASC";
                }
                else
                {
                    indexColumns[i] = Quoter.QuoteColumnName(columnDef.Name) + " DESC";
                }
            }

            return FormatStatement(CreateIndex
                , GetUniqueString(expression)
                , GetClusterTypeString(expression)
                , Quoter.QuoteIndexName(expression.Index.Name, expression.Index.SchemaName)
                , Quoter.QuoteTableName(expression.Index.TableName)
                , string.Join(", ", indexColumns));
        }

        /// <inheritdoc />
        public override string Generate(DeleteIndexExpression expression)
        {
            // SQLite prefixes the index name, rather than the table name with the schema

            return FormatStatement(DropIndex, Quoter.QuoteIndexName(expression.Index.Name, expression.Index.SchemaName));
        }
    }
}
