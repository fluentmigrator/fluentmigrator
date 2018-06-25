#region License

// Copyright (c) 2007-2018, Sean Chambers <schambers80@gmail.com>
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
using System.Data.Common;
using System.Diagnostics;

using FluentMigrator.Runner.Initialization;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;

namespace FluentMigrator.Runner.Processors
{
    public abstract class GenericProcessorBase : ProcessorBase
    {
        [NotNull, ItemNotNull]
        private readonly Lazy<DbProviderFactory> _dbProviderFactory;

        [NotNull, ItemNotNull]
        private readonly Lazy<DbConnection> _lazyConnection;

        private bool _disposed = false;

        protected GenericProcessorBase(
            [NotNull] Func<DbProviderFactory> factoryAccessor,
            [NotNull] IMigrationGenerator generator,
            [NotNull] ILogger logger,
            [NotNull] ProcessorOptions options,
            [NotNull] IConnectionStringAccessor connectionStringAccessor)
            : base(generator, logger, options)
        {
            _dbProviderFactory = new Lazy<DbProviderFactory>(factoryAccessor);

            var connectionString = connectionStringAccessor.ConnectionString;

            _lazyConnection = new Lazy<DbConnection>(
                () =>
                {
                    var connection = DbProviderFactory.CreateConnection();
                    Debug.Assert(connection != null, nameof(Connection) + " != null");
                    connection.ConnectionString = connectionString;
                    connection.Open();
                    return connection;
                });
        }

        [NotNull]
        public DbConnection Connection => _lazyConnection.Value;

        [CanBeNull]
        public DbTransaction Transaction { get; protected set; }

        [NotNull]
        protected DbProviderFactory DbProviderFactory => _dbProviderFactory.Value;

        protected virtual void EnsureConnectionIsOpen()
        {
            if (Connection.State != ConnectionState.Open)
            {
                Connection.Open();
            }
        }

        protected virtual void EnsureConnectionIsClosed()
        {
            if (_lazyConnection.IsValueCreated && Connection.State != ConnectionState.Closed)
            {
                Connection.Close();
            }
        }

        public override void BeginTransaction()
        {
            if (Transaction != null) return;

            EnsureConnectionIsOpen();

            Logger.LogSay("Beginning Transaction");

            Transaction = Connection.BeginTransaction();
        }

        public override void RollbackTransaction()
        {
            if (Transaction == null) return;

            Logger.LogSay("Rolling back transaction");
            Transaction.Rollback();
            Transaction.Dispose();
            WasCommitted = true;
            Transaction = null;
        }

        public override void CommitTransaction()
        {
            if (Transaction == null) return;

            Logger.LogSay("Committing Transaction");
            Transaction.Commit();
            Transaction.Dispose();
            WasCommitted = true;
            Transaction = null;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (!isDisposing || _disposed)
                return;

            _disposed = true;

            RollbackTransaction();
            EnsureConnectionIsClosed();
            if (_lazyConnection.IsValueCreated)
            {
                Connection.Dispose();
            }
        }

        protected virtual DbCommand CreateCommand(string commandText)
        {
            return CreateCommand(commandText, Connection, Transaction);
        }

        protected virtual DbCommand CreateCommand(string commandText, DbConnection connection, DbTransaction transaction)
        {
            var result = DbProviderFactory.CreateCommand();
            Debug.Assert(result != null, nameof(result) + " != null");
            result.Connection = connection;
            if (transaction != null)
                result.Transaction = transaction;
            result.CommandText = commandText;

            if (Options.Timeout != null)
            {
                result.CommandTimeout = (int)Options.Timeout.Value.TotalSeconds;
            }

            return result;
        }
    }
}
