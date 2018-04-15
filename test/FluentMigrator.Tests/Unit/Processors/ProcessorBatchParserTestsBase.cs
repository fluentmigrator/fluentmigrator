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

using System.Collections.Generic;
using System.Data;

using FluentMigrator.Runner.Processors;

using Moq;

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Processors
{
    [Category("BatchParser")]
    public abstract class ProcessorBatchParserTestsBase
    {
        protected static IMigrationProcessorOptions ProcessorOptions { get; } = new ProcessorOptions();
        protected List<Mock<IDbCommand>> MockedCommands { get; set; }
        protected Mock<IDbConnection> MockedConnection { get; set; }
        protected Mock<IDbFactory> MockedDbFactory { get; set; }
        private ConnectionState _connectionState;

        [SetUp]
        public void SetUp()
        {
            MockedCommands = new List<Mock<IDbCommand>>();
            _connectionState = ConnectionState.Closed;
            MockedConnection = new Mock<IDbConnection>(MockBehavior.Strict);
            MockedConnection.SetupGet(conn => conn.State).Returns(() => _connectionState);
            MockedConnection.Setup(conn => conn.Open()).Callback(() => _connectionState = ConnectionState.Open);
            MockedConnection.Setup(conn => conn.Close()).Callback(() => _connectionState = ConnectionState.Closed);
            MockedConnection.SetupGet(conn => conn.ConnectionString).Returns("server=this");
            MockedDbFactory = new Mock<IDbFactory>();
            MockedDbFactory.Setup(factory => factory.CreateCommand(
                    It.IsAny<string>(), MockedConnection.Object,
                    null, ProcessorOptions))
                .Returns((string commandText, IDbConnection connection, IDbTransaction transaction, IMigrationProcessorOptions _) =>
                {
                    var commandMock = new Mock<IDbCommand>(MockBehavior.Strict);
                    commandMock.SetupProperty(cmd => cmd.CommandText, commandText);
                    commandMock.SetupGet(cmd => cmd.Connection).Returns(connection);
                    commandMock.SetupGet(cmd => cmd.Transaction).Returns(transaction);
                    commandMock.Setup(cmd => cmd.ExecuteNonQuery()).Returns(1);
                    commandMock.Setup(cmd => cmd.Dispose());
                    MockedCommands.Add(commandMock);
                    return commandMock.Object;
                });
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var mockedCommand in MockedCommands)
            {
                mockedCommand.VerifyNoOtherCalls();
            }

            MockedDbFactory.VerifyNoOtherCalls();
        }

        [Test]
        public void TestOneCommandForMultipleLines()
        {
            var processor = CreateProcessor();

            var command = "SELECT 1\nSELECT 2";
            processor.Execute(command);

            foreach (var mockedCommand in MockedCommands)
            {
                mockedCommand.Verify(cmd => cmd.ExecuteNonQuery());
                mockedCommand.Verify(cmd => cmd.Dispose());
            }

            MockedDbFactory.Verify(factory => factory.CreateCommand(command, MockedConnection.Object, null, ProcessorOptions));
        }

        [Test]
        public void TestThatTwoCommandsGetSeparatedByGo()
        {
            var processor = CreateProcessor();

            processor.Execute("SELECT 1\nGO\nSELECT 2");

            for (int index = 0; index < MockedCommands.Count; index++)
            {
                var command = $"SELECT {index + 1}";
                var mockedCommand = MockedCommands[index];
                mockedCommand.VerifySet(cmd => cmd.CommandText = command);
                mockedCommand.Verify(cmd => cmd.ExecuteNonQuery());
                mockedCommand.Verify(cmd => cmd.Dispose());
            }

            MockedDbFactory.Verify(factory => factory.CreateCommand(string.Empty, MockedConnection.Object, null, ProcessorOptions));
            MockedDbFactory.Verify(factory => factory.CreateCommand(string.Empty, MockedConnection.Object, null, ProcessorOptions));
        }

        [Test]
        public void TestThatTwoCommandsAreNotSeparatedByGoInComment()
        {
            var processor = CreateProcessor();

            var command = "SELECT 1\n /* GO */\nSELECT 2";
            processor.Execute(command);

            for (int index = 0; index < MockedCommands.Count; index++)
            {
                var mockedCommand = MockedCommands[index];
                mockedCommand.Verify(cmd => cmd.ExecuteNonQuery());
                mockedCommand.Verify(cmd => cmd.Dispose());
            }

            MockedDbFactory.Verify(factory => factory.CreateCommand(command, MockedConnection.Object, null, ProcessorOptions));
        }

        [Test]
        public void TestThatTwoCommandsAreNotSeparatedByGoInString()
        {
            var processor = CreateProcessor();

            var command = "SELECT '\nGO\n'\nSELECT 2";
            processor.Execute(command);

            for (int index = 0; index < MockedCommands.Count; index++)
            {
                var mockedCommand = MockedCommands[index];
                mockedCommand.Verify(cmd => cmd.ExecuteNonQuery());
                mockedCommand.Verify(cmd => cmd.Dispose());
            }

            MockedDbFactory.Verify(factory => factory.CreateCommand(command, MockedConnection.Object, null, ProcessorOptions));
        }

        [Test]
        public void Issue442()
        {
            var processor = CreateProcessor();

            var command = "SELECT '\n\n\n';\nSELECT 2;";
            processor.Execute(command);

            for (int index = 0; index < MockedCommands.Count; index++)
            {
                var mockedCommand = MockedCommands[index];
                mockedCommand.Verify(cmd => cmd.ExecuteNonQuery());
                mockedCommand.Verify(cmd => cmd.Dispose());
            }

            MockedDbFactory.Verify(factory => factory.CreateCommand(command, MockedConnection.Object, null, ProcessorOptions));
        }

        [Test]
        public void Issue842()
        {
            var processor = CreateProcessor();

            var command = @"insert into MyTable (Id, Data)\nvalues (42, 'This is a list of games played by people\n\nDooM\nPokemon GO\nPotato-o-matic');";
            processor.Execute(command);

            for (int index = 0; index < MockedCommands.Count; index++)
            {
                var mockedCommand = MockedCommands[index];
                mockedCommand.Verify(cmd => cmd.ExecuteNonQuery());
                mockedCommand.Verify(cmd => cmd.Dispose());
            }

            MockedDbFactory.Verify(factory => factory.CreateCommand(command, MockedConnection.Object, null, ProcessorOptions));
        }

        [Test]
        public void TestThatGoWithRunCount()
        {
            var processor = CreateProcessor();

            processor.Execute("SELECT 1\nGO 3\nSELECT 2\nGO\nSELECT 3\nGO 2");

            var expected = new (string command, int count)[]
            {
                ("SELECT 1", 3),
                ("SELECT 2", 1),
                ("SELECT 3", 2),
            };

            Assert.AreEqual(expected.Length, MockedCommands.Count);

            for (var index = 0; index < MockedCommands.Count; index++)
            {
                var (command, count) = expected[index];
                var mockedCommand = MockedCommands[index];
                mockedCommand.VerifySet(cmd => cmd.CommandText = command);
                mockedCommand.Verify(cmd => cmd.ExecuteNonQuery(), Times.Exactly(count));
                mockedCommand.Verify(cmd => cmd.Dispose());

                MockedDbFactory.Verify(factory => factory.CreateCommand(string.Empty, MockedConnection.Object, null, ProcessorOptions));
            }
        }

        protected abstract IMigrationProcessor CreateProcessor();
    }
}
