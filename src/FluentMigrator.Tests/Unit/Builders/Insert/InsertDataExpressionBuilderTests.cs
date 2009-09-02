using FluentMigrator.Builders.Insert;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Builders.Insert
{
	[TestFixture]
	public class InsertDataExpressionBuilderTests
	{
		[Test]
		public void RowsGetSetWhenRowIsCalled()
		{
			var expression = new InsertDataExpression();

			var builder = new InsertDataExpressionBuilder(expression);
			builder
				.Row(new { Data1 = "Row1Data1", Data2 = "Row1Data2" })
				.Row(new { Data1 = "Row2Data1", Data2 = "Row2Data2" });

			expression.Rows.Count.ShouldBe(2);

			expression.Rows[0][0].Key.ShouldBe("Data1");
			expression.Rows[0][0].Value.ShouldBe("Row1Data1");

			expression.Rows[0][1].Key.ShouldBe("Data2");
			expression.Rows[0][1].Value.ShouldBe("Row1Data2");

			expression.Rows[1][0].Key.ShouldBe("Data1");
			expression.Rows[1][0].Value.ShouldBe("Row2Data1");

			expression.Rows[1][1].Key.ShouldBe("Data2");
			expression.Rows[1][1].Value.ShouldBe("Row2Data2");
		}
	}
}