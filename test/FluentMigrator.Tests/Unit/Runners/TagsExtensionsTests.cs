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

using FluentMigrator.Runner.Extensions;

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Runners
{
    [TestFixture]
    public class TagsExtensionsTests
    {
        [Test]
        public void ToTags_WithOneTag_ShouldReturnListWithOneTag()
        {
            List<string> tags = "Test".ToTags();

            Assert.That(tags[0], Is.EqualTo("Test"));
        }

        [Test]
        public void ToTags_WithNullString_ShouldReturnEmptyList()
        {
            var tags = ((string) null).ToTags();

            Assert.That(tags, Is.Not.Null);
        }

        [Test]
        public void ToTags_WithThreeTags_ShouldReturnListWithThreeTags()
        {
            List<string> tags = "Dev,Test,Prod".ToTags();

            var expectedTags = new[] { "Dev", "Test", "Prod" };
            CollectionAssert.AreEquivalent(expectedTags, tags);
        }
    }
}
