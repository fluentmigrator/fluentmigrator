using FluentMigrator.Model;
using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Definitions
{
    [TestFixture]
    public class TableDefinitionTests
    {
        [Test]
        public void WhenDefaultSchemaConventionIsAppliedAndSchemaIsNotSetThenSchemaShouldBeNull()
        {
            var table = new TableDefinition {Name = "Test"};

            table.ApplyConventions(new MigrationConventions());

            Assert.That(table.SchemaName, Is.Null);
        }

        [Test]
        public void WhenDefaultSchemaConventionIsAppliedAndSchemaIsSetThenSchemaShouldNotBeChanged()
        {
            var table = new TableDefinition { Name = "Test", SchemaName = "testschema"};

            table.ApplyConventions(new MigrationConventions());

            Assert.That(table.SchemaName, Is.EqualTo("testschema"));
        }

        [Test]
        public void WhenDefaultSchemaConventionIsChangedAndSchemaIsNotSetThenSetSchema()
        {
            var table = new TableDefinition { Name = "Test" };
            var migrationConventions = new MigrationConventions {GetDefaultSchema = () => "testdefault"};

            table.ApplyConventions(migrationConventions);

            Assert.That(table.SchemaName, Is.EqualTo("testdefault"));
        }
    }
}