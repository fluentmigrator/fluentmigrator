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
using System.Data;

namespace FluentMigrator.Runner.Processors
{
    /// <summary>
    /// Represents a factory for creating database-related objects such as connections and commands.
    /// </summary>
    /// <remarks>
    /// This interface is marked as obsolete and is intended for legacy support. Implementations of this interface
    /// provide methods to create database connections and commands, which are essential for database migration processing.
    /// </remarks>
    [Obsolete]
    public interface IDbFactory
    {
        /// <summary>
        /// Creates a new database connection using the specified connection string.
        /// </summary>
        /// <param name="connectionString">The connection string used to establish the database connection.</param>
        /// <returns>An <see cref="IDbConnection"/> instance representing the database connection.</returns>
        /// <remarks>
        /// This method is marked as obsolete and is intended for legacy support. 
        /// It relies on the underlying database provider factory to create the connection.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="connectionString"/> is <c>null</c> or empty.
        /// </exception>
        [Obsolete]
        IDbConnection CreateConnection(string connectionString);

        /// <summary>
        /// Creates a new <see cref="IDbCommand"/> instance with the specified parameters.
        /// </summary>
        /// <param name="commandText">The SQL command text to be executed.</param>
        /// <param name="connection">The <see cref="IDbConnection"/> to associate with the command.</param>
        /// <param name="transaction">The <see cref="IDbTransaction"/> to associate with the command, or <c>null</c> if no transaction is used.</param>
        /// <param name="options">The <see cref="IMigrationProcessorOptions"/> that specify additional options for the command.</param>
        /// <returns>A new <see cref="IDbCommand"/> instance configured with the provided parameters.</returns>
        /// <remarks>
        /// This method is marked as obsolete and is intended for legacy support. It is used to create database commands
        /// that are essential for executing SQL statements during database migrations.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="connection"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the <paramref name="connection"/> is not in an open state.
        /// </exception>
        [Obsolete]
        IDbCommand CreateCommand(string commandText, IDbConnection connection, IDbTransaction transaction, IMigrationProcessorOptions options);
    }
}
