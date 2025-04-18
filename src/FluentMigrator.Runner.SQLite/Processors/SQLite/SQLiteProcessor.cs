#region License
//
// Copyright (c) 2007-2024, Fluent Migrator Project
// Copyright (c) 2010, Nathan Brown
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
using System.Data.Common;
using System.IO;

using FluentMigrator.Expressions;
using FluentMigrator.Runner.BatchParser;
using FluentMigrator.Runner.BatchParser.Sources;
using FluentMigrator.Runner.BatchParser.SpecialTokenSearchers;
using FluentMigrator.Runner.Generators.SQLite;
using FluentMigrator.Runner.Initialization;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Processors.SQLite
{

    // ReSharper disable once InconsistentNaming
    public class SQLiteProcessor : GenericProcessorBase
    {
        [CanBeNull]
        private readonly IServiceProvider _serviceProvider;
        [NotNull]
        private readonly SQLiteQuoter _quoter;

        public override string DatabaseType => ProcessorIdConstants.SQLite;

        public override IList<string> DatabaseTypeAliases { get; } = new List<string>();

        public SQLiteProcessor(
            [NotNull] SQLiteDbFactory factory,
            [NotNull] SQLiteGenerator generator,
            [NotNull] ILogger<SQLiteProcessor> logger,
            [NotNull] IOptionsSnapshot<ProcessorOptions> options,
            [NotNull] IConnectionStringAccessor connectionStringAccessor,
            [NotNull] IServiceProvider serviceProvider,
            [NotNull] SQLiteQuoter quoter)
            : base(() => factory.Factory, generator, logger, options.Value, connectionStringAccessor)
        {
            _serviceProvider = serviceProvider;
            _quoter = quoter;
        }

        public override bool SchemaExists(string schemaName)
        {
            return true;
        }

        public override bool TableExists(string schemaName, string tableName)
        {
            return Exists("select count(*) from {1}sqlite_master where name={0} and type='table'",
                _quoter.QuoteValue(tableName),
                !string.IsNullOrWhiteSpace(schemaName) ? _quoter.QuoteValue(schemaName) + "." : string.Empty);
        }

        public override bool ColumnExists(string schemaName, string tableName, string columnName)
        {
            return Exists("select count(*) from {2}sqlite_master AS t, {2}pragma_table_info(t.name) AS c where t.type = 'table' AND t.name = {0} AND c.name = {1}",
                _quoter.QuoteValue(tableName),
                _quoter.QuoteValue(columnName),
                !string.IsNullOrWhiteSpace(schemaName) ? _quoter.QuoteValue(schemaName) + "." : string.Empty);
        }

        public override bool ConstraintExists(string schemaName, string tableName, string constraintName)
        {
            return Exists("select count(*) from {2}sqlite_master where name={0} and tbl_name={1} and type='index' and sql LIKE 'CREATE UNIQUE INDEX %'",
                   _quoter.QuoteValue(constraintName),
                   _quoter.QuoteValue(tableName),
                   !string.IsNullOrWhiteSpace(schemaName) ? _quoter.QuoteValue(schemaName) + "." : string.Empty);
        }

        public override bool IndexExists(string schemaName, string tableName, string indexName)
        {
            return Exists("select count(*) from {2}sqlite_master where name={0} and tbl_name={1} and type='index'",
                _quoter.QuoteValue(indexName),
                _quoter.QuoteValue(tableName),
                !string.IsNullOrWhiteSpace(schemaName) ? _quoter.QuoteValue(schemaName) + "." : string.Empty);
        }

        public override bool SequenceExists(string schemaName, string sequenceName)
        {
            return false;
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
                try
                {
                    if (!reader.Read()) return false;
                    if (int.Parse(reader[0].ToString()) <= 0) return false;
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public override DataSet ReadTableData(string schemaName, string tableName)
        {
            return Read("select * from {0}", _quoter.QuoteTableName(tableName, schemaName));
        }

        public override bool DefaultValueExists(string schemaName, string tableName, string columnName, object defaultValue)
        {
            return false;
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
            if (string.IsNullOrEmpty(sql))
                return;

            if (Options.PreviewOnly)
            {
                ExecuteBatchNonQuery(
                    sql,
                    (sqlBatch) =>
                    {
                        Logger.LogSql(sqlBatch);
                    },
                    (sqlBatch, goCount) =>
                    {
                        Logger.LogSql(sqlBatch);
                        Logger.LogSql($"GO {goCount}");
                    });
                return;
            }

            Logger.LogSql(sql);

            EnsureConnectionIsOpen();

            if (ContainsGo(sql))
            {
                ExecuteBatchNonQuery(
                    sql,
                    (sqlBatch) =>
                    {
                        using (var command = CreateCommand(sqlBatch))
                        {
                            command.ExecuteNonQuery();
                        }
                    },
                    (sqlBatch, goCount) =>
                    {
                        using (var command = CreateCommand(sqlBatch))
                        {
                            for (var i = 0; i != goCount; ++i)
                            {
                                command.ExecuteNonQuery();
                            }
                        }
                    });
            }
            else
            {
                ExecuteNonQuery(sql);
            }


        }

        private bool ContainsGo(string sql)
        {
            var containsGo = false;
            var parser = _serviceProvider?.GetService<SQLiteBatchParser>() ?? new SQLiteBatchParser();
            parser.SpecialToken += (sender, args) => containsGo = true;
            using (var source = new TextReaderSource(new StringReader(sql), true))
            {
                parser.Process(source);
            }

            return containsGo;
        }

        private void ExecuteNonQuery(string sql)
        {
            using (var command = CreateCommand(sql))
            {
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (DbException ex)
                {
                    throw new Exception(ex.Message + Environment.NewLine + "While Processing:" + Environment.NewLine + "\"" + command.CommandText + "\"", ex);
                }
            }
        }

        private void ExecuteBatchNonQuery(string sql, Action<string> executeBatch, Action<string, int> executeGo)
        {
            string sqlBatch = string.Empty;

            try
            {
                var parser = _serviceProvider?.GetService<SQLiteBatchParser>() ?? new SQLiteBatchParser();
                parser.SqlText += (sender, args) => { sqlBatch = args.SqlText.Trim(); };
                parser.SpecialToken += (sender, args) =>
                {
                    if (string.IsNullOrEmpty(sqlBatch))
                        return;

                    if (args.Opaque is GoSearcher.GoSearcherParameters goParams)
                    {
                        executeGo(sqlBatch, goParams.Count);
                    }

                    sqlBatch = null;
                };

                using (var source = new TextReaderSource(new StringReader(sql), true))
                {
                    parser.Process(source, stripComments: Options.StripComments);
                }

                if (!string.IsNullOrEmpty(sqlBatch))
                {
                    executeBatch(sqlBatch);
                }
            }
            catch (DbException ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "While Processing:" + Environment.NewLine + "\"" + sqlBatch + "\"", ex);
            }
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
    }
}
