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
    internal class ConnectionStringNameTests
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
            var sut = new ConnectionStringManager(CONNECTION_NAME, GetPath("WithConnectionString.config"), TARGET, DATABASE);
            sut.LoadConnectionString();
            Assert.That(sut.ConnectionString, Is.EqualTo("From Arbitrary Config"));
        }

        [Test]
        public void ItTriesAppConfigSecond()
        {
            var sut = new ConnectionStringManager(CONNECTION_NAME, GetPath("WithConnectionString.exe"), TARGET, DATABASE);
            sut.LoadConnectionString();
            Assert.That(sut.ConnectionString, Is.EqualTo("From App Config"));
        }

        [Test]
        public void ItTriesMachineConfigThird()
        {
            var sut = new ConnectionStringManager("LocalSqlServer", GetPath("nonexists.exe"), TARGET, DATABASE);
            sut.LoadConnectionString();
            Assert.That(sut.ConnectionString, Is.StringStarting("data source=.\\SQLEXPRESS"));
        }

        // TODO: For proper testing remove dependency on .NET Config Manager
    }
}
