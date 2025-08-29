#region License
// Copyright (c) 2007-2024, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System.Collections.Generic;
using System.Linq;
using System.Text;

using FluentMigrator.Exceptions;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Generic;
using FluentMigrator.Runner.Processors.Snowflake;

using JetBrains.Annotations;

using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Generators.Snowflake
{
    public class SnowflakeGenerator : GenericGenerator
    {
        public SnowflakeGenerator(
            [NotNull] SnowflakeOptions sfOptions)
            : this(sfOptions, new OptionsWrapper<GeneratorOptions>(new GeneratorOptions())) { }

        public SnowflakeGenerator(
            [NotNull] SnowflakeOptions sfOptions,
            [NotNull] IOptions<GeneratorOptions> generatorOptions)
            : this(new SnowflakeQuoter(sfOptions.QuoteIdentifiers), sfOptions, generatorOptions) { }

        public SnowflakeGenerator(
            [NotNull] SnowflakeQuoter quoter,
            [NotNull] SnowflakeOptions sfOptions,
            [NotNull] IOptions<GeneratorOptions> generatorOptions)
            : base(new SnowflakeColumn(sfOptions), quoter, new SnowflakeDescriptionGenerator(quoter), generatorOptions)
        {
        }

        /// <inheritdoc />
        public override string AlterColumn => "ALTER TABLE {0} ALTER {1}";

        /// <inheritdoc />
        public override string Generate(AlterDefaultConstraintExpression expression)
        {
            throw new DatabaseOperationNotSupportedException("Snowflake database does not support adding or changing default constraint after column has been created.");
        }

        /// <inheritdoc />
        public override string Generate(DeleteDefaultConstraintExpression expression)
        {
            return FormatStatement($"ALTER TABLE {Quoter.QuoteTableName(expression.TableName, expression.SchemaName)} ALTER COLUMN {Quoter.QuoteColumnName(expression.ColumnName)} DROP DEFAULT");
        }

        /// <inheritdoc />
        public override string GeneratorId => GeneratorIdConstants.Snowflake;

        /// <inheritdoc />
        public override List<string> GeneratorIdAliases => new List<string> { GeneratorIdConstants.Snowflake };

        /// <inheritdoc />
        public override string Generate(CreateSchemaExpression expression)
        {
            return FormatStatement(CreateSchema, Quoter.QuoteSchemaName(expression.SchemaName));
        }

        /// <inheritdoc />
        public override string Generate(DeleteSchemaExpression expression)
        {
            return FormatStatement(DropSchema, Quoter.QuoteSchemaName(expression.SchemaName));
        }

        /// <inheritdoc />
        public override string Generate(DeleteTableExpression expression)
        {
            return FormatStatement($"DROP TABLE{(expression.IfExists ? " IF EXISTS" : "")} {Quoter.QuoteTableName(expression.TableName, expression.SchemaName)}");
        }

        /// <inheritdoc />
        public override string Generate(CreateTableExpression expression)
        {
            if (expression.Columns.Any(x => x.Expression != null))
            {
                CompatibilityMode.HandleCompatibility("Computed columns are not supported");
            }
            return base.Generate(expression);
        }

        /// <inheritdoc />
        public override string Generate(CreateColumnExpression expression)
        {
            if (expression.Column.Expression != null)
            {
                CompatibilityMode.HandleCompatibility("Computed columns are not supported");
            }
            return base.Generate(expression);
        }

        /// <inheritdoc />
        public override string Generate(AlterColumnExpression expression)
        {
            if (expression.Column.Expression != null)
            {
                CompatibilityMode.HandleCompatibility("Computed columns are not supported");
            }
            if (!(expression.Column.DefaultValue is ColumnDefinition.UndefinedDefaultValue))
            {
                throw new DatabaseOperationNotSupportedException("Snowflake database does not support adding or changing default constraint after column has been created.");
            }

            var errors = ValidateAdditionalFeatureCompatibility(expression.Column.AdditionalFeatures);
            if (!string.IsNullOrEmpty(errors))
            {
                return errors;
            }

            return FormatStatement(AlterColumn, Quoter.QuoteTableName(expression.TableName, expression.SchemaName), ((SnowflakeColumn)Column).GenerateAlterColumn(expression.Column));
        }

        /// <inheritdoc />
        public override string Generate(RenameTableExpression expression)
        {
            return FormatStatement($"ALTER TABLE {Quoter.QuoteTableName(expression.OldName, expression.SchemaName)} RENAME TO {Quoter.QuoteTableName(expression.NewName, expression.SchemaName)}");
        }

        /// <inheritdoc />
        public override string Generate(AlterSchemaExpression expression)
        {
            return FormatStatement($"ALTER TABLE {Quoter.QuoteTableName(expression.TableName, expression.SourceSchemaName)} RENAME TO {Quoter.QuoteTableName(expression.TableName, expression.DestinationSchemaName)}");
        }

        /// <inheritdoc />
        public override string Generate(CreateIndexExpression expression)
        {
            return CompatibilityMode.HandleCompatibility("Indices not supported");
        }

        /// <inheritdoc />
        public override string Generate(DeleteIndexExpression expression)
        {
            return CompatibilityMode.HandleCompatibility("Indices not supported");
        }

        public override string Generate(CreateSequenceExpression expression)
        {
            var result = new StringBuilder("CREATE SEQUENCE ");
            var seq = expression.Sequence;
            result.AppendFormat(Quoter.QuoteSequenceName(seq.Name, seq.SchemaName));

            if (seq.StartWith.HasValue)
            {
                result.AppendFormat(" START {0}", seq.StartWith);
            }

            if (seq.Increment.HasValue)
            {
                result.AppendFormat(" INCREMENT {0}", seq.Increment);
            }

            AppendSqlStatementEndToken(result);

            return result.ToString();
        }
    }
}
