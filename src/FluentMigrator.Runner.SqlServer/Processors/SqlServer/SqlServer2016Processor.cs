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
using System.Data.Common;

using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.Runner.Initialization;

using JetBrains.Annotations;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Processors.SqlServer
{
    /// <summary>
    /// Represents a migration processor for SQL Server 2016.
    /// </summary>
    /// <remarks>
    /// This processor is responsible for executing database migrations specifically targeting
    /// SQL Server 2016. It extends the functionality of <see cref="SqlServerProcessor"/> and
    /// utilizes the <see cref="SqlServer2016Generator"/> for generating SQL scripts.
    /// </remarks>
    public class SqlServer2016Processor : SqlServerProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServer2016Processor"/> class.
        /// </summary>
        /// <param name="logger">The logger used to log messages and errors.</param>
        /// <param name="quoter">The SQL quoter used for quoting SQL identifiers and literals.</param>
        /// <param name="generator">The SQL generator used to generate SQL scripts for SQL Server 2016.</param>
        /// <param name="options">The processor options that define configuration settings.</param>
        /// <param name="connectionStringAccessor">The accessor for retrieving the database connection string.</param>
        /// <param name="serviceProvider">The service provider used for dependency injection.</param>
        /// <remarks>
        /// This constructor initializes the processor with the specified dependencies, enabling it to execute
        /// database migrations targeting SQL Server 2016.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown if any of the required parameters are <c>null</c>.
        /// </exception>
        public SqlServer2016Processor(
            [NotNull] ILogger<SqlServer2016Processor> logger,
            [NotNull] SqlServer2008Quoter quoter,
            [NotNull] SqlServer2016Generator generator,
            [NotNull] IOptionsSnapshot<ProcessorOptions> options,
            [NotNull] IConnectionStringAccessor connectionStringAccessor,
            [NotNull] IServiceProvider serviceProvider)
            : this(
                SqlClientFactory.Instance,
                logger,
                quoter,
                generator,
                options,
                connectionStringAccessor,
                serviceProvider)
        {
        }

        /// <inheritdoc />
        protected SqlServer2016Processor(
            [NotNull] DbProviderFactory factory,
            [NotNull] ILogger logger,
            [NotNull] SqlServer2008Quoter quoter,
            [NotNull] SqlServer2016Generator generator,
            [NotNull] IOptionsSnapshot<ProcessorOptions> options,
            [NotNull] IConnectionStringAccessor connectionStringAccessor,
            [NotNull] IServiceProvider serviceProvider)
            : base(
                new[] { ProcessorIdConstants.SqlServer2016, ProcessorIdConstants.SqlServer },
                factory,
                generator,
                quoter,
                logger,
                options,
                connectionStringAccessor,
                serviceProvider)
        {
        }
    }
}
