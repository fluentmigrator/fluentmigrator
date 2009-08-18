using System;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Tests.Helpers;
using Xunit;

namespace FluentMigrator.Tests.Unit.Expressions
{
	public class DeleteColumnExpressionTests
	{
		[Fact]
		public void ErrorIsReturnedWhenTableNameIsNull()
		{
			var expression = new DeleteColumnExpression { TableName = null };
			var errors = ValidationHelper.CollectErrors(expression);
			Assert.Contains(ErrorMessages.TableNameCannotBeNullOrEmpty, errors);
		}

		[Fact]
		public void ErrorIsReturnedWhenTableNameIsEmptyString()
		{
			var expression = new DeleteColumnExpression { TableName = String.Empty };
			var errors = ValidationHelper.CollectErrors(expression);
			Assert.Contains(ErrorMessages.TableNameCannotBeNullOrEmpty, errors);
		}

		[Fact]
		public void ErrorIsNotReturnedWhenTableNameIsNotNullEmptyString()
		{
			var expression = new DeleteColumnExpression { TableName = "Bacon" };
			var errors = ValidationHelper.CollectErrors(expression);
			Assert.DoesNotContain(ErrorMessages.TableNameCannotBeNullOrEmpty, errors);
		}

		[Fact]
		public void ErrorIsReturnedWhenColumnNameIsNull()
		{
			var expression = new DeleteColumnExpression { ColumnName = null };
			var errors = ValidationHelper.CollectErrors(expression);
			Assert.Contains(ErrorMessages.ColumnNameCannotBeNullOrEmpty, errors);
		}

		[Fact]
		public void ErrorIsReturnedWhenColumnNameIsEmptyString()
		{
			var expression = new DeleteColumnExpression { ColumnName = String.Empty };
			var errors = ValidationHelper.CollectErrors(expression);
			Assert.Contains(ErrorMessages.ColumnNameCannotBeNullOrEmpty, errors);
		}

		[Fact]
		public void ErrorIsNotReturnedWhenColumnNameIsNotNullEmptyString()
		{
			var expression = new DeleteColumnExpression { ColumnName = "Bacon" };
			var errors = ValidationHelper.CollectErrors(expression);
			Assert.DoesNotContain(ErrorMessages.ColumnNameCannotBeNullOrEmpty, errors);
		}

		[Fact]
		public void ReverseThrowsException()
		{
			var expression = new DeleteColumnExpression();
			Assert.Throws<NotSupportedException>(() => expression.Reverse());
		}
	}
}