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

using System.Data;

namespace FluentMigrator.Runner.Processors
{
    public abstract class GenericProcessorBase : ProcessorBase
    {
        private readonly string connectionString;

        protected GenericProcessorBase(IDbConnection connection, IDbFactory factory
                                       , IMigrationGenerator generator, IAnnouncer announcer, IMigrationProcessorOptions options)
            : base(generator, announcer, options)
        {
            Connection = connection;

            // Prefetch connectionstring as after opening the security info could no longer be present
            // for instance on sql server
            connectionString = connection.ConnectionString;
            
            Factory = factory;
        }

        public override string ConnectionString { get { return connectionString; } }

        public IDbConnection Connection { get; protected set; }
        public IDbFactory Factory { get; protected set; }
        public IDbTransaction Transaction { get; protected set; }

        public virtual bool SupportsTransactions
        {
            get { return false; }
        }

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

        public override void BeginTransaction()
        {
            if (!SupportsTransactions || Transaction != null) return;

            EnsureConnectionIsOpen();

            Announcer.Say("Beginning Transaction");
            Transaction = Connection.BeginTransaction();
        }

        public override void RollbackTransaction()
        {
            if (Transaction == null) return;

            Announcer.Say("Rolling back transaction");
            Transaction.Rollback();
            Transaction.Dispose();
            WasCommitted = true;
            Transaction = null;
        }

        public override void CommitTransaction()
        {
            if (Transaction == null) return;

            Announcer.Say("Committing Transaction");
            Transaction.Commit();
            Transaction.Dispose();
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