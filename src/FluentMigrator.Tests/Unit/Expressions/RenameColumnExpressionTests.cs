using System;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Tests.Helpers;
using Xunit;

namespace FluentMigrator.Tests.Unit.Expressions
{
	public class RenameColumnExpressionTests
	{
		[Fact]
		public void ErrorIsReturnedWhenOldNameIsNull()
		{
			var expression = new RenameColumnExpression { OldName = null };
			var errors = ValidationHelper.CollectErrors(expression);
			Assert.Contains(ErrorMessages.OldColumnNameCannotBeNullOrEmpty, errors);
		}

		[Fact]
		public void ErrorIsReturnedWhenOldNameIsEmptyString()
		{
			var expression = new RenameColumnExpression { OldName = String.Empty };
			var errors = ValidationHelper.CollectErrors(expression);
			Assert.Contains(ErrorMessages.OldColumnNameCannotBeNullOrEmpty, errors);
		}

		[Fact]
		public void ErrorIsNotReturnedWhenOldNameIsNotNullEmptyString()
		{
			var expression = new RenameColumnExpression { OldName = "Bacon" };
			var errors = ValidationHelper.CollectErrors(expression);
			Assert.DoesNotContain(ErrorMessages.OldColumnNameCannotBeNullOrEmpty, errors);
		}

		[Fact]
		public void ErrorIsReturnedWhenNewNameIsNull()
		{
			var expression = new RenameColumnExpression { NewName = null };
			var errors = ValidationHelper.CollectErrors(expression);
			Assert.Contains(ErrorMessages.NewColumnNameCannotBeNullOrEmpty, errors);
		}

		[Fact]
		public void ErrorIsReturnedWhenNewNameIsEmptyString()
		{
			var expression = new RenameColumnExpression { NewName = String.Empty };
			var errors = ValidationHelper.CollectErrors(expression);
			Assert.Contains(ErrorMessages.NewColumnNameCannotBeNullOrEmpty, errors);
		}

		[Fact]
		public void ErrorIsNotReturnedWhenNewNameIsNotNullOrEmptyString()
		{
			var expression = new RenameColumnExpression { NewName = "Bacon" };
			var errors = ValidationHelper.CollectErrors(expression);
			Assert.DoesNotContain(ErrorMessages.NewColumnNameCannotBeNullOrEmpty, errors);
		}

		[Fact]
		public void ReverseReturnsRenameColumnExpression()
		{
			var expression = new RenameColumnExpression { TableName = "Bacon", OldName = "BaconId", NewName = "ChunkyBaconId" };
			var reverse = expression.Reverse();
			Assert.IsType<RenameColumnExpression>(reverse);
		}

		[Fact]
		public void ReverseSetsTableNameOldNameAndNewNameOnGeneratedExpression()
		{
			var expression = new RenameColumnExpression { TableName = "Bacon", OldName = "BaconId", NewName = "ChunkyBaconId" };
			var reverse = expression.Reverse() as RenameColumnExpression;
			Assert.Equal("Bacon", reverse.TableName);
			Assert.Equal("ChunkyBaconId", reverse.OldName);
			Assert.Equal("BaconId", reverse.NewName);
		}
	}
}