using System.Linq;
using FluentMigrator.Model;
using NUnit.Framework;
using NUnit.Should;
using FluentMigrator.Expressions;
using Moq;
using FluentMigrator.Builders.Delete;

namespace FluentMigrator.Tests.Unit.Builders.Delete
{
    [TestFixture]
    public class DeleteDataExpressionTests
    {

        [Test]
        public void CallingRowAddAColumn()
        {
            var expressionMock = new Mock<DeleteDataExpression>();

            var builder = new DeleteDataExpressionBuilder(expressionMock.Object);
            var data = new { TestColumn = "TestValue" };
            builder.Row(data);

            var result = expressionMock.Object;
            IDataDefinition rowobject = result.Rows.First();

            Assert.IsInstanceOf<ReflectedDataDefinition>(rowobject);
            Assert.AreEqual(data, ((ReflectedDataDefinition)rowobject).Data);
        }

        [Test]
        public void CallingRowTwiceAddTwoColumns()
        {
            var expressionMock = new Mock<DeleteDataExpression>();

            var builder = new DeleteDataExpressionBuilder(expressionMock.Object);
            builder.Row(new { TestColumn = "TestValue" });
            builder.Row(new { TestColumn2 = "TestValue2" });

            var result = expressionMock.Object;

            Assert.AreEqual(2, result.Rows.Count);
        }

        [Test]
        public void CallingAllRowsSetsAllRowsToTrue()
        {
            var expressionMock = new Mock<DeleteDataExpression>();
            expressionMock.VerifySet(x => x.IsAllRows = true, Times.AtMostOnce(), "IsAllRows property not set");

            var builder = new DeleteDataExpressionBuilder(expressionMock.Object);
            builder.AllRows();

            expressionMock.VerifyAll();
        }

        [Test]
        public void CallingInSchemaSetSchemaName()
        {
            var expressionMock = new Mock<DeleteDataExpression>();
            expressionMock.VerifySet(x => x.SchemaName = "TestSchema", Times.AtMostOnce(), "SchemaName property not set");

            var builder = new DeleteDataExpressionBuilder(expressionMock.Object);
            builder.InSchema("TestSchema");

            expressionMock.VerifyAll();
        }

        [Test]
        public void CallingIsNullAddsANullColumn()
        {
            var expressionMock = new Mock<DeleteDataExpression>();

            var builder = new DeleteDataExpressionBuilder(expressionMock.Object);
            builder.IsNull("TestColumn");

            var result = expressionMock.Object;
            IDataDefinition rowobject = result.Rows.First();

            Assert.IsInstanceOf<ExplicitDataDefinition>(rowobject);
            ExplicitDataDefinition rowDefinition = (ExplicitDataDefinition)rowobject;
            rowDefinition.Data.First().ColumnName.ShouldBe("TestColumn");
            rowDefinition.Data.First().Value.ShouldBeNull();

        }
    }
}
