using System.Collections.Generic;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Expressions
{
	[TestFixture]
	public class CreateTableExpressionTests
	{
		[Test]
		public void ToStringIsDescriptive()
		{
			new CreateTableExpression
				{
					TableName = "Table"
				}.ToString().ShouldBe("CreateTable Table");
		}
	}
}
