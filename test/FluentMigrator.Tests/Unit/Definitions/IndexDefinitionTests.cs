using FluentMigrator.Model;
using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Definitions
{
    [TestFixture]
    public class IndexDefinitionTests
    {
        [Test]
        public void ShouldApplyIndexNameConventionWhenIndexNameIsNull()
        {
            var indexDefinition = new IndexDefinition();
            var conventions = new MigrationConventions { GetIndexName = definition => "IX_Table_Name" };

            indexDefinition.ApplyConventions(conventions);

            Assert.AreEqual("IX_Table_Name", indexDefinition.Name);
        }
    }
}