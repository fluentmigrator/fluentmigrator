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

using FluentMigrator.Runner.Generators.MySql;
using FluentMigrator.Runner.Initialization;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Processors.MySql
{
    public class MySql4Processor : MySqlProcessor
    {
        /// <inheritdoc />
        public MySql4Processor(
            [NotNull] MySqlDbFactory factory,
            [NotNull] MySql4Generator generator,
            [NotNull] ILogger<MySql4Processor> logger,
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
        public override string DatabaseType => ProcessorIdConstants.MySql4;

        /// <inheritdoc />
        public override IList<string> DatabaseTypeAliases { get; } = new List<string>()
        {
            ProcessorIdConstants.MariaDB,
            ProcessorIdConstants.MySql,
            ProcessorIdConstants.MySql_4
        };
    }
}
