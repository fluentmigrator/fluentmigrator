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

using System.Collections.Generic;

using FluentMigrator.Runner.Generators.Oracle;
using FluentMigrator.Runner.Initialization;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Processors.Oracle
{
    /// <summary>
    /// A processor for Oracle 12c using the .NET database access library
    /// </summary>
    public class Oracle12CProcessor : OracleProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Oracle12CProcessor"/> class.
        /// </summary>
        /// <param name="factory">The DB object factory</param>
        /// <param name="generator">The SQL generator</param>
        /// <param name="logger">The logger</param>
        /// <param name="options">The processor options</param>
        /// <param name="connectionStringAccessor">The accessor for the connection strings</param>
        public Oracle12CProcessor(
            [NotNull] OracleDbFactory factory,
            [NotNull] IOracle12CGenerator generator,
            [NotNull] ILogger<Oracle12CProcessor> logger,
            [NotNull] IOptionsSnapshot<ProcessorOptions> options,
            [NotNull] IConnectionStringAccessor connectionStringAccessor) : base(
            factory,
            generator,
            logger,
            options,
            connectionStringAccessor)
        {
        }

        /// <inheritdoc />
        public override string DatabaseType => ProcessorIdConstants.Oracle12c;

        /// <inheritdoc />
        public override IList<string> DatabaseTypeAliases { get; } = new List<string>()
            { ProcessorIdConstants.Oracle, "Oracle 12c" };
    }
}
