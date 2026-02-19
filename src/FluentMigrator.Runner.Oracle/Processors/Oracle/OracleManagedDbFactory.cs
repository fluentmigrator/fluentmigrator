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

namespace FluentMigrator.Runner.Processors.Oracle
{
    /// <summary>
    /// Represents a database factory for Oracle Managed database processors.
    /// </summary>
    /// <remarks>
    /// This class extends the <see cref="OracleBaseDbFactory"/> to provide specific functionality
    /// for creating Oracle Managed database factories. It utilizes the managed Oracle database
    /// driver for database operations.
    /// </remarks>
    /// <seealso cref="OracleBaseDbFactory"/>
    /// <seealso cref="ReflectionBasedDbFactory"/>
    public class OracleManagedDbFactory : OracleBaseDbFactory
    {
        private static readonly TestEntry[] _entries =
        {
            new TestEntry("Oracle.ManagedDataAccess", "Oracle.ManagedDataAccess.Client.OracleClientFactory", () => Type.GetType("Oracle.ManagedDataAccess.Client.OracleClientFactory, Oracle.ManagedDataAccess")),
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="OracleManagedDbFactory"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor is marked as obsolete and initializes the factory with a default
        /// <see langword="null"/> service provider. It is primarily used for backward compatibility.
        /// </remarks>
        /// <seealso cref="OracleBaseDbFactory"/>
        /// <seealso cref="ReflectionBasedDbFactory"/>
        [Obsolete]
        public OracleManagedDbFactory()
            : this(serviceProvider: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OracleManagedDbFactory"/> class.
        /// </summary>
        /// <param name="serviceProvider">
        /// The <see cref="IServiceProvider"/> used to resolve dependencies required by the factory.
        /// </param>
        /// <remarks>
        /// This constructor initializes the <see cref="OracleManagedDbFactory"/> with the specified
        /// <paramref name="serviceProvider"/> and predefined test entries for the Oracle Managed
        /// database provider. It leverages the functionality provided by the base class
        /// <see cref="OracleBaseDbFactory"/>.
        /// </remarks>
        public OracleManagedDbFactory(IServiceProvider serviceProvider)
            : base(serviceProvider, _entries)
        {
        }
    }
}
