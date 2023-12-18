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

using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.Snowflake
{
    public static class SnowflakeTestExtensions
    {
        public static void ShouldBe(this string actual, string expected, bool quotingEnabled)
        {
            expected = quotingEnabled ? expected : expected.Replace(@"""", string.Empty);
            actual.ShouldBe(expected);
        }

        public static void ShouldBe(this IEnumerable<string> actual, IEnumerable<string> expected, bool quotingEnabled)
        {
            expected = expected.Select(e => quotingEnabled ? e : e.Replace(@"""", string.Empty));
            Assert.That(actual, Is.EqualTo(expected).AsCollection);
        }
    }
}
