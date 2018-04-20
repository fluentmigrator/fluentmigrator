#region License
//
// Copyright (c) 2007-2018, Sean Chambers <schambers80@gmail.com>
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

using System.Linq;
using FluentMigrator.Runner.Versioning;
using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Versioning
{
    [TestFixture]
    public class VersionInfoTests
    {
        private VersionInfo _versionInfo;

        [SetUp]
        public void SetUp()
        {
            _versionInfo = new VersionInfo();
        }

        [Test]
        public void CanAddAppliedMigration()
        {
            _versionInfo.AddAppliedMigration(200909060953);
            _versionInfo.HasAppliedMigration(200909060953).ShouldBeTrue();
        }

        [Test]
        public void CanGetLatestMigration()
        {
            _versionInfo.AddAppliedMigration(200909060953);
            _versionInfo.AddAppliedMigration(200909060935);
            _versionInfo.Latest().ShouldBe(200909060953);
        }

        [Test]
        public void CanGetAppliedMigrationsLatestFirst()
        {
            _versionInfo.AddAppliedMigration(200909060953);
            _versionInfo.AddAppliedMigration(200909060935);
            var applied = _versionInfo.AppliedMigrations().ToList();
            applied[0].ShouldBe(200909060953);
            applied[1].ShouldBe(200909060935);
        }
    }
}
