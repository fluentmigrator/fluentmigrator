using System;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;
using FluentMigrator.Tests.Helpers;
using Xunit;

namespace FluentMigrator.Tests.Unit.Expressions
{
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
    }
}