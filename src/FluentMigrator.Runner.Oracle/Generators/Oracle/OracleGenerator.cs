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

using System.Collections.Generic;
using System.Linq;
using System.Text;

using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Generic;
using FluentMigrator.Runner.Helpers;

using JetBrains.Annotations;

using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Generators.Oracle
{
    public class OracleGenerator : GenericGenerator, IOracleGenerator
    {
        public OracleGenerator()
            : this(false)
        {
        }

        public OracleGenerator(bool useQuotedIdentifiers)
            : this(GetQuoter(useQuotedIdentifiers))
        {
        }

        public OracleGenerator(
            [NotNull] OracleQuoterBase quoter)
            : this(quoter, new OptionsWrapper<GeneratorOptions>(new GeneratorOptions()))
        {
        }

        public OracleGenerator(
            [NotNull] OracleQuoterBase quoter,
            [NotNull] IOptions<GeneratorOptions> generatorOptions)
            : base(new OracleColumn(quoter), quoter, new OracleDescriptionGenerator(), generatorOptions)
        {
        }

        public OracleGenerator(
            [NotNull] IColumn column,
            [NotNull] OracleQuoterBase quoter,
            [NotNull] IOptions<GeneratorOptions> generatorOptions)
            : base(column, quoter, new OracleDescriptionGenerator(), generatorOptions)
        {
        }

        protected OracleGenerator(
            [NotNull] IColumn column,
            [NotNull] OracleQuoterBase quoter,
            [NotNull] IDescriptionGenerator descriptionGenerator,
            [NotNull] IOptions<GeneratorOptions> generatorOptions)
            : base(column, quoter, descriptionGenerator, generatorOptions)
        {
        }

        protected static OracleQuoterBase GetQuoter(bool useQuotedIdentifiers)
        {
            return useQuotedIdentifiers ? new OracleQuoterQuotedIdentifier() : new OracleQuoter();
        }


        public override string DropTable
        {
            get
            {
                return "DROP TABLE {0}";
            }
        }
        public override string Generate(DeleteTableExpression expression)
        {
            if (expression.IfExists)
            {
                return CompatibilityMode.HandleCompatibility("If Exists logic is not supported");
            }

            return string.Format(DropTable, ExpandTableName(Quoter.QuoteTableName(expression.SchemaName),Quoter.QuoteTableName(expression.TableName)));
        }

        public override string Generate(CreateSequenceExpression expression)
        {
            var result = new StringBuilder("CREATE SEQUENCE ");
            var seq = expression.Sequence;
            if (string.IsNullOrEmpty(seq.SchemaName))
            {
                result.AppendFormat(Quoter.QuoteSequenceName(seq.Name));
            }
            else
            {
                result.AppendFormat("{0}", Quoter.QuoteSequenceName(seq.Name, seq.SchemaName));
            }

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
                    return CompatibilityMode.HandleCompatibility("Oracle does not support Cache value equal to 1; if you intended to disable caching, set Cache to null. For information on Oracle limitations, see: https://docs.oracle.com/en/database/oracle/oracle-database/18/sqlrf/CREATE-SEQUENCE.html#GUID-E9C78A8C-615A-4757-B2A8-5E6EFB130571__GUID-7E390BE1-2F6C-4E5A-9D5C-5A2567D636FB");
                }
                result.AppendFormat(" CACHE {0}", seq.Cache);
            }
            else
            {
                result.Append(" NOCACHE");
            }

            if (seq.Cycle)
            {
                result.Append(" CYCLE");
            }

            return result.ToString();
        }

        public override string AddColumn
        {
            get { return "ALTER TABLE {0} ADD {1}"; }
        }

        public override string AlterColumn
        {
            get { return "ALTER TABLE {0} MODIFY {1}"; }
        }

        public override string RenameTable
        {
            get { return "ALTER TABLE {0} RENAME TO {1}"; }
        }

        public override string InsertData
        {
            get { return "INTO {0} ({1}) VALUES ({2})"; }
        }

        private static string ExpandTableName(string schema, string table)
        {
            return string.IsNullOrEmpty(schema) ? table : string.Concat(schema,".",table);
        }

        private static string WrapStatementInExecuteImmediateBlock(string statement)
        {
            if (string.IsNullOrEmpty(statement))
            {
                return string.Empty;
            }

            return string.Format("EXECUTE IMMEDIATE '{0}';", FormatHelper.FormatSqlEscape(statement));
        }

        private static string WrapInBlock(string sql)
        {
            if (string.IsNullOrEmpty(sql))
            {
                return string.Empty;
            }

            return string.Format("BEGIN {0} END;", sql);
        }

        private string InnerGenerate(CreateTableExpression expression)
        {
            var tableName = Quoter.QuoteTableName(expression.TableName);
            var schemaName = Quoter.QuoteSchemaName(expression.SchemaName);

            return string.Format("CREATE TABLE {0} ({1})",ExpandTableName(schemaName,tableName), Column.Generate(expression.Columns, tableName));
        }

        protected override StringBuilder AppendSqlStatementEndToken(StringBuilder stringBuilder)
        {
            return stringBuilder.AppendLine().AppendLine(";");
        }

        /// <inheritdoc />
        public override string GeneratorId => GeneratorIdConstants.Oracle;

        /// <inheritdoc />
        public override List<string> GeneratorIdAliases => new List<string> { GeneratorIdConstants.Oracle };

        public override string Generate(CreateTableExpression expression)
        {
            if (expression.Columns.Any(x => x.ExpressionStored))
            {
                CompatibilityMode.HandleCompatibility("Stored computed columns are not supported");
            }
            var descriptionStatements = DescriptionGenerator.GenerateDescriptionStatements(expression);
            var statements = descriptionStatements as string[] ?? descriptionStatements.ToArray();

            if (!statements.Any())
            {
                return InnerGenerate(expression);
            }

            var wrappedCreateTableStatement = WrapStatementInExecuteImmediateBlock(InnerGenerate(expression));
            var createTableWithDescriptionsBuilder = new StringBuilder(wrappedCreateTableStatement);

            foreach (var descriptionStatement in statements)
            {
                if (!string.IsNullOrEmpty(descriptionStatement))
                {
                    var wrappedStatement = WrapStatementInExecuteImmediateBlock(descriptionStatement);
                    createTableWithDescriptionsBuilder.Append(wrappedStatement);
                }
            }

            return WrapInBlock(createTableWithDescriptionsBuilder.ToString());
        }

        public override string Generate(AlterTableExpression expression)
        {
            var descriptionStatement = DescriptionGenerator.GenerateDescriptionStatement(expression);

            if (string.IsNullOrEmpty(descriptionStatement))
            {
                return base.Generate(expression);
            }

            return descriptionStatement;
        }

        public override string Generate(CreateColumnExpression expression)
        {
            if (expression.Column.ExpressionStored)
            {
                CompatibilityMode.HandleCompatibility("Stored computed columns are not supported");
            }
            var descriptionStatement = DescriptionGenerator.GenerateDescriptionStatement(expression);

            if (string.IsNullOrEmpty(descriptionStatement))
                return base.Generate(expression);

            var wrappedCreateColumnStatement = WrapStatementInExecuteImmediateBlock(base.Generate(expression));

            var createColumnWithDescriptionBuilder = new StringBuilder(wrappedCreateColumnStatement);
            createColumnWithDescriptionBuilder.Append(WrapStatementInExecuteImmediateBlock(descriptionStatement));

            return WrapInBlock(createColumnWithDescriptionBuilder.ToString());
        }

        public override string Generate(AlterColumnExpression expression)
        {
            if (expression.Column.ExpressionStored)
            {
                CompatibilityMode.HandleCompatibility("Stored computed columns are not supported");
            }
            var descriptionStatement = DescriptionGenerator.GenerateDescriptionStatement(expression);

            if (string.IsNullOrEmpty(descriptionStatement))
                return base.Generate(expression);

            var wrappedAlterColumnStatement = WrapStatementInExecuteImmediateBlock(base.Generate(expression));

            var alterColumnWithDescriptionBuilder = new StringBuilder(wrappedAlterColumnStatement);
            alterColumnWithDescriptionBuilder.Append(WrapStatementInExecuteImmediateBlock(descriptionStatement));

            return WrapInBlock(alterColumnWithDescriptionBuilder.ToString());
        }

        public override string Generate(CreateIndexExpression expression)
        {
            var indexColumns = new string[expression.Index.Columns.Count];

            for (var i = 0; i < expression.Index.Columns.Count; i++)
            {
                var columnDef = expression.Index.Columns.ElementAt(i);

                var direction = columnDef.Direction == Direction.Ascending ? "ASC" : "DESC";
                indexColumns[i] = $"{Quoter.QuoteColumnName(columnDef.Name)} {direction}";
            }

            return string.Format(CreateIndex
                , GetUniqueString(expression)
                , GetClusterTypeString(expression)
                , Quoter.QuoteIndexName(expression.Index.Name, expression.Index.SchemaName)
                , Quoter.QuoteTableName(expression.Index.TableName, expression.Index.SchemaName)
                , string.Join(", ", indexColumns));
        }

        public override string Generate(InsertDataExpression expression)
        {
            var columnNames = new List<string>();
            var columnValues = new List<string>();
            var insertStrings = new List<string>();

            foreach (InsertionDataDefinition row in expression.Rows)
            {
                columnNames.Clear();
                columnValues.Clear();
                foreach (KeyValuePair<string, object> item in row)
                {
                    columnNames.Add(Quoter.QuoteColumnName(item.Key));
                    columnValues.Add(Quoter.QuoteValue(item.Value));
                }

                string columns = string.Join(", ", columnNames.ToArray());
                string values = string.Join(", ", columnValues.ToArray());
                insertStrings.Add(string.Format(InsertData, ExpandTableName(Quoter.QuoteSchemaName(expression.SchemaName), Quoter.QuoteTableName(expression.TableName)), columns, values));
            }
            return "INSERT ALL " + string.Join(" ", insertStrings.ToArray()) + " SELECT 1 FROM DUAL";
        }

        public override string Generate(AlterDefaultConstraintExpression expression)
        {
            return string.Format(AlterColumn, Quoter.QuoteTableName(expression.TableName, expression.SchemaName), Column.Generate(new ColumnDefinition
            {
                ModificationType = ColumnModificationType.Alter,
                Name = expression.ColumnName,
                DefaultValue = expression.DefaultValue
            }));
        }

        public override string Generate(DeleteDefaultConstraintExpression expression)
        {
            return Generate(new AlterDefaultConstraintExpression
            {
                SchemaName = expression.SchemaName,
                TableName = expression.TableName,
                ColumnName = expression.ColumnName,
                DefaultValue = null
            });
        }

        public override string Generate(DeleteIndexExpression expression)
        {
            var quotedSchema = Quoter.QuoteSchemaName(expression.Index.SchemaName);
            var quotedIndex = Quoter.QuoteIndexName(expression.Index.Name);
            var indexName = string.IsNullOrEmpty(quotedSchema) ? quotedIndex : $"{quotedSchema}.{quotedIndex}";
            return string.Format("DROP INDEX {0}", indexName);
        }
    }
}
