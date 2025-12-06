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
using System.Data;

using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators.MySql;
using FluentMigrator.Runner.Helpers;
using FluentMigrator.Runner.Initialization;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Processors.MySql
{
    /// <summary>
    /// The MySQL processor for FluentMigrator.
    /// </summary>
    public class MySqlProcessor : GenericProcessorBase
    {
        private readonly MySqlQuoter _quoter = new MySqlQuoter();

        /// <inheritdoc />
        public override string DatabaseType => ProcessorIdConstants.MySql;

        /// <inheritdoc />
        public override IList<string> DatabaseTypeAliases { get; } = new List<string> { ProcessorIdConstants.MariaDB };

        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlProcessor"/> class.
        /// </summary>
        /// <param name="factory">The MySQL database factory.</param>
        /// <param name="generator">The migration generator.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="options">The processor options.</param>
        /// <param name="connectionStringAccessor">The connection string accessor.</param>
        protected MySqlProcessor(
            [NotNull] MySqlDbFactory factory,
            [NotNull] IMigrationGenerator generator,
            [NotNull] ILogger<MySqlProcessor> logger,
            [NotNull] IOptionsSnapshot<ProcessorOptions> options,
            [NotNull] IConnectionStringAccessor connectionStringAccessor)
            : base(() => factory.Factory, generator, logger, options.Value, connectionStringAccessor)
        {
        }

        /// <inheritdoc />
        public override bool SchemaExists(string schemaName)
        {
            return true;
        }

        /// <inheritdoc />
        public override bool TableExists(string schemaName, string tableName)
        {
            return Exists(@"select table_name from information_schema.tables
                            where table_schema = SCHEMA() and table_name='{0}'", FormatHelper.FormatSqlEscape(tableName));
        }

        /// <inheritdoc />
        public override bool ColumnExists(string schemaName, string tableName, string columnName)
        {
            const string sql = @"select column_name from information_schema.columns
                            where table_schema = SCHEMA() and table_name='{0}'
                            and column_name='{1}'";
            return Exists(sql, FormatHelper.FormatSqlEscape(tableName), FormatHelper.FormatSqlEscape(columnName));
        }

        /// <inheritdoc />
        public override bool ConstraintExists(string schemaName, string tableName, string constraintName)
        {
            const string sql = @"select constraint_name from information_schema.table_constraints
                            where table_schema = SCHEMA() and table_name='{0}'
                            and constraint_name='{1}'";
            return Exists(sql, FormatHelper.FormatSqlEscape(tableName), FormatHelper.FormatSqlEscape(constraintName));
        }

        /// <inheritdoc />
        public override bool IndexExists(string schemaName, string tableName, string indexName)
        {
            const string sql = @"select index_name from information_schema.statistics
                            where table_schema = SCHEMA() and table_name='{0}'
                            and index_name='{1}'";
            return Exists(sql, FormatHelper.FormatSqlEscape(tableName), FormatHelper.FormatSqlEscape(indexName));
        }

        /// <inheritdoc />
        public override bool SequenceExists(string schemaName, string sequenceName)
        {
            return false;
        }

        /// <inheritdoc />
        public override bool DefaultValueExists(string schemaName, string tableName, string columnName, object defaultValue)
        {
            var defaultValueAsString = string.Format("%{0}%", FormatHelper.FormatSqlEscape(defaultValue.ToString()));
            return Exists("SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = SCHEMA() AND TABLE_NAME = '{0}' AND COLUMN_NAME = '{1}' AND COLUMN_DEFAULT LIKE '{2}'",
               FormatHelper.FormatSqlEscape(tableName), FormatHelper.FormatSqlEscape(columnName), defaultValueAsString);
        }

        /// <inheritdoc />
        public override void Execute(string template, params object[] args)
        {
            var commandText = string.Format(template, args);
            Logger.LogSql(commandText);

            if (Options.PreviewOnly)
            {
                return;
            }

            EnsureConnectionIsOpen();

            using (var command = CreateCommand(commandText))
            {
                command.ExecuteNonQuery();
            }
        }

        /// <inheritdoc />
        public override bool Exists(string template, params object[] args)
        {
            EnsureConnectionIsOpen();

            using (var command = CreateCommand(string.Format(template, args)))
            {
                using (var reader = command.ExecuteReader())
                {
                    try
                    {
                        return reader.Read();
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
        }

        /// <inheritdoc />
        public override DataSet ReadTableData(string schemaName, string tableName)
        {
            return Read("select * from {0}", _quoter.QuoteTableName(tableName, schemaName));
        }

        /// <inheritdoc />
        public override DataSet Read(string template, params object[] args)
        {
            EnsureConnectionIsOpen();

            using (var command = CreateCommand(string.Format(template, args)))
            using (var reader = command.ExecuteReader())
            {
                return reader.ReadDataSet();
            }
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

            using (var command = CreateCommand(sql))
            {
                command.ExecuteNonQuery();
            }
        }

        /// <inheritdoc />
        public override void Process(PerformDBOperationExpression expression)
        {
            var message = string.IsNullOrEmpty(expression.Description) 
                ? "Performing DB Operation" 
                : $"Performing DB Operation: {expression.Description}";
            Logger.LogSay(message);

            if (Options.PreviewOnly)
            {
                return;
            }

            EnsureConnectionIsOpen();

            // MySqlConnector requires the transaction to be set explicitly on the command, and it cannot be nested.
            expression.Operation?.Invoke(Connection, Transaction);
        }

        /// <inheritdoc />
        public override void Process(RenameColumnExpression expression)
        {
            // MySql 8.0+ supports column rename without needing to know the column type
            if (Generator is MySql8Generator)
            {
                base.Process(expression);
                return;
            }

            if (Generator is not MySql4Generator mysql4Generator)
            {
                throw new InvalidOperationException("MySql4Generator is required for this operation");
            }

            var columnDefinitionSql = string.Format(@"
SELECT CONCAT(
          CAST(COLUMN_TYPE AS CHAR),
          IF(ISNULL(CHARACTER_SET_NAME),
             '',
             CONCAT(' CHARACTER SET ', CHARACTER_SET_NAME)),
          IF(ISNULL(COLLATION_NAME),
             '',
             CONCAT(' COLLATE ', COLLATION_NAME)),
          ' ',
          IF(IS_NULLABLE = 'NO', 'NOT NULL ', ''),
          IF(IS_NULLABLE = 'NO' AND COLUMN_DEFAULT IS NULL,
             '',
             CONCAT('DEFAULT ', IF(COLUMN_DEFAULT = 'NULL', 'NULL', QUOTE(COLUMN_DEFAULT)), ' ')),
          IF(COLUMN_COMMENT = '', '', CONCAT('COMMENT ', QUOTE(COLUMN_COMMENT), ' ')),
          UPPER(extra))
  FROM INFORMATION_SCHEMA.COLUMNS
 WHERE TABLE_NAME = '{0}' AND COLUMN_NAME = '{1}' AND TABLE_SCHEMA = database()", FormatHelper.FormatSqlEscape(expression.TableName), FormatHelper.FormatSqlEscape(expression.OldName));

            var fieldValue = Read(columnDefinitionSql).Tables[0].Rows[0][0];
            var columnDefinition = fieldValue as string;

            Process(mysql4Generator.GenerateWithoutEndStatement(expression) + " " + columnDefinition);
        }
    }
}
