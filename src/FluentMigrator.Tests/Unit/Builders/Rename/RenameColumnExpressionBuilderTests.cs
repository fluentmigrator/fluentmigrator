using FluentMigrator.Builders.Rename.Column;
using FluentMigrator.Expressions;
using Moq;
using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Builders.Rename
{
	[TestFixture]
	public class RenameColumnExpressionBuilderTests
	{
		[Test]
		public void CallingToSetsNewName()
		{
			var expressionMock = new Mock<RenameColumnExpression>();
			expressionMock.SetupSet(x => x.NewName = "Bacon").AtMostOnce();

			var builder = new RenameColumnExpressionBuilder(expressionMock.Object);
			builder.To("Bacon");

			expressionMock.VerifyAll();
		}
	}
}