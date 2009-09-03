using System;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Tests.Helpers;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Expressions
{
	[TestFixture]
	public class DeleteTableExpressionTests
	{
		[Test]
		public void ErrorIsReturnedWhenTableNameIsNull()
		{
			var expression = new DeleteTableExpression { TableName = null };
			var errors = ValidationHelper.CollectErrors(expression);
			errors.ShouldContain(ErrorMessages.TableNameCannotBeNullOrEmpty);
		}

		[Test]
		public void ErrorIsReturnedWhenTableNameIsEmptyString()
		{
			var expression = new DeleteTableExpression { TableName = String.Empty };
			var errors = ValidationHelper.CollectErrors(expression);
			errors.ShouldContain(ErrorMessages.TableNameCannotBeNullOrEmpty);
		}

		[Test]
		public void ErrorIsNotReturnedWhenTableNameIsNotNullEmptyString()
		{
			var expression = new DeleteTableExpression { TableName = "Bacon" };
			var errors = ValidationHelper.CollectErrors(expression);
			errors.ShouldNotContain(ErrorMessages.TableNameCannotBeNullOrEmpty);
		}

		[Test]
		[ExpectedException(typeof(NotSupportedException))]
		public void ReverseThrowsException()
		{
			new DeleteColumnExpression().Reverse();
		}
	}
}