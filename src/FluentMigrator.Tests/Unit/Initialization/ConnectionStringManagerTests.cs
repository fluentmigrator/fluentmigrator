using System.Configuration;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using Moq;
using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Initialization
{
    [TestFixture]
    [Category("NotWorkingOnMono")]
    public class ConnectionStringManagerTests
    {
        [SetUp]
        public void Setup()
        {
            announcerMock = new Mock<IAnnouncer>(MockBehavior.Loose);
            announcerMock.Setup(a => a.Say(It.IsAny<string>()));

            configManagerMock = new Mock<INetConfigManager>(MockBehavior.Strict);
        }

        private const string TARGET = "FluentMigrator.Tests.dll";
        private const string DATABASE = "sqlserver2008";
        private const string CONNECTION_NAME = "Test.Connection";
        private Mock<IAnnouncer> announcerMock;
        private Mock<INetConfigManager> configManagerMock;

        private static string GetPath(string relative)
        {
            return string.Format(@"Unit\Initialization\Fixtures\{0}", relative);
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
            var sut = new ConnectionStringManager(configManagerMock.Object, announcerMock.Object, null, configPath, TARGET, DATABASE);
            configManagerMock.Setup(m => m.LoadFromFile(configPath))
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

            configManagerMock.Setup(x => x.LoadFromFile(TARGET))
                             .Returns(LoadFromFile(configPath));

            configManagerMock.Setup(x => x.LoadFromMachineConfiguration())
                             .Returns(LoadFromFile(machineConfigPath));

            var sut = new ConnectionStringManager(configManagerMock.Object, announcerMock.Object, CONNECTION_NAME, null, TARGET, DATABASE);
            sut.LoadConnectionString();

            configManagerMock.VerifyAll();

            Assert.That(sut.ConnectionString, Is.EqualTo("From Machine Config"));
        }

        [Test]
        public void ShouldLoadNamedConnectionFromSpecifiedConfigFile()
        {
            string configPath = GetPath("WithConnectionString.config");
            var sut = new ConnectionStringManager(configManagerMock.Object, announcerMock.Object, CONNECTION_NAME, configPath, TARGET, DATABASE);
            configManagerMock.Setup(m => m.LoadFromFile(configPath))
                             .Returns(LoadFromFile(configPath));

            sut.LoadConnectionString();

            Assert.That(sut.ConnectionString, Is.EqualTo("From Arbitrary Config"));
        }

        [Test]
        public void ShouldLoadNamedConnectionFromTargetAssemblyConfig()
        {
            string configPath = GetPath("WithConnectionString.exe.config");

            configManagerMock.Setup(x => x.LoadFromFile(TARGET))
                             .Returns(LoadFromFile(configPath));

            var sut = new ConnectionStringManager(configManagerMock.Object, announcerMock.Object, CONNECTION_NAME, null, TARGET, DATABASE);

            sut.LoadConnectionString();

            Assert.That(sut.ConnectionString, Is.EqualTo("From App Config"));
        }

        [Test]
        public void ShouldObfuscatePasswordOfConnectionString()
        {
            string configPath = GetPath("WithWrongConnectionString.config");
            string machineConfigPath = GetPath("FromMachineConfig.config");

            configManagerMock.Setup(x => x.LoadFromFile(TARGET))
                             .Returns(LoadFromFile(configPath));

            configManagerMock.Setup(x => x.LoadFromMachineConfiguration())
                             .Returns(LoadFromFile(machineConfigPath));

            var sut = new ConnectionStringManager(configManagerMock.Object, announcerMock.Object,
                                                  @"server=.\SQLEXPRESS;uid=test;pwd=test;Trusted_Connection=yes;database=FluentMigrator", null, TARGET,
                                                  DATABASE);

            sut.LoadConnectionString();

            announcerMock.Verify(a => a.Say(@"Using Database sqlserver2008 and Connection String server=.\SQLEXPRESS;uid=test;pwd=********;Trusted_Connection=yes;database=FluentMigrator"), Times.Once());
        }

        [Test]
        public void ShouldUseAsConnectionStringIfNoConnectionMatchesAndNoMatchInMachineConfig()
        {
            string configPath = GetPath("WithWrongConnectionString.config");
            string machineConfigPath = GetPath("FromMachineConfig.config");

            configManagerMock.Setup(x => x.LoadFromFile(TARGET))
                             .Returns(LoadFromFile(configPath));

            configManagerMock.Setup(x => x.LoadFromMachineConfiguration())
                             .Returns(LoadFromFile(machineConfigPath));

            var sut = new ConnectionStringManager(configManagerMock.Object, announcerMock.Object, "This is a connection string", null, TARGET, DATABASE);

            sut.LoadConnectionString();

            Assert.That(sut.ConnectionString, Is.EqualTo("This is a connection string"));
        }
    }
}