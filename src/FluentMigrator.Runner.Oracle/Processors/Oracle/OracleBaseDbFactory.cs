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

using System;

namespace FluentMigrator.Runner.Processors.Oracle
{
    /// <summary>
    /// Represents the base database factory for Oracle database processors.
    /// </summary>
    /// <remarks>
    /// This class provides foundational functionality for creating Oracle database factories
    /// and serves as a base class for specific Oracle database factory implementations, such as
    /// <see cref="OracleDbFactory"/> and <see cref="OracleManagedDbFactory"/>.
    /// </remarks>
    public class OracleBaseDbFactory : ReflectionBasedDbFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OracleBaseDbFactory"/> class.
        /// </summary>
        /// <param name="serviceProvider">
        /// The <see cref="IServiceProvider"/> used to resolve dependencies required by the factory.
        /// </param>
        /// <param name="testEntries">
        /// An array of <see cref="ReflectionBasedDbFactory.TestEntry"/> objects that represent the
        /// test entries used for creating database provider factories via reflection.
        /// </param>
        /// <remarks>
        /// This constructor is primarily used by derived classes, such as <see cref="OracleDbFactory"/>
        /// and <see cref="OracleManagedDbFactory"/>, to initialize the base functionality for Oracle
        /// database factories.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when the <paramref name="testEntries"/> array is empty.
        /// </exception>
        protected OracleBaseDbFactory(IServiceProvider serviceProvider, params TestEntry[] testEntries)
            : base(serviceProvider, testEntries)
        {
        }
    }
}
