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

namespace FluentMigrator.Runner.Processors.Firebird
{
    /// <summary>
    /// Represents a database factory for Firebird, leveraging reflection-based mechanisms 
    /// to create and manage Firebird database provider factories.
    /// </summary>
    /// <remarks>
    /// This class is specifically designed to work with Firebird SQL databases and integrates 
    /// with the FluentMigrator framework. It uses reflection to locate and instantiate the 
    /// Firebird database provider factory.
    /// </remarks>
    public class FirebirdDbFactory : ReflectionBasedDbFactory
    {
        private static readonly TestEntry[] _entries =
        {
            new TestEntry("FirebirdSql.Data.FirebirdClient", "FirebirdSql.Data.FirebirdClient.FirebirdClientFactory", () => Type.GetType("FirebirdSql.Data.FirebirdClient.FirebirdClientFactory, FirebirdSql.Data.FirebirdClient")),
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="FirebirdDbFactory"/> class with default settings.
        /// </summary>
        /// <remarks>
        /// This constructor is marked as <see cref="ObsoleteAttribute"/> and should be avoided in favor of the 
        /// parameterized constructor that accepts an <see cref="IServiceProvider"/>. It initializes the 
        /// Firebird database provider factory using reflection-based mechanisms.
        /// </remarks>
        /// <seealso cref="FirebirdDbFactory(IServiceProvider)"/>
        [Obsolete]
        public FirebirdDbFactory()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FirebirdDbFactory"/> class.
        /// </summary>
        /// <param name="serviceProvider">
        /// The <see cref="IServiceProvider"/> instance used to resolve dependencies 
        /// for the Firebird database provider factory.
        /// </param>
        /// <remarks>
        /// This constructor leverages the base <see cref="ReflectionBasedDbFactory"/> 
        /// to initialize the Firebird database provider factory using predefined test entries.
        /// </remarks>
        public FirebirdDbFactory(IServiceProvider serviceProvider)
            : base(serviceProvider, _entries)
        {
        }
    }
}
