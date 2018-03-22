using System;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;
using FluentMigrator.Tests.Helpers;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Expressions
{
    [TestFixture]
    public class DeleteSequenceExpressionTests
    {
        [Test]
        public void ErrorIsReturnedWhenSequenceNameIsEmptyString()
        {
            var expression = new DeleteSequenceExpression { SequenceName = String.Empty };

            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.SequenceNameCannotBeNullOrEmpty);
        }

        [Test]
        public void ErrorIsNotReturnedWhenSequenceNameIsSet()
        {
            var expression = new DeleteSequenceExpression { SequenceName = "sequence1" };

            var errors = ValidationHelper.CollectErrors(expression);
            Assert.That(errors.Count, Is.EqualTo(0));
        }

        [Test]
        public void WhenDefaultSchemaConventionIsAppliedAndSchemaIsNotSetThenSchemaShouldBeNull()
        {
            var expression = new DeleteSequenceExpression { SequenceName = "sequence1" };

            expression.ApplyConventions(new MigrationConventions());

            Assert.That(expression.SchemaName, Is.Null);
        }

        [Test]
        public void WhenDefaultSchemaConventionIsAppliedAndSchemaIsSetThenSchemaShouldNotBeChanged()
        {
            var expression = new DeleteSequenceExpression { SchemaName = "testschema", SequenceName = "sequence1" };

            expression.ApplyConventions(new MigrationConventions());

            Assert.That(expression.SchemaName, Is.EqualTo("testschema"));
        }

        [Test]
        public void WhenDefaultSchemaConventionIsChangedAndSchemaIsNotSetThenSetSchema()
        {
            var expression = new DeleteSequenceExpression { SequenceName = "sequence1" };
            var migrationConventions = new MigrationConventions { GetDefaultSchema = () => "testdefault" };

            expression.ApplyConventions(migrationConventions);

            Assert.That(expression.SchemaName, Is.EqualTo("testdefault"));
        }
    }
}