#region License
//
// Copyright (c) 2018, Fluent Migrator Project
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
#if NETFRAMEWORK
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization.NetFramework;
using FluentMigrator.Tests.Logging;

using Microsoft.Extensions.Logging;

using Moq;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Initialization
{
    [TestFixture]
    public class ConnectionStringManagerTests
    {
        [SetUp]
        public void Setup()
        {
            _logMessages = new List<string>();
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(new TextLineLoggerProvider(_logMessages, new FluentMigratorLoggerOptions() { ShowElapsedTime = true }));
            _logger = loggerFactory.CreateLogger<ConnectionStringManager>();
            _configManagerMock = new Mock<INetConfigManager>(MockBehavior.Strict);
        }

        private const string TARGET = "FluentMigrator.Tests.dll";
        private const string DATABASE = "sqlserver2008";
        private const string CONNECTION_NAME = "Test.Connection";
        private ICollection<string> _logMessages;
        private ILogger<ConnectionStringManager> _logger;
        private Mock<INetConfigManager> _configManagerMock;

        private static string GetPath(string relative)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Unit", "Initialization", "Fixtures", relative);
        }

        private static Configuration LoadFromFile(string path)
        {
            var fileMap = new ExeConfigurationFileMap {ExeConfigFilename = path};

            return ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
        }

        [Test]
        public void ShouldLoadMachineNameConnectionFromSpecifiedConfigIfNoConnectionNameSpecified()
        {
            string configPath = GetPath("WithConnectionString.config");
            var sut = new ConnectionStringManager(_configManagerMock.Object, _logger, null, configPath, TARGET, DATABASE);
            _configManagerMock.Setup(m => m.LoadFromFile(configPath))
                             .Returns(LoadFromFile(configPath));
            sut.MachineNameProvider = () => "MACHINENAME";

            sut.LoadConnectionString();

            Assert.That(sut.ConnectionString, Is.EqualTo("From Machine Name"));
        }

        [Test]
        public void ShouldLoadNamedConnectionFromMachineConfigIfTargetAssemblyConfigHasNoMatch()
        {
            string configPath = GetPath("WithWrongConnectionString.config");
            string machineConfigPath = GetPath("FromMachineConfig.config");

            _configManagerMock.Setup(x => x.LoadFromFile(TARGET))
                             .Returns(LoadFromFile(configPath));

            _configManagerMock.Setup(x => x.LoadFromMachineConfiguration())
                             .Returns(LoadFromFile(machineConfigPath));

            var sut = new ConnectionStringManager(_configManagerMock.Object, _logger, CONNECTION_NAME, null, TARGET, DATABASE);
            sut.LoadConnectionString();

            _configManagerMock.VerifyAll();

            Assert.That(sut.ConnectionString, Is.EqualTo("From Machine Config"));
        }

        [Test]
        public void ShouldLoadNamedConnectionFromSpecifiedConfigFile()
        {
            string configPath = GetPath("WithConnectionString.config");
            var sut = new ConnectionStringManager(_configManagerMock.Object, _logger, CONNECTION_NAME, configPath, TARGET, DATABASE);
            _configManagerMock.Setup(m => m.LoadFromFile(configPath))
                             .Returns(LoadFromFile(configPath));

            sut.LoadConnectionString();

            Assert.That(sut.ConnectionString, Is.EqualTo("From Arbitrary Config"));
        }

        [Test]
        public void ShouldLoadNamedConnectionFromTargetAssemblyConfig()
        {
            string configPath = GetPath("WithConnectionString.exe.config");

            _configManagerMock.Setup(x => x.LoadFromFile(TARGET))
                             .Returns(LoadFromFile(configPath));

            var sut = new ConnectionStringManager(_configManagerMock.Object, _logger, CONNECTION_NAME, null, TARGET, DATABASE);

            sut.LoadConnectionString();

            Assert.That(sut.ConnectionString, Is.EqualTo("From App Config"));
        }

        [Test]
        public void ShouldObfuscatePasswordOfConnectionString()
        {
            string configPath = GetPath("WithWrongConnectionString.config");
            string machineConfigPath = GetPath("FromMachineConfig.config");

            _configManagerMock.Setup(x => x.LoadFromFile(TARGET))
                             .Returns(LoadFromFile(configPath));

            _configManagerMock.Setup(x => x.LoadFromMachineConfiguration())
                             .Returns(LoadFromFile(machineConfigPath));

            var sut = new ConnectionStringManager(
                _configManagerMock.Object,
                _logger,
                @"server=.\SQLEXPRESS;uid=test;pwd=test;Trusted_Connection=yes;database=FluentMigrator",
                null,
                TARGET,
                DATABASE);

            sut.LoadConnectionString();

            var expectedMessage = @"Using Database sqlserver2008 and Connection String server=.\SQLEXPRESS;uid=test;pwd=********;Trusted_Connection=yes;database=FluentMigrator";
            _logMessages.Count(x => x.Contains(expectedMessage)).ShouldBe(1);
        }

        [Test]
        public void ShouldUseAsConnectionStringIfNoConnectionMatchesAndNoMatchInMachineConfig()
        {
            string configPath = GetPath("WithWrongConnectionString.config");
            string machineConfigPath = GetPath("FromMachineConfig.config");

            _configManagerMock.Setup(x => x.LoadFromFile(TARGET))
                             .Returns(LoadFromFile(configPath));

            _configManagerMock.Setup(x => x.LoadFromMachineConfiguration())
                             .Returns(LoadFromFile(machineConfigPath));

            var sut = new ConnectionStringManager(_configManagerMock.Object, _logger, "This is a connection string", null, TARGET, DATABASE);

            sut.LoadConnectionString();

            Assert.That(sut.ConnectionString, Is.EqualTo("This is a connection string"));
        }
    }
}
#endif