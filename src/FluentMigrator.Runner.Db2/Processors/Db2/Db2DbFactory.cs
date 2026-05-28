#region License
//
// Copyright (c) 2007-2024, Fluent Migrator Project
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

namespace FluentMigrator.Runner.Processors.DB2
{
    /// <summary>
    /// Represents a database factory for DB2, providing functionality to create and manage
    /// DB2 database connections using reflection-based mechanisms.
    /// </summary>
    /// <remarks>
    /// This class extends <see cref="FluentMigrator.Runner.Processors.ReflectionBasedDbFactory"/> and
    /// utilizes predefined test entries to locate and initialize the appropriate DB2 database factory.
    /// </remarks>
    public class Db2DbFactory : ReflectionBasedDbFactory
    {
        private static readonly TestEntry[] _testEntries =
        {
            // We no longer ship assemblies for DB2 on .NET Framework, as we are moving away from saving third party assemblies in the repository,
            // but we will attempt to provide loading support.
#if NETFRAMEWORK
            new TestEntry("Net.IBM.Data.Db2", "Net.IBM.Data.Db2.DB2Factory", () => Type.GetType("Net.IBM.Data.Db2.DB2Factory, Net.IBM.Data.Db2")),
#else
            new TestEntry("Net.IBM.Data.DB2.Core", "Net.IBM.Data.DB2.Core.DB2Factory", () => Type.GetType("Net.IBM.Data.DB2.Core.DB2Factory, Net.IBM.Data.DB2.Core")),
            new TestEntry("Net.IBM.Data.DB2", "Net.IBM.Data.DB2.DB2Factory", () => Type.GetType("Net.IBM.Data.DB2.DB2Factory, Net.IBM.Data.DB2")),
#endif
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentMigrator.Runner.Processors.DB2.Db2DbFactory"/> class.
        /// </summary>
        /// <param name="serviceProvider">
        /// The service provider used to resolve dependencies required by the factory.
        /// </param>
        /// <remarks>
        /// This constructor passes the predefined DB2 test entries to the base class
        /// <see cref="FluentMigrator.Runner.Processors.ReflectionBasedDbFactory"/> for initialization.
        /// </remarks>
        public Db2DbFactory(IServiceProvider serviceProvider)
            : base(serviceProvider, _testEntries)
        {
        }
    }
}
