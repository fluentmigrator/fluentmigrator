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
using System.Data.Common;

namespace FluentMigrator.Runner.Processors
{
    public abstract class DbFactoryBase : IDbFactory
    {
        private readonly object @lock = new object();
        private volatile DbProviderFactory factory;

        protected DbFactoryBase(DbProviderFactory factory)
        {
            this.factory = factory;
        }

        protected DbFactoryBase()
        {
        }

        private DbProviderFactory Factory
        {
            get
            {
                if (factory == null)
                {
                    lock (@lock)
                    {
                        if (factory == null)
                        {
                            factory = CreateFactory();
                        }
                    }
                }
                return factory;
            }
        }

        protected abstract DbProviderFactory CreateFactory();

        #region IDbFactory Members

        public IDbConnection CreateConnection(string connectionString)
        {
            var connection = Factory.CreateConnection();
            connection.ConnectionString = connectionString;
            return connection;
        }

        public IDbCommand CreateCommand(string commandText, IDbConnection connection, IDbTransaction transaction)
        {
            var command = connection.CreateCommand();
            command.CommandText = commandText;
            if (transaction != null) command.Transaction = transaction;
            return command;
        }

        public IDbDataAdapter CreateDataAdapter(IDbCommand command)
        {
            IDbDataAdapter dataAdapter = Factory.CreateDataAdapter();
            dataAdapter.SelectCommand = command;
            return dataAdapter;
        }

        public IDbCommand CreateCommand(string commandText, IDbConnection connection)
        {
            return CreateCommand(commandText, connection, null);
        }

        #endregion
    }
}