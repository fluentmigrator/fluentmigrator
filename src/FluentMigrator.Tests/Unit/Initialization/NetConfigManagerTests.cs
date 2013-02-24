using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Should;
using FluentMigrator.Runner.Initialization;
using System.Configuration;

namespace FluentMigrator.Tests.Unit.Initialization
{
    [TestFixture]
    [Category("NotWorkingOnMono")]
    public class NetConfigManagerTests
    {
        private static string GetPath(string relative)
        {
            return string.Format(@"Unit\Initialization\Fixtures\{0}", relative);
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

        [Test]
        public void LoadsConfigurationFromConfigFile()
        {
            var sut = new NetConfigManager();

            Configuration config = sut.LoadFromFile(GetPath("WithConnectionString.config"));

            config.ConnectionStrings.ConnectionStrings[0].ConnectionString.ShouldBe("From Arbitrary Config");
        }

        [Test]
        public void LoadsConfigurationFromExeConfigFile()
        {
            var sut = new NetConfigManager();

            Configuration config = sut.LoadFromFile(GetPath("WithConnectionString.exe.config"));

            config.ConnectionStrings.ConnectionStrings[0].ConnectionString.ShouldBe("From App Config");
        }

        [Test]
        public void AddsConfigExtensionWhenNoExtensionIsSpecified()
        {
            var sut = new NetConfigManager();

            Configuration config = sut.LoadFromFile(GetPath("WithConnectionString.exe"));

            config.ConnectionStrings.ConnectionStrings[0].ConnectionString.ShouldBe("From App Config");
        }

        [Test]
        public void LoadsConfigurationFromMachineConfigFile()
        {
            var sut = new NetConfigManager();

            Configuration config = sut.LoadFromMachineConfiguration();

            config.EvaluationContext.IsMachineLevel.ShouldBeTrue();
        }
    }
}
