using FluentMigrator.Builders.Execute;
using FluentMigrator.Infrastructure;
using FluentMigrator.Tests.Helpers;
using Xunit;

namespace FluentMigrator.Tests.Unit.Expressions
{
    [TestFixture]
    public class PerformDBOperationExpressionTests
    {
        [Test]
        public void ErrorIsReturnedWhenOperationIsNull()
        {
            var expression = new PerformDBOperationExpression() { Operation = null };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.OperationCannotBeNull);
        }
    }
}