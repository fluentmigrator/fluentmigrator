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
using FluentMigrator.Runner.BatchParser;
using FluentMigrator.Runner.BatchParser.Sources;
using FluentMigrator.Runner.Generators.Snowflake;
using FluentMigrator.Runner.Helpers;
using FluentMigrator.Runner.Initialization;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Microsoft.Extensions.DependencyInjection;

namespace FluentMigrator.Runner.Processors.Snowflake
{
    public class SnowflakeProcessor : GenericProcessorBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly bool _quoteIdentifiers;

        /// <inheritdoc />
        public SnowflakeProcessor(
            [NotNull] SnowflakeDbFactory factory,
            [NotNull] SnowflakeGenerator generator,
            [NotNull] SnowflakeQuoter quoter,
            [NotNull] ILogger<SnowflakeProcessor> logger,
            [NotNull] IOptionsSnapshot<ProcessorOptions> options,
            [NotNull] IConnectionStringAccessor connectionStringAccessor,
            [NotNull] SnowflakeOptions sfOptions,
            [NotNull] IServiceProvider serviceProvider) : base(() => factory.Factory, generator, logger, options.Value, connectionStringAccessor)
        {
            _quoteIdentifiers = sfOptions.QuoteIdentifiers;
            Quoter = quoter;
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public override string DatabaseType => "Snowflake";

        /// <inheritdoc />
        public override IList<string> DatabaseTypeAliases => new List<string>();

        public SnowflakeQuoter Quoter { get; }

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

        private bool ContainsMultipleStatements(string sql)
        {
            var containsMultipleStatements = false;
            var parser = _serviceProvider?.GetService<SnowflakeBatchParser>() ?? new SnowflakeBatchParser();
            parser.SpecialToken += (sender, args) => containsMultipleStatements = true;
            using (var source = new TextReaderSource(new StringReader(sql), true))
            {
                parser.Process(source);
            }

            return containsMultipleStatements;
        }

        private void ExecuteBatchNonQuery(string sql)
        {
            var sqlBatch = string.Empty;

            try
            {
                var parser = _serviceProvider?.GetService<SnowflakeBatchParser>() ?? new SnowflakeBatchParser();
                parser.SqlText += (sender, args) => sqlBatch = args.SqlText.Trim();
                parser.SpecialToken += (sender, args) =>
                {
                    if (string.IsNullOrEmpty(sqlBatch))
                    {
                        return;
                    }

                    using (var command = CreateCommand(sqlBatch))
                    {
                        command.ExecuteNonQuery();
                    }

                    sqlBatch = null;
                };

                using (var source = new TextReaderSource(new StringReader(sql), true))
                {
                    parser.Process(source, stripComments: Options.StripComments);
                }

                if (!string.IsNullOrEmpty(sqlBatch))
                {
                    using (var command = CreateCommand(sqlBatch))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                using (var message = new StringWriter())
                {
                    message.WriteLine("An error occured executing the following sql:");
                    message.WriteLine(string.IsNullOrEmpty(sqlBatch) ? sql : sqlBatch);
                    message.WriteLine("The error was {0}", ex.Message);

                    throw new Exception(message.ToString(), ex);
                }
            }
        }

        private void ExecuteNonQuery(string sql)
        {
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
                        message.WriteLine("An error occured executing the following sql:");
                        message.WriteLine(sql);
                        message.WriteLine("The error was {0}", ex.Message);

                        throw new Exception(message.ToString(), ex);
                    }
                }
            }
        }

        private string FormatSnowflakeMetadataQueryIdentifier(string identifier)
        {
            return FormatHelper.FormatSqlEscape(_quoteIdentifiers ? identifier : identifier.ToUpperInvariant());
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

            if (ContainsMultipleStatements(sql))
            {
                ExecuteBatchNonQuery(sql);
            }
            else
            {
                ExecuteNonQuery(sql);
            }
        }

        /// <inheritdoc />
        public override void Execute(string template, params object[] args)
        {
            Process(string.Format(template, args));
        }

        /// <inheritdoc />
        public override bool SchemaExists(string schemaName)
        {
            return Exists("SELECT 1 WHERE EXISTS (SELECT * FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{0}')", FormatSnowflakeMetadataQueryIdentifier(schemaName));
        }

        /// <inheritdoc />
        public override bool TableExists(string schemaName, string tableName)
        {
            return Exists(
                "SELECT 1 WHERE EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{0}' AND TABLE_NAME = '{1}' AND TABLE_TYPE = 'BASE TABLE')",
                FormatSnowflakeMetadataQueryIdentifier(schemaName),
                FormatSnowflakeMetadataQueryIdentifier(tableName));
        }

        /// <inheritdoc />
        public override bool ColumnExists(string schemaName, string tableName, string columnName)
        {
            return Exists(
                "SELECT 1 WHERE EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = '{0}' AND TABLE_NAME = '{1}' AND COLUMN_NAME = '{2}')",
                FormatSnowflakeMetadataQueryIdentifier(schemaName),
                FormatSnowflakeMetadataQueryIdentifier(tableName),
                FormatSnowflakeMetadataQueryIdentifier(columnName));
        }

        /// <inheritdoc />
        public override bool ConstraintExists(string schemaName, string tableName, string constraintName)
        {
            return Exists(
                "SELECT 1 WHERE EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_CATALOG = CURRENT_DATABASE() AND TABLE_SCHEMA = '{0}' AND TABLE_NAME = '{1}' AND CONSTRAINT_NAME = '{2}')",
                FormatSnowflakeMetadataQueryIdentifier(schemaName),
                FormatSnowflakeMetadataQueryIdentifier(tableName),
                FormatSnowflakeMetadataQueryIdentifier(constraintName));
        }

        /// <inheritdoc />
        public override bool IndexExists(string schemaName, string tableName, string indexName)
        {
            return false;
        }

        /// <inheritdoc />
        public override bool SequenceExists(string schemaName, string sequenceName)
        {
            return Exists(
                "SELECT 1 WHERE EXISTS (SELECT * FROM INFORMATION_SCHEMA.SEQUENCES WHERE SEQUENCE_SCHEMA = '{0}' AND SEQUENCE_NAME = '{1}')",
                FormatSnowflakeMetadataQueryIdentifier(schemaName),
                FormatSnowflakeMetadataQueryIdentifier(sequenceName));
        }

        /// <inheritdoc />
        public override bool DefaultValueExists(string schemaName, string tableName, string columnName, object defaultValue)
        {
            var defaultValueAsString = $"%{FormatHelper.FormatSqlEscape(defaultValue.ToString())}%";
            return Exists(
                "SELECT 1 WHERE EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = '{0}' AND TABLE_NAME = '{1}' AND COLUMN_NAME = '{2}' AND COLUMN_DEFAULT LIKE '{3}')",
                FormatSnowflakeMetadataQueryIdentifier(schemaName),
                FormatSnowflakeMetadataQueryIdentifier(tableName),
                FormatSnowflakeMetadataQueryIdentifier(columnName),
                defaultValueAsString);
        }

        /// <inheritdoc />
        public override DataSet ReadTableData(string schemaName, string tableName)
        {
            return Read($"SELECT * FROM {Quoter.QuoteTableName(tableName, schemaName)}");
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
    }
}
