#region License
//
// Copyright (c) 2007-2024, Fluent Migrator Project
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

using FluentMigrator.Builders.Merge;
using FluentMigrator.Expressions;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Builders.Merge
{
    [TestFixture]
    [Category("Builder")]
    [Category("MergeData")]
    public class MergeDataExpressionBuilderTests
    {
        [Test]
        public void RowsGetSetWhenRowIsCalled()
        {
            var expression = new MergeDataExpression();

            var builder = new MergeDataExpressionBuilder(expression);
            builder
                .Row(new { Id = 1, Name = "John" })
                .Row(new { Id = 2, Name = "Jane" });

            expression.Rows.Count.ShouldBe(2);

            expression.Rows[0][0].Key.ShouldBe("Id");
            expression.Rows[0][0].Value.ShouldBe(1);

            expression.Rows[0][1].Key.ShouldBe("Name");
            expression.Rows[0][1].Value.ShouldBe("John");

            expression.Rows[1][0].Key.ShouldBe("Id");
            expression.Rows[1][0].Value.ShouldBe(2);

            expression.Rows[1][1].Key.ShouldBe("Name");
            expression.Rows[1][1].Value.ShouldBe("Jane");
        }

        [Test]
        public void RowsGetPopulatedWhenRowWithDictionaryIsCalled()
        {
            var expression = new MergeDataExpression();

            new MergeDataExpressionBuilder(expression).Row(
                new Dictionary<string, object>
                {
                    ["Id"] = 1,
                    ["Name"] = "John"
                });

            expression.Rows.Count.ShouldBe(1);

            expression.Rows[0][0].Key.ShouldBe("Id");
            expression.Rows[0][0].Value.ShouldBe(1);

            expression.Rows[0][1].Key.ShouldBe("Name");
            expression.Rows[0][1].Value.ShouldBe("John");
        }

        [Test]
        public void MatchColumnsGetSetWhenMatchIsCalled()
        {
            var expression = new MergeDataExpression();

            var builder = new MergeDataExpressionBuilder(expression);
            builder.Match("Id", "Code");

            expression.MatchColumns.Count.ShouldBe(2);
            expression.MatchColumns[0].ShouldBe("Id");
            expression.MatchColumns[1].ShouldBe("Code");
        }

        [Test]
        public void SchemaNameGetSetWhenInSchemaIsCalled()
        {
            var expression = new MergeDataExpression();

            var builder = new MergeDataExpressionBuilder(expression);
            builder.InSchema("TestSchema");

            expression.SchemaName.ShouldBe("TestSchema");
        }

        [Test]
        public void MatchColumnsNoDuplicatesWhenMatchIsCalledMultipleTimes()
        {
            var expression = new MergeDataExpression();

            var builder = new MergeDataExpressionBuilder(expression);
            builder.Match("Id");
            builder.Match("Id", "Code");

            expression.MatchColumns.Count.ShouldBe(2);
            expression.MatchColumns[0].ShouldBe("Id");
            expression.MatchColumns[1].ShouldBe("Code");
        }

        [Test]
        public void AdditionalFeaturesReturnsExpressionAdditionalFeatures()
        {
            var expression = new MergeDataExpression();
            var builder = new MergeDataExpressionBuilder(expression);

            expression.AdditionalFeatures.Add("TestKey", "TestValue");

            builder.AdditionalFeatures.ShouldContain(
                new KeyValuePair<string, object>("TestKey", "TestValue"));
        }
    }
}