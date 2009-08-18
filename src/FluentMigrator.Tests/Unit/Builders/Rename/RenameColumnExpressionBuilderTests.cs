using System;
using FluentMigrator.Builders.Rename.Column;
using FluentMigrator.Expressions;
using Moq;
using Xunit;

namespace FluentMigrator.Tests.Unit.Builders.Rename
{
	public class RenameColumnExpressionBuilderTests
	{
		[Fact]
		public void CallingToSetsNewName()
		{
			var expressionMock = new Mock<RenameColumnExpression>();
			expressionMock.ExpectSet(x => x.NewName, "Bacon").AtMostOnce();

			var builder = new RenameColumnExpressionBuilder(expressionMock.Object);
			builder.To("Bacon");

			expressionMock.VerifyAll();
		}
	}
}