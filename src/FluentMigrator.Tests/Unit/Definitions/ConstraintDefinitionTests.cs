using FluentMigrator.Model;
using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Definitions
{
    [TestFixture]
    public class ConstraintDefinitionTests
    {
        [Test]
        public void WhenDefaultSchemaConventionIsAppliedAndSchemaIsNotSetThenSchemaShouldBeNull()
        {
            var constraintDefinition = new ConstraintDefinition(ConstraintType.PrimaryKey) { ConstraintName = "Test" };

            constraintDefinition.ApplyConventions(new MigrationConventions());

            Assert.That(constraintDefinition.SchemaName, Is.Null);
        }

        [Test]
        public void WhenDefaultSchemaConventionIsAppliedAndSchemaIsSetThenSchemaShouldNotBeChanged()
        {
            var constraintDefinition = new ConstraintDefinition(ConstraintType.PrimaryKey) { ConstraintName = "Test", SchemaName = "testschema" };

            constraintDefinition.ApplyConventions(new MigrationConventions());

            Assert.That(constraintDefinition.SchemaName, Is.EqualTo("testschema"));
        }

        [Test]
        public void WhenDefaultSchemaConventionIsChangedAndSchemaIsNotSetThenSetSchema()
        {
            var constraintDefinition = new ConstraintDefinition(ConstraintType.PrimaryKey) { ConstraintName = "Test" };
            var migrationConventions = new MigrationConventions { GetDefaultSchema = () => "testdefault" };

            constraintDefinition.ApplyConventions(migrationConventions);

            Assert.That(constraintDefinition.SchemaName, Is.EqualTo("testdefault"));
        } 
    }
}