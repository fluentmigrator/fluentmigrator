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
    /// The SQL Server 2008 processor for FluentMigrator.
    /// </summary>
    public class SqlServer2008Processor : SqlServerProcessor
    {
        /// <inheritdoc />
        public SqlServer2008Processor(
            [NotNull] ILogger<SqlServer2008Processor> logger,
            [NotNull] SqlServer2008Quoter quoter,
            [NotNull] SqlServer2008Generator generator,
            [NotNull] IOptionsSnapshot<ProcessorOptions> options,
            [NotNull] IConnectionStringAccessor connectionStringAccessor,
            [NotNull] IServiceProvider serviceProvider)
            : base(new[] { ProcessorIdConstants.SqlServer2008, ProcessorIdConstants.SqlServer }, generator, quoter, logger, options, connectionStringAccessor, serviceProvider)
        {
        }
    }
}
