#region License
//
// Copyright (c) 2018, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;
using System.Linq;
using NUnit.Framework;

using FluentMigrator.Expressions;
using Moq;
using FluentMigrator.Builders.Delete;
using FluentMigrator.Validation;

using Shouldly;
using System.Globalization;

namespace FluentMigrator.Tests.Unit.Builders.Delete
{
    [TestFixture]
    [Category("Builder")]
    [Category("DeleteData")]
    public class DeleteDataExpressionTests
    {

        [Test]
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

        [Test]
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
            var rowobject = result.Rows.First().First();
            rowobject.Key.ShouldBe("TestColumn");
            rowobject.Value.ShouldBeNull();

        }

        [Test]
        [SetUICulture("")] // Ensure validation messages are in English
        public void DefaultMigrationExpressionValidatorShouldReturnErrorWhenTableNameIsNotSpecified()
        {
            var mockServiceProvider = new Mock<IServiceProvider>();
            var validator = new DefaultMigrationExpressionValidator(mockServiceProvider.Object);
            var expressionMock = new Mock<DeleteDataExpression>();

            var builder = new DeleteDataExpressionBuilder(expressionMock.Object);
            builder.IsNull("TestColumn");

            var result = expressionMock.Object;

            var validationResults = validator.Validate(result).ToList();

            validationResults.ShouldNotBeNull();
            validationResults.Count.ShouldBeGreaterThan(0);
            validationResults.ShouldContain(r => string.Equals(r.ErrorMessage, "The TableName field is required.", StringComparison.Ordinal));
        }
    }
}
