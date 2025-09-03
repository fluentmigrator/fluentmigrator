#region License
//
// Copyright (c) 2018, Fluent Migrator Project
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
using System.Linq;
using System.Text;

using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Generic;
using FluentMigrator.Runner.Processors.Firebird;

using JetBrains.Annotations;

using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Generators.Firebird
{
    /// <summary>
    /// The Firebird SQL generator for FluentMigrator.
    /// </summary>
    public class FirebirdGenerator : GenericGenerator
    {
        /// <inheritdoc />
        [Obsolete("Use the Truncator property")]
        protected readonly FirebirdTruncator truncator;

        /// <inheritdoc />
        public FirebirdGenerator(
            [NotNull] FirebirdOptions fbOptions)
            : this(fbOptions, new OptionsWrapper<GeneratorOptions>(new GeneratorOptions()))
        {
        }

        /// <inheritdoc />
        public FirebirdGenerator(
            [NotNull] FirebirdOptions fbOptions,
            [NotNull] IOptions<GeneratorOptions> generatorOptions)
            : this(new FirebirdQuoter(fbOptions.ForceQuote), fbOptions, generatorOptions)
        {
        }

        /// <inheritdoc />
        public FirebirdGenerator(
            [NotNull] FirebirdQuoter quoter,
            [NotNull] FirebirdOptions fbOptions,
            [NotNull] IOptions<GeneratorOptions> generatorOptions)
            : base(new FirebirdColumn(fbOptions), quoter, new EmptyDescriptionGenerator(), generatorOptions)
        {
            FBOptions = fbOptions ?? throw new ArgumentNullException(nameof(fbOptions));
#pragma warning disable 618
            truncator = new FirebirdTruncator(FBOptions.TruncateLongNames, FBOptions.PackKeyNames);
#pragma warning restore 618
        }

        //It's kind of a hack to mess with system tables, but this is the cleanest and time-tested method to alter the nullable constraint.
        // It's even mentioned in the firebird official FAQ.
        // Currently the only drawback is that the integrity is not checked by the engine, you have to ensure it manually
        /// <summary>
        /// Gets the SQL for altering a column's nullable constraint in Firebird pre-3.0.
        /// </summary>
        public string AlterColumnSetNullablePre3 => "UPDATE RDB$RELATION_FIELDS SET RDB$NULL_FLAG = {0} WHERE lower(rdb$relation_name) = lower({1}) AND lower(RDB$FIELD_NAME) = lower({2})";

        /// <summary>
        /// Gets the SQL for altering a column's nullable constraint in Firebird 3.0+.
        /// </summary>
        /// <remarks>
        /// ALTER TABLE table_name ALTER column_name { DROP | SET } [NOT] NULL
        /// </remarks>
        public string AlterColumnSetNullable3 => "ALTER TABLE {0} ALTER {1} {2} {3}";

        /// <summary>
        /// Gets the SQL for altering a column's type.
        /// </summary>
        /// <remarks>
        /// Altering a column's nullable constraint is supported in Firebird 3.0+.
        /// </remarks>
        public string AlterColumnSetType => "ALTER TABLE {0} ALTER COLUMN {1} TYPE {2}";

        /// <inheritdoc />
        public override string AddColumn => "ALTER TABLE {0} ADD {1}";

        /// <inheritdoc />
        public override string DropColumn => "ALTER TABLE {0} DROP {1}";

        /// <inheritdoc />
        public override string RenameColumn => "ALTER TABLE {0} ALTER COLUMN {1} TO {2}";

        /// <inheritdoc />
        public override string DropTableIfExists => "IF( EXISTS( SELECT 1 FROM RDB$RELATIONS WHERE (rdb$flags IS NOT NULL) AND LOWER(RDB$RELATION_NAME) = LOWER('{0}'))) THEN EXECUTE STATEMENT 'DROP TABLE {0}')";

        /// <summary>
        /// Gets the Firebird options.
        /// </summary>
        protected FirebirdOptions FBOptions { get; }

        /// <inheritdoc />
#pragma warning disable 618
        public FirebirdTruncator Truncator => truncator;
#pragma warning restore 618

        /// <inheritdoc />
        public override string Generate(AlterDefaultConstraintExpression expression)
        {
            Truncator.Truncate(expression);
            return FormatStatement("ALTER TABLE {0} ALTER COLUMN {1} SET DEFAULT {2}",
                Quoter.QuoteTableName(expression.TableName),
                Quoter.QuoteColumnName(expression.ColumnName),
                Quoter.QuoteValue(expression.DefaultValue)
                );
        }

        /// <inheritdoc />
        public override string Generate(DeleteDefaultConstraintExpression expression)
        {
            Truncator.Truncate(expression);
            return FormatStatement("ALTER TABLE {0} ALTER COLUMN {1} DROP DEFAULT",
                Quoter.QuoteTableName(expression.TableName),
                Quoter.QuoteColumnName(expression.ColumnName)
                );
        }

        /// <inheritdoc />
        public override string Generate(CreateIndexExpression expression)
        {
            //Firebird doesn't have particular asc or desc order per column, only per the whole index
            // CREATE [UNIQUE] [ASC[ENDING] | [DESC[ENDING]] INDEX indexname
            //  ON tablename  { (<col> [, <col> ...]) | COMPUTED BY (expression) }
            //  <col>  ::=  a column not of type ARRAY, BLOB or COMPUTED BY
            //
            // Assuming the first column's direction for the index's direction.

            Truncator.Truncate(expression);

            StringBuilder indexColumns = new StringBuilder("");
            Direction indexDirection = Direction.Ascending;
            int columnCount = expression.Index.Columns.Count;
            for (int i = 0; i < columnCount; i++)
            {
                IndexColumnDefinition columnDef = expression.Index.Columns.ElementAt(i);

                if (i > 0)
                    indexColumns.Append(", ");
                else indexDirection = columnDef.Direction;

                indexColumns.Append(Quoter.QuoteColumnName(columnDef.Name));
            }

            return FormatStatement(CreateIndex,
                GetUniqueString(expression),
                indexDirection == Direction.Ascending ? "ASC " : "DESC ",
                Quoter.QuoteIndexName(expression.Index.Name),
                Quoter.QuoteTableName(expression.Index.TableName),
                indexColumns);
        }

        /// <inheritdoc />
        public override string Generate(AlterColumnExpression expression)
        {
            Truncator.Truncate(expression);
            if (expression.Column.ExpressionStored)
            {
                CompatibilityMode.HandleCompatibility("Stored computed columns are not supported");
            }
            return CompatibilityMode.HandleCompatibility("Alter column is not supported as expected");
        }

        /// <inheritdoc />
        public override string Generate(CreateSequenceExpression expression)
        {
            Truncator.Truncate(expression);
            return FormatStatement("CREATE SEQUENCE {0}", Quoter.QuoteSequenceName(expression.Sequence.Name));
        }

        /// <inheritdoc />
        public override string Generate(DeleteSequenceExpression expression)
        {
            Truncator.Truncate(expression);
            return FormatStatement("DROP SEQUENCE {0}", Quoter.QuoteSequenceName(expression.SequenceName));
        }

        /// <summary>
        /// Generates SQL to alter a sequence's starting value.
        /// </summary>
        /// <param name="sequence">The sequence definition.</param>
        /// <returns>The SQL statement.</returns>
        public string GenerateAlterSequence(SequenceDefinition sequence)
        {
            Truncator.Truncate(sequence);
            if (sequence.StartWith != null)
                return FormatStatement("ALTER SEQUENCE {0} RESTART WITH {1}", Quoter.QuoteSequenceName(sequence.Name), sequence.StartWith.ToString());

            return string.Empty;
        }

        /// <inheritdoc />
        public override string GeneratorId => GeneratorIdConstants.Firebird;

        /// <inheritdoc />
        public override List<string> GeneratorIdAliases => new List<string> { GeneratorIdConstants.Firebird };

        /// <inheritdoc />
        public override string Generate(CreateTableExpression expression)
        {
            Truncator.Truncate(expression);
            if (expression.Columns.Any(x => x.ExpressionStored))
            {
                CompatibilityMode.HandleCompatibility("Stored computed columns are not supported");
            }
            return base.Generate(expression);
        }

        /// <inheritdoc />
        public override string Generate(DeleteTableExpression expression)
        {
            Truncator.Truncate(expression);
            return base.Generate(expression);
        }

        /// <inheritdoc />
        public override string Generate(RenameTableExpression expression)
        {
            Truncator.Truncate(expression);
            return CompatibilityMode.HandleCompatibility("Rename table is not supported");
        }

        /// <inheritdoc />
        public override string Generate(CreateColumnExpression expression)
        {
            Truncator.Truncate(expression);
            if (expression.Column.ExpressionStored)
            {
                CompatibilityMode.HandleCompatibility("Stored computed columns are not supported");
            }
            return base.Generate(expression);
        }

        /// <inheritdoc />
        public override string Generate(DeleteColumnExpression expression)
        {
            Truncator.Truncate(expression);
            return base.Generate(expression);
        }

        /// <inheritdoc />
        public override string Generate(RenameColumnExpression expression)
        {
            Truncator.Truncate(expression);
            return base.Generate(expression);
        }

        /// <inheritdoc />
        public override string Generate(DeleteIndexExpression expression)
        {
            Truncator.Truncate(expression);
            return base.Generate(expression);
        }

        /// <inheritdoc />
        public override string Generate(CreateConstraintExpression expression)
        {
            Truncator.Truncate(expression);
            return base.Generate(expression);
        }

        /// <inheritdoc />
        public override string Generate(DeleteConstraintExpression expression)
        {
            Truncator.Truncate(expression);
            return base.Generate(expression);
        }

        /// <inheritdoc />
        public override string Generate(CreateForeignKeyExpression expression)
        {
            Truncator.Truncate(expression);
            return base.Generate(expression);
        }

        /// <inheritdoc />
        public override string GenerateForeignKeyName(ForeignKeyDefinition foreignKey)
        {
            Truncator.Truncate(foreignKey);
            return Truncator.Truncate(base.GenerateForeignKeyName(foreignKey));
        }

        /// <inheritdoc />
        public override string Generate(DeleteForeignKeyExpression expression)
        {
            Truncator.Truncate(expression);
            return base.Generate(expression);
        }

        /// <inheritdoc />
        public override string Generate(InsertDataExpression expression)
        {
            Truncator.Truncate(expression);
            return base.Generate(expression);
        }

        /// <inheritdoc />
        public override string Generate(UpdateDataExpression expression)
        {
            Truncator.Truncate(expression);
            return base.Generate(expression);
        }

        /// <inheritdoc />
        public override string Generate(DeleteDataExpression expression)
        {
            Truncator.Truncate(expression);
            return base.Generate(expression);
        }

        /// <summary>
        /// Generates SQL to set or drop the nullable constraint for Firebird pre-3.0.
        /// </summary>
        /// <param name="tableName">The table name.</param>
        /// <param name="column">The column definition.</param>
        /// <returns>The SQL statement.</returns>
        public virtual string GenerateSetNullPre3(string tableName, ColumnDefinition column)
        {
            Truncator.Truncate(column);
            return FormatStatement(AlterColumnSetNullablePre3,
                !column.IsNullable.HasValue || !column.IsNullable.Value ? "1" : "NULL",
                Quoter.QuoteValue(tableName),
                Quoter.QuoteValue(column.Name)
                );
        }

        /// <summary>
        /// Generates SQL to set or drop the nullable constraint for Firebird 3.0+.
        /// </summary>
        /// <param name="tableName">The table name.</param>
        /// <param name="column">The column definition.</param>
        /// <returns>The SQL statement.</returns>
        public virtual string GenerateSetNull3(string tableName, ColumnDefinition column)
        {
            Truncator.Truncate(column);
            var dropSet = !column.IsNullable.HasValue ? "DROP" : "SET";
            var nullable = column.IsNullable.GetValueOrDefault() ? "NULL" : "NOT NULL";
            return FormatStatement(AlterColumnSetNullable3,
                Quoter.QuoteTableName(tableName),
                Quoter.QuoteColumnName(column.Name),
                dropSet,
                nullable
            );
        }

        /// <summary>
        /// Generates SQL to alter a column's type.
        /// </summary>
        /// <param name="tableName">The table name.</param>
        /// <param name="column">The column definition.</param>
        /// <returns>The SQL statement.</returns>
        public virtual string GenerateSetType(string tableName, ColumnDefinition column)
        {
            Truncator.Truncate(column);
            return FormatStatement(AlterColumnSetType,
                Quoter.QuoteTableName(tableName),
                Quoter.QuoteColumnName(column.Name),
                ((FirebirdColumn)Column).GenerateForTypeAlter(column)
                );
        }

        /// <summary>
        /// Checks if two column definitions have matching types.
        /// </summary>
        /// <param name="col1">First column definition.</param>
        /// <param name="col2">Second column definition.</param>
        /// <returns>True if types match; otherwise, false.</returns>
        public static bool ColumnTypesMatch(ColumnDefinition col1, ColumnDefinition col2)
        {
            FirebirdColumn column = new FirebirdColumn(new FirebirdOptions());
            string colDef1 = column.GenerateForTypeAlter(col1);
            string colDef2 = column.GenerateForTypeAlter(col2);
            return colDef1 == colDef2;
        }

        /// <summary>
        /// Checks if two column definitions have matching default values.
        /// </summary>
        /// <param name="col1">First column definition.</param>
        /// <param name="col2">Second column definition.</param>
        /// <returns>True if default values match; otherwise, false.</returns>
        public static bool DefaultValuesMatch(ColumnDefinition col1, ColumnDefinition col2)
        {
            if (col1.DefaultValue is ColumnDefinition.UndefinedDefaultValue && col2.DefaultValue is ColumnDefinition.UndefinedDefaultValue)
                return true;
            if (col1.DefaultValue is ColumnDefinition.UndefinedDefaultValue || col2.DefaultValue is ColumnDefinition.UndefinedDefaultValue)
                return true;
            FirebirdColumn column = new FirebirdColumn(new FirebirdOptions());
            string col1Value = column.GenerateForDefaultAlter(col1);
            string col2Value = column.GenerateForDefaultAlter(col2);
            return col1Value != col2Value;
        }
    }
}
