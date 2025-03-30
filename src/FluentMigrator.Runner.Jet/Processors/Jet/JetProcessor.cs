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
using System.Data.OleDb;
using System.Diagnostics;

using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators.Jet;
using FluentMigrator.Runner.Initialization;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Processors.Jet
{
    public class JetProcessor : ProcessorBase
    {
        private readonly Lazy<OleDbConnection> _connection;
        private OleDbTransaction _transaction;
        public OleDbConnection Connection => _connection.Value;
        public OleDbTransaction Transaction => _transaction;
        private bool _disposed = false;

        public JetProcessor(
            [NotNull] JetGenerator generator,
            [NotNull] ILogger<JetProcessor> logger,
            [NotNull] IOptionsSnapshot<ProcessorOptions> options,
            [NotNull] IConnectionStringAccessor connectionStringAccessor)
            : base(generator, logger, options.Value)
        {
            var factory = OleDbFactory.Instance;
            var connectionString = connectionStringAccessor.ConnectionString ?? options.Value.ConnectionString;
            if (factory != null)
            {
                _connection = new Lazy<OleDbConnection>(
                    () =>
                    {
                        var conn = (OleDbConnection) factory.CreateConnection();
                        Debug.Assert(conn != null, nameof(conn) + " != null");
                        conn.ConnectionString = connectionString;
                        return conn;
                    });
            }

#pragma warning disable 612
            ConnectionString = connectionString;
#pragma warning restore 612
        }

        [Obsolete]
        public override string ConnectionString { get; }

        public override string DatabaseType { get; } = ProcessorIdConstants.Jet;

        public override IList<string> DatabaseTypeAliases { get; } = new List<string>();

        protected void EnsureConnectionIsOpen()
        {
            if (Connection.State != ConnectionState.Open)
                Connection.Open();
        }

        protected void EnsureConnectionIsClosed()
        {
            if (Connection.State != ConnectionState.Closed)
                Connection.Close();
        }

        public override void Process(PerformDBOperationExpression expression)
        {
            Logger.LogSay("Performing DB Operation");

            if (Options.PreviewOnly)
                return;

            EnsureConnectionIsOpen();

            expression.Operation?.Invoke(Connection, _transaction);
        }

        protected override void Process(string sql)
        {
            Logger.LogSql(sql);

            if (Options.PreviewOnly || string.IsNullOrEmpty(sql))
                return;

            EnsureConnectionIsOpen();

            using (var command = new OleDbCommand(sql, Connection, Transaction))
            {
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (OleDbException ex)
                {
                    throw new Exception(string.Format("Exception while processing \"{0}\"", sql), ex);
                }
            }
        }

        public override DataSet ReadTableData(string schemaName, string tableName)
        {
            return Read("SELECT * FROM [{0}]", tableName);
        }

        public override DataSet Read(string template, params object[] args)
        {
            EnsureConnectionIsOpen();

            var ds = new DataSet();
            using (var command = new OleDbCommand(string.Format(template, args), Connection, Transaction))
            using (var adapter = new OleDbDataAdapter(command))
            {
                adapter.Fill(ds);
                return ds;
            }
        }

        public override bool Exists(string template, params object[] args)
        {
            EnsureConnectionIsOpen();

            using (var command = new OleDbCommand(string.Format(template, args), Connection, Transaction))
            using (var reader = command.ExecuteReader())
            {
                Debug.Assert(reader != null, nameof(reader) + " != null");
                return reader.Read();
            }
        }

        public override bool SequenceExists(string schemaName, string sequenceName)
        {
            return false;
        }

        public override void Execute(string template, params object[] args)
        {
            Process(string.Format(template, args));
        }

        public override bool SchemaExists(string tableName)
        {
            return true;
        }

        public override bool TableExists(string schemaName, string tableName)
        {
            EnsureConnectionIsOpen();

            var restrict = new object[] { null, null, tableName, "TABLE" };
            using (var tables = Connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, restrict))
            {
                Debug.Assert(tables != null, nameof(tables) + " != null");
                for (int i = 0; i < tables.Rows.Count; i++)
                {
                    var name = tables.Rows[i].ItemArray[2].ToString();
                    if (name == tableName)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public override bool ColumnExists(string schemaName, string tableName, string columnName)
        {
            EnsureConnectionIsOpen();

            var restrict = new object[] { null, null, tableName, null };
            using (var columns = Connection.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, restrict))
            {
                Debug.Assert(columns != null, nameof(columns) + " != null");
                for (int i = 0; i < columns.Rows.Count; i++)
                {
                    var name = columns.Rows[i].ItemArray[3].ToString();
                    if (name == columnName)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public override bool ConstraintExists(string schemaName, string tableName, string constraintName)
        {
            EnsureConnectionIsOpen();

            var restrict = new object[] { null, null, constraintName, null, null, tableName };
            using (var constraints = Connection.GetOleDbSchemaTable(OleDbSchemaGuid.Table_Constraints, restrict))
            {
                Debug.Assert(constraints != null, nameof(constraints) + " != null");
                return constraints.Rows.Count > 0;
            }
        }

        public override bool IndexExists(string schemaName, string tableName, string indexName)
        {
            EnsureConnectionIsOpen();

            var restrict = new object[] { null, null, indexName, null, tableName };
            using (var indexes = Connection.GetOleDbSchemaTable(OleDbSchemaGuid.Indexes, restrict))
            {
                Debug.Assert(indexes != null, nameof(indexes) + " != null");
                return indexes.Rows.Count > 0;
            }
        }

        public override bool DefaultValueExists(string schemaName, string tableName, string columnName, object defaultValue)
        {
            return false;
        }

        public override void BeginTransaction()
        {
            if (_transaction != null) return;

            EnsureConnectionIsOpen();

            Logger.LogSay("Beginning Transaction");
            _transaction = Connection.BeginTransaction();
        }

        public override void RollbackTransaction()
        {
            if (_transaction == null) return;

            Logger.LogSay("Rolling back transaction");
            _transaction.Rollback();
            WasCommitted = true;
            _transaction = null;
        }

        public override void CommitTransaction()
        {
            if (_transaction == null) return;

            Logger.LogSay("Committing Transaction");
            _transaction.Commit();
            WasCommitted = true;
            _transaction = null;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (!isDisposing || _disposed)
                return;

            _disposed = true;

            RollbackTransaction();
            EnsureConnectionIsClosed();
        }
    }
}
