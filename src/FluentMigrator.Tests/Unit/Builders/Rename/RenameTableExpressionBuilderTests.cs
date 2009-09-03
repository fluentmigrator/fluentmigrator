using FluentMigrator.Builders.Rename.Table;
using FluentMigrator.Expressions;
using Moq;
using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Builders.Rename
{
	[TestFixture]
	public class RenameTableExpressionBuilderTests
	{
		[Test]
		public void CallingToSetsNewName()
		{
			var expressionMock = new Mock<RenameTableExpression>();
			expressionMock.SetupSet(x => x.NewName = "Bacon").AtMostOnce();

			var builder = new RenameTableExpressionBuilder(expressionMock.Object);
			builder.To("Bacon");

			expressionMock.VerifyAll();
		}
	}
}