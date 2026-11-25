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
using System.Globalization;
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
    /// <summary>
    /// The Oracle SQL generator for FluentMigrator.
    /// </summary>
    public class OracleGenerator : GenericGenerator, IOracleGenerator, ICreateTableIfNotExistsGenerator
    {
        /// <inheritdoc />
        public OracleGenerator()
            : this(false)
        {
        }

        /// <inheritdoc />
        public OracleGenerator(bool useQuotedIdentifiers)
            : this(GetQuoter(useQuotedIdentifiers))
        {
        }

        /// <inheritdoc />
        public OracleGenerator(
            [NotNull] OracleQuoterBase quoter)
            : this(quoter, new OptionsWrapper<GeneratorOptions>(new GeneratorOptions()))
        {
        }

        /// <inheritdoc />
        public OracleGenerator(
            [NotNull] OracleQuoterBase quoter,
            [NotNull] IOptions<GeneratorOptions> generatorOptions)
            : base(new OracleColumn(quoter), quoter, new OracleDescriptionGenerator(quoter), generatorOptions)
        {
        }

        /// <inheritdoc />
        public OracleGenerator(
            [NotNull] IColumn column,
            [NotNull] OracleQuoterBase quoter,
            [NotNull] IOptions<GeneratorOptions> generatorOptions)
            : base(column, quoter, new OracleDescriptionGenerator(quoter), generatorOptions)
        {
        }

        /// <inheritdoc />
        protected OracleGenerator(
            [NotNull] IColumn column,
            [NotNull] OracleQuoterBase quoter,
            [NotNull] IDescriptionGenerator descriptionGenerator,
            [NotNull] IOptions<GeneratorOptions> generatorOptions)
            : base(column, quoter, descriptionGenerator, generatorOptions)
        {
        }

        /// <summary>
        /// Gets the appropriate quoter based on identifier quoting preference.
        /// </summary>
        /// <param name="useQuotedIdentifiers">Whether to use quoted identifiers.</param>
        /// <returns>The <see cref="OracleQuoterBase"/> instance.</returns>
        protected static OracleQuoterBase GetQuoter(bool useQuotedIdentifiers)
        {
            return useQuotedIdentifiers ? new OracleQuoterQuotedIdentifier() : new OracleQuoter();
        }

        /// <inheritdoc />
        public override string Generate(DeleteTableExpression expression)
        {
            if (expression.IfExists)
            {
                return CompatibilityMode.HandleCompatibility("If Exists logic is not supported");
            }

            return FormatStatement(DropTable, ExpandTableName(Quoter.QuoteTableName(expression.SchemaName), Quoter.QuoteTableName(expression.TableName)));
        }

        /// <inheritdoc />
        public override string Generate(CreateSequenceExpression expression)
        {
            var result = new StringBuilder("CREATE SEQUENCE ");
            var seq = expression.Sequence;
            if (string.IsNullOrEmpty(seq.SchemaName))
            {
                result.Append(Quoter.QuoteSequenceName(seq.Name));
            }
            else
            {
                result.AppendFormat("{0}", Quoter.QuoteSequenceName(seq.Name, seq.SchemaName));
            }

            if (seq.Increment.HasValue)
            {
                result.AppendFormat(CultureInfo.InvariantCulture, " INCREMENT BY {0}", seq.Increment);
            }

            if (seq.MinValue.HasValue)
            {
                result.AppendFormat(CultureInfo.InvariantCulture, " MINVALUE {0}", seq.MinValue);
            }

            if (seq.MaxValue.HasValue)
            {
                result.AppendFormat(CultureInfo.InvariantCulture, " MAXVALUE {0}", seq.MaxValue);
            }

            if (seq.StartWith.HasValue)
            {
                result.AppendFormat(CultureInfo.InvariantCulture, " START WITH {0}", seq.StartWith);
            }

            const long MINIMUM_CACHE_VALUE = 2;
            if (seq.Cache.HasValue)
            {
                if (seq.Cache.Value < MINIMUM_CACHE_VALUE)
                {
                    return CompatibilityMode.HandleCompatibility("Oracle does not support Cache value equal to 1; if you intended to disable caching, set Cache to null. For information on Oracle limitations, see: https://docs.oracle.com/en/database/oracle/oracle-database/18/sqlrf/CREATE-SEQUENCE.html#GUID-E9C78A8C-615A-4757-B2A8-5E6EFB130571__GUID-7E390BE1-2F6C-4E5A-9D5C-5A2567D636FB");
                }
                result.AppendFormat(CultureInfo.InvariantCulture, " CACHE {0}", seq.Cache);
            }
            else
            {
                result.Append(" NOCACHE");
            }

            if (seq.Cycle)
            {
                result.Append(" CYCLE");
            }

            AppendSqlStatementEndToken(result);

            return result.ToString();
        }

        /// <inheritdoc />
        public override string AddColumn => "ALTER TABLE {0} ADD {1}";

        /// <inheritdoc />
        public override string AlterColumn => "ALTER TABLE {0} MODIFY {1}";

        /// <inheritdoc />
        public override string RenameTable => "ALTER TABLE {0} RENAME TO {1}";

        /// <inheritdoc />
        public override string InsertData => "INTO {0} ({1}) VALUES ({2})";

        /// <summary>
        /// Expands the table name to include the schema if present.
        /// </summary>
        /// <param name="schema">The schema name.</param>
        /// <param name="table">The table name.</param>
        /// <returns>The expanded table name.</returns>
        private static string ExpandTableName(string schema, string table)
        {
            return string.IsNullOrEmpty(schema) ? table : string.Concat(schema, ".", table);
        }

        /// <summary>
        /// Wraps a statement in an EXECUTE IMMEDIATE block.
        /// </summary>
        /// <param name="statement">The SQL statement.</param>
        /// <returns>The wrapped statement.</returns>
        private string WrapStatementInExecuteImmediateBlock(string statement)
        {
            if (string.IsNullOrEmpty(statement))
            {
                return string.Empty;
            }

            // Dynamic SQL must not carry the statement end token, otherwise Oracle raises ORA-00922/ORA-00911
            statement = statement.TrimEnd();
            if (statement.EndsWith(";"))
            {
                statement = statement.Substring(0, statement.Length - 1);
            }

            return FormatStatement("EXECUTE IMMEDIATE '{0}'", FormatHelper.FormatSqlEscape(statement));
        }

        /// <summary>
        /// Wraps SQL in a BEGIN...END block.
        /// </summary>
        /// <param name="sql">The SQL statement.</param>
        /// <returns>The wrapped SQL.</returns>
        private string WrapInBlock(string sql)
        {
            if (string.IsNullOrEmpty(sql))
            {
                return string.Empty;
            }

            return FormatStatement("BEGIN {0} END", sql);
        }

        /// <summary>
        /// Generates the CREATE TABLE statement for Oracle.
        /// </summary>
        /// <param name="expression">The create table expression.</param>
        /// <returns>The SQL statement.</returns>
        private string InnerGenerate(CreateTableExpression expression)
        {
            var tableName = Quoter.QuoteTableName(expression.TableName);
            var schemaName = Quoter.QuoteSchemaName(expression.SchemaName);

            return FormatStatement("CREATE TABLE {0} ({1})", ExpandTableName(schemaName, tableName), Column.Generate(expression.Columns, tableName));
        }

        /// <inheritdoc />
        public override string GeneratorId => GeneratorIdConstants.Oracle;

        /// <inheritdoc />
        public override List<string> GeneratorIdAliases => new List<string> { GeneratorIdConstants.Oracle };

        /// <inheritdoc />
        public override string Generate(CreateTableExpression expression)
        {
            if (expression.Columns.Any(x => x.ExpressionStored))
            {
                CompatibilityMode.HandleCompatibility("Stored computed columns are not supported");
            }
            var descriptionStatements = DescriptionGenerator.GenerateDescriptionStatements(expression);
            var statements = descriptionStatements as string[] ?? descriptionStatements.ToArray();

            if (expression.IfNotExists)
            {
                return GenerateCreateTableIfNotExists(expression, statements);
            }

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

        /// <inheritdoc />
        public virtual bool GeneratesWithTable(CreateTableExpression expression, IMigrationExpression expressionForNewTable) =>
            IsIndexOrForeignKey(expressionForNewTable);

        /// <summary>
        /// Oracle has no <c>CREATE TABLE IF NOT EXISTS</c> before 23ai. The statement, the table's descriptions and its <see cref="CreateTableExpression.ExpressionsForNewTable"/> run as
        /// dynamic SQL in a PL/SQL block, once, when the data dictionary has no table of the name the
        /// <c>CREATE TABLE</c> would store (see <see cref="ToStoredName"/>).
        /// </summary>
        /// <remarks>
        /// <para>
        /// The names are compared exactly. <c>USERS</c> and <c>"Users"</c> are different tables, so a case-insensitive
        /// check, as in <c>OracleProcessorBase.TableExists</c>, would skip the <c>CREATE TABLE</c> for one when the
        /// other exists, and the migration would be recorded without the table.
        /// </para>
        /// <para>
        /// A table without a schema is created in the session's current schema, which <c>ALTER SESSION SET
        /// CURRENT_SCHEMA</c> can make another than the login's. So the guard looks in that schema, not in
        /// <c>USER_TABLES</c>, which lists the login's tables: otherwise an existing table there would be created again
        /// (ORA-00955), and a table of the same name owned by the login would skip the <c>CREATE TABLE</c>.
        /// </para>
        /// <para>
        /// The block has no <c>DECLARE</c>: the loop over a query that returns one row or none needs no variable, and
        /// a <c>DECLARE ... BEGIN</c> would be split into two blocks by <c>OracleSqlParser</c>, which runs scripts.
        /// </para>
        /// </remarks>
        private string GenerateCreateTableIfNotExists(CreateTableExpression expression, IEnumerable<string> descriptionStatements)
        {
            var tableName = FormatHelper.FormatSqlEscape(ToStoredName(expression.TableName));
            var owner = string.IsNullOrEmpty(expression.SchemaName)
                ? "SYS_CONTEXT('USERENV', 'CURRENT_SCHEMA')"
                : $"'{FormatHelper.FormatSqlEscape(ToStoredName(expression.SchemaName))}'";
            var catalogQuery = $"SELECT 1 FROM ALL_TABLES WHERE OWNER = {owner} AND TABLE_NAME = '{tableName}'";

            var builder = new StringBuilder();
            builder.AppendLine("BEGIN");
            builder.AppendLine($"FOR missing_table IN (SELECT 1 FROM DUAL WHERE NOT EXISTS ({catalogQuery})) LOOP");
            builder.AppendLine(WrapStatementInExecuteImmediateBlock(InnerGenerate(expression)));
            foreach (var descriptionStatement in descriptionStatements.Where(s => !string.IsNullOrEmpty(s)))
            {
                builder.AppendLine(WrapStatementInExecuteImmediateBlock(descriptionStatement));
            }

            foreach (var statement in GenerateExpressionsForNewTable(expression, IsIndexOrForeignKey))
            {
                builder.AppendLine(WrapStatementInExecuteImmediateBlock(statement));
            }

            builder.AppendLine("END LOOP;");
            builder.Append("END");
            AppendSqlStatementEndToken(builder);
            return builder.ToString();
        }

        /// <summary>
        /// The name as the data dictionary stores it: a name the quoter quotes (because it is quoted already, isn't a
        /// valid unquoted identifier or is a keyword, or because the quoter quotes every name) as written, and any
        /// other name in upper case.
        /// </summary>
        private string ToStoredName(string name)
        {
            if (Quoter.IsQuoted(name))
            {
                return Quoter.UnQuote(name);
            }

            return Quoter.IsQuoted(Quoter.Quote(name)) ? name : name.ToUpperInvariant();
        }

        /// <inheritdoc />
        public override string Generate(AlterTableExpression expression)
        {
            var descriptionStatement = DescriptionGenerator.GenerateDescriptionStatement(expression);

            if (string.IsNullOrEmpty(descriptionStatement))
            {
                return base.Generate(expression);
            }

            // Ended like every other statement, so a script can hold it. Inside EXECUTE IMMEDIATE, where the
            // description generator's statements usually go, there is no ';'.
            return descriptionStatement + ";";
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        /// <remarks>Oracle's unique-index NULL handling is mixed: keys whose indexed columns are all NULL are not indexed at all, while composite keys with only some NULL columns treat those NULLs as equal. Neither portable semantics can be promised.</remarks>
        protected override bool IsUniqueIndexNullsDistinctSupported(bool nullsDistinct) => false;

        /// <inheritdoc />
        public override string Generate(CreateIndexExpression expression)
        {
            ValidateUniqueIndexNullsDistinct(expression);

            var indexColumns = new string[expression.Index.Columns.Count];

            for (var i = 0; i < expression.Index.Columns.Count; i++)
            {
                var columnDef = expression.Index.Columns.ElementAt(i);

                var direction = columnDef.Direction == Direction.Ascending ? "ASC" : "DESC";
                indexColumns[i] = $"{Quoter.QuoteColumnName(columnDef.Name)} {direction}";
            }

            return FormatStatement(CreateIndex
                , GetUniqueString(expression)
                , GetClusterTypeString(expression)
                , Quoter.QuoteIndexName(expression.Index.Name, expression.Index.SchemaName)
                , Quoter.QuoteTableName(expression.Index.TableName, expression.Index.SchemaName)
                , string.Join(", ", indexColumns));
        }

        /// <inheritdoc />
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

            return FormatStatement("INSERT ALL {0} SELECT 1 FROM DUAL", string.Join(" ", insertStrings.ToArray()));
        }

        /// <inheritdoc />
        public override string Generate(AlterDefaultConstraintExpression expression)
        {
            return FormatStatement(AlterColumn, Quoter.QuoteTableName(expression.TableName, expression.SchemaName), Column.Generate(new ColumnDefinition
            {
                ModificationType = ColumnModificationType.Alter,
                Name = expression.ColumnName,
                DefaultValue = expression.DefaultValue
            }));
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override string Generate(DeleteIndexExpression expression)
        {
            var quotedSchema = Quoter.QuoteSchemaName(expression.Index.SchemaName);
            var quotedIndex = Quoter.QuoteIndexName(expression.Index.Name);
            var indexName = string.IsNullOrEmpty(quotedSchema) ? quotedIndex : $"{quotedSchema}.{quotedIndex}";
            return FormatStatement("DROP INDEX {0}", indexName);
        }
    }
}
