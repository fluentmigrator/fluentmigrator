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
using FluentMigrator.Runner.Generators.Oracle;
using FluentMigrator.Runner.Initialization;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Processors.DotConnectOracle
{
    /// <summary>
    /// The DotConnect Oracle migration processor.
    /// </summary>
    public class DotConnectOracleProcessor : GenericProcessorBase
    {
        /// <inheritdoc />
        public override string DatabaseType => "DotConnectOracle";

        /// <inheritdoc />
        public override IList<string> DatabaseTypeAliases { get; } = new List<string>();

        /// <inheritdoc />
        public DotConnectOracleProcessor(
            [NotNull] DotConnectOracleDbFactory factory,
            [NotNull] IOracleGenerator generator,
            [NotNull] ILogger<DotConnectOracleProcessor> logger,
            [NotNull] IOptionsSnapshot<ProcessorOptions> options,
            [NotNull] IConnectionStringAccessor connectionStringAccessor)
            : base(() => factory.Factory, generator, logger, options.Value, connectionStringAccessor)
        {
        }

        /// <inheritdoc />
        public override bool SchemaExists(string schemaName)
        {
            if (schemaName == null)
                throw new ArgumentNullException(nameof(schemaName));

            if (schemaName.Length == 0)
                return false;

            return Exists("SELECT 1 FROM ALL_USERS WHERE USERNAME = '{0}'", schemaName.ToUpper());
        }

        /// <inheritdoc />
        public override bool TableExists(string schemaName, string tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            if (tableName.Length == 0)
                return false;

            if (string.IsNullOrEmpty(schemaName))
                return Exists("SELECT 1 FROM USER_TABLES WHERE TABLE_NAME = '{0}'", tableName.ToUpper());

            return Exists("SELECT 1 FROM ALL_TABLES WHERE OWNER = '{0}' AND TABLE_NAME = '{1}'", schemaName.ToUpper(), tableName.ToUpper());
        }

        /// <inheritdoc />
        public override bool ColumnExists(string schemaName, string tableName, string columnName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (columnName == null)
                throw new ArgumentNullException(nameof(columnName));

            if (columnName.Length == 0 || tableName.Length == 0)
                return false;

            if (string.IsNullOrEmpty(schemaName))
                return Exists("SELECT 1 FROM USER_TAB_COLUMNS WHERE TABLE_NAME = '{0}' AND COLUMN_NAME = '{1}'", tableName.ToUpper(), columnName.ToUpper());

            return Exists("SELECT 1 FROM ALL_TAB_COLUMNS WHERE OWNER = '{0}' AND TABLE_NAME = '{1}' AND COLUMN_NAME = '{2}'", schemaName.ToUpper(), tableName.ToUpper(), columnName.ToUpper());
        }

        /// <inheritdoc />
        public override bool ConstraintExists(string schemaName, string tableName, string constraintName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (constraintName == null)
                throw new ArgumentNullException(nameof(constraintName));

            //In Oracle DB constraint name is unique within the schema, so the table name is not used in the query

            if (constraintName.Length == 0)
                return false;

            if (string.IsNullOrEmpty(schemaName))
                return Exists("SELECT 1 FROM USER_CONSTRAINTS WHERE CONSTRAINT_NAME = '{0}'", constraintName.ToUpper());

            return Exists("SELECT 1 FROM ALL_CONSTRAINTS WHERE OWNER = '{0}' AND CONSTRAINT_NAME = '{1}'", schemaName.ToUpper(), constraintName.ToUpper());
        }

        /// <inheritdoc />
        public override bool IndexExists(string schemaName, string tableName, string indexName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (indexName == null)
                throw new ArgumentNullException(nameof(indexName));

            //In Oracle DB index name is unique within the schema, so the table name is not used in the query

            if (indexName.Length == 0)
                return false;

            if (string.IsNullOrEmpty(schemaName))
                return Exists("SELECT 1 FROM USER_INDEXES WHERE INDEX_NAME = '{0}'", indexName.ToUpper());

            return Exists("SELECT 1 FROM ALL_INDEXES WHERE OWNER = '{0}' AND INDEX_NAME = '{1}'", schemaName.ToUpper(), indexName.ToUpper());
        }

        /// <inheritdoc />
        public override bool SequenceExists(string schemaName, string sequenceName)
        {
            return false;
        }

        /// <inheritdoc />
        public override bool DefaultValueExists(string schemaName, string tableName, string columnName, object defaultValue)
        {
            return false;
        }

        /// <inheritdoc />
        public override void Execute(string template, params object[] args)
        {
            Process(string.Format(template, args));
        }

        /// <inheritdoc />
        public override bool Exists(string template, params object[] args)
        {
            if (template == null)
                throw new ArgumentNullException(nameof(template));

            EnsureConnectionIsOpen();

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
                throw new ArgumentNullException(nameof(tableName));

            if (string.IsNullOrEmpty(schemaName))
                return Read("SELECT * FROM {0}", tableName.ToUpper());

            return Read("SELECT * FROM {0}.{1}", schemaName.ToUpper(), tableName.ToUpper());
        }

        /// <inheritdoc />
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

            expression.Operation?.Invoke(Connection, Transaction);
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
    }
}
