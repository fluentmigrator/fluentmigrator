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

using System.Data;
using System.Data.Common;

using JetBrains.Annotations;

namespace FluentMigrator.Runner.Initialization
{
    /// <summary>
    /// Creates the <see cref="IDbConnection"/> used by a migration processor.
    /// </summary>
    /// <remarks>
    /// Implementations are resolved from the dependency injection container with a scoped
    /// lifetime, which means <see cref="CreateConnection"/> is called at most once per
    /// migration processor (that is, per connection) and never once per command. This makes
    /// the abstraction suitable for rotating credentials such as Azure Entra ID access tokens,
    /// AWS RDS IAM authentication tokens or secrets fetched from a secret store.
    /// </remarks>
    [PublicAPI]
    public interface IMigrationConnectionFactory
    {
        /// <summary>
        /// Gets a value indicating whether this factory can create a database connection.
        /// </summary>
        bool HasConnection { get; }

        /// <summary>
        /// Gets a value indicating whether the migration processor owns the connections
        /// returned by <see cref="CreateConnection"/>.
        /// </summary>
        /// <remarks>
        /// When <c>true</c> (the default for the built-in implementations) the processor closes
        /// and disposes the connection when the processor itself is disposed. Return <c>false</c>
        /// when the connection is owned by the caller, for example when an application hands an
        /// externally managed, possibly already open, connection to the migration runner.
        /// </remarks>
        bool OwnsConnection { get; }

        /// <summary>
        /// Creates a database connection.
        /// </summary>
        /// <param name="providerFactory">
        /// The provider factory configured for the selected processor.
        /// </param>
        /// <returns>
        /// A database connection. The caller owns the returned connection.
        /// </returns>
        [NotNull]
        IDbConnection CreateConnection([NotNull] DbProviderFactory providerFactory);
    }
}
