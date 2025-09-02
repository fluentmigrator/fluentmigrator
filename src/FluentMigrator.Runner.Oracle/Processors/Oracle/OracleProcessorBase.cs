#region License
// Copyright (c) 2018, Fluent Migrator Project
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

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using FluentMigrator.Expressions;
using FluentMigrator.Runner.BatchParser;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.Oracle;
using FluentMigrator.Runner.Helpers;
using FluentMigrator.Runner.Initialization;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Processors.Oracle
{
    /// <summary>
    /// Base class for Oracle processors in FluentMigrator.
    /// </summary>
    public class OracleProcessorBase : GenericProcessorBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OracleProcessorBase"/> class.
        /// </summary>
        /// <param name="databaseType">The database type name.</param>
        /// <param name="factory">The Oracle database factory.</param>
        /// <param name="generator">The migration generator.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="options">The processor options.</param>
        /// <param name="connectionStringAccessor">The connection string accessor.</param>
        protected OracleProcessorBase(
            [NotNull] string databaseType,
            [NotNull] OracleBaseDbFactory factory,
            [NotNull] IMigrationGenerator generator,
            [NotNull] ILogger logger,
            [NotNull] IOptionsSnapshot<ProcessorOptions> options,
            [NotNull] IConnectionStringAccessor connectionStringAccessor)
            : base(() => factory.Factory, generator, logger, options.Value, connectionStringAccessor)
        {
            DatabaseType = databaseType;
        }

        /// <inheritdoc />
        public override string DatabaseType { get; }

        /// <inheritdoc />
        public override IList<string> DatabaseTypeAliases { get; } = new List<string>() { ProcessorIdConstants.Oracle };

        /// <summary>
        /// Gets the quoter for Oracle SQL.
        /// </summary>
        public IQuoter Quoter => ((OracleGenerator) Generator).Quoter;

        /// <inheritdoc />
        public override bool SchemaExists(string schemaName)
        {
            if (schemaName == null)
            {
                throw new ArgumentNullException(nameof(schemaName));
            }

            if (schemaName.Length == 0)
            {
                return false;
            }

            return Exists("SELECT 1 FROM ALL_USERS WHERE USERNAME = '{0}'", schemaName.ToUpper());
        }

        /// <inheritdoc />
        public override bool TableExists(string schemaName, string tableName)
        {
            if (tableName == null)
            {
                throw new ArgumentNullException(nameof(tableName));
            }

            if (tableName.Length == 0)
            {
                return false;
            }

            if (string.IsNullOrEmpty(schemaName))
            {
                return Exists("SELECT 1 FROM USER_TABLES WHERE upper(TABLE_NAME) = '{0}'",
                    FormatHelper.FormatSqlEscape(tableName.ToUpper()));
            }

            return Exists("SELECT 1 FROM ALL_TABLES WHERE upper(OWNER) = '{0}' AND upper(TABLE_NAME) = '{1}'",
                schemaName.ToUpper(), FormatHelper.FormatSqlEscape(tableName.ToUpper()));
        }

        /// <inheritdoc />
        public override bool ColumnExists(string schemaName, string tableName, string columnName)
        {
            if (tableName == null)
            {
                throw new ArgumentNullException(nameof(tableName));
            }

            if (columnName == null)
            {
                throw new ArgumentNullException(nameof(columnName));
            }

            if (columnName.Length == 0 || tableName.Length == 0)
            {
                return false;
            }

            if (string.IsNullOrEmpty(schemaName))
            {
                return Exists(
                    "SELECT 1 FROM USER_TAB_COLUMNS WHERE upper(TABLE_NAME) = '{0}' AND upper(COLUMN_NAME) = '{1}'",
                    FormatHelper.FormatSqlEscape(tableName.ToUpper()),
                    FormatHelper.FormatSqlEscape(columnName.ToUpper()));
            }

            return Exists(
                "SELECT 1 FROM ALL_TAB_COLUMNS WHERE upper(OWNER) = '{0}' AND upper(TABLE_NAME) = '{1}' AND upper(COLUMN_NAME) = '{2}'",
                schemaName.ToUpper(), FormatHelper.FormatSqlEscape(tableName.ToUpper()),
                FormatHelper.FormatSqlEscape(columnName.ToUpper()));
        }

        /// <inheritdoc />
        public override bool ConstraintExists(string schemaName, string tableName, string constraintName)
        {
            if (tableName == null)
            {
                throw new ArgumentNullException(nameof(tableName));
            }

            if (constraintName == null)
            {
                throw new ArgumentNullException(nameof(constraintName));
            }

            //In Oracle DB constraint name is unique within the schema, so the table name is not used in the query

            if (constraintName.Length == 0)
            {
                return false;
            }

            if (string.IsNullOrEmpty(schemaName))
            {
                return Exists("SELECT 1 FROM USER_CONSTRAINTS WHERE upper(CONSTRAINT_NAME) = '{0}'",
                    FormatHelper.FormatSqlEscape(constraintName.ToUpper()));
            }

            return Exists("SELECT 1 FROM ALL_CONSTRAINTS WHERE upper(OWNER) = '{0}' AND upper(CONSTRAINT_NAME) = '{1}'",
                schemaName.ToUpper(),
                FormatHelper.FormatSqlEscape(constraintName.ToUpper()));
        }

        /// <inheritdoc />
        public override bool IndexExists(string schemaName, string tableName, string indexName)
        {
            if (tableName == null)
            {
                throw new ArgumentNullException(nameof(tableName));
            }

            if (indexName == null)
            {
                throw new ArgumentNullException(nameof(indexName));
            }

            //In Oracle DB index name is unique within the schema, so the table name is not used in the query

            if (indexName.Length == 0)
            {
                return false;
            }

            if (string.IsNullOrEmpty(schemaName))
            {
                return Exists("SELECT 1 FROM USER_INDEXES WHERE upper(INDEX_NAME) = '{0}'",
                    FormatHelper.FormatSqlEscape(indexName.ToUpper()));
            }

            return Exists("SELECT 1 FROM ALL_INDEXES WHERE upper(OWNER) = '{0}' AND upper(INDEX_NAME) = '{1}'",
                schemaName.ToUpper(), FormatHelper.FormatSqlEscape(indexName.ToUpper()));
        }

        /// <inheritdoc />
        public override bool SequenceExists(string schemaName, string sequenceName)
        {
            if (string.IsNullOrEmpty(schemaName))
            {
                return Exists("SELECT 1 FROM USER_SEQUENCES WHERE upper(SEQUENCE_NAME) = '{0}'",
                    FormatHelper.FormatSqlEscape(sequenceName.ToUpper()));
            }

            return Exists("SELECT 1 FROM ALL_SEQUENCES WHERE upper(SEQUENCE_OWNER) = '{0}' AND upper(SEQUENCE_NAME) = '{1}'",
                schemaName.ToUpper(), FormatHelper.FormatSqlEscape(sequenceName.ToUpper()));
        }

        /// <inheritdoc />
        public override bool DefaultValueExists(string schemaName, string tableName, string columnName,
            object defaultValue)
        {
            if (tableName == null)
            {
                throw new ArgumentNullException(nameof(tableName));
            }

            if (columnName == null)
            {
                throw new ArgumentNullException(nameof(columnName));
            }

            if (columnName.Length == 0 || tableName.Length == 0)
            {
                return false;
            }

            if (string.IsNullOrEmpty(schemaName))
            {
                return Exists(
                    "SELECT 1 FROM USER_TAB_COLUMNS WHERE upper(TABLE_NAME) = '{0}' AND upper(COLUMN_NAME) = '{1}' AND DATA_DEFAULT IS NOT NULL",
                    FormatHelper.FormatSqlEscape(tableName.ToUpper()),
                    FormatHelper.FormatSqlEscape(columnName.ToUpper()));
            }

            return Exists(
                "SELECT 1 FROM ALL_TAB_COLUMNS WHERE upper(OWNER) = '{0}' AND upper(TABLE_NAME) = '{1}' AND upper(COLUMN_NAME) = '{2}' AND DATA_DEFAULT IS NOT NULL",
                schemaName.ToUpper(), FormatHelper.FormatSqlEscape(tableName.ToUpper()),
                FormatHelper.FormatSqlEscape(columnName.ToUpper()));
        }

        /// <inheritdoc />
        public override void Execute([StructuredMessageTemplate] string template, params object[] args)
        {
            Process(string.Format(template, args));
        }

        /// <inheritdoc />
        public override bool Exists([StructuredMessageTemplate] string template, params object[] args)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            EnsureConnectionIsOpen();

            Logger.LogSql(string.Format(template, args));
            using (var command = CreateCommand(string.Format(template, args)))
            using (var reader = command.ExecuteReader())
            {
                return reader.Read();
            }
        }

        /// <inheritdoc />
        public override DataSet ReadTableData(string schemaName, string tableName)
        {
            if (tableName == null)
            {
                throw new ArgumentNullException(nameof(tableName));
            }

            if (string.IsNullOrEmpty(schemaName))
            {
                return Read("SELECT * FROM {0}", Quoter.QuoteTableName(tableName));
            }

            return Read("SELECT * FROM {0}.{1}", Quoter.QuoteSchemaName(schemaName), Quoter.QuoteTableName(tableName));
        }

        /// <inheritdoc />
        public override DataSet Read([StructuredMessageTemplate] string template, params object[] args)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            EnsureConnectionIsOpen();

            using (var command = CreateCommand(string.Format(template, args)))
            using (var reader = command.ExecuteReader())
            {
                return reader.ReadDataSet();
            }
        }

        /// <inheritdoc />
        public override void Process(PerformDBOperationExpression expression)
        {
            Logger.LogSay("Performing DB Operation");

            if (Options.PreviewOnly)
            {
                return;
            }

            EnsureConnectionIsOpen();

            expression.Operation?.Invoke(Connection, Transaction);
        }

        /// <summary>
        /// Splits a SQL script into individual statements, taking into account Oracle-specific syntax rules.
        /// <remarks>We could use <see cref="SqlBatchParser"/> but it does not handle multiple lines strings.</remarks>
        /// </summary>
        private static List<string> SplitOracleSqlStatements(string sqlScript)
        {
            var statements = new List<string>();
            var currentStatement = new StringBuilder();
            var inString = false;
            var inIdentifier = false;
            var inSingleLineComment = false;
            var inMultiLineComment = false;
            var prevChar = '\0';

            foreach (var c in sqlScript)
            {
                if (inSingleLineComment)
                {
                    currentStatement.Append(c);
                    if (c == '\n')
                    {
                        inSingleLineComment = false;
                    }
                    continue;
                }

                if (inMultiLineComment)
                {
                    currentStatement.Append(c);
                    if (prevChar == '*' && c == '/')
                    {
                        inMultiLineComment = false;
                    }
                    prevChar = c;
                    continue;
                }

                if (inString)
                {
                    currentStatement.Append(c);
                    if (c == '\'' && prevChar != '\\')
                    {
                        inString = false;
                    }
                    prevChar = c;
                    continue;
                }

                if (inIdentifier)
                {
                    currentStatement.Append(c);
                    if (c == '"')
                    {
                        inIdentifier = false;
                    }
                    prevChar = c;
                    continue;
                }

                switch (c)
                {
                    // Check for comment start
                    case '-' when prevChar == '-':
                        inSingleLineComment = true;
                        currentStatement.Append(c);
                        continue;
                    case '*' when prevChar == '/':
                        inMultiLineComment = true;
                        currentStatement.Append(c);
                        continue;
                    // Check for string start
                    case '\'':
                        inString = true;
                        currentStatement.Append(c);
                        prevChar = c;
                        continue;
                    // Check for inIdentifier start
                    case '"':
                        inIdentifier = true;
                        currentStatement.Append(c);
                        prevChar = c;
                        continue;
                    // Check for statement terminator
                    case ';':
                        statements.Add(currentStatement.ToString().TrimEnd(';').Trim());
                        currentStatement.Clear();
                        prevChar = '\0';
                        continue;
                    default:
                        currentStatement.Append(c);
                        prevChar = c;
                        break;
                }
            }

            // Add any remaining content
            if (currentStatement.Length > 0)
            {
                statements.Add(currentStatement.ToString());
            }

            return statements;
        }

        /// <inheritdoc />
        protected override void Process(string sql)
        {
            Logger.LogSql(sql);

            if (Options.PreviewOnly || string.IsNullOrEmpty(sql))
            {
                return;
            }

            EnsureConnectionIsOpen();

            var batches = SplitOracleSqlStatements(sql);

            foreach (var batch in batches)
            {
                using (var command = CreateCommand(batch))
                {
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        ReThrowWithSql(ex, batch);
                    }
                }
            }
        }
    }
}
