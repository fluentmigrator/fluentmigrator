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

namespace FluentMigrator.Runner.Processors.Snowflake
{
    /// <summary>
    /// Represents a database factory specifically designed for Snowflake database integration.
    /// </summary>
    /// <remarks>
    /// This class extends <see cref="FluentMigrator.Runner.Processors.ReflectionBasedDbFactory"/> to provide
    /// Snowflake-specific database factory functionality. It is typically used in conjunction with the
    /// FluentMigrator framework to facilitate database migrations targeting Snowflake.
    /// </remarks>
    public class SnowflakeDbFactory : ReflectionBasedDbFactory
    {
        private static readonly TestEntry[] _entries =
        {
            new TestEntry("Snowflake.Data", "Snowflake.Data.Client.SnowflakeDbFactory"),
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentMigrator.Runner.Processors.Snowflake.SnowflakeDbFactory"/> class.
        /// </summary>
        /// <param name="serviceProvider">
        /// The <see cref="IServiceProvider"/> used to resolve dependencies required by the database factory.
        /// </param>
        /// <remarks>
        /// This constructor passes Snowflake-specific configuration entries to the base class
        /// <see cref="FluentMigrator.Runner.Processors.ReflectionBasedDbFactory"/> to enable integration
        /// with the Snowflake database.
        /// </remarks>
        public SnowflakeDbFactory(IServiceProvider serviceProvider)
            : base(serviceProvider, _entries)
        {
        }
    }
}
