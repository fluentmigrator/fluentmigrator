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
            string nullString = null;
            List<string> tags = nullString.ToTags();

            Assert.That(tags, Is.Not.Null);
        }

        [Test]
        public void ToTags_WithThreeTags_ShouldReturnListWithThreeTags()
        {
            List<string> tags = "Dev,Test,Prod".ToTags();

            var expectedTags = new string[] { "Dev", "Test", "Prod" };
            CollectionAssert.AreEquivalent(expectedTags, tags);  
        }
    }
}