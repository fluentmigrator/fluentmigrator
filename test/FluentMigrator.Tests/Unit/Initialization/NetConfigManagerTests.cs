using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Should;
using FluentMigrator.Runner.Initialization;
using System.Configuration;
using System.IO;

using FluentMigrator.Runner.Initialization.NetFramework;

namespace FluentMigrator.Tests.Unit.Initialization
{
    [TestFixture]
    public class NetConfigManagerTests
    {
        private static string GetPath(string relative)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Unit", "Initialization", "Fixtures", relative);
        }

        [Test]
        public void ThrowsIfNullPath()
        {
            var sut = new NetConfigManager();

            Assert.Throws<ArgumentException>(() => sut.LoadFromFile(null));
        }

        [Test]
        public void ThrowsIfPathDoesNotExist()
        {
            var sut = new NetConfigManager();

            Assert.Throws<ArgumentException>(() => sut.LoadFromFile(GetPath("FileDoesNotExist.config")));
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
        [Category("NotWorkingOnMono")]
        public void LoadsConfigurationFromMachineConfigFile()
        {
            var sut = new NetConfigManager();

            Configuration config = sut.LoadFromMachineConfiguration();

            config.EvaluationContext.IsMachineLevel.ShouldBeTrue();
        }
    }
}
