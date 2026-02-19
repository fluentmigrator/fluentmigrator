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
using System.Data;

using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators.Postgres;
using FluentMigrator.Runner.Helpers;
using FluentMigrator.Runner.Initialization;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Processors.Postgres
{
    /// <summary>
    /// The PostgreSQL migration processor.
    /// </summary>
    public class PostgresProcessor : GenericProcessorBase
    {
        private readonly PostgresQuoter _quoter;

        /// <inheritdoc />
        public override string DatabaseType => ProcessorIdConstants.Postgres;

        /// <inheritdoc />
        public override IList<string> DatabaseTypeAliases { get; } = new List<string> { ProcessorIdConstants.PostgreSQL };

        /// <inheritdoc />
        public PostgresProcessor(
            [NotNull] PostgresDbFactory factory,
            [NotNull] PostgresGenerator generator,
            [NotNull] ILogger<PostgresProcessor> logger,
            [NotNull] IOptionsSnapshot<ProcessorOptions> options,
            [NotNull] IConnectionStringAccessor connectionStringAccessor,
            [NotNull] PostgresOptions pgOptions)
            : base(() => factory.Factory, generator, logger, options.Value, connectionStringAccessor)
        {
            if (pgOptions == null)
            {
                throw new ArgumentNullException(nameof(pgOptions));
            }

            _quoter = new PostgresQuoter(pgOptions);
        }

        /// <inheritdoc />
        public override void Execute(string template, params object[] args)
        {
            Process(string.Format(template, args));
        }

        /// <inheritdoc />
        public override bool SchemaExists(string schemaName)
        {
            return Exists("select * from information_schema.schemata where schema_name = '{0}'", FormatToSafeSchemaName(schemaName));
        }

        /// <inheritdoc />
        public override bool TableExists(string schemaName, string tableName)
        {
            return Exists("select * from information_schema.tables where table_schema = '{0}' and table_name = '{1}'", FormatToSafeSchemaName(schemaName), FormatToSafeName(tableName));
        }

        /// <inheritdoc />
        public override bool ColumnExists(string schemaName, string tableName, string columnName)
        {
            return Exists("select * from information_schema.columns where table_schema = '{0}' and table_name = '{1}' and column_name = '{2}'", FormatToSafeSchemaName(schemaName), FormatToSafeName(tableName), FormatToSafeName(columnName));
        }

        /// <inheritdoc />
        public override bool ConstraintExists(string schemaName, string tableName, string constraintName)
        {
            return Exists("select * from information_schema.table_constraints where constraint_catalog = current_catalog and table_schema = '{0}' and table_name = '{1}' and constraint_name = '{2}'", FormatToSafeSchemaName(schemaName), FormatToSafeName(tableName), FormatToSafeName(constraintName));
        }

        /// <inheritdoc />
        public override bool IndexExists(string schemaName, string tableName, string indexName)
        {
            return Exists("select * from pg_catalog.pg_indexes where schemaname='{0}' and tablename = '{1}' and indexname = '{2}'", FormatToSafeSchemaName(schemaName), FormatToSafeName(tableName), FormatToSafeName(indexName));
        }

        /// <inheritdoc />
        public override bool SequenceExists(string schemaName, string sequenceName)
        {
            return Exists("select * from information_schema.sequences where sequence_catalog = current_catalog and sequence_schema ='{0}' and sequence_name = '{1}'", FormatToSafeSchemaName(schemaName), FormatToSafeName(sequenceName));
        }

        /// <inheritdoc />
        public override DataSet ReadTableData(string schemaName, string tableName)
        {
            return Read("SELECT * FROM {0}", _quoter.QuoteTableName(tableName, schemaName));
        }

        /// <inheritdoc />
        public override bool DefaultValueExists(string schemaName, string tableName, string columnName, object defaultValue)
        {
            string defaultValueAsString = string.Format("%{0}%", FormatHelper.FormatSqlEscape(defaultValue.ToString()));
            return Exists("select * from information_schema.columns where table_schema = '{0}' and table_name = '{1}' and column_name = '{2}' and column_default like '{3}'", FormatToSafeSchemaName(schemaName), FormatToSafeName(tableName), FormatToSafeName(columnName), defaultValueAsString);
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
        public override bool Exists(string template, params object[] args)
        {
            EnsureConnectionIsOpen();

            using (var command = CreateCommand(string.Format(template, args)))
            using (var reader = command.ExecuteReader())
            {
                return reader.Read();
            }
        }

        /// <inheritdoc />
        protected override void Process(string sql)
        {
            Logger.LogSql(sql);

            if (Options.PreviewOnly || string.IsNullOrEmpty(sql))
                return;

            EnsureConnectionIsOpen();

            using (var command = CreateCommand(sql))
            {
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    ReThrowWithSql(ex, sql);
                }
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
                return;

            EnsureConnectionIsOpen();

            expression.Operation?.Invoke(Connection, Transaction);
        }

        /// <summary>
        /// Formats a schema name for safe SQL usage.
        /// </summary>
        /// <param name="schemaName">The schema name.</param>
        /// <returns>The formatted schema name.</returns>
        private string FormatToSafeSchemaName(string schemaName)
        {
            var schemaNameCased = schemaName;
            if (!_quoter.Options.ForceQuote)
            {
                schemaNameCased = schemaName?.ToLowerInvariant();
            }

            return FormatHelper.FormatSqlEscape(_quoter.UnQuoteSchemaName(schemaNameCased));
        }

        /// <summary>
        /// Formats a SQL identifier for safe SQL usage.
        /// </summary>
        /// <param name="sqlName">The identifier name.</param>
        /// <returns>The formatted name.</returns>
        private string FormatToSafeName(string sqlName)
        {
            var sqlNameCased = sqlName;
            if (!_quoter.Options.ForceQuote)
            {
                sqlNameCased = sqlName?.ToLowerInvariant();
            }

            return FormatHelper.FormatSqlEscape(_quoter.UnQuote(sqlNameCased));
        }
    }
}
