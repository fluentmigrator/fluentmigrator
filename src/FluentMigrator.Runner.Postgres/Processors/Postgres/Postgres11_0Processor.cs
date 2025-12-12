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

using FluentMigrator.Runner.Generators.Postgres;
using FluentMigrator.Runner.Initialization;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Processors.Postgres
{
    /// <summary>
    /// Represents a migration processor for PostgreSQL 11.0.
    /// </summary>
    /// <remarks>
    /// This class provides specific functionality for handling migrations targeting PostgreSQL version 11.0.
    /// It extends the <see cref="PostgresProcessor"/> to include PostgreSQL 11.0-specific behavior and configurations.
    /// </remarks>
    public class Postgres11_0Processor : PostgresProcessor
    {
        /// <inheritdoc />
        public Postgres11_0Processor(
            [NotNull] IPostgresDbFactory factory,
            [NotNull] Postgres11_0Generator generator,
            [NotNull] ILogger<Postgres11_0Processor> logger,
            [NotNull] IOptionsSnapshot<ProcessorOptions> options,
            [NotNull] IConnectionStringAccessor connectionStringAccessor,
            [NotNull] PostgresOptions pgOptions)
            : base(factory, generator, logger, options, connectionStringAccessor, pgOptions)
        {
        }

        /// <inheritdoc />
        public override string DatabaseType => ProcessorIdConstants.PostgreSQL11_0;

        /// <inheritdoc />
        public override IList<string> DatabaseTypeAliases { get; } = new List<string> { ProcessorIdConstants.PostgreSQL11_0, ProcessorIdConstants.PostgreSQL };

    }
}
