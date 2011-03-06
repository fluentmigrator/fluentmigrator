using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using FluentMigrator.Expressions;
using FluentMigrator.Tests.Helpers;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Expressions
{
    [TestFixture]
    public class CreateConstraintExpressionTests
    {
        [Test]
        public void ErrorIsReturnedWhenTableNameisEmpty()
        {
            CreateConstraintExpression expression = new CreateConstraintExpression(Model.ConstraintType.PrimaryKey);
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain("Table name cannot be empty");
        }

        [Test]
        public void ErrorIsReturnedWhenNoColumnsAreSpecified()
        {
            CreateConstraintExpression expression = new CreateConstraintExpression(Model.ConstraintType.PrimaryKey);
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain("At least one column must be specified");
        }
    }
}
