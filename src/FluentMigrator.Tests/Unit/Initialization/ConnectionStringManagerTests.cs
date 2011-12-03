using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator.Runner;
using NUnit.Framework;
using FluentMigrator.Runner.Initialization;
using System.IO;
using Moq;
using System.Configuration;
using System.Reflection;

namespace FluentMigrator.Tests.Unit.Initialization
{
    [TestFixture]
    internal class ConnectionStringManagerTests
    {
        private const string TARGET = "FluentMigrator.Tests.dll";
        private const string DATABASE = "sqlserver2008";
        private const string CONNECTION_NAME = "Test.Connection";
        private IAnnouncer announcer;

        [SetUp]
        public void Setup()
        {
            var announcerMock = new Mock<IAnnouncer>(MockBehavior.Strict);
            announcerMock.Setup(a => a.Say(It.IsAny<string>()));
            announcer = announcerMock.Object;
        }

        private static string GetPath(string relative)
        {
            return string.Format(@"..\..\Unit\Initialization\Fixtures\{0}", relative);
        }

        private static Configuration LoadFromFile(string path)
        {
            var fileMap = new ExeConfigurationFileMap() { ExeConfigFilename = path };

            return ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
        }

        [Test]
        public void IfPathSpecifiedLoadItFirst()
        {
            string configPath = GetPath("WithConnectionString.config");

            var configManagerMock = new Mock<INetConfigManager>();

            configManagerMock.Setup(x => x.LoadFromFile(It.IsAny<string>()))
                             .Returns(LoadFromFile(configPath));

            var sut = new ConnectionStringManager(configManagerMock.Object, announcer, CONNECTION_NAME, configPath, TARGET, DATABASE);
            sut.LoadConnectionString();

            configManagerMock.VerifyAll();

            Assert.That(sut.ConnectionString, Is.EqualTo("From Arbitrary Config"));
        }

        [Test]
        public void IfNoPathSpecifiedLoadFromTargetAssemblyPath()
        {
            string configPath = GetPath("WithConnectionString.exe.config");

            var configManagerMock = new Mock<INetConfigManager>();

            configManagerMock.Setup(x => x.LoadFromFile(It.IsAny<string>()))
                             .Returns(LoadFromFile(configPath));

            var sut = new ConnectionStringManager(configManagerMock.Object, announcer, CONNECTION_NAME, null, TARGET, DATABASE);

            sut.LoadConnectionString();

            configManagerMock.VerifyAll();

            Assert.That(sut.ConnectionString, Is.EqualTo("From App Config"));
        }

        [Test]
        public void IfNoConnectionMatchesAppConfigLoadFromMachineConfig()
        {
            string configPath = GetPath("WithWrongConnectionString.config");
            string machineConfigPath = GetPath("FromMachineConfig.config");

            var configManagerMock = new Mock<INetConfigManager>();

            configManagerMock.Setup(x => x.LoadFromFile(It.IsAny<string>()))
                             .Returns(LoadFromFile(configPath));

            configManagerMock.Setup(x => x.LoadFromMachineConfiguration())
                             .Returns(LoadFromFile(machineConfigPath));

            var sut = new ConnectionStringManager(configManagerMock.Object, announcer, CONNECTION_NAME, null, TARGET, DATABASE);
            sut.LoadConnectionString();

            configManagerMock.VerifyAll();

            Assert.That(sut.ConnectionString, Is.EqualTo("From Machine Config"));
        }

        [Test]
        public void IfNoConnectionMatchesAndNoMatchInMachineConfigUseAsConnectionString()
        {
            string configPath = GetPath("WithWrongConnectionString.config");
            string machineConfigPath = GetPath("FromMachineConfig.config");

            var configManagerMock = new Mock<INetConfigManager>();

            configManagerMock.Setup(x => x.LoadFromFile(It.IsAny<string>()))
                             .Returns(LoadFromFile(configPath));

            configManagerMock.Setup(x => x.LoadFromMachineConfiguration())
                             .Returns(LoadFromFile(machineConfigPath));

            var sut = new ConnectionStringManager(configManagerMock.Object, announcer, "This is a connection string", null, TARGET, DATABASE);
            sut.LoadConnectionString();

            configManagerMock.VerifyAll();

            Assert.That(sut.ConnectionString, Is.EqualTo("This is a connection string"));
        }

    }
}
