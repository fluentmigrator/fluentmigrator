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

namespace FluentMigrator.Runner.Processors
{
#pragma warning disable 612
    public abstract class DbFactoryBase : IDbFactory
#pragma warning restore 612
    {
        private readonly object _lock = new object();
        private volatile DbProviderFactory _factory;

        protected DbFactoryBase(DbProviderFactory factory)
        {
            _factory = factory;
        }

        protected DbFactoryBase()
        {
        }

        /// <summary>
        /// Gets the DB provider factory
        /// </summary>
        public virtual DbProviderFactory Factory
        {
            get
            {
                if (_factory == null)
                {
                    lock (_lock)
                    {
                        if (_factory == null)
                        {
                            _factory = CreateFactory();
                        }
                    }
                }
                return _factory;
            }
        }

        protected abstract DbProviderFactory CreateFactory();

        [Obsolete]
        public IDbConnection CreateConnection(string connectionString)
        {
            var connection = Factory.CreateConnection();
            Debug.Assert(connection != null, nameof(connection) + " != null");
            connection.ConnectionString = connectionString;
            return connection;
        }

        [Obsolete]
        public virtual IDbCommand CreateCommand(string commandText, IDbConnection connection, IDbTransaction transaction, IMigrationProcessorOptions options)
        {
            var command = connection.CreateCommand();
            command.CommandText = commandText;
            if (options?.Timeout != null) command.CommandTimeout = options.Timeout.Value;
            if (transaction != null) command.Transaction = transaction;
            return command;
        }
    }
}
