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

namespace FluentMigrator.Runner.Processors.DotConnectOracle
{
    /// <summary>
    /// Represents a database factory for DotConnect Oracle, utilizing reflection-based mechanisms
    /// to create and manage database provider factories.
    /// </summary>
    /// <remarks>
    /// This class is specifically designed to work with the DotConnect Oracle provider
    /// and is used internally by FluentMigrator to facilitate database migrations.
    /// </remarks>
    public class DotConnectOracleDbFactory : ReflectionBasedDbFactory
    {
        private static readonly TestEntry[] _entries =
        {
            new TestEntry("DevArt.Data.Oracle", "Devart.Data.Oracle.OracleProviderFactory"),
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="DotConnectOracleDbFactory"/> class.
        /// </summary>
        /// <param name="serviceProvider">
        /// The service provider used to resolve dependencies required by the factory.
        /// </param>
        /// <remarks>
        /// This constructor leverages the base <see cref="ReflectionBasedDbFactory"/> functionality
        /// to initialize the database factory with predefined test entries specific to the DotConnect Oracle provider.
        /// </remarks>
        public DotConnectOracleDbFactory(IServiceProvider serviceProvider)
            : base(serviceProvider, _entries)
        {
        }
    }
}
