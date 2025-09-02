#region License

// Copyright (c) 2007-2024, Fluent Migrator Project
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
    /// <summary>
    /// Base class for generic database processors in FluentMigrator.
    /// </summary>
    public abstract class GenericProcessorBase : ProcessorBase
    {
        [NotNull, ItemCanBeNull]
        private readonly Lazy<DbProviderFactory> _dbProviderFactory;

        [NotNull, ItemCanBeNull]
        private readonly Lazy<IDbConnection> _lazyConnection;

        [CanBeNull]
        private IDbConnection _connection;

        private bool _disposed = false;

        /// <inheritdoc />
        [Obsolete]
        protected GenericProcessorBase(
            IDbConnection connection,
            IDbFactory factory,
            IMigrationGenerator generator,
            IAnnouncer announcer,
            [NotNull] IMigrationProcessorOptions options)
            : base(generator, announcer, options)
        {
            _dbProviderFactory = new Lazy<DbProviderFactory>(() => (factory as DbFactoryBase)?.Factory);

            // Set the connection string, because it cannot be set by
            // the base class (due to the missing information)
            Options.ConnectionString = connection?.ConnectionString;

            Factory = factory;

            _lazyConnection = new Lazy<IDbConnection>(() => connection);
        }

        /// <inheritdoc />
        protected GenericProcessorBase(
            [CanBeNull] Func<DbProviderFactory> factoryAccessor,
            [NotNull] IMigrationGenerator generator,
            [NotNull] ILogger logger,
            [NotNull] ProcessorOptions options,
            [NotNull] IConnectionStringAccessor connectionStringAccessor)
            : base(generator, logger, options)
        {
            _dbProviderFactory = new Lazy<DbProviderFactory>(() => factoryAccessor?.Invoke());

            var connectionString = connectionStringAccessor.ConnectionString;

#pragma warning disable 612
            var legacyFactory = new DbFactoryWrapper(this);

            Factory = legacyFactory;
#pragma warning restore 612

            _lazyConnection = new Lazy<IDbConnection>(
                () =>
                {
                    if (DbProviderFactory == null)
                        return null;
                    var connection = DbProviderFactory.CreateConnection();
                    Debug.Assert(connection != null, nameof(Connection) + " != null");
                    connection.ConnectionString = connectionString;
                    connection.Open();
                    return connection;
                });
        }

        /// <summary>
        /// Gets the database connection.
        /// </summary>
        public IDbConnection Connection
        {
            get => _connection ?? _lazyConnection.Value;
            protected set => _connection = value;
        }

        /// <summary>
        /// Gets the database factory.
        /// </summary>
        [Obsolete]
        [NotNull]
        public IDbFactory Factory { get; protected set; }

        /// <summary>
        /// Gets the current database transaction.
        /// </summary>
        [CanBeNull]
        public IDbTransaction Transaction { get; protected set; }

        /// <summary>
        /// Gets the database provider factory.
        /// </summary>
        [CanBeNull]
        protected DbProviderFactory DbProviderFactory => _dbProviderFactory.Value;

        /// <summary>
        /// Ensures the database connection is open.
        /// </summary>
        protected virtual void EnsureConnectionIsOpen()
        {
            if (Connection != null && Connection.State != ConnectionState.Open)
            {
                Connection.Open();
            }
        }

        /// <summary>
        /// Ensures the database connection is closed.
        /// </summary>
        protected virtual void EnsureConnectionIsClosed()
        {
            if ((_connection != null || (_lazyConnection.IsValueCreated && Connection != null)) && Connection.State != ConnectionState.Closed)
            {
                Connection.Close();
            }
        }

        /// <inheritdoc />
        public override void BeginTransaction()
        {
            if (Transaction != null) return;

            EnsureConnectionIsOpen();

            Logger.LogSay("Beginning Transaction");

            Transaction = Connection?.BeginTransaction();
        }

        /// <inheritdoc />
        public override void RollbackTransaction()
        {
            if (Transaction == null) return;

            Logger.LogSay("Rolling back transaction");
            Transaction.Rollback();
            Transaction.Dispose();
            WasCommitted = true;
            Transaction = null;
        }

        /// <inheritdoc />
        public override void CommitTransaction()
        {
            if (Transaction == null) return;

            Logger.LogSay("Committing Transaction");
            Transaction.Commit();
            Transaction.Dispose();
            WasCommitted = true;
            Transaction = null;
        }

        /// <inheritdoc />
        protected override void Dispose(bool isDisposing)
        {
            if (!isDisposing || _disposed)
                return;

            _disposed = true;

            RollbackTransaction();
            EnsureConnectionIsClosed();
            if ((_connection != null || (_lazyConnection.IsValueCreated && Connection != null)))
            {
                Connection.Dispose();
            }
        }

        /// <summary>
        /// Creates a database command for the specified command text.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <returns>The database command.</returns>
        protected virtual IDbCommand CreateCommand(string commandText)
        {
            return CreateCommand(commandText, Connection, Transaction);
        }

        /// <summary>
        /// Creates a database command for the specified command text, connection, and transaction.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="connection">The database connection.</param>
        /// <param name="transaction">The database transaction.</param>
        /// <returns>The database command.</returns>
        protected virtual IDbCommand CreateCommand(string commandText, IDbConnection connection, IDbTransaction transaction)
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

            if (Options.Timeout != null)
            {
                result.CommandTimeout = (int) Options.Timeout.Value.TotalSeconds;
            }

            return result;
        }

        /// <summary>
        /// Wrapper for legacy database factory.
        /// </summary>
        [Obsolete]
        private class DbFactoryWrapper : IDbFactory
        {
            private readonly GenericProcessorBase _processor;

            /// <inheritdoc />
            public DbFactoryWrapper(GenericProcessorBase processor)
            {
                _processor = processor;
            }

            /// <inheritdoc />
            public IDbConnection CreateConnection(string connectionString)
            {
                Debug.Assert(_processor.DbProviderFactory != null, "_processor.DbProviderFactory != null");
                var result = _processor.DbProviderFactory.CreateConnection();
                Debug.Assert(result != null, nameof(result) + " != null");
                result.ConnectionString = connectionString;
                return result;
            }

            /// <inheritdoc />
            [Obsolete]
            public IDbCommand CreateCommand(
                string commandText,
                IDbConnection connection,
                IDbTransaction transaction,
                IMigrationProcessorOptions options)
            {
                return _processor.CreateCommand(commandText);
            }
        }
    }
}
