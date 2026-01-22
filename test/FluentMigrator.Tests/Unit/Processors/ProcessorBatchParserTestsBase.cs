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
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

using FluentMigrator.Runner.Initialization;

using Moq;
using Moq.Protected;

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Processors
{
    [Category("BatchParser")]
    public abstract class ProcessorBatchParserTestsBase
    {
        private const string ConnectionString = "server=this";

        private ConnectionState _connectionState;

        protected Mock<DbConnection> MockedConnection { get; private set; }
        protected Mock<DbProviderFactory> MockedDbProviderFactory { get; private set; }
        protected Mock<IConnectionStringAccessor> MockedConnectionStringAccessor { get; private set; }
        protected List<Mock<DbCommand>> MockedCommands { get; private set; }
        protected List<string> CapturedCommandTexts { get; private set; }

        [SetUp]
        public void SetUp()
        {
            _connectionState = ConnectionState.Closed;

            MockedCommands = new List<Mock<DbCommand>>();
            CapturedCommandTexts = new List<string>();
            MockedConnection = new Mock<DbConnection>(MockBehavior.Loose);
            MockedDbProviderFactory = new Mock<DbProviderFactory>(MockBehavior.Loose);
            MockedConnectionStringAccessor = new Mock<IConnectionStringAccessor>(MockBehavior.Loose);

            MockedConnection.SetupGet(conn => conn.State).Returns(() => _connectionState);
            MockedConnection.Setup(conn => conn.Open()).Callback(() => _connectionState = ConnectionState.Open);
            MockedConnection.Setup(conn => conn.Close()).Callback(() => _connectionState = ConnectionState.Closed);
            MockedConnection.SetupProperty(conn => conn.ConnectionString);
            MockedConnection.Protected().Setup("Dispose", ItExpr.IsAny<bool>());

            MockedConnectionStringAccessor.SetupGet(a => a.ConnectionString).Returns(ConnectionString);

            MockedDbProviderFactory.Setup(factory => factory.CreateConnection())
                .Returns(MockedConnection.Object);

            MockedDbProviderFactory.Setup(factory => factory.CreateCommand())
                .Returns(
                    () =>
                    {
                        var commandMock = new Mock<DbCommand>(MockBehavior.Loose);
                        // Use callback to capture CommandText to avoid flaky VerifySet behavior with Moq/Castle.DynamicProxy
                        commandMock
                            .SetupSet(cmd => cmd.CommandText = It.IsAny<string>())
                            .Callback<string>(value => CapturedCommandTexts.Add(value));
                        commandMock.Setup(cmd => cmd.ExecuteNonQuery()).Returns(1);
                        commandMock.Protected().SetupGet<DbConnection>("DbConnection").Returns(MockedConnection.Object);
                        commandMock.Protected().SetupSet<DbConnection>("DbConnection", ItExpr.Is<DbConnection>(v => v == MockedConnection.Object));
                        commandMock.Protected().Setup("Dispose", ItExpr.IsAny<bool>());
                        MockedCommands.Add(commandMock);
                        return commandMock.Object;
                    });
        }

        /*
        [TearDown]
        public void TearDown()
        {
            foreach (var mockedCommand in MockedCommands)
            {
                mockedCommand.VerifyNoOtherCalls();
            }

            MockedDbProviderFactory.VerifyNoOtherCalls();
        }
        */

        [Test]
        public void TestOneCommandForMultipleLines()
        {
            var command = $"SELECT 1{Environment.NewLine}SELECT 2";
            using (var processor = CreateProcessor())
            {
                processor.Execute(command);
            }

            MockedDbProviderFactory.Verify(factory => factory.CreateConnection());

            Assert.That(CapturedCommandTexts, Has.Count.EqualTo(1));
            Assert.That(CapturedCommandTexts[0], Is.EqualTo(command));

            foreach (var mockedCommand in MockedCommands)
            {
                MockedDbProviderFactory.Verify(factory => factory.CreateCommand());
                mockedCommand.VerifySet(cmd => cmd.Connection = MockedConnection.Object);
                mockedCommand.Verify(cmd => cmd.ExecuteNonQuery());
                mockedCommand.Protected().Verify("Dispose", Times.Exactly(1), ItExpr.IsAny<bool>());
            }

            MockedConnection.Protected().Verify("Dispose", Times.Exactly(1), ItExpr.IsAny<bool>());
        }

        [Test]
        public virtual void TestThatTwoCommandsGetSeparatedByGo()
        {
            using (var processor = CreateProcessor())
            {
                processor.Execute("SELECT 1\nGO\nSELECT 2");
            }

            MockedDbProviderFactory.Verify(factory => factory.CreateConnection());

            Assert.That(CapturedCommandTexts, Has.Count.EqualTo(2));

            for (int index = 0; index < MockedCommands.Count; index++)
            {
                var command = $"SELECT {index + 1}";
                var mockedCommand = MockedCommands[index];
                Assert.That(CapturedCommandTexts[index], Is.EqualTo(command));
                MockedDbProviderFactory.Verify(factory => factory.CreateCommand());
                mockedCommand.VerifySet(cmd => cmd.Connection = MockedConnection.Object);
                mockedCommand.Verify(cmd => cmd.ExecuteNonQuery());
                mockedCommand.Protected().Verify("Dispose", Times.Exactly(1), ItExpr.IsAny<bool>());
            }

            MockedConnection.Protected().Verify("Dispose", Times.Exactly(1), ItExpr.IsAny<bool>());
        }

        [Test]
        public void TestThatTwoCommandsAreNotSeparatedByGoInComment()
        {
            var command = "SELECT 1\n /* GO */\nSELECT 2";

            using (var processor = CreateProcessor())
            {
                processor.Execute(command);
            }

            MockedDbProviderFactory.Verify(factory => factory.CreateConnection());

            Assert.That(CapturedCommandTexts, Has.Count.EqualTo(1));
            Assert.That(CapturedCommandTexts[0], Is.EqualTo(command));

            foreach (var mockedCommand in MockedCommands)
            {
                MockedDbProviderFactory.Verify(factory => factory.CreateCommand());
                mockedCommand.VerifySet(cmd => cmd.Connection = MockedConnection.Object);
                mockedCommand.Verify(cmd => cmd.ExecuteNonQuery());
                mockedCommand.Protected().Verify("Dispose", Times.Exactly(1), ItExpr.IsAny<bool>());
            }

            MockedConnection.Protected().Verify("Dispose", Times.Exactly(1), ItExpr.IsAny<bool>());
        }

        [Test]
        public void TestThatTwoCommandsAreNotSeparatedByGoInString()
        {
            var command = "SELECT '\nGO\n'\nSELECT 2";

            using (var processor = CreateProcessor())
            {
                processor.Execute(command);
            }

            MockedDbProviderFactory.Verify(factory => factory.CreateConnection());

            Assert.That(CapturedCommandTexts, Has.Count.EqualTo(1));
            Assert.That(CapturedCommandTexts[0], Is.EqualTo(command));

            foreach (var mockedCommand in MockedCommands)
            {
                MockedDbProviderFactory.Verify(factory => factory.CreateCommand());
                mockedCommand.VerifySet(cmd => cmd.Connection = MockedConnection.Object);
                mockedCommand.Verify(cmd => cmd.ExecuteNonQuery());
                mockedCommand.Protected().Verify("Dispose", Times.Exactly(1), ItExpr.IsAny<bool>());
            }

            MockedConnection.Protected().Verify("Dispose", Times.Exactly(1), ItExpr.IsAny<bool>());
        }

        [Test]
        public virtual void Issue442()
        {
            var command = "SELECT '\n\n\n';\nSELECT 2;";

            using (var processor = CreateProcessor())
            {
                processor.Execute(command);
            }

            MockedDbProviderFactory.Verify(factory => factory.CreateConnection());

            Assert.That(CapturedCommandTexts, Has.Count.EqualTo(1));
            Assert.That(CapturedCommandTexts[0], Is.EqualTo(command));

            foreach (var mockedCommand in MockedCommands)
            {
                MockedDbProviderFactory.Verify(factory => factory.CreateCommand());
                mockedCommand.VerifySet(cmd => cmd.Connection = MockedConnection.Object);
                mockedCommand.Verify(cmd => cmd.ExecuteNonQuery());
                mockedCommand.Protected().Verify("Dispose", Times.Exactly(1), ItExpr.IsAny<bool>());
            }

            MockedConnection.Protected().Verify("Dispose", Times.Exactly(1), ItExpr.IsAny<bool>());
        }

        [Test]
        public virtual void Issue842()
        {
            var command = @"insert into MyTable (Id, Data)\nvalues (42, 'This is a list of games played by people\n\nDooM\nPokemon GO\nPotato-o-matic');";

            using (var processor = CreateProcessor())
            {
                processor.Execute(command);
            }

            MockedDbProviderFactory.Verify(factory => factory.CreateConnection());

            Assert.That(CapturedCommandTexts, Has.Count.EqualTo(1));
            Assert.That(CapturedCommandTexts[0], Is.EqualTo(command));

            foreach (var mockedCommand in MockedCommands)
            {
                MockedDbProviderFactory.Verify(factory => factory.CreateCommand());
                mockedCommand.VerifySet(cmd => cmd.Connection = MockedConnection.Object);
                mockedCommand.Verify(cmd => cmd.ExecuteNonQuery());
                mockedCommand.Protected().Verify("Dispose", Times.Exactly(1), ItExpr.IsAny<bool>());
            }

            MockedConnection.Protected().Verify("Dispose", Times.Exactly(1), ItExpr.IsAny<bool>());
        }

        [Test]
        public virtual void TestThatGoWithRunCount()
        {
            using (var processor = CreateProcessor())
            {
                processor.Execute("SELECT 1\nGO 3\nSELECT 2\nGO\nSELECT 3\nGO 2");
            }

            var expected = new (string command, int count)[]
            {
                ("SELECT 1", 3),
                ("SELECT 2", 1),
                ("SELECT 3", 2),
            };

            Assert.That(MockedCommands, Has.Count.EqualTo(expected.Length));
            Assert.That(CapturedCommandTexts, Has.Count.EqualTo(expected.Length));

            MockedDbProviderFactory.Verify(factory => factory.CreateConnection());

            for (var index = 0; index < MockedCommands.Count; index++)
            {
                var (command, count) = expected[index];
                var mockedCommand = MockedCommands[index];
                Assert.That(CapturedCommandTexts[index], Is.EqualTo(command));
                MockedDbProviderFactory.Verify(factory => factory.CreateCommand());
                mockedCommand.VerifySet(cmd => cmd.Connection = MockedConnection.Object);
                mockedCommand.Verify(cmd => cmd.ExecuteNonQuery(), Times.Exactly(count));
                mockedCommand.Protected().Verify("Dispose", Times.Exactly(1), ItExpr.IsAny<bool>());
            }

            MockedConnection.Protected().Verify("Dispose", Times.Exactly(1), ItExpr.IsAny<bool>());
        }

        protected abstract IMigrationProcessor CreateProcessor();
    }
}
