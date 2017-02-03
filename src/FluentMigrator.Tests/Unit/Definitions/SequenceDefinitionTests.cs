using FluentMigrator.Model;
using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Definitions
{
    [TestFixture]
    public class SequenceDefinitionTests
    {
        [Test]
        public void WhenDefaultSchemaConventionIsAppliedAndSchemaIsNotSetThenSchemaShouldBeNull()
        {
            var sequenceDefinition = new SequenceDefinition { Name = "Test" };

            sequenceDefinition.ApplyConventions(new MigrationConventions());

            Assert.That(sequenceDefinition.SchemaName, Is.Null);
        }

        [Test]
        public void WhenDefaultSchemaConventionIsAppliedAndSchemaIsSetThenSchemaShouldNotBeChanged()
        {
            var sequenceDefinition = new SequenceDefinition { Name = "Test", SchemaName = "testschema" };

            sequenceDefinition.ApplyConventions(new MigrationConventions());

            Assert.That(sequenceDefinition.SchemaName, Is.EqualTo("testschema"));
        }

        [Test]
        public void WhenDefaultSchemaConventionIsChangedAndSchemaIsNotSetThenSetSchema()
        {
            var sequenceDefinition = new SequenceDefinition { Name = "Test" };
            var migrationConventions = new MigrationConventions { GetDefaultSchema = () => "testdefault" };

            sequenceDefinition.ApplyConventions(migrationConventions);

            Assert.That(sequenceDefinition.SchemaName, Is.EqualTo("testdefault"));
        } 
    }
}