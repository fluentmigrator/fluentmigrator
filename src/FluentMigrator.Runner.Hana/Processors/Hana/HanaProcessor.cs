using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.Hana;
using FluentMigrator.Runner.Helpers;
using FluentMigrator.Runner.Initialization;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Processors.Hana
{
    public class HanaProcessor : GenericProcessorBase
    {

        public HanaProcessor(
            [NotNull] HanaDbFactory factory,
            [NotNull] HanaGenerator generator,
            [NotNull] ILogger<HanaProcessor> logger,
            [NotNull] IOptionsSnapshot<ProcessorOptions> options,
            [NotNull] IConnectionStringAccessor connectionStringAccessor)
            : base(() => factory.Factory, generator, logger, options.Value, connectionStringAccessor)
        {
        }

        public override string DatabaseType => ProcessorIdConstants.Hana;

        public override IList<string> DatabaseTypeAliases { get; } = new List<string>();

        public IQuoter Quoter => ((HanaGenerator)Generator).Quoter;

        public override bool SchemaExists(string schemaName)
        {
            return false;
        }

        public override bool TableExists(string schemaName, string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentNullException(nameof(tableName));

            return Exists(
                "SELECT 1 FROM TABLES WHERE SCHEMA_NAME = CURRENT_SCHEMA AND TABLE_NAME = '{0}'",
                FormatHelper.FormatSqlEscape(Quoter.UnQuote(tableName)));
        }

        public override bool ColumnExists(string schemaName, string tableName, string columnName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (columnName == null)
                throw new ArgumentNullException(nameof(columnName));

            return Exists("SELECT 1 FROM TABLE_COLUMNS WHERE SCHEMA_NAME = CURRENT_SCHEMA AND upper(TABLE_NAME) = '{0}' AND upper(COLUMN_NAME) = '{1}'",
                FormatHelper.FormatSqlEscape(Quoter.UnQuote(tableName).ToUpper()),
                FormatHelper.FormatSqlEscape(Quoter.UnQuote(columnName).ToUpper()));
        }

        public override bool ConstraintExists(string schemaName, string tableName, string constraintName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (constraintName == null)
                throw new ArgumentNullException(nameof(constraintName));

            if (constraintName.Length == 0)
                return false;

            return Exists("SELECT 1 FROM CONSTRAINTS WHERE SCHEMA_NAME = CURRENT_SCHEMA and upper(CONSTRAINT_NAME) = '{0}'",
                FormatHelper.FormatSqlEscape(Quoter.UnQuote(constraintName).ToUpper()));
        }

        public override bool IndexExists(string schemaName, string tableName, string indexName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (indexName == null)
                throw new ArgumentNullException(nameof(indexName));

            if (indexName.Length == 0)
                return false;

            return Exists("SELECT 1 FROM INDEXES WHERE SCHEMA_NAME = CURRENT_SCHEMA AND upper(INDEX_NAME) = '{0}'",
                    FormatHelper.FormatSqlEscape(Quoter.UnQuote(indexName).ToUpper()));
        }

        public override bool SequenceExists(string schemaName, string sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            if (string.IsNullOrEmpty(sequenceName))
                return false;

            return Exists("SELECT 1 FROM SEQUENCES WHERE SCHEMA_NAME = CURRENT_SCHEMA and upper(SEQUENCE_NAME) = '{0}'",
                FormatHelper.FormatSqlEscape(Quoter.UnQuote(sequenceName).ToUpper()));
        }

        public override bool DefaultValueExists(string schemaName, string tableName, string columnName, object defaultValue)
        {
            return false;
        }

        public override void Execute(string template, params object[] args)
        {
            Process(string.Format(template, args));
        }

        public override bool Exists(string template, params object[] args)
        {
            if (template == null)
                throw new ArgumentNullException(nameof(template));

            EnsureConnectionIsOpen();

            var querySql = string.Format(template, args);

            Logger.LogSql($"{querySql};");

            using (var command = CreateCommand(string.Format(template, args)))
            using (var reader = command.ExecuteReader())
            {
                return reader.Read();
            }
        }

        public override DataSet ReadTableData(string schemaName, string tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return Read("SELECT * FROM {0}", Quoter.QuoteTableName(tableName, schemaName));
        }

        public override DataSet Read(string template, params object[] args)
        {
            if (template == null)
                throw new ArgumentNullException(nameof(template));

            EnsureConnectionIsOpen();

            using (var command = CreateCommand(string.Format(template, args)))
            using (var reader = command.ExecuteReader())
            {
                return reader.ReadDataSet();
            }
        }

        public override void Process(PerformDBOperationExpression expression)
        {
            Logger.LogSay("Performing DB Operation");

            if (Options.PreviewOnly)
                return;

            EnsureConnectionIsOpen();

            expression.Operation?.Invoke(Connection, Transaction);
        }

        protected override void Process(string sql)
        {
            Logger.LogSql(sql);

            if (Options.PreviewOnly || string.IsNullOrEmpty(sql))
                return;

            EnsureConnectionIsOpen();

            var batches = Regex.Split(sql, @"^\s*;\s*$", RegexOptions.Multiline)
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(c => c.Trim());

            foreach (var batch in batches)
            {
                var batchCommand = batch.EndsWith(";")
                    ? batch.Remove(batch.Length - 1)
                    : batch;

                using (var command = CreateCommand(batchCommand))
                    command.ExecuteNonQuery();
            }
        }

    }
}
