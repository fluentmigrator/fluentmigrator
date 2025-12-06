#region License
// Copyright (c) 2019, Fluent Migrator Project
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

using System.Data.Common;

namespace FluentMigrator.Example.AwsAuroraMigrator
{
    class DelegatingDbProviderFactory : DbProviderFactory
    {
        private readonly DbDataSource _dbDataSource;

        private readonly DbProviderFactory _dbProviderFactory;

        public DelegatingDbProviderFactory(DbDataSource dbDataSource, DbProviderFactory dbProviderFactory)
        {
            _dbDataSource = dbDataSource ?? throw new ArgumentNullException(nameof(dbDataSource));
            _dbProviderFactory = dbProviderFactory ?? throw new ArgumentNullException(nameof(dbProviderFactory));
        }

        public override bool CanCreateDataSourceEnumerator => _dbProviderFactory.CanCreateDataSourceEnumerator;
        public override DbCommand? CreateCommand() => _dbProviderFactory.CreateCommand();
        public override DbCommandBuilder? CreateCommandBuilder() => _dbProviderFactory.CreateCommandBuilder();

        /// <summary>
        /// Delegates connection creation to the underlying <see cref="DbDataSource"/>, which encapsulates all the information needed to connect to a specific database.
        /// </summary>
        /// <returns></returns>
        public override DbConnection CreateConnection() => _dbDataSource.CreateConnection();

        public override DbConnectionStringBuilder? CreateConnectionStringBuilder() => _dbProviderFactory.CreateConnectionStringBuilder();

        public override DbDataAdapter? CreateDataAdapter() => _dbProviderFactory.CreateDataAdapter();

        public override DbDataSourceEnumerator? CreateDataSourceEnumerator() => _dbProviderFactory.CreateDataSourceEnumerator();

        public override DbParameter? CreateParameter() => _dbProviderFactory.CreateParameter();
    }
}
