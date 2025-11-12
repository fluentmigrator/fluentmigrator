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

namespace FluentMigrator.Runner.Processors.DB2.iSeries
{
    /// <summary>
    /// Represents a database factory for IBM DB2 iSeries, leveraging reflection-based mechanisms
    /// to create and manage database provider factories. NOTE: This is only relevant for .NET Framework.
    /// </summary>
    /// <remarks>
    /// This class is specifically designed to support the IBM DB2 iSeries database by utilizing
    /// the <c>IBM.Data.DB2.iSeries</c> provider. It extends the <see cref="FluentMigrator.Runner.Processors.ReflectionBasedDbFactory"/>
    /// to provide functionality for dynamically resolving and creating database provider factories.
    /// </remarks>
    public class Db2ISeriesDbFactory : ReflectionBasedDbFactory
    {
        private static readonly TestEntry[] _testEntries =
        {
            new TestEntry("Net.IBM.Data.DB2.iSeries", "Net.IBM.Data.DB2.iSeries.iDB2Factory"),
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentMigrator.Runner.Processors.DB2.iSeries.Db2ISeriesDbFactory"/> class.
        /// </summary>
        /// <param name="serviceProvider">
        /// The service provider used to resolve dependencies required by the factory.
        /// </param>
        /// <remarks>
        /// This constructor leverages the base <see cref="FluentMigrator.Runner.Processors.ReflectionBasedDbFactory"/> 
        /// class to dynamically resolve and create database provider factories for IBM DB2 iSeries.
        /// </remarks>
        public Db2ISeriesDbFactory(IServiceProvider serviceProvider)
            : base(serviceProvider, _testEntries)
        {
        }
    }
}
