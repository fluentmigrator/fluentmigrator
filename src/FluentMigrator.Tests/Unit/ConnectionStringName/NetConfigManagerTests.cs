using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Should;
using FluentMigrator.Runner.Initialization;
using System.Configuration;

namespace FluentMigrator.Tests.Unit.ConnectionStringName
{
    [TestFixture]
    public class NetConfigManagerTests
    {
        private static string GetPath(string relative)
        {
            return string.Format(@"..\..\Unit\ConnectionStringName\Fixtures\{0}", relative);
        }

        [Test]
        public void LoadsConfigurationFromConfigFile()
        {
            var sut = new NetConfigManager();

            Configuration config = sut.LoadFromFile(GetPath("WithConnectionString.config"));

            config.ConnectionStrings.ConnectionStrings[1].ConnectionString.ShouldBe("From Arbitrary Config");
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(ArgumentException))]
        public void ThrowsIfNullPath()
        {
            var sut = new NetConfigManager();

            sut.LoadFromFile(null);
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(ArgumentException))]
        public void ThrowsIfPathDoesNotExist()
        {
            var sut = new NetConfigManager();

            sut.LoadFromFile(GetPath("FileDoesNotExist.config"));
        }

    }
}
