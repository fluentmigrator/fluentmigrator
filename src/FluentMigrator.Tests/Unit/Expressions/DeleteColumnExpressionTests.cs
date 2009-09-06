using System;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Tests.Helpers;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Expressions
{
	[TestFixture]
	public class DeleteColumnExpressionTests
	{
		[Test]
		public void ErrorIsReturnedWhenTableNameIsNull()
		{
			var expression = new DeleteColumnExpression { TableName = null };
			var errors = ValidationHelper.CollectErrors(expression);
			errors.ShouldContain(ErrorMessages.TableNameCannotBeNullOrEmpty);
		}

		[Test]
		public void ErrorIsReturnedWhenTableNameIsEmptyString()
		{
			var expression = new DeleteColumnExpression { TableName = String.Empty };
			var errors = ValidationHelper.CollectErrors(expression);
			errors.ShouldContain(ErrorMessages.TableNameCannotBeNullOrEmpty);
		}

		[Test]
		public void ErrorIsNotReturnedWhenTableNameIsNotNullEmptyString()
		{
			var expression = new DeleteColumnExpression { TableName = "Bacon" };
			var errors = ValidationHelper.CollectErrors(expression);
			errors.ShouldNotContain(ErrorMessages.TableNameCannotBeNullOrEmpty);
		}

		[Test]
		public void ErrorIsReturnedWhenColumnNameIsNull()
		{
			var expression = new DeleteColumnExpression { ColumnName = null };
			var errors = ValidationHelper.CollectErrors(expression);
			errors.ShouldContain(ErrorMessages.ColumnNameCannotBeNullOrEmpty);
		}

		[Test]
		public void ErrorIsReturnedWhenColumnNameIsEmptyString()
		{
			var expression = new DeleteColumnExpression { ColumnName = String.Empty };
			var errors = ValidationHelper.CollectErrors(expression);
			errors.ShouldContain(ErrorMessages.ColumnNameCannotBeNullOrEmpty);
		}

		[Test]
		public void ErrorIsNotReturnedWhenColumnNameIsNotNullEmptyString()
		{
			var expression = new DeleteColumnExpression { ColumnName = "Bacon" };
			var errors = ValidationHelper.CollectErrors(expression);
			errors.ShouldNotContain(ErrorMessages.ColumnNameCannotBeNullOrEmpty);
		}

		[Test]
		[ExpectedException(typeof(NotSupportedException))]
		public void ReverseThrowsException()
		{
			new DeleteColumnExpression().Reverse();
		}

		[Test]
		public void ToStringIsDescriptive()
		{
			var expression = new DeleteColumnExpression { TableName = "Test", ColumnName = "Bacon" };
			expression.ToString().ShouldBe("DeleteColumn Test Bacon");
		}
	}
}