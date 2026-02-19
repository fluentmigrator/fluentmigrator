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

using FluentMigrator.Generation;
using FluentMigrator.Runner.Generators.Hana;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.Hana
{
    [TestFixture]
    [Category("Generator")]
    [Category("Quoter")]
    [Category("Hana")]
    public class HanaQuoterTest
    {
        private IQuoter _quoter = default(HanaQuoter);

        [SetUp]
        public void SetUp()
        {
            _quoter = new HanaQuoter();
        }

        [TestCase(SystemMethods.CurrentDateTime, "CURRENT_TIMESTAMP")]
        [TestCase(SystemMethods.CurrentUTCDateTime, "CURRENT_UTCTIMESTAMP")]
        public void TestFormatSystemMethods(SystemMethods systemMethod, string expectedFormat)
        {
            var result = _quoter.QuoteValue(systemMethod);
            Assert.That(result, Is.EqualTo(expectedFormat));
        }

        [TestCase(true, "TRUE")]
        [TestCase(false, "FALSE")]
        public void BoolIsFormattedAsTrueAndFalse(bool value, string expectedBoolFormat)
        {
            _quoter.QuoteValue(value)
                .ShouldBe(expectedBoolFormat);
        }
    }
}
