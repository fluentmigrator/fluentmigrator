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
using System.Data;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

using FluentMigrator.Expressions;
using FluentMigrator.Runner.BatchParser;
using FluentMigrator.Runner.BatchParser.RangeSearchers;
using FluentMigrator.Runner.BatchParser.Sources;
using FluentMigrator.Runner.BatchParser.SpecialTokenSearchers;
using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.Runner.Helpers;
using FluentMigrator.Runner.Initialization;

using JetBrains.Annotations;

using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Processors.SqlServer
{
    public sealed class SqlServerCeProcessor : GenericProcessorBase
    {
        public override string DatabaseType => "SqlServerCe";

        public override IList<string> DatabaseTypeAliases { get; } = new List<string> { "SqlServer" };

        [Obsolete]
        public SqlServerCeProcessor(IDbConnection connection, IMigrationGenerator generator, IAnnouncer announcer, IMigrationProcessorOptions options, IDbFactory factory)
            : base(connection, factory, generator, announcer, options)
        {
        }

        public SqlServerCeProcessor(
            [NotNull] SqlServerCeDbFactory factory,
            [NotNull] SqlServerCeGenerator generator,
            [NotNull] IAnnouncer announcer,
            [NotNull] IOptions<ProcessorOptions> options,
            [NotNull] IConnectionStringAccessor connectionStringAccessor)
            : base(() => factory.Factory, generator, announcer, options.Value, connectionStringAccessor)
        {
        }

        public override bool SchemaExists(string schemaName)
        {
            return true; // SqlServerCe has no schemas
        }

        public override bool TableExists(string schemaName, string tableName)
        {
            return Exists("SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}'", FormatHelper.FormatSqlEscape(tableName));
        }

        public override bool ColumnExists(string schemaName, string tableName, string columnName)
        {
            return Exists("SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{0}' AND COLUMN_NAME = '{1}'",
                FormatHelper.FormatSqlEscape(tableName), FormatHelper.FormatSqlEscape(columnName));
        }

        public override bool ConstraintExists(string schemaName, string tableName, string constraintName)
        {
            return Exists("SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE TABLE_NAME = '{0}' AND CONSTRAINT_NAME = '{1}'",
                FormatHelper.FormatSqlEscape(tableName), FormatHelper.FormatSqlEscape(constraintName));
        }

        public override bool IndexExists(string schemaName, string tableName, string indexName)
        {
            return Exists("SELECT NULL FROM INFORMATION_SCHEMA.INDEXES WHERE INDEX_NAME = '{0}'", FormatHelper.FormatSqlEscape(indexName));
        }

        public override bool SequenceExists(string schemaName, string sequenceName)
        {
            return false;
        }

        public override bool DefaultValueExists(string schemaName, string tableName, string columnName, object defaultValue)
        {
            return false;
        }

        public override void Execute(string template, params object[] args)
        {
            Process(String.Format(template, args));
        }

        public override bool Exists(string template, params object[] args)
        {
            EnsureConnectionIsOpen();

            using (var command = CreateCommand(String.Format(template, args)))
            using (var reader = command.ExecuteReader())
            {
                return reader.Read();
            }
        }

        public override DataSet ReadTableData(string schemaName, string tableName)
        {
            return Read("SELECT * FROM [{0}]", tableName);
        }

        public override DataSet Read(string template, params object[] args)
        {
            EnsureConnectionIsOpen();

            using (var command = CreateCommand(String.Format(template, args)))
            using (var reader = command.ExecuteReader())
            {
                return reader.ReadDataSet();
            }
        }

        protected override void Process(string sql)
        {
            Announcer.Sql(sql);

            if (Options.PreviewOnly || string.IsNullOrEmpty(sql))
                return;

            EnsureConnectionIsOpen();

            using (var command = CreateCommand(string.Empty))
            {
                foreach (string statement in SplitIntoSingleStatements(sql))
                {
                    try
                    {
                        command.CommandText = statement;
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        using (var message = new StringWriter())
                        {
                            message.WriteLine("An error occurred executing the following sql:");
                            message.WriteLine(statement);
                            message.WriteLine("The error was {0}", ex.Message);

                            throw new Exception(message.ToString(), ex);
                        }
                    }
                }
            }
        }

        private IEnumerable<string> SplitIntoSingleStatements(string sql)
        {
            var sqlStatements = new List<string>();

            // The default range searchers
            var rangeSearchers = new List<IRangeSearcher>
            {
                new MultiLineComment(),
                new DoubleDashSingleLineComment(),
                new PoundSignSingleLineComment(),
                new SqlString(),
                new SqlServerIdentifier(),
            };

            // The special token searchers
            var specialTokenSearchers = new List<ISpecialTokenSearcher>()
            {
                new GoSearcher(),
                new SemicolonSearcher(),
            };

            var parser = new SqlBatchParser(rangeSearchers, specialTokenSearchers);
            parser.SqlText += (sender, args) =>
            {
                var content = args.SqlText.Trim();
                if (!string.IsNullOrEmpty(content))
                {
                    sqlStatements.Add(content + ";");
                }
            };

            using (var source = new TextReaderSource(new StringReader(sql), true))
            {
                parser.Process(source);
            }

            return sqlStatements;
        }

        public override void Process(PerformDBOperationExpression expression)
        {
            EnsureConnectionIsOpen();

            expression.Operation?.Invoke(Connection, Transaction);
        }

        /// <inheritdoc />
        protected override IDbCommand CreateCommand(string commandText, IDbConnection connection, IDbTransaction transaction)
        {
            IDbCommand result;
            if (DbProviderFactory != null)
            {
                result = DbProviderFactory.CreateCommand();
                Debug.Assert(result != null, nameof(result) + " != null");
                result.Connection = connection;
                if (transaction != null)
                    result.Transaction = transaction;
                result.CommandText = commandText;
            }
            else
            {
#pragma warning disable 612
                result = Factory.CreateCommand(commandText, connection, transaction, Options);
#pragma warning restore 612
            }

            // SQL Server CE does not support non-zero command timeout values!! :/

            return result;
        }
    }
}
