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

namespace FluentMigrator.Runner.Processors.MySql
{
    /// <summary>
    /// Represents a factory for creating MySQL database processors and related components.
    /// </summary>
    /// <remarks>
    /// This class is used internally by FluentMigrator to provide support for MySQL database migrations.
    /// It extends the <see cref="FluentMigrator.Runner.Processors.ReflectionBasedDbFactory"/> to leverage reflection-based
    /// database factory capabilities.
    /// </remarks>
    /// <seealso cref="FluentMigrator.Runner.Processors.ReflectionBasedDbFactory"/>
    public class MySqlDbFactory : ReflectionBasedDbFactory
    {
        private static readonly TestEntry[] _entries =
        {
            new TestEntry("MySql.Data", "MySql.Data.MySqlClient.MySqlClientFactory"),
            new TestEntry("MySqlConnector", "MySql.Data.MySqlClient.MySqlClientFactory"),
            new TestEntry("MySqlConnector", "MySqlConnector.MySqlConnectorFactory"),
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentMigrator.Runner.Processors.MySql.MySqlDbFactory"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor is marked as obsolete and initializes the factory with a default service provider.
        /// </remarks>
        /// <seealso cref="FluentMigrator.Runner.Processors.MySql.MySqlDbFactory(IServiceProvider)"/>
        [Obsolete]
        public MySqlDbFactory()
            : this(serviceProvider: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentMigrator.Runner.Processors.MySql.MySqlDbFactory"/> class
        /// with the specified service provider.
        /// </summary>
        /// <param name="serviceProvider">
        /// The <see cref="IServiceProvider"/> instance used to resolve dependencies for the factory.
        /// </param>
        /// <remarks>
        /// This constructor allows for the customization of the service provider used by the factory.
        /// It leverages the base <see cref="FluentMigrator.Runner.Processors.ReflectionBasedDbFactory"/> constructor
        /// to initialize the factory with the provided service provider and predefined test entries.
        /// </remarks>
        /// <seealso cref="FluentMigrator.Runner.Processors.ReflectionBasedDbFactory"/>
        public MySqlDbFactory(IServiceProvider serviceProvider)
            : base(serviceProvider, _entries)
        {
        }
    }
}
