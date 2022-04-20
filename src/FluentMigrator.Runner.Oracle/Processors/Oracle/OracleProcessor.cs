#region License
//
// Copyright (c) 2007-2018, Sean Chambers <schambers80@gmail.com>
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
using System.Data;

using FluentMigrator.Runner.Generators.Oracle;
using FluentMigrator.Runner.Initialization;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace FluentMigrator.Runner.Processors.Oracle
{
    public class OracleProcessor : OracleProcessorBase
    {
        [Obsolete]
        public OracleProcessor(
            IDbConnection connection,
            IMigrationGenerator generator,
            IAnnouncer announcer,
            IMigrationProcessorOptions options,
            IDbFactory factory)
            : base(ProcessorId.Oracle, connection, generator, announcer, options, factory)
        {
        }

        public OracleProcessor(
            [NotNull] OracleDbFactory factory,
            [NotNull] IOracleGenerator generator,
            [NotNull] ILogger<OracleProcessor> logger,
            [NotNull] IOptionsSnapshot<ProcessorOptions> options,
            [NotNull] IConnectionStringAccessor connectionStringAccessor)
            : base(ProcessorId.Oracle, factory, generator, logger, options, connectionStringAccessor)
        {
        }
    }
}
