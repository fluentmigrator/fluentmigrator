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

    public class FirebirdGenerator : GenericGenerator
    {
        // ReSharper disable once InconsistentNaming
        [Obsolete("Use the Truncator property")]
        protected readonly FirebirdTruncator truncator;

        public FirebirdGenerator(
            [NotNull] FirebirdOptions fbOptions)
            : this(fbOptions, new OptionsWrapper<GeneratorOptions>(new GeneratorOptions()))
        {
        }

        public FirebirdGenerator(
            [NotNull] FirebirdOptions fbOptions,
            [NotNull] IOptions<GeneratorOptions> generatorOptions)
            : this(new FirebirdQuoter(fbOptions.ForceQuote), fbOptions, generatorOptions)
        {
        }

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
        public string AlterColumnSetNullablePre3 { get { return "UPDATE RDB$RELATION_FIELDS SET RDB$NULL_FLAG = {0} WHERE lower(rdb$relation_name) = lower({1}) AND lower(RDB$FIELD_NAME) = lower({2})"; } }

        /// <summary>
        /// ALTER TABLE table_name ALTER column_name { DROP | SET } [NOT] NULL
        /// </summary>
        public string AlterColumnSetNullable3 { get { return "ALTER TABLE {0} ALTER {1} {2} {3}"; } }

        public string AlterColumnSetType { get { return "ALTER TABLE {0} ALTER COLUMN {1} TYPE {2}"; } }

        public override string AddColumn { get { return "ALTER TABLE {0} ADD {1}"; } }
        public override string DropColumn { get { return "ALTER TABLE {0} DROP {1}"; } }
        public override string RenameColumn { get { return "ALTER TABLE {0} ALTER COLUMN {1} TO {2}"; } }
        public override string DropTableIfExists { get { return "IF( EXISTS( SELECT 1 FROM RDB$RELATIONS WHERE (rdb$flags IS NOT NULL) AND LOWER(RDB$RELATION_NAME) = LOWER('{0}'))) THEN EXECUTE STATEMENT 'DROP TABLE {0}')"; } }

        protected FirebirdOptions FBOptions { get; }

#pragma warning disable 618
        public FirebirdTruncator Truncator => truncator;
#pragma warning restore 618

        public override string Generate(AlterDefaultConstraintExpression expression)
        {
            Truncator.Truncate(expression);
            return FormatStatement("ALTER TABLE {0} ALTER COLUMN {1} SET DEFAULT {2}",
                Quoter.QuoteTableName(expression.TableName),
                Quoter.QuoteColumnName(expression.ColumnName),
                Quoter.QuoteValue(expression.DefaultValue)
                );
        }

        public override string Generate(DeleteDefaultConstraintExpression expression)
        {
            Truncator.Truncate(expression);
            return FormatStatement("ALTER TABLE {0} ALTER COLUMN {1} DROP DEFAULT",
                Quoter.QuoteTableName(expression.TableName),
                Quoter.QuoteColumnName(expression.ColumnName)
                );
        }

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

            return FormatStatement(CreateIndex
                , GetUniqueString(expression)
                , indexDirection == Direction.Ascending ? "ASC " : "DESC "
                , Quoter.QuoteIndexName(expression.Index.Name)
                , Quoter.QuoteTableName(expression.Index.TableName)
                , indexColumns);
        }

        public override string Generate(AlterColumnExpression expression)
        {
            Truncator.Truncate(expression);
            if (expression.Column.ExpressionStored)
            {
                CompatibilityMode.HandleCompatibility("Stored computed columns are not supported");
            }
            return CompatibilityMode.HandleCompatibility("Alter column is not supported as expected");
        }


        public override string Generate(CreateSequenceExpression expression)
        {
            Truncator.Truncate(expression);
            return FormatStatement("CREATE SEQUENCE {0}", Quoter.QuoteSequenceName(expression.Sequence.Name));
        }

        public override string Generate(DeleteSequenceExpression expression)
        {
            Truncator.Truncate(expression);
            return FormatStatement("DROP SEQUENCE {0}", Quoter.QuoteSequenceName(expression.SequenceName));
        }

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

        public override string Generate(CreateTableExpression expression)
        {
            Truncator.Truncate(expression);
            if (expression.Columns.Any(x => x.ExpressionStored))
            {
                CompatibilityMode.HandleCompatibility("Stored computed columns are not supported");
            }
            return base.Generate(expression);
        }

        public override string Generate(DeleteTableExpression expression)
        {
            Truncator.Truncate(expression);
            return base.Generate(expression);
        }

        public override string Generate(RenameTableExpression expression)
        {
            Truncator.Truncate(expression);
            return CompatibilityMode.HandleCompatibility("Rename table is not supported");
        }

        public override string Generate(CreateColumnExpression expression)
        {
            Truncator.Truncate(expression);
            if (expression.Column.ExpressionStored)
            {
                CompatibilityMode.HandleCompatibility("Stored computed columns are not supported");
            }
            return base.Generate(expression);
        }

        public override string Generate(DeleteColumnExpression expression)
        {
            Truncator.Truncate(expression);
            return base.Generate(expression);
        }

        public override string Generate(RenameColumnExpression expression)
        {
            Truncator.Truncate(expression);
            return base.Generate(expression);
        }

        public override string Generate(DeleteIndexExpression expression)
        {
            Truncator.Truncate(expression);
            return base.Generate(expression);
        }

        public override string Generate(CreateConstraintExpression expression)
        {
            Truncator.Truncate(expression);
            return base.Generate(expression);
        }

        public override string Generate(DeleteConstraintExpression expression)
        {
            Truncator.Truncate(expression);
            return base.Generate(expression);
        }

        public override string Generate(CreateForeignKeyExpression expression)
        {
            Truncator.Truncate(expression);
            return base.Generate(expression);
        }

        public override string GenerateForeignKeyName(ForeignKeyDefinition foreignKey)
        {
            Truncator.Truncate(foreignKey);
            return Truncator.Truncate(base.GenerateForeignKeyName(foreignKey));
        }

        public override string Generate(DeleteForeignKeyExpression expression)
        {
            Truncator.Truncate(expression);
            return base.Generate(expression);
        }

        public override string Generate(InsertDataExpression expression)
        {
            Truncator.Truncate(expression);
            return base.Generate(expression);
        }

        public override string Generate(UpdateDataExpression expression)
        {
            Truncator.Truncate(expression);
            return base.Generate(expression);
        }

        public override string Generate(DeleteDataExpression expression)
        {
            Truncator.Truncate(expression);
            return base.Generate(expression);
        }

        public virtual string GenerateSetNullPre3(string tableName, ColumnDefinition column)
        {
            Truncator.Truncate(column);
            return FormatStatement(AlterColumnSetNullablePre3,
                !column.IsNullable.HasValue || !column.IsNullable.Value  ? "1" : "NULL",
                Quoter.QuoteValue(tableName),
                Quoter.QuoteValue(column.Name)
                );
        }

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

        public virtual string GenerateSetType(string tableName, ColumnDefinition column)
        {
            Truncator.Truncate(column);
            return FormatStatement(AlterColumnSetType,
                Quoter.QuoteTableName(tableName),
                Quoter.QuoteColumnName(column.Name),
                ((FirebirdColumn) Column).GenerateForTypeAlter(column)
                );
        }

        public static bool ColumnTypesMatch(ColumnDefinition col1, ColumnDefinition col2)
        {
            FirebirdColumn column = new FirebirdColumn(new FirebirdOptions());
            string colDef1 = column.GenerateForTypeAlter(col1);
            string colDef2 = column.GenerateForTypeAlter(col2);
            return colDef1 == colDef2;
        }

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
