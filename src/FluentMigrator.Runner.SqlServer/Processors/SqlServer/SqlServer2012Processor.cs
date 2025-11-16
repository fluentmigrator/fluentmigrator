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

using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.Runner.Initialization;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Processors.SqlServer
{
    /// <summary>
    /// Represents a migration processor for SQL Server 2012.
    /// </summary>
    /// <remarks>
    /// This processor is specifically designed to handle migrations targeting SQL Server 2012.
    /// It extends the functionality of <see cref="SqlServerProcessor"/> by utilizing the
    /// <see cref="SqlServer2012Generator"/> and <see cref="SqlServer2008Quoter"/> for SQL generation
    /// and quoting, respectively.
    /// </remarks>
    public class SqlServer2012Processor : SqlServerProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServer2012Processor"/> class.
        /// </summary>
        /// <param name="logger">The logger used for logging migration operations.</param>
        /// <param name="quoter">The SQL quoter specific to SQL Server 2008.</param>
        /// <param name="generator">The SQL generator specific to SQL Server 2012.</param>
        /// <param name="options">The processor options for configuring migration behavior.</param>
        /// <param name="connectionStringAccessor">The accessor for retrieving the database connection string.</param>
        /// <param name="serviceProvider">The service provider for resolving dependencies.</param>
        /// <remarks>
        /// This constructor sets up the processor to handle migrations for SQL Server 2012,
        /// utilizing the <see cref="SqlServer2012Generator"/> for SQL generation and
        /// <see cref="SqlServer2008Quoter"/> for quoting.
        /// </remarks>
        public SqlServer2012Processor(
            [NotNull] ILogger<SqlServer2012Processor> logger,
            [NotNull] SqlServer2008Quoter quoter,
            [NotNull] SqlServer2012Generator generator,
            [NotNull] IOptionsSnapshot<ProcessorOptions> options,
            [NotNull] IConnectionStringAccessor connectionStringAccessor,
            [NotNull] IServiceProvider serviceProvider)
            : base(new[] { ProcessorIdConstants.SqlServer2012, ProcessorIdConstants.SqlServer }, generator, quoter, logger, options, connectionStringAccessor, serviceProvider)
        {
        }
    }
}
