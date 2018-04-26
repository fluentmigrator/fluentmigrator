#region License
// Copyright (c) 2018, FluentMigrator Project
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

using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.BatchParser;
using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.SqlServer;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Moq;

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Processors.SqlServer2000
{
    [Category("SqlServer2000")]
    public class BatchParserTests : ProcessorBatchParserTestsBase
    {
        protected override IMigrationProcessor CreateProcessor()
        {
            var mockedConnStringReader = new Mock<IConnectionStringReader>();
            mockedConnStringReader.SetupGet(r => r.Priority).Returns(0);
            mockedConnStringReader.Setup(r => r.GetConnectionString(It.IsAny<string>())).Returns("server=this");

            var serviceProvider = new ServiceCollection()
                .AddTransient<SqlServerBatchParser>()
                .BuildServiceProvider();

            var opt = new OptionsWrapper<ProcessorOptions>(new ProcessorOptions());
            return new Processor(
                MockedDbProviderFactory.Object,
                new NullAnnouncer(),
                new SqlServer2000Generator(),
                opt,
                MockedConnectionStringAccessor.Object,
                serviceProvider);
        }

        private class Processor : SqlServer2000Processor
        {
            /// <inheritdoc />
            public Processor(DbProviderFactory factory, [NotNull] IAnnouncer announcer, [NotNull] SqlServer2000Generator generator, [NotNull] IOptions<ProcessorOptions> options, [NotNull] IConnectionStringAccessor connectionStringAccessor, [NotNull] IServiceProvider serviceProvider)
                : base(factory, announcer, generator, options, connectionStringAccessor, serviceProvider)
            {
            }
        }
    }
}
