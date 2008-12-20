using System;
using FluentMigrator.Builders.Rename.Table;
using FluentMigrator.Expressions;
using Moq;
using Xunit;

namespace FluentMigrator.Tests.Unit.Builders.Rename
{
	public class RenameTableExpressionBuilderTests
	{
		[Fact]
		public void CallingToSetsNewName()
		{
			var expressionMock = new Mock<RenameTableExpression>();
			expressionMock.ExpectSet(x => x.NewName, "Bacon").AtMostOnce();

			var builder = new RenameTableExpressionBuilder(expressionMock.Object);
			builder.To("Bacon");

			expressionMock.VerifyAll();
		}
	}
}