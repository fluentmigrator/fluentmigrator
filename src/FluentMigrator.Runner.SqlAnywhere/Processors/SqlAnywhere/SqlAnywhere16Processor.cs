#region License
//
// Copyright (c) 2007-2018, Sean Chambers <schambers80@gmail.com>
// Copyright (c) 2010, Nathan Brown
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
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;

using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Runner.BatchParser;
using FluentMigrator.Runner.BatchParser.Sources;
using FluentMigrator.Runner.BatchParser.SpecialTokenSearchers;
using FluentMigrator.Runner.Helpers;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.SqlAnywhere;

using JetBrains.Annotations;

using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Processors.SqlAnywhere
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class SqlAnywhere16Processor : SqlAnywhereProcessor
    {
        public SqlAnywhere16Processor(
            [NotNull] SqlAnywhereDbFactory factory,
            [NotNull] IMigrationGenerator generator,
            [NotNull] IAnnouncer announcer,
            [NotNull] IOptions<ProcessorOptions> options,
            [NotNull] IConnectionStringAccessor connectionStringAccessor)
            : base("SqlAnywhere16", factory.Factory, generator, announcer, options, connectionStringAccessor)
        {
        }
    }
}
