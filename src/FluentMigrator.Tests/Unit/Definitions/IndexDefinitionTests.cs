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

        [Test]
        public void WhenDefaultSchemaConventionIsAppliedAndSchemaIsNotSetThenSchemaShouldBeNull()
        {
            var indexDefinition = new IndexDefinition { Name = "Test" };

            indexDefinition.ApplyConventions(new MigrationConventions());

            Assert.That(indexDefinition.SchemaName, Is.Null);
        }

        [Test]
        public void WhenDefaultSchemaConventionIsAppliedAndSchemaIsSetThenSchemaShouldNotBeChanged()
        {
            var indexDefinition = new IndexDefinition{ Name = "Test", SchemaName = "testschema" };

            indexDefinition.ApplyConventions(new MigrationConventions());

            Assert.That(indexDefinition.SchemaName, Is.EqualTo("testschema"));
        }

        [Test]
        public void WhenDefaultSchemaConventionIsChangedAndSchemaIsNotSetThenSetSchema()
        {
            var indexDefinition = new IndexDefinition { Name = "Test" };
            var migrationConventions = new MigrationConventions {GetDefaultSchema = () => "testdefault"};
            indexDefinition.ApplyConventions(migrationConventions);

            Assert.That(indexDefinition.SchemaName, Is.EqualTo("testdefault"));
        }
    }
}