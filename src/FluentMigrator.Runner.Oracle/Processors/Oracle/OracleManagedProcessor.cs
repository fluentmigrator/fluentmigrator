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

using FluentMigrator.Runner.Generators.Oracle;
using FluentMigrator.Runner.Initialization;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Processors.Oracle
{
    /// <summary>
    /// Represents a processor for managing Oracle databases using the managed Oracle driver.
    /// </summary>
    /// <remarks>
    /// This class extends <see cref="OracleProcessorBase"/> and provides functionality specific to the managed Oracle driver.
    /// It is used to execute database operations and schema queries for Oracle databases.
    /// </remarks>
    public class OracleManagedProcessor : OracleProcessorBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OracleManagedProcessor"/> class.
        /// </summary>
        /// <param name="factory">
        /// The <see cref="OracleManagedDbFactory"/> responsible for creating database connections.
        /// </param>
        /// <param name="generator">
        /// The <see cref="IOracleGenerator"/> used to generate SQL statements for Oracle databases.
        /// </param>
        /// <param name="logger">
        /// The <see cref="ILogger{TCategoryName}"/> instance used for logging operations.
        /// </param>
        /// <param name="options">
        /// The <see cref="IOptionsSnapshot{TOptions}"/> containing configuration options for the processor.
        /// </param>
        /// <param name="connectionStringAccessor">
        /// The <see cref="IConnectionStringAccessor"/> used to access the database connection string.
        /// </param>
        /// <remarks>
        /// This constructor initializes the base <see cref="OracleProcessorBase"/> with the managed Oracle driver
        /// and provides the required dependencies for executing database operations.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown if any of the parameters are <c>null</c>.
        /// </exception>
        public OracleManagedProcessor(
            [NotNull] OracleManagedDbFactory factory,
            [NotNull] IOracleGenerator generator,
            [NotNull] ILogger<OracleManagedProcessor> logger,
            [NotNull] IOptionsSnapshot<ProcessorOptions> options,
            [NotNull] IConnectionStringAccessor connectionStringAccessor)
            : base(ProcessorIdConstants.OracleManaged, factory, generator, logger, options, connectionStringAccessor)
        {
        }
    }
}
