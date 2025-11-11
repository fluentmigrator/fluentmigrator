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
using System.IO;

using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators.Redshift;
using FluentMigrator.Runner.Helpers;
using FluentMigrator.Runner.Initialization;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Processors.Redshift
{
    /// <summary>
    /// The Amazon Redshift processor for FluentMigrator.
    /// </summary>
    public class RedshiftProcessor : GenericProcessorBase
    {
        private readonly RedshiftQuoter _quoter = new RedshiftQuoter();

        /// <inheritdoc />
        public override string DatabaseType => ProcessorIdConstants.Redshift;

        /// <inheritdoc />
        public override IList<string> DatabaseTypeAliases { get; } = new List<string>();

        /// <inheritdoc />
        public RedshiftProcessor(
            [NotNull] RedshiftDbFactory factory,
            [NotNull] RedshiftGenerator generator,
            [NotNull] ILogger<RedshiftProcessor> logger,
            [NotNull] IOptionsSnapshot<ProcessorOptions> options,
            [NotNull] IConnectionStringAccessor connectionStringAccessor)
            : base(() => factory.Factory, generator, logger, options.Value, connectionStringAccessor)
        {
        }

        /// <inheritdoc />
        public override void Execute(string template, params object[] args)
        {
            Process(string.Format(template, args));
        }

        /// <inheritdoc />
        public override bool SchemaExists(string schemaName)
        {
            return Exists("select * from information_schema.schemata where schema_name ilike '{0}'", FormatToSafeSchemaName(schemaName));
        }

        /// <inheritdoc />
        public override bool TableExists(string schemaName, string tableName)
        {
            return Exists("select * from information_schema.tables where table_schema ilike '{0}' and table_name ilike '{1}'", FormatToSafeSchemaName(schemaName), FormatToSafeName(tableName));
        }

        /// <inheritdoc />
        public override bool ColumnExists(string schemaName, string tableName, string columnName)
        {
            return Exists("select * from information_schema.columns where table_schema ilike '{0}' and table_name ilike '{1}' and column_name ilike '{2}'", FormatToSafeSchemaName(schemaName), FormatToSafeName(tableName), FormatToSafeName(columnName));
        }

        /// <inheritdoc />
        public override bool ConstraintExists(string schemaName, string tableName, string constraintName)
            => false;

        /// <inheritdoc />
        public override bool IndexExists(string schemaName, string tableName, string indexName)
            => false;

        /// <inheritdoc />
        public override bool SequenceExists(string schemaName, string sequenceName)
            => false;

        /// <inheritdoc />
        public override DataSet ReadTableData(string schemaName, string tableName)
        {
            return Read("SELECT * FROM {0}", _quoter.QuoteTableName(tableName, schemaName));
        }

        /// <inheritdoc />
        public override bool DefaultValueExists(string schemaName, string tableName, string columnName, object defaultValue)
        {
            string defaultValueAsString = string.Format("%{0}%", FormatHelper.FormatSqlEscape(defaultValue.ToString()));
            return Exists("select * from information_schema.columns where table_schema ilike '{0}' and table_name ilike '{1}' and column_name ilike '{2}' and column_default like '{3}'", FormatToSafeSchemaName(schemaName), FormatToSafeName(tableName), FormatToSafeName(columnName), defaultValueAsString);
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
                    using (var message = new StringWriter())
                    {
                        ReThrowWithSql(ex, sql);
                    }
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
        /// Formats the schema name to a safe SQL value.
        /// </summary>
        /// <param name="schemaName">The schema name.</param>
        /// <returns>The formatted schema name.</returns>
        private string FormatToSafeSchemaName(string schemaName)
        {
            return FormatHelper.FormatSqlEscape(_quoter.UnQuoteSchemaName(schemaName));
        }

        /// <summary>
        /// Formats the SQL name to a safe SQL value.
        /// </summary>
        /// <param name="sqlName">The SQL name.</param>
        /// <returns>The formatted SQL name.</returns>
        private string FormatToSafeName(string sqlName)
        {
            return FormatHelper.FormatSqlEscape(_quoter.UnQuote(sqlName));
        }
    }
}
