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
using System.Configuration;
using System.IO;

using FluentMigrator.Runner.Initialization.NetFramework;

using NUnit.Framework;

using Shouldly;

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
#endif