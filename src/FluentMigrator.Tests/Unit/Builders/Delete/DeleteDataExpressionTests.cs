﻿using System.Linq;
using Xunit;
using FluentMigrator.Expressions;
using Moq;
using FluentMigrator.Builders.Delete;

namespace FluentMigrator.Tests.Unit.Builders.Delete
{
    public class DeleteDataExpressionTests
    {

        [Fact]
        public void CallingRowAddAColumn()
        {
            var expressionMock = new Mock<DeleteDataExpression>();

            var builder = new DeleteDataExpressionBuilder(expressionMock.Object);
            builder.Row(new { TestColumn = "TestValue" });

            var result = expressionMock.Object;
            var rowobject = result.Rows.First().First();
            rowobject.Key.ShouldBe("TestColumn");
            rowobject.Value.ShouldBe("TestValue");
        }

        [Fact]
        public void CallingRowTwiceAddTwoColumns()
        {
            var expressionMock = new Mock<DeleteDataExpression>();

            var builder = new DeleteDataExpressionBuilder(expressionMock.Object);
            builder.Row(new { TestColumn = "TestValue" });
            builder.Row(new { TestColumn2 = "TestValue2" });

            var result = expressionMock.Object;
            var rowobject = result.Rows[0];
            rowobject[0].Key.ShouldBe("TestColumn");
            rowobject[0].Value.ShouldBe("TestValue");

            rowobject = result.Rows[1];
            rowobject[0].Key.ShouldBe("TestColumn2");
            rowobject[0].Value.ShouldBe("TestValue2");
        }

        [Fact]
        public void CallingAllRowsSetsAllRowsToTrue()
        {
            var expressionMock = new Mock<DeleteDataExpression>();
            expressionMock.VerifySet(x => x.IsAllRows = true, Times.AtMostOnce(), "IsAllRows property not set");

            var builder = new DeleteDataExpressionBuilder(expressionMock.Object);
            builder.AllRows();

            expressionMock.VerifyAll();
        }

        [Fact]
        public void CallingInSchemaSetSchemaName()
        {
            var expressionMock = new Mock<DeleteDataExpression>();
            expressionMock.VerifySet(x => x.SchemaName = "TestSchema", Times.AtMostOnce(), "SchemaName property not set");

            var builder = new DeleteDataExpressionBuilder(expressionMock.Object);
            builder.InSchema("TestSchema");

            expressionMock.VerifyAll();
        }

        [Fact]
        public void CallingIsNullAddsANullColumn()
        {
            var expressionMock = new Mock<DeleteDataExpression>();


            var builder = new DeleteDataExpressionBuilder(expressionMock.Object);
            builder.IsNull("TestColumn");

            var result = expressionMock.Object;
            var rowobject = result.Rows.First().First();
            rowobject.Key.ShouldBe("TestColumn");
            rowobject.Value.ShouldBeNull();

        }
    }
}
