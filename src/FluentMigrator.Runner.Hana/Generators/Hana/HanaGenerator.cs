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
using FluentMigrator.Runner.Generators.Generic;

using JetBrains.Annotations;

using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Generators.Hana
{
    /// <summary>
    /// The SAP Hana SQL generator for FluentMigrator.
    /// </summary>
    [Obsolete("Hana support will go away unless someone in the community steps up to provide support.")]
    public class HanaGenerator : GenericGenerator
    {
        /// <inheritdoc />
        public HanaGenerator()
            : this(new HanaQuoter())
        {
        }

        /// <inheritdoc />
        public HanaGenerator(
            [NotNull] HanaQuoter quoter)
            : this(quoter, new OptionsWrapper<GeneratorOptions>(new GeneratorOptions()))
        {
        }

        /// <inheritdoc />
        public HanaGenerator(
            [NotNull] HanaQuoter quoter,
            [NotNull] IOptions<GeneratorOptions> generatorOptions)
            : base(new HanaColumn(quoter), quoter, new HanaDescriptionGenerator(), generatorOptions)
        {
        }

        /// <inheritdoc />
        public override string Generate(DeleteTableExpression expression)
        {
            if (expression.IfExists)
            {
                return CompatibilityMode.HandleCompatibility("If exists syntax is not supported");
            }
            return base.Generate(expression);
        }

        /// <inheritdoc />
        public override string Generate(CreateSequenceExpression expression)
        {
            var result = new StringBuilder("CREATE SEQUENCE ");
            var seq = expression.Sequence;

            result.AppendFormat(Quoter.QuoteSequenceName(seq.Name));

            if (seq.Increment.HasValue)
            {
                result.AppendFormat(" INCREMENT BY {0}", seq.Increment);
            }

            if (seq.MinValue.HasValue)
            {
                result.AppendFormat(" MINVALUE {0}", seq.MinValue);
            }

            if (seq.MaxValue.HasValue)
            {
                result.AppendFormat(" MAXVALUE {0}", seq.MaxValue);
            }

            if (seq.StartWith.HasValue)
            {
                result.AppendFormat(" START WITH {0}", seq.StartWith);
            }

            const long MINIMUM_CACHE_VALUE = 2;
            if (seq.Cache.HasValue)
            {
                if (seq.Cache.Value < MINIMUM_CACHE_VALUE)
                {
                    return CompatibilityMode.HandleCompatibility("Cache size must be greater than 1; if you intended to disable caching, set Cache to null.");
                }
                result.AppendFormat(" CACHE {0}", seq.Cache);
            }
            else
            {
                result.Append(" NO CACHE");
            }

            if (seq.Cycle)
            {
                result.Append(" CYCLE");
            }

            AppendSqlStatementEndToken(result);

            return result.ToString();
        }

        /// <inheritdoc />
        public override string AddColumn => "ALTER TABLE {0} ADD ({1})";
        /// <inheritdoc />
        public override string AlterColumn => "ALTER TABLE {0} ALTER ({1})";
        /// <inheritdoc />
        public override string DropColumn => "ALTER TABLE {0} DROP ({1})";
        /// <inheritdoc />
        public override string RenameColumn => "RENAME COLUMN {0}.{1} TO {2}";

        private string InnerGenerate(CreateTableExpression expression)
        {
            var tableName = Quoter.QuoteTableName(expression.TableName);
            return FormatStatement("CREATE COLUMN TABLE {0} ({1})", tableName, Column.Generate(expression.Columns, tableName));
        }

        /// <inheritdoc />
        public override string GeneratorId => GeneratorIdConstants.Hana;

        /// <inheritdoc />
        public override List<string> GeneratorIdAliases => new List<string> { GeneratorIdConstants.Hana };

        /// <inheritdoc />
        public override string Generate(CreateTableExpression expression)
        {
            var descriptionStatements = DescriptionGenerator.GenerateDescriptionStatements(expression);
            var statements = descriptionStatements as string[] ?? descriptionStatements.ToArray();

            if (!statements.Any())
                return InnerGenerate(expression);

            var wrappedCreateTableStatement = InnerGenerate(expression);
            var createTableWithDescriptionsBuilder = new StringBuilder(wrappedCreateTableStatement);

            foreach (var descriptionStatement in statements)
            {
                if (!string.IsNullOrEmpty(descriptionStatement))
                {
                    createTableWithDescriptionsBuilder.Append(descriptionStatement);
                }
            }

            return WrapInBlock(createTableWithDescriptionsBuilder.ToString());
        }

        /// <inheritdoc />
        public override string Generate(AlterTableExpression expression)
        {
            var descriptionStatement = DescriptionGenerator.GenerateDescriptionStatement(expression);

            return string.IsNullOrEmpty(descriptionStatement) ? base.Generate(expression) : descriptionStatement;
        }

        /// <inheritdoc />
        public override string Generate(CreateColumnExpression expression)
        {
            var descriptionStatement = DescriptionGenerator.GenerateDescriptionStatement(expression);

            if (string.IsNullOrEmpty(descriptionStatement))
                return base.Generate(expression);

            var wrappedCreateColumnStatement = base.Generate(expression);

            var createColumnWithDescriptionBuilder = new StringBuilder(wrappedCreateColumnStatement);
            createColumnWithDescriptionBuilder.Append(descriptionStatement);

            return WrapInBlock(createColumnWithDescriptionBuilder.ToString());
        }

        /// <inheritdoc />
        public override string Generate(AlterColumnExpression expression)
        {
            var descriptionStatement = DescriptionGenerator.GenerateDescriptionStatement(expression);

            if (string.IsNullOrEmpty(descriptionStatement))
                return base.Generate(expression);

            var wrappedAlterColumnStatement = base.Generate(expression);

            var alterColumnWithDescriptionBuilder = new StringBuilder(wrappedAlterColumnStatement);
            alterColumnWithDescriptionBuilder.Append(descriptionStatement);

            return WrapInBlock(alterColumnWithDescriptionBuilder.ToString());
        }

        /// <inheritdoc />
        public override string Generate(DeleteConstraintExpression expression)
        {
            if (expression.Constraint.IsPrimaryKeyConstraint)
            {
                return FormatStatement("ALTER TABLE {0} DROP PRIMARY KEY", Quoter.QuoteTableName(expression.Constraint.TableName));
            }

            return base.Generate(expression);
        }

        /// <inheritdoc />
        public override string Generate(AlterDefaultConstraintExpression expression)
        {
            return CompatibilityMode.HandleCompatibility("Default constraints are not supported");
        }

        /// <inheritdoc />
        public override string Generate(DeleteDefaultConstraintExpression expression)
        {
            return CompatibilityMode.HandleCompatibility("Default constraints are not supported");
        }

        private string WrapInBlock(string sql)
        {
            return string.IsNullOrEmpty(sql)
                ? string.Empty
                : FormatStatement("BEGIN {0} END", sql);
        }
    }
}
