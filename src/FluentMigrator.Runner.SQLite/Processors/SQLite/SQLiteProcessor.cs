#region License
//
// Copyright (c) 2007-2018, Sean Chambers <schambers80@gmail.com>
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

        public override string DatabaseType
        {
            get { return "SQLite"; }
        }

        public override IList<string> DatabaseTypeAliases { get; } = new List<string>();

        [Obsolete]
        public SQLiteProcessor(
            IDbConnection connection,
            IMigrationGenerator generator,
            IAnnouncer announcer,
            [NotNull] IMigrationProcessorOptions options,
            IDbFactory factory)
            : base(connection, factory, generator, announcer, options)
        {
        }

        public SQLiteProcessor(
            [NotNull] SQLiteDbFactory factory,
            [NotNull] SQLiteGenerator generator,
            [NotNull] ILogger<SQLiteProcessor> logger,
            [NotNull] IOptionsSnapshot<ProcessorOptions> options,
            [NotNull] IConnectionStringAccessor connectionStringAccessor,
            [NotNull] IServiceProvider serviceProvider)
            : base(() => factory.Factory, generator, logger, options.Value, connectionStringAccessor)
        {
            _serviceProvider = serviceProvider;
        }

        public override bool SchemaExists(string schemaName)
        {
            return true;
        }

        public override bool TableExists(string schemaName, string tableName)
        {
            return Exists("select count(*) from sqlite_master where name=\"{0}\" and type='table'", tableName);
        }

        public override bool ColumnExists(string schemaName, string tableName, string columnName)
        {
            var dataSet = Read("PRAGMA table_info([{0}])", tableName);
            if (dataSet.Tables.Count == 0)
                return false;
            var table = dataSet.Tables[0];
            if (!table.Columns.Contains("Name"))
                return false;
            return table.Select(string.Format("Name='{0}'", columnName.Replace("'", "''"))).Length > 0;
        }

        public override bool ConstraintExists(string schemaName, string tableName, string constraintName)
        {
            return false;
        }

        public override bool IndexExists(string schemaName, string tableName, string indexName)
        {
            return Exists("select count(*) from sqlite_master where name='{0}' and tbl_name='{1}' and type='index'", indexName, tableName);
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
            return Read("select * from [{0}]", tableName);
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

            expression.Operation?.Invoke(Connection, null);
        }

        protected override void Process(string sql)
        {
            Logger.LogSql(sql);

            if (Options.PreviewOnly || string.IsNullOrEmpty(sql))
                return;

            EnsureConnectionIsOpen();

            if (ContainsGo(sql))
            {
                ExecuteBatchNonQuery(sql);

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
                    throw new Exception(ex.Message + "\r\nWhile Processing:\r\n\"" + command.CommandText + "\"", ex);
                }
            }
        }

        private void ExecuteBatchNonQuery(string sql)
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
                        using (var command = CreateCommand(sqlBatch))
                        {
                            for (var i = 0; i != goParams.Count; ++i)
                            {
                                command.ExecuteNonQuery();
                            }
                        }
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
            catch (DbException ex)
            {
                throw new Exception(ex.Message + "\r\nWhile Processing:\r\n\"" + sqlBatch + "\"", ex);
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
