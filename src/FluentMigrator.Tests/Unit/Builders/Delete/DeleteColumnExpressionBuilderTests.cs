using System;
using FluentMigrator.Builders.Delete.Column;
using FluentMigrator.Expressions;
using Moq;
using Xunit;

namespace FluentMigrator.Tests.Unit.Builders.Delete
{
	public class DeleteColumnExpressionBuilderTests
	{
		[Fact]
		public void CallingFromTableSetsTableName()
		{
			var expressionMock = new Mock<DeleteColumnExpression>();
			expressionMock.SetupSet(x => x.TableName = "Bacon").AtMostOnce();

			var builder = new DeleteColumnExpressionBuilder(expressionMock.Object);
			builder.FromTable("Bacon");

			expressionMock.VerifyAll();
		}
	}
}