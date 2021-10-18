#region License
//
// Copyright (c) 2007-2018, Sean Chambers <schambers80@gmail.com>
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

using System.Collections.Generic;

using FluentMigrator.Builders.Insert;
using FluentMigrator.Expressions;
using FluentMigrator.Postgres;
using FluentMigrator.SqlServer;

using NUnit.Framework;

using Shouldly;

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

        [Test]
        public void RowsGetPopulatedWhenRowWithDictionaryIsCalled()
        {
            var values = new Dictionary<string, object>();
            values["Data1"] = "Row1Data1";
            values["Data2"] = "Row1Data2";

            var expression = new InsertDataExpression();

            new InsertDataExpressionBuilder(expression).Row(values);

            expression.Rows.Count.ShouldBe(1);

            expression.Rows[0][0].Key.ShouldBe("Data1");
            expression.Rows[0][0].Value.ShouldBe("Row1Data1");
            expression.Rows[0][1].Key.ShouldBe("Data2");
            expression.Rows[0][1].Value.ShouldBe("Row1Data2");
        }

        [Test]
        public void SqlServerIdentityInsertAddsCorrectAdditionalFeature()
        {
            var expression = new InsertDataExpression();
            var builder = new InsertDataExpressionBuilder(expression);
            builder.WithIdentityInsert();

            expression.AdditionalFeatures.ShouldContain(
                new KeyValuePair<string, object>(SqlServerExtensions.IdentityInsert, true));
        }

        [Test]
        public void SqlServerIdentityInsertCalledTwiceAddsCorrectAdditionalFeature()
        {
            var expression = new InsertDataExpression();
            var builder = new InsertDataExpressionBuilder(expression);
            builder.WithIdentityInsert().WithIdentityInsert();

            expression.AdditionalFeatures.ShouldContain(
                new KeyValuePair<string, object>(SqlServerExtensions.IdentityInsert, true));
        }

        [Test]
        public void PostgresOverridingSystemValueAddsCorrectAdditionalFeature()
        {
            var expression = new InsertDataExpression();
            var builder = new InsertDataExpressionBuilder(expression);
            builder.WithOverridingSystemValue();

            expression.AdditionalFeatures.ShouldContain(
                new KeyValuePair<string, object>(
                    PostgresExtensions.OverridingIdentityValues,
                    PostgresOverridingIdentityValuesType.System));
        }

        [Test]
        public void PostgresOverridingSystemValueCalledTwiceAddsCorrectAdditionalFeature()
        {
            var expression = new InsertDataExpression();
            var builder = new InsertDataExpressionBuilder(expression);
            builder.WithOverridingSystemValue().WithOverridingSystemValue();

            expression.AdditionalFeatures.ShouldContain(
                new KeyValuePair<string, object>(
                    PostgresExtensions.OverridingIdentityValues,
                    PostgresOverridingIdentityValuesType.System));
        }

        [Test]
        public void PostgresOverridingUserValueAddsCorrectAdditionalFeature()
        {
            var expression = new InsertDataExpression();
            var builder = new InsertDataExpressionBuilder(expression);
            builder.WithOverridingUserValue();

            expression.AdditionalFeatures.ShouldContain(
                new KeyValuePair<string, object>(
                    PostgresExtensions.OverridingIdentityValues,
                    PostgresOverridingIdentityValuesType.User));
        }

        [Test]
        public void PostgresOverridingUserValueCalledTwiceAddsCorrectAdditionalFeature()
        {
            var expression = new InsertDataExpression();
            var builder = new InsertDataExpressionBuilder(expression);
            builder.WithOverridingUserValue().WithOverridingUserValue();

            expression.AdditionalFeatures.ShouldContain(
                new KeyValuePair<string, object>(
                    PostgresExtensions.OverridingIdentityValues,
                    PostgresOverridingIdentityValuesType.User));
        }

        [Test]
        public void PostgresOverridingIdentityValuesCalledWithDifferentTypeAddsCorrectAdditionalFeature()
        {
            // If both WithOverridingSystemValue() and WithOverridingUserValue() are called on the same expression,
            // then the latest value should be set in the additional features

            var expressionForUserValue = new InsertDataExpression();
            var builderForUserValue = new InsertDataExpressionBuilder(expressionForUserValue);
            builderForUserValue.WithOverridingSystemValue().WithOverridingUserValue();

            var expressionForSystemValue = new InsertDataExpression();
            var builderForSystemValue = new InsertDataExpressionBuilder(expressionForSystemValue);
            builderForSystemValue.WithOverridingUserValue().WithOverridingSystemValue();

            expressionForUserValue.AdditionalFeatures.ShouldContain(
                new KeyValuePair<string, object>(
                    PostgresExtensions.OverridingIdentityValues,
                    PostgresOverridingIdentityValuesType.User));
            expressionForSystemValue.AdditionalFeatures.ShouldContain(
                new KeyValuePair<string, object>(
                    PostgresExtensions.OverridingIdentityValues,
                    PostgresOverridingIdentityValuesType.System));
        }
    }
}
