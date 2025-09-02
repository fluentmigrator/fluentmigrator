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
    /// Represents a database factory for Oracle database processors.
    /// </summary>
    /// <remarks>
    /// This class extends the <see cref="OracleBaseDbFactory"/> to provide specific functionality
    /// for creating Oracle database factories. It serves as an implementation tailored for Oracle
    /// database processors and supports dependency injection via <see cref="IServiceProvider"/>.
    /// </remarks>
    /// <seealso cref="OracleBaseDbFactory"/>
    /// <seealso cref="ReflectionBasedDbFactory"/>
    /// <seealso cref="IDbFactory"/>
    public class OracleDbFactory : OracleBaseDbFactory
    {
        private static readonly TestEntry[] _testEntries =
        {
            new TestEntry("Oracle.DataAccess", "Oracle.DataAccess.Client.OracleClientFactory"),
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="OracleDbFactory"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor is marked as obsolete and initializes the factory without a service provider.
        /// It is recommended to use the constructor that accepts an <see cref="IServiceProvider"/> for dependency injection.
        /// </remarks>
        /// <seealso cref="OracleDbFactory(IServiceProvider)"/>
        [Obsolete]
        public OracleDbFactory()
            : this(serviceProvider: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OracleDbFactory"/> class with the specified service provider.
        /// </summary>
        /// <param name="serviceProvider">
        /// The <see cref="IServiceProvider"/> used to resolve dependencies required by the factory.
        /// </param>
        /// <remarks>
        /// This constructor allows for dependency injection via the <paramref name="serviceProvider"/> parameter.
        /// It initializes the base functionality for Oracle database factories by passing the provided service
        /// provider and predefined test entries to the base class constructor.
        /// </remarks>
        /// <seealso cref="OracleBaseDbFactory"/>
        /// <seealso cref="ReflectionBasedDbFactory"/>
        /// <seealso cref="IDbFactory"/>
        public OracleDbFactory(IServiceProvider serviceProvider)
            : base(serviceProvider, _testEntries)
        {
        }
    }
}
