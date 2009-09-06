using System;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Tests.Helpers;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Expressions
{
	[TestFixture]
	public class RenameTableExpressionTests
	{
		[Test]
		public void ErrorIsReturnedWhenOldNameIsNull()
		{
			var expression = new RenameTableExpression { OldName = null };
			var errors = ValidationHelper.CollectErrors(expression);
			errors.ShouldContain(ErrorMessages.OldTableNameCannotBeNullOrEmpty);
		}

		[Test]
		public void ErrorIsReturnedWhenOldNameIsEmptyString()
		{
			var expression = new RenameTableExpression { OldName = String.Empty };
			var errors = ValidationHelper.CollectErrors(expression);
			errors.ShouldContain(ErrorMessages.OldTableNameCannotBeNullOrEmpty);
		}

		[Test]
		public void ErrorIsNotReturnedWhenOldNameIsNotNullEmptyString()
		{
			var expression = new RenameTableExpression { OldName = "Bacon" };
			var errors = ValidationHelper.CollectErrors(expression);
			errors.ShouldNotContain(ErrorMessages.OldTableNameCannotBeNullOrEmpty);
		}

		[Test]
		public void ErrorIsReturnedWhenNewNameIsNull()
		{
			var expression = new RenameTableExpression { NewName = null };
			var errors = ValidationHelper.CollectErrors(expression);
			errors.ShouldContain(ErrorMessages.NewTableNameCannotBeNullOrEmpty);
		}

		[Test]
		public void ErrorIsReturnedWhenNewNameIsEmptyString()
		{
			var expression = new RenameTableExpression { NewName = String.Empty };
			var errors = ValidationHelper.CollectErrors(expression);
			errors.ShouldContain(ErrorMessages.NewTableNameCannotBeNullOrEmpty);
		}

		[Test]
		public void ErrorIsNotReturnedWhenNewNameIsNotNullOrEmptyString()
		{
			var expression = new RenameTableExpression { NewName = "Bacon" };
			var errors = ValidationHelper.CollectErrors(expression);
			errors.ShouldNotContain(ErrorMessages.NewTableNameCannotBeNullOrEmpty);
		}

		[Test]
		public void ReverseReturnsRenameTableExpression()
		{
			var expression = new RenameTableExpression { OldName = "Bacon", NewName = "ChunkyBacon" };
			var reverse = expression.Reverse();
			reverse.ShouldBeOfType<RenameTableExpression>();
		}

		[Test]
		public void ReverseSetsOldNameAndNewNameOnGeneratedExpression()
		{
			var expression = new RenameTableExpression { OldName = "Bacon", NewName = "ChunkyBacon" };
			var reverse = expression.Reverse() as RenameTableExpression;
			reverse.OldName.ShouldBe("ChunkyBacon");
			reverse.NewName.ShouldBe("Bacon");
		}

		[Test]
		public void ToStringIsDescriptive()
		{
			var expression = new RenameTableExpression { OldName = "Bacon", NewName = "ChunkyBacon" };
			expression.ToString().ShouldBe("RenameTable Bacon ChunkyBacon");		
		}
	}
}