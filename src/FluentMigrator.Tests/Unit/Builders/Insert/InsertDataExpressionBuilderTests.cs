using FluentMigrator.Builders.Insert;
using Moq;
using Xunit;

namespace FluentMigrator.Tests.Unit.Builders.Insert
{
    public class InsertDataExpressionBuilderTests
    {
        [Fact]
        public void RowsGetSetWhenRowIsCalled()
        {
            var expression = new InsertDataExpression();
           
            var builder = new InsertDataExpressionBuilder(expression);
            builder
                .Row(new { Data1 = "Row1Data1", Data2 = "Row1Data2" })
                .Row(new { Data1 = "Row2Data1", Data2 = "Row2Data2" });

            Assert.Equal(2, expression.Rows.Count);

            Assert.Equal("Data1", expression.Rows[0][0].Key);
            Assert.Equal("Row1Data1", expression.Rows[0][0].Value);

            Assert.Equal("Data2", expression.Rows[0][1].Key);
            Assert.Equal("Row1Data2", expression.Rows[0][1].Value);

            Assert.Equal("Data1", expression.Rows[1][0].Key);
            Assert.Equal("Row2Data1", expression.Rows[1][0].Value);

            Assert.Equal("Data2", expression.Rows[1][1].Key);
            Assert.Equal("Row2Data2", expression.Rows[1][1].Value);
        }
    }
}