using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using FluentMigrator.Runner.Initialization;
using System.Configuration;

namespace FluentMigrator.Tests.Unit.Initialization
{
    [Trait("BrokenRuntimes", "Mono")]
    public class NetConfigManagerTests
    {
        private static string GetPath(string relative)
        {
            return string.Format(@"Unit\Initialization\Fixtures\{0}", relative);
        }

        [Fact]
        [ExpectedException(ExpectedException = typeof(ArgumentException))]
        public void ThrowsIfNullPath()
        {
            var sut = new NetConfigManager();

            sut.LoadFromFile(null);
        }

        [Fact]
        [ExpectedException(ExpectedException = typeof(ArgumentException))]
        public void ThrowsIfPathDoesNotExist()
        {
            var sut = new NetConfigManager();

            sut.LoadFromFile(GetPath("FileDoesNotExist.config"));
        }

        [Fact]
        public void LoadsConfigurationFromConfigFile()
        {
            var sut = new NetConfigManager();

            Configuration config = sut.LoadFromFile(GetPath("WithConnectionString.config"));

            config.ConnectionStrings.ConnectionStrings[0].ConnectionString.ShouldBe("From Arbitrary Config");
        }

        [Fact]
        public void LoadsConfigurationFromExeConfigFile()
        {
            var sut = new NetConfigManager();

            Configuration config = sut.LoadFromFile(GetPath("WithConnectionString.exe.config"));

            config.ConnectionStrings.ConnectionStrings[0].ConnectionString.ShouldBe("From App Config");
        }

        [Fact]
        public void AddsConfigExtensionWhenNoExtensionIsSpecified()
        {
            var sut = new NetConfigManager();

            Configuration config = sut.LoadFromFile(GetPath("WithConnectionString.exe"));

            config.ConnectionStrings.ConnectionStrings[0].ConnectionString.ShouldBe("From App Config");
        }

        [Fact]
        public void LoadsConfigurationFromMachineConfigFile()
        {
            var sut = new NetConfigManager();

            Configuration config = sut.LoadFromMachineConfiguration();

            config.EvaluationContext.IsMachineLevel.ShouldBeTrue();
        }
    }
}
