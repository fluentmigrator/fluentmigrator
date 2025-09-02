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

namespace FluentMigrator.Runner.Processors
{
#pragma warning disable 612
    /// <summary>
    /// Provides a base implementation for database factory classes, enabling the creation
    /// of database connections and commands. This abstract class serves as a foundation
    /// for specific database factory implementations.
    /// </summary>
    /// <remarks>
    /// This class implements the <see cref="IDbFactory"/> interface and provides
    /// mechanisms to create database connections and commands. It also offers
    /// support for lazy initialization of the underlying <see cref="DbProviderFactory"/>.
    /// </remarks>
    /// <seealso cref="IDbFactory"/>
    /// <seealso cref="DbProviderFactory"/>
    public abstract class DbFactoryBase : IDbFactory
#pragma warning restore 612
    {
        private readonly object _lock = new object();
        private volatile DbProviderFactory _factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbFactoryBase"/> class with the specified
        /// <see cref="DbProviderFactory"/>.
        /// </summary>
        /// <param name="factory">
        /// The <see cref="DbProviderFactory"/> used to create database connections and commands.
        /// </param>
        /// <remarks>
        /// This constructor allows derived classes to specify the <see cref="DbProviderFactory"/> 
        /// to be used for creating database-related objects. The provided factory is stored 
        /// internally and used by the <see cref="Factory"/> property.
        /// </remarks>
        protected DbFactoryBase(DbProviderFactory factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbFactoryBase"/> class.
        /// </summary>
        /// <remarks>
        /// This parameterless constructor is intended for use by derived classes that require
        /// custom initialization logic or deferred setup of the <see cref="DbProviderFactory"/>.
        /// </remarks>
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

        /// <summary>
        /// Creates an instance of the <see cref="DbProviderFactory"/> specific to the database implementation.
        /// </summary>
        /// <remarks>
        /// This method must be implemented by derived classes to provide the appropriate
        /// <see cref="DbProviderFactory"/> for the database being targeted. The factory is used
        /// to create database connections and commands.
        /// </remarks>
        /// <returns>
        /// An instance of <see cref="DbProviderFactory"/> that corresponds to the specific database implementation.
        /// </returns>
        /// <exception cref="AggregateException">
        /// Thrown if the factory cannot be created, typically due to missing or invalid dependencies.
        /// </exception>
        protected abstract DbProviderFactory CreateFactory();

        /// <summary>
        /// Creates a new database connection using the specified connection string.
        /// </summary>
        /// <param name="connectionString">The connection string used to establish the database connection.</param>
        /// <returns>An <see cref="IDbConnection"/> instance representing the database connection.</returns>
        /// <remarks>
        /// This method utilizes the <see cref="Factory"/> property to create the connection.
        /// Ensure that the <see cref="Factory"/> is properly initialized before calling this method.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the <see cref="Factory"/> fails to create a connection.
        /// </exception>
        /// <seealso cref="IDbFactory.CreateConnection(string)"/>
        [Obsolete]
        public IDbConnection CreateConnection(string connectionString)
        {
            var connection = Factory.CreateConnection();
            Debug.Assert(connection != null, nameof(connection) + " != null");
            connection!.ConnectionString = connectionString;
            return connection;
        }

        /// <summary>
        /// Creates a new database command using the specified command text, connection, transaction, and options.
        /// </summary>
        /// <param name="commandText">The SQL command text to be executed.</param>
        /// <param name="connection">The database connection to associate with the command.</param>
        /// <param name="transaction">The database transaction to associate with the command, if any.</param>
        /// <param name="options">The migration processor options, including timeout settings.</param>
        /// <returns>An <see cref="IDbCommand"/> instance representing the database command.</returns>
        /// <remarks>
        /// This method initializes a new command using the provided connection and sets its properties
        /// such as <see cref="IDbCommand.CommandText"/>, <see cref="IDbCommand.CommandTimeout"/>, and
        /// <see cref="IDbCommand.Transaction"/> based on the given parameters.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="connection"/> is <c>null</c>.
        /// </exception>
        /// <seealso cref="IDbConnection"/>
        /// <seealso cref="IDbTransaction"/>
        /// <seealso cref="IMigrationProcessorOptions"/>
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
