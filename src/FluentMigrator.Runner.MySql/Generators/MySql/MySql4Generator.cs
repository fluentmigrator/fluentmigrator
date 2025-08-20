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

using System;
using System.Collections.Generic;
using System.Linq;

using FluentMigrator.Expressions;
using FluentMigrator.Generation;
using FluentMigrator.Runner.Generators.Generic;

using JetBrains.Annotations;

using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Generators.MySql
{
    public class MySql4Generator : GenericGenerator
    {
        public MySql4Generator()
            : this(new MySqlQuoter())
        {
        }

        public MySql4Generator(
            [NotNull] MySqlQuoter quoter)
            : this(quoter, new OptionsWrapper<GeneratorOptions>(new GeneratorOptions()))
        {
        }

        public MySql4Generator(
            [NotNull] MySqlQuoter quoter,
            [NotNull] IOptions<GeneratorOptions> generatorOptions)
            : this(
                new MySqlColumn(new MySql4TypeMap(), quoter),
                quoter,
                new EmptyDescriptionGenerator(),
                generatorOptions)
        {
        }

        protected MySql4Generator(
            [NotNull] IColumn column,
            [NotNull] IQuoter quoter,
            [NotNull] IDescriptionGenerator descriptionGenerator,
            [NotNull] IOptions<GeneratorOptions> generatorOptions)
            : base(column, quoter, descriptionGenerator, generatorOptions)
        {
        }

        public override string AlterColumn { get { return "ALTER TABLE {0} MODIFY COLUMN {1}"; } }
        public override string DeleteConstraint { get { return "ALTER TABLE {0} DROP {1}{2}"; } }

        /// <inheritdoc />
        public override string GeneratorId => GeneratorIdConstants.MySql4;

        /// <inheritdoc />
        public override List<string> GeneratorIdAliases => [GeneratorIdConstants.MySql4, GeneratorIdConstants.MySql];

        public override string Generate(CreateTableExpression expression)
        {
            if (string.IsNullOrEmpty(expression.TableName)) throw new ArgumentNullException(nameof(expression), @"expression.TableName cannot be empty");
            if (expression.Columns.Count == 0) throw new ArgumentException("You must specify at least one column");

            string errors = ValidateAdditionalFeatureCompatibility(expression.Columns.SelectMany(x => x.AdditionalFeatures));
            if (!string.IsNullOrEmpty(errors)) return errors;

            string quotedTableName = Quoter.QuoteTableName(expression.TableName);

            string tableDescription = string.IsNullOrEmpty(expression.TableDescription)
                ? string.Empty
                : string.Format(" COMMENT {0}", Quoter.QuoteValue(expression.TableDescription));

            return FormatStatement("CREATE TABLE {0} ({1}){2} ENGINE = INNODB",
                quotedTableName,
                Column.Generate(expression.Columns, quotedTableName),
                tableDescription);
        }

        public override string Generate(AlterTableExpression expression)
        {
            if (string.IsNullOrEmpty(expression.TableDescription))
                return base.Generate(expression);

            return FormatStatement("ALTER TABLE {0} COMMENT {1}", Quoter.QuoteTableName(expression.TableName), Quoter.QuoteValue(expression.TableDescription));
        }

        public override string Generate(DeleteIndexExpression expression)
        {
            return FormatStatement("DROP INDEX {0} ON {1}", Quoter.QuoteIndexName(expression.Index.Name), Quoter.QuoteTableName(expression.Index.TableName));
        }

        public override string Generate(RenameColumnExpression expression)
        {
            return FormatStatement(GenerateWithoutEndStatement(expression));
        }

        internal string GenerateWithoutEndStatement(RenameColumnExpression expression)
        {
            return string.Format("ALTER TABLE {0} CHANGE {1} {2}", Quoter.QuoteTableName(expression.TableName), Quoter.QuoteColumnName(expression.OldName), Quoter.QuoteColumnName(expression.NewName));
        }

        public override string Generate(AlterDefaultConstraintExpression expression)
        {
            // Available since MySQL 4.0.22 (2005)
            var defaultValue = ((MySqlColumn)Column).FormatDefaultValue(expression.DefaultValue);
            return FormatStatement(
                "ALTER TABLE {0} ALTER {1} SET {2}",
                Quoter.QuoteTableName(expression.TableName),
                Quoter.QuoteColumnName(expression.ColumnName),
                defaultValue);
        }

        public override string Generate(CreateSequenceExpression expression)
        {
            return CompatibilityMode.HandleCompatibility("Sequences is not supporteed for MySql");
        }

        public override string Generate(DeleteSequenceExpression expression)
        {
            return CompatibilityMode.HandleCompatibility("Sequences is not supporteed for MySql");
        }

        public override string Generate(DeleteConstraintExpression expression)
        {
            if (expression.Constraint.IsPrimaryKeyConstraint)
            {
                return FormatStatement(DeleteConstraint, Quoter.QuoteTableName(expression.Constraint.TableName), "PRIMARY KEY", "");
            }
            return FormatStatement(DeleteConstraint, Quoter.QuoteTableName(expression.Constraint.TableName), "INDEX ", Quoter.Quote(expression.Constraint.ConstraintName));
        }

        public override string Generate(DeleteForeignKeyExpression expression)
        {
            return FormatStatement(DeleteConstraint, Quoter.QuoteTableName(expression.ForeignKey.ForeignTable), "FOREIGN KEY ", Quoter.QuoteColumnName(expression.ForeignKey.Name));
        }

        public override string Generate(DeleteDefaultConstraintExpression expression)
        {
            // Available since MySQL 4.0.22 (2005)
            return FormatStatement(
                "ALTER TABLE {0} ALTER {1} DROP DEFAULT",
                Quoter.QuoteTableName(expression.TableName),
                Quoter.QuoteColumnName(expression.ColumnName));
        }
    }
}
