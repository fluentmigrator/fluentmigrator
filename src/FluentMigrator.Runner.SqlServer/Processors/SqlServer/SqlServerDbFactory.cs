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
using System.Data.Common;

using Microsoft.Data.SqlClient;

namespace FluentMigrator.Runner.Processors.SqlServer
{
    /// <summary>
    /// Represents a factory for creating SQL Server database-related objects, such as connections and commands.
    /// </summary>
    /// <remarks>
    /// This class is marked as obsolete and is intended for legacy support. It provides an implementation
    /// of the <see cref="DbFactoryBase"/> class, specifically tailored for SQL Server using the 
    /// <see cref="Microsoft.Data.SqlClient.SqlClientFactory"/>.
    /// </remarks>
    /// <seealso cref="DbFactoryBase"/>
    /// <seealso cref="Microsoft.Data.SqlClient.SqlClientFactory"/>
    [Obsolete]
    public class SqlServerDbFactory : DbFactoryBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerDbFactory"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor initializes the base <see cref="DbFactoryBase"/> class with the 
        /// <see cref="Microsoft.Data.SqlClient.SqlClientFactory.Instance"/>. It is used to create 
        /// SQL Server-specific database connections and commands.
        /// </remarks>
        /// <seealso cref="DbFactoryBase"/>
        /// <seealso cref="Microsoft.Data.SqlClient.SqlClientFactory"/>
        public SqlServerDbFactory()
            : base(SqlClientFactory.Instance)
        {
        }

        /// <inheritdoc />
        protected override DbProviderFactory CreateFactory()
        {
            return SqlClientFactory.Instance;
        }
    }
}
