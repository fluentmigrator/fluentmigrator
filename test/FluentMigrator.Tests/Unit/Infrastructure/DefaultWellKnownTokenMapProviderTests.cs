#region License
//
// Copyright (c) 2007-2024, Fluent Migrator Project
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

using FluentMigrator.Runner.Infrastructure;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Infrastructure
{
    [TestFixture]
    [Category("Infrastructure")]
    [Category("WellKnownTokenMapProvider")]
    public class DefaultWellKnownTokenMapProviderTests
    {
        [Test]
        public void GetWellKnownTokenMapReturnsDefaultSchemaFromConventionSet()
        {
            var conventionSet = ConventionSets.CreateTestSchemaName(null);
            var provider = new DefaultWellKnownTokenMapProvider(conventionSet);

            var tokenMap = provider.GetWellKnownTokenMap();

            tokenMap.ShouldContainKeyAndValue("DefaultSchema", "testdefault");
        }

        [Test]
        public void GetWellKnownTokenMapReturnsNullDefaultSchemaWhenNoDefaultSchemaConfigured()
        {
            var conventionSet = ConventionSets.CreateNoSchemaName(null);
            var provider = new DefaultWellKnownTokenMapProvider(conventionSet);

            var tokenMap = provider.GetWellKnownTokenMap();

            tokenMap.ShouldContainKey("DefaultSchema");
            tokenMap["DefaultSchema"].ShouldBeNull();
        }
    }
}
