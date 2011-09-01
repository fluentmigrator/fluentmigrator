using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using FluentMigrator.Runner.Initialization;
using System.IO;

namespace FluentMigrator.Tests.Unit.ConnectionStringName
{
    [TestFixture]
    class ConnectionStringNameTests
    {
        private static string GetPath(string relative)
        {
            return string.Format(@"..\..\Unit\ConnectionStringName\Fixtures\{0}", relative);
        }

        private const string TARGET = "FluentMigrator.Tests.dll";
        private const string DATABASE = "sqlserver2008";
        private const string CONNECTION_NAME = "Test.Connection";

        [Test]
        public void ItFirstTriesConfigPath()
        {
            var sut = new NetConfigManager(CONNECTION_NAME, GetPath("WithConnectionString.config"), TARGET, DATABASE);
            sut.LoadConnectionString();
            Assert.That(sut.ConnectionString, Is.EqualTo("From Arbitrary Config"));
        }

        [Test]
        public void ItFailsIfTheConfigPathWasSpecifiedButCouldntResolveString()
        {
            var sut = new NetConfigManager(CONNECTION_NAME, GetPath("WithWrongConnectionString.config"), TARGET, DATABASE);
            Assert.Throws<ArgumentException>(() =>
                sut.LoadConnectionString());
        }

        [Test]
        public void ItFailsIfTheConfigPathWasSpecifiedButCouldntResolveFile()
        {
            var sut = new NetConfigManager(CONNECTION_NAME, GetPath("WithWrongPath.config"), TARGET, DATABASE);
            Assert.Throws<FileNotFoundException>(() =>
                sut.LoadConnectionString());
        }

        [Test]
        public void ItTriesAppConfigSecond()
        {
            var sut = new NetConfigManager(CONNECTION_NAME, GetPath("WithConnectionString.exe"), TARGET, DATABASE);
            sut.LoadConnectionString();
            Assert.That(sut.ConnectionString, Is.EqualTo("From App Config"));
        }

        [Test]
        public void ItFailsSilentlyOnMissingAppConfig()
        {
            var sut = new NetConfigManager(CONNECTION_NAME, GetPath("WithNoConfig.exe"), TARGET, DATABASE);
            Assert.Throws<ArgumentException>(() =>
                sut.LoadConnectionString());
        }

        [Test]
        public void ItFailsSilentlyOnMissingAppConfigConnectionString()
        {
            var sut = new NetConfigManager(CONNECTION_NAME, GetPath("WithNoConnectionString.exe"), TARGET, DATABASE);
            Assert.Throws<ArgumentException>(() =>
                sut.LoadConnectionString());
        }

        // TODO: figure out how to test machine.config settings
    }
}
