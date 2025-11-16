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
    /// Represents a migration processor for SQL Server 2014.
    /// </summary>
    /// <remarks>
    /// This processor provides functionality specific to SQL Server 2014, including support for its unique features and syntax.
    /// It extends the <see cref="SqlServerProcessor"/> to inherit common SQL Server processing capabilities.
    /// </remarks>
    public class SqlServer2014Processor : SqlServerProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServer2014Processor"/> class.
        /// </summary>
        /// <param name="logger">The logger used to log migration processing details.</param>
        /// <param name="quoter">The SQL quoter specific to SQL Server 2008, used for quoting SQL identifiers and literals.</param>
        /// <param name="generator">The SQL generator specific to SQL Server 2014, used to generate SQL statements.</param>
        /// <param name="options">The processor options containing configuration settings.</param>
        /// <param name="connectionStringAccessor">The accessor for retrieving the connection string.</param>
        /// <param name="serviceProvider">The service provider for resolving dependencies.</param>
        /// <remarks>
        /// This constructor sets up the SQL Server 2014 processor with the necessary dependencies,
        /// enabling it to handle migrations specific to SQL Server 2014.
        /// </remarks>
        public SqlServer2014Processor(
            [NotNull] ILogger<SqlServer2014Processor> logger,
            [NotNull] SqlServer2008Quoter quoter,
            [NotNull] SqlServer2014Generator generator,
            [NotNull] IOptionsSnapshot<ProcessorOptions> options,
            [NotNull] IConnectionStringAccessor connectionStringAccessor,
            [NotNull] IServiceProvider serviceProvider)
            : base(new[] { ProcessorIdConstants.SqlServer2014, ProcessorIdConstants.SqlServer }, generator, quoter, logger, options, connectionStringAccessor, serviceProvider)
        {
        }
    }
}
