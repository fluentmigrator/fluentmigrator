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
using System.Collections.Generic;
using System.Linq;

namespace FluentMigrator.Runner.Processors.Hana
{
    /// <summary>
    /// Represents a database factory specifically designed for SAP HANA.
    /// </summary>
    /// <remarks>
    /// This class extends <see cref="FluentMigrator.Runner.Processors.ReflectionBasedDbFactory"/> 
    /// to provide functionality tailored to the SAP HANA database.
    /// </remarks>
    public class HanaDbFactory : ReflectionBasedDbFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HanaDbFactory"/> class.
        /// </summary>
        /// <param name="serviceProvider">
        /// The <see cref="IServiceProvider"/> used to resolve dependencies required by the factory.
        /// </param>
        /// <remarks>
        /// This constructor leverages the base implementation provided by 
        /// <see cref="FluentMigrator.Runner.Processors.ReflectionBasedDbFactory"/> and 
        /// supplies test entries specific to SAP HANA.
        /// </remarks>
        public HanaDbFactory(IServiceProvider serviceProvider)
            : base(serviceProvider, GetTestEntries().ToArray())
        {
        }

        /// <summary>
        /// Provides a collection of test entries for SAP HANA database provider factories.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> of <see cref="FluentMigrator.Runner.Processors.ReflectionBasedDbFactory.TestEntry"/> 
        /// objects, each representing a test entry for a SAP HANA database provider factory.
        /// </returns>
        /// <remarks>
        /// This method yields test entries containing the assembly name and the database provider factory type name 
        /// required for dynamically loading and creating SAP HANA database provider factories.
        /// </remarks>
        private static IEnumerable<TestEntry> GetTestEntries()
        {
            yield return new TestEntry("Sap.Data.Hana", "Sap.Data.Hana.HanaFactory");
            yield return new TestEntry("Sap.Data.Hana.v4.5", "Sap.Data.Hana.HanaFactory");
        }
    }
}
