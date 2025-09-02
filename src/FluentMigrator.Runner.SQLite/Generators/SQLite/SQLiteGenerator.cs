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
        public override string Generate(CreateForeignKeyExpression expression)
        {
            // If a FK name starts with $$IGNORE$$_ then it means it was handled by the CREATE TABLE
            // routine and we know it's been handled so we should just not bother erroring.
            if (expression.ForeignKey.Name.StartsWith("$$IGNORE$$_"))
                return string.Empty;

            return CompatibilityMode.HandleCompatibility("Foreign keys are not supported in SQLite");
        }

        /// <inheritdoc />
        public override string Generate(DeleteForeignKeyExpression expression)
        {
            return CompatibilityMode.HandleCompatibility("Foreign keys are not supported in SQLite");
        }

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
