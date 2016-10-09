using System;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;
using FluentMigrator.Tests.Helpers;
using Xunit;

namespace FluentMigrator.Tests.Unit.Expressions
{
    public class CreateSequenceExpressionTests
    {
        [Test]
        public void ErrorIsReturnedWhenSequenceNameIsEmptyString()
        {
            var expression = new CreateSequenceExpression { Sequence = new SequenceDefinition { Name = String.Empty } };

            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.SequenceNameCannotBeNullOrEmpty);
        }

        [Test]
        public void ErrorIsNotReturnedWhenSequenceNameIsSet()
        {
            var expression = new CreateSequenceExpression { Sequence = new SequenceDefinition { Name = "sequence1" } };

            var errors = ValidationHelper.CollectErrors(expression);
            Assert.That(errors.Count, Is.EqualTo(0));
        }
    }
}