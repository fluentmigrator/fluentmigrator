using System;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Tests.Helpers;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Expressions
{
	public class RenameColumnExpressionTests
	{
		[Test]
		public void ErrorIsReturnedWhenOldNameIsNull()
		{
			var expression = new RenameColumnExpression { OldName = null };
			var errors = ValidationHelper.CollectErrors(expression);
			errors.ShouldContain(ErrorMessages.OldColumnNameCannotBeNullOrEmpty);
		}

		[Test]
		public void ErrorIsReturnedWhenOldNameIsEmptyString()
		{
			var expression = new RenameColumnExpression { OldName = String.Empty };
			var errors = ValidationHelper.CollectErrors(expression);
			errors.ShouldContain(ErrorMessages.OldColumnNameCannotBeNullOrEmpty);
		}

		[Test]
		public void ErrorIsNotReturnedWhenOldNameIsNotNullEmptyString()
		{
			var expression = new RenameColumnExpression { OldName = "Bacon" };
			var errors = ValidationHelper.CollectErrors(expression);
			errors.ShouldContain(ErrorMessages.OldColumnNameCannotBeNullOrEmpty);
		}

		[Test]
		public void ErrorIsReturnedWhenNewNameIsNull()
		{
			var expression = new RenameColumnExpression { NewName = null };
			var errors = ValidationHelper.CollectErrors(expression);
			errors.ShouldContain(ErrorMessages.NewColumnNameCannotBeNullOrEmpty);
		}

		[Test]
		public void ErrorIsReturnedWhenNewNameIsEmptyString()
		{
			var expression = new RenameColumnExpression { NewName = String.Empty };
			var errors = ValidationHelper.CollectErrors(expression);
			errors.ShouldContain(ErrorMessages.NewColumnNameCannotBeNullOrEmpty);
		}

		[Test]
		public void ErrorIsNotReturnedWhenNewNameIsNotNullOrEmptyString()
		{
			var expression = new RenameColumnExpression { NewName = "Bacon" };
			var errors = ValidationHelper.CollectErrors(expression);
			errors.ShouldNotContain(ErrorMessages.NewColumnNameCannotBeNullOrEmpty);
		}

		[Test]
		public void ReverseReturnsRenameColumnExpression()
		{
			var expression = new RenameColumnExpression { TableName = "Bacon", OldName = "BaconId", NewName = "ChunkyBaconId" };
			var reverse = expression.Reverse();
			reverse.ShouldBeOfType<RenameColumnExpression>();
		}

		[Test]
		public void ReverseSetsTableNameOldNameAndNewNameOnGeneratedExpression()
		{
			var expression = new RenameColumnExpression { TableName = "Bacon", OldName = "BaconId", NewName = "ChunkyBaconId" };
			var reverse = expression.Reverse() as RenameColumnExpression;

			reverse.TableName.ShouldBe("Bacon");
			reverse.OldName.ShouldBe("ChunkyBaconId");
			reverse.NewName.ShouldBe("BaconId");
		}
	}
}