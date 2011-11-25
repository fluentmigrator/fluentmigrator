﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Should;
using FluentMigrator.Expressions;
using Moq;
using FluentMigrator.Builders.Delete;

namespace FluentMigrator.Tests.Unit.Builders.Delete {
    [TestFixture]
    public class DeleteDataExpressionTests {

        [Test]
        public void CallingIsNullAddsANullColumn() {
            var expressionMock = new Mock<DeleteDataExpression>();
            

            var builder = new DeleteDataExpressionBuilder(expressionMock.Object);
            builder.IsNull("TestColumn");

            var result = expressionMock.Object;
            var rowobject = result.Rows.First().First();
            rowobject.Key.ShouldBe("TestColumn");
            rowobject.Value.ShouldBeNull();

        }

        [Test]
        public void CallingRowAddAColumn() {
            var expressionMock = new Mock<DeleteDataExpression>();


            var builder = new DeleteDataExpressionBuilder(expressionMock.Object);
            builder.Row(new { TestColumn = "TestValue" });

            var result = expressionMock.Object;
            var rowobject = result.Rows.First().First();
            rowobject.Key.ShouldBe("TestColumn");
            rowobject.Value.ShouldBe("TestValue");

        }

        [Test]
        public void CallingRowTwiceAddTwoColumns() {
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
    }
}
