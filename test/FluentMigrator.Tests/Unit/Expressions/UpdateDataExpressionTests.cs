using System.Collections.Generic;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Tests.Helpers;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Expressions
{
    [TestFixture]
    public class UpdateDataExpressionTests {
        private UpdateDataExpression expression;

        [SetUp]
        public void Initialize() 
        {
            expression =
                new UpdateDataExpression() 
                {
                    TableName = "ExampleTable",
                    Set = new List<KeyValuePair<string, object>> 
                    {
                        new KeyValuePair<string, object>("Column", "value")
                    },
                    IsAllRows = false
                };
        }

        [Test]
        public void NullUpdateTargetCausesErrorMessage() 
        {
            // null is the default value, but it might not always be, so I'm codifying it here anyway
            expression.Where = null;

            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.UpdateDataExpressionMustSpecifyWhereClauseOrAllRows);
        }

        [Test]
        public void EmptyUpdateTargetCausesErrorMessage() 
        {
            // The same should be true for an empty list
            expression.Where = new List<KeyValuePair<string, object>>();

            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.UpdateDataExpressionMustSpecifyWhereClauseOrAllRows);
        }

        [Test]
        public void DoesNotRequireWhereConditionWhenIsAllRowsIsSet() 
        {
            expression.IsAllRows = true;

            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldNotContain(ErrorMessages.UpdateDataExpressionMustSpecifyWhereClauseOrAllRows);
        }

        [Test]
        public void DoesNotAllowWhereConditionWhenIsAllRowsIsSet() 
        {
            expression.IsAllRows = true;
            expression.Where = new List<KeyValuePair<string, object>> {new KeyValuePair<string, object>("key", "value")};

            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.UpdateDataExpressionMustNotSpecifyBothWhereClauseAndAllRows);
        }
    }
}
