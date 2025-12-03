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

using System.Linq;

using FluentMigrator.Runner.Generators.Oracle;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Oracle;
using FluentMigrator.Tests.Logging;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Processors.Oracle
{
    [Category("Oracle")]
    [Category("BatchParser")]
    public class BatchParserTests : ProcessorBatchParserTestsBase
    {
        protected override IMigrationProcessor CreateProcessor()
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<ILoggerProvider, TestLoggerProvider>()
                .BuildServiceProvider();

            var mockedDbFactory = new Mock<OracleDbFactory>(serviceProvider);
            mockedDbFactory.SetupGet(conn => conn.Factory).Returns(MockedDbProviderFactory.Object);

            var logger = serviceProvider.GetRequiredService<ILogger<OracleProcessor>>();

            var opt = new OptionsManager<ProcessorOptions>(new OptionsFactory<ProcessorOptions>(
                Enumerable.Empty<IConfigureOptions<ProcessorOptions>>(),
                Enumerable.Empty<IPostConfigureOptions<ProcessorOptions>>()));
            return new OracleProcessor(
                mockedDbFactory.Object,
                new OracleGenerator(),
                logger,
                opt,
                MockedConnectionStringAccessor.Object);
        }

        /// <summary>
        /// Oracle does not use GO as a batch separator, so this test is overridden to verify
        /// that GO is treated as literal text in Oracle.
        /// </summary>
        [Test]
        public override void TestThatTwoCommandsGetSeparatedByGo()
        {
            // Oracle doesn't use GO as batch separator - it uses semicolons
            // GO in Oracle is just treated as identifier/text
            var command = "SELECT 1\nGO\nSELECT 2";
            using (var processor = CreateProcessor())
            {
                processor.Execute(command);
            }

            MockedDbProviderFactory.Verify(factory => factory.CreateConnection());

            // Oracle will treat this as a single command since GO is not a separator
            Assert.That(MockedCommands, Has.Count.EqualTo(1));
            var mockedCommand = MockedCommands[0];
            MockedDbProviderFactory.Verify(factory => factory.CreateCommand());
            mockedCommand.VerifySet(cmd => cmd.Connection = MockedConnection.Object);
            mockedCommand.VerifySet(cmd => cmd.CommandText = command);
            mockedCommand.Verify(cmd => cmd.ExecuteNonQuery());
        }

        /// <summary>
        /// Oracle uses semicolons as statement separators, not GO with run count.
        /// </summary>
        [Test]
        public override void TestThatGoWithRunCount()
        {
            // GO with run count is not supported in Oracle
            var command = "SELECT 1\nGO 3\nSELECT 2\nGO\nSELECT 3\nGO 2";

            using (var processor = CreateProcessor())
            {
                processor.Execute(command);
            }

            // Oracle treats GO as literal text, so it's a single command
            Assert.That(MockedCommands, Has.Count.EqualTo(1));
            MockedDbProviderFactory.Verify(factory => factory.CreateConnection());

            var mockedCommand = MockedCommands[0];
            MockedDbProviderFactory.Verify(factory => factory.CreateCommand());
            mockedCommand.VerifySet(cmd => cmd.Connection = MockedConnection.Object);
            mockedCommand.VerifySet(cmd => cmd.CommandText = command);
            mockedCommand.Verify(cmd => cmd.ExecuteNonQuery());
        }

        /// <summary>
        /// Oracle handles Issue442 with semicolons instead of GO.
        /// </summary>
        [Test]
        public override void Issue442()
        {
            // In Oracle, these are two separate statements split by semicolon
            using (var processor = CreateProcessor())
            {
                processor.Execute("SELECT '\n\n\n' FROM dual; SELECT 2 FROM dual;");
            }

            MockedDbProviderFactory.Verify(factory => factory.CreateConnection());

            // Oracle splits on semicolons (outside of PL/SQL blocks)
            Assert.That(MockedCommands, Has.Count.EqualTo(2));
        }

        /// <summary>
        /// Oracle handles Issue842 - multiline string data should not be split.
        /// </summary>
        [Test]
        public override void Issue842()
        {
            var command = @"insert into MyTable (Id, Data)\nvalues (42, 'This is a list of games played by people\n\nDooM\nPokemon GO\nPotato-o-matic')";

            using (var processor = CreateProcessor())
            {
                processor.Execute(command);
            }

            MockedDbProviderFactory.Verify(factory => factory.CreateConnection());

            // This is a single statement with no semicolon
            Assert.That(MockedCommands, Has.Count.EqualTo(1));
            var mockedCommand = MockedCommands[0];
            MockedDbProviderFactory.Verify(factory => factory.CreateCommand());
            mockedCommand.VerifySet(cmd => cmd.Connection = MockedConnection.Object);
            mockedCommand.VerifySet(cmd => cmd.CommandText = command);
            mockedCommand.Verify(cmd => cmd.ExecuteNonQuery());
        }

        /// <summary>
        /// Test that Oracle PL/SQL blocks with BEGIN/END are handled correctly.
        /// </summary>
        [Test]
        public void TestPlSqlBlockIsNotSplit()
        {
            var command = "BEGIN\n    DBMS_OUTPUT.PUT_LINE('Hello');\nEND;";

            using (var processor = CreateProcessor())
            {
                processor.Execute(command);
            }

            MockedDbProviderFactory.Verify(factory => factory.CreateConnection());

            // PL/SQL block should be treated as a single command
            Assert.That(MockedCommands, Has.Count.EqualTo(1));
            var mockedCommand = MockedCommands[0];
            MockedDbProviderFactory.Verify(factory => factory.CreateCommand());
            mockedCommand.VerifySet(cmd => cmd.Connection = MockedConnection.Object);
            mockedCommand.VerifySet(cmd => cmd.CommandText = command);
            mockedCommand.Verify(cmd => cmd.ExecuteNonQuery());
        }

        /// <summary>
        /// Test that multiple Oracle statements are split on semicolons.
        /// </summary>
        [Test]
        public void TestMultipleStatementsSplitOnSemicolon()
        {
            using (var processor = CreateProcessor())
            {
                processor.Execute("SELECT 1 FROM dual; SELECT 2 FROM dual;");
            }

            MockedDbProviderFactory.Verify(factory => factory.CreateConnection());

            // Should be split into 2 commands
            Assert.That(MockedCommands, Has.Count.EqualTo(2));
        }

        /// <summary>
        /// Test that DECLARE blocks are handled correctly.
        /// </summary>
        [Test]
        public void TestDeclareBlockIsNotSplit()
        {
            var command = "DECLARE\n    v_count NUMBER;\nBEGIN\n    v_count := 1;\nEND;";

            using (var processor = CreateProcessor())
            {
                processor.Execute(command);
            }

            MockedDbProviderFactory.Verify(factory => factory.CreateConnection());

            // DECLARE/BEGIN/END block should be treated as a single command
            Assert.That(MockedCommands, Has.Count.EqualTo(1));
            var mockedCommand = MockedCommands[0];
            MockedDbProviderFactory.Verify(factory => factory.CreateCommand());
            mockedCommand.VerifySet(cmd => cmd.Connection = MockedConnection.Object);
            mockedCommand.VerifySet(cmd => cmd.CommandText = command);
            mockedCommand.Verify(cmd => cmd.ExecuteNonQuery());
        }
    }
}
