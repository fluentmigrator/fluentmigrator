#region License

// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
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

namespace FluentMigrator.Runner.Processors
{
    public abstract class GenericProcessorBase : ProcessorBase
    {
        private Func<string> connectionStringFactory;
        private bool transactionRequested;

        protected GenericProcessorBase(Func<IDbConnection> connectionFactory, Func<IDbFactory> factoryFactory
                                       , IMigrationGenerator generator, IAnnouncer announcer, IMigrationProcessorOptions options)
            : base(generator, announcer, options)
        {
            this.connectionFactory = connectionFactory;
            this._factoryFactory = factoryFactory;
            this.connectionStringFactory = () => Connection.ConnectionString;
        }

        public override string ConnectionString { get { return connectionStringFactory(); } }

        private Func<IDbConnection> connectionFactory;
        private IDbConnection connection;
        public IDbConnection Connection
        {
            get
            {
                if (connection == null) 
                {
                    connection = connectionFactory();
                    // Prefetch connectionstring as after opening the security info could no longer be present
                    // for instance on sql server
                    var connectionString = connection.ConnectionString;
                    connectionStringFactory = () => connectionString;
                }
                return connection;
            }
            protected set
            {
                connection = value;
            }
        }

        private Func<IDbFactory> _factoryFactory;
        private IDbFactory _factory;
        public IDbFactory Factory 
        {
            get
            {
                return _factory = _factory ?? _factoryFactory();
            }
            protected set
            {
                _factory = value;
            }
        }

        public IDbTransaction Transaction { get; protected set; }

        public virtual bool SupportsTransactions
        {
            get { return false; }
        }

        protected void EnsureConnectionIsOpen()
        {
            if (Connection.State != ConnectionState.Open)
                Connection.Open();
            if (transactionRequested) 
            {
                transactionRequested = false;
                BeginTransactionInternal();
            }
        }

        protected void EnsureConnectionIsClosed()
        {
            if (connection == null) return;
            if (Connection.State != ConnectionState.Closed)
                Connection.Close();
        }

        public override void BeginTransaction()
        {
            if (!SupportsTransactions || Transaction != null) return;

            if (connection == null) 
            {
                transactionRequested = true;
                return;
            }

            EnsureConnectionIsOpen();

            BeginTransactionInternal();
        }

        private void BeginTransactionInternal()
        {
            Announcer.Say("Beginning Transaction");
            Transaction = Connection.BeginTransaction();
        }

        public override void RollbackTransaction()
        {
            if (Transaction == null) return;

            Announcer.Say("Rolling back transaction");
            Transaction.Rollback();
            WasCommitted = true;
            Transaction = null;
        }

        public override void CommitTransaction()
        {
            if (Transaction == null) return;

            Announcer.Say("Committing Transaction");
            Transaction.Commit();
            WasCommitted = true;
            Transaction = null;
        }

        protected override void Dispose(bool isDisposing)
        {
            RollbackTransaction();
            EnsureConnectionIsClosed();
        }
    }
}