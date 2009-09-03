using FluentMigrator.Builders.Delete.Column;
using FluentMigrator.Expressions;
using Moq;
using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Builders.Delete
{
	[TestFixture]
	public class DeleteColumnExpressionBuilderTests
	{
		[Test]
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