using System;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using Xunit;

namespace FluentMigrator.Tests.Expressions
{
	public class DeleteTableExpressionTests
	{
		[Fact]
		public void ErrorIsReturnedWhenTableNameIsNull()
		{
			var expression = new DeleteTableExpression { TableName = null };
			var errors = ValidationHelper.CollectErrors(expression);
			Assert.Contains(ErrorMessages.TableNameCannotBeNullOrEmpty, errors);
		}

		[Fact]
		public void ErrorIsReturnedWhenTableNameIsEmptyString()
		{
			var expression = new DeleteTableExpression { TableName = String.Empty };
			var errors = ValidationHelper.CollectErrors(expression);
			Assert.Contains(ErrorMessages.TableNameCannotBeNullOrEmpty, errors);
		}

		[Fact]
		public void ErrorIsNotReturnedWhenTableNameIsNotNullEmptyString()
		{
			var expression = new DeleteTableExpression { TableName = "Bacon" };
			var errors = ValidationHelper.CollectErrors(expression);
			Assert.DoesNotContain(ErrorMessages.TableNameCannotBeNullOrEmpty, errors);
		}
	}
}