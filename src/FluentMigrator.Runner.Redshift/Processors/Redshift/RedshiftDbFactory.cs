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

namespace FluentMigrator.Runner.Processors.Redshift
{
    /// <summary>
    /// Provides a database factory implementation for Amazon Redshift, enabling the creation of database connections
    /// and commands specific to Redshift using the Npgsql provider.
    /// </summary>
    /// <remarks>
    /// This class extends <see cref="FluentMigrator.Runner.Processors.ReflectionBasedDbFactory"/> to leverage reflection-based
    /// mechanisms for dynamically loading and creating database provider factories.
    /// </remarks>
    public class RedshiftDbFactory : ReflectionBasedDbFactory
    {
        private static readonly TestEntry[] _entries =
        {
            new TestEntry("Npgsql", "Npgsql.NpgsqlFactory", () => Type.GetType("Npgsql.NpgsqlFactory, Npgsql")),
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentMigrator.Runner.Processors.Redshift.RedshiftDbFactory"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor is marked as <see cref="System.ObsoleteAttribute"/> and is intended for backward compatibility.
        /// It initializes the factory with a default service provider.
        /// </remarks>
        /// <seealso cref="FluentMigrator.Runner.Processors.Redshift.RedshiftDbFactory(IServiceProvider)"/>
        [Obsolete]
        public RedshiftDbFactory()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentMigrator.Runner.Processors.Redshift.RedshiftDbFactory"/> class
        /// with the specified service provider.
        /// </summary>
        /// <param name="serviceProvider">
        /// The <see cref="IServiceProvider"/> used to resolve dependencies required by the factory.
        /// If <c>null</c>, a default service provider will be used.
        /// </param>
        /// <remarks>
        /// This constructor allows for dependency injection of a service provider, enabling more flexible
        /// and testable configurations for Redshift database connections.
        /// </remarks>
        public RedshiftDbFactory(IServiceProvider serviceProvider)
            : base(serviceProvider, _entries)
        {
        }
    }
}
