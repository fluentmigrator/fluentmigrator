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

using System.Collections.Generic;
using System.Data;
using System.Linq;

using FluentMigrator.Expressions;
using FluentMigrator.Generation;
using FluentMigrator.Runner.Generators.DB2.iSeries;
using FluentMigrator.Runner.Helpers;
using FluentMigrator.Runner.Initialization;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Processors.DB2.iSeries
{
    public class Db2ISeriesProcessor : GenericProcessorBase
    {

        public Db2ISeriesProcessor(
            [NotNull] Db2ISeriesDbFactory factory,
            [NotNull] Db2ISeriesGenerator generator,
            [NotNull] Db2ISeriesQuoter quoter,
            [NotNull] ILogger<Db2ISeriesProcessor> logger,
            [NotNull] IOptionsSnapshot<ProcessorOptions> options,
            [NotNull] IConnectionStringAccessor connectionStringAccessor)
            : base(() => factory.Factory, generator, logger, options.Value, connectionStringAccessor)
        {
            Quoter = quoter;
        }

        public override string DatabaseType => ProcessorIdConstants.Db2ISeries;

        public override IList<string> DatabaseTypeAliases { get; } = new List<string> { ProcessorIdConstants.IbmDb2ISeries, ProcessorIdConstants.DB2 };

        public IQuoter Quoter
        {
            get;
            set;
        }

        public override bool ColumnExists(string schemaName, string tableName, string columnName)
        {
            var schema = string.IsNullOrEmpty(schemaName) ? string.Empty : "TABLE_SCHEMA = '" + FormatToSafeName(schemaName) + "' AND ";

            var doesExist = Exists("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE {0} TABLE_NAME = '{1}' AND COLUMN_NAME='{2}'", schema, FormatToSafeName(tableName), FormatToSafeName(columnName));
            return doesExist;
        }

        public override bool ConstraintExists(string schemaName, string tableName, string constraintName)
        {
            var schema = string.IsNullOrEmpty(schemaName) ? string.Empty : "TABLE_SCHEMA = '" + FormatToSafeName(schemaName) + "' AND ";

            return Exists("SELECT CONSTRAINT_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE {0} TABLE_NAME = '{1}' AND CONSTRAINT_NAME='{2}'", schema, FormatToSafeName(tableName), FormatToSafeName(constraintName));
        }

        public override bool DefaultValueExists(string schemaName, string tableName, string columnName, object defaultValue)
        {
            var schema = string.IsNullOrEmpty(schemaName) ? string.Empty : "TABLE_SCHEMA = '" + FormatToSafeName(schemaName) + "' AND ";
            var defaultValueAsString = string.Format("%{0}%", FormatHelper.FormatSqlEscape(defaultValue.ToString()));

            return Exists("SELECT COLUMN_DEFAULT FROM INFORMATION_SCHEMA.COLUMNS WHERE {0} TABLE_NAME = '{1}' AND COLUMN_NAME = '{2}' AND COLUMN_DEFAULT LIKE '{3}'", schema, FormatToSafeName(tableName), columnName.ToUpper(), defaultValueAsString);
        }

        public override void Execute(string template, params object[] args)
        {
            Process(string.Format(template, args));
        }

        public override bool Exists(string template, params object[] args)
        {
            EnsureConnectionIsOpen();

            using (var command = CreateCommand(string.Format(template, args)))
            using (var reader = command.ExecuteReader())
            {
                return reader.Read();
            }
        }

        public override bool IndexExists(string schemaName, string tableName, string indexName)
        {
            var schema = string.IsNullOrEmpty(schemaName) ? string.Empty : "INDEX_SCHEMA = '" + FormatToSafeName(schemaName) + "' AND ";

            var doesExist = Exists(
                "SELECT NAME FROM INFORMATION_SCHEMA.SYSINDEXES WHERE {0}TABLE_NAME = '{1}' AND NAME = '{2}'",
                schema,
                FormatToSafeName(tableName),
                FormatToSafeName(indexName));

            return doesExist;
        }

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

        public override DataSet Read(string template, params object[] args)
        {
            EnsureConnectionIsOpen();

            using (var command = CreateCommand(string.Format(template, args)))
            using (var reader = command.ExecuteReader())
            {
                return reader.ReadDataSet();
            }
        }

        public override DataSet ReadTableData(string schemaName, string tableName)
        {
            return Read("SELECT * FROM {0}", Quoter.QuoteTableName(tableName, schemaName));
        }

        public override bool SchemaExists(string schemaName)
        {
            return Exists("SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{0}'", FormatToSafeName(schemaName));
        }

        public override bool SequenceExists(string schemaName, string sequenceName)
        {
            return false;
        }

        public override bool TableExists(string schemaName, string tableName)
        {
            var schema = string.IsNullOrEmpty(schemaName) ? string.Empty : "TABLE_SCHEMA = '" + FormatToSafeName(schemaName) + "' AND ";

            return Exists("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE {0}TABLE_NAME = '{1}'", schema, FormatToSafeName(tableName));
        }

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

        private string FormatToSafeName(string sqlName)
        {
            var rawName = Quoter.UnQuote(sqlName);

            return rawName.Contains('\'') ? FormatHelper.FormatSqlEscape(rawName) : rawName.ToUpper();
        }
    }
}
