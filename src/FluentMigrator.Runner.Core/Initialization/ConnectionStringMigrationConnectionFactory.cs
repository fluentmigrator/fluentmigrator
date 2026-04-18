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
using System.Data.Common;

using JetBrains.Annotations;

namespace FluentMigrator.Runner.Initialization
{
    public sealed class ConnectionStringMigrationConnectionFactory : IMigrationConnectionFactory
    {
        private readonly IConnectionStringAccessor _connectionStringAccessor;

        public ConnectionStringMigrationConnectionFactory(
            [NotNull] IConnectionStringAccessor connectionStringAccessor)
        {
            _connectionStringAccessor = connectionStringAccessor
                ?? throw new ArgumentNullException(nameof(connectionStringAccessor));
        }

        public bool HasConnection =>
            !string.IsNullOrWhiteSpace(_connectionStringAccessor.ConnectionString);

        public IDbConnection CreateConnection(DbProviderFactory providerFactory)
        {
            if (providerFactory == null)
            {
                throw new ArgumentNullException(nameof(providerFactory));
            }

            var connectionString = _connectionStringAccessor.ConnectionString;

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("No connection string was configured.");
            }

            var connection = providerFactory.CreateConnection();

            if (connection == null)
            {
                throw new InvalidOperationException(
                    $"The provider factory '{providerFactory.GetType().FullName}' returned null from CreateConnection().");
            }

            connection.ConnectionString = connectionString;
            return connection;
        }
    }
}
