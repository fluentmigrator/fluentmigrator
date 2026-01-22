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
using System.Linq;

using FluentMigrator.Runner.BatchParser;
using FluentMigrator.Runner.Generators.Snowflake;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Snowflake;
using FluentMigrator.Tests.Logging;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;
using Moq.Protected;

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Processors.Snowflake
{
    [Category("Snowflake")]
    public class BatchParserTests : ProcessorBatchParserTestsBase
    {
        protected override IMigrationProcessor CreateProcessor()
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<ILoggerProvider, TestLoggerProvider>()
                .AddTransient<SnowflakeBatchParser>()
                .BuildServiceProvider();

            var mockedDbFactory = new Mock<SnowflakeDbFactory>(serviceProvider);
            mockedDbFactory.SetupGet(conn => conn.Factory).Returns(MockedDbProviderFactory.Object);

            var mockedConnStringReader = new Mock<IConnectionStringReader>();
            mockedConnStringReader.SetupGet(r => r.Priority).Returns(0);
            mockedConnStringReader.Setup(r => r.GetConnectionString(It.IsAny<string>())).Returns(() =>
            {

                return "server=this";
            });

            var logger = serviceProvider.GetRequiredService<ILogger<SnowflakeProcessor>>();

            var opt = new OptionsManager<ProcessorOptions>(new OptionsFactory<ProcessorOptions>(
                Enumerable.Empty<IConfigureOptions<ProcessorOptions>>(),
                Enumerable.Empty<IPostConfigureOptions<ProcessorOptions>>()));
            var sfOptions = SnowflakeOptions.QuotingDisabled();
            return new SnowflakeProcessor(
                mockedDbFactory.Object,
                new SnowflakeGenerator(sfOptions),
                new SnowflakeQuoter(sfOptions), 
                logger,
                opt,
                MockedConnectionStringAccessor.Object,
                sfOptions,
                serviceProvider);
        }

        [Test]
        public override void Issue442()
        {

            using (var processor = CreateProcessor())
            {
                processor.Execute($"SELECT '{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}';{Environment.NewLine}SELECT 2;");
            }

            var expected = new []
            {
                $"SELECT '{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}'",
                "SELECT 2"
            };

            Assert.That(MockedCommands, Has.Count.EqualTo(expected.Length));
            Assert.That(CapturedCommandTexts, Has.Count.EqualTo(expected.Length));

            MockedDbProviderFactory.Verify(factory => factory.CreateConnection());

            for (var index = 0; index < MockedCommands.Count; index++)
            {
                var command = expected[index];
                var mockedCommand = MockedCommands[index];
                Assert.That(CapturedCommandTexts[index], Is.EqualTo(command));
                MockedDbProviderFactory.Verify(factory => factory.CreateCommand());
                mockedCommand.VerifySet(cmd => cmd.Connection = MockedConnection.Object);
                mockedCommand.Verify(cmd => cmd.ExecuteNonQuery(), Times.Exactly(1));
                mockedCommand.Protected().Verify("Dispose", Times.Exactly(1), ItExpr.IsAny<bool>());
            }

            MockedConnection.Protected().Verify("Dispose", Times.Exactly(1), ItExpr.IsAny<bool>());
        }

        [Test]
        public override void Issue842()
        {
            // No GO support in Snowflake
        }

        [Test]
        public override void TestThatGoWithRunCount()
        {
            // No GO support in Snowflake
        }

        [Test]
        public override void TestThatTwoCommandsGetSeparatedByGo()
        {
            // No GO support in Snowflake
        }
    }
}
