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
using System.Linq;

using FluentMigrator.Builders.Update;
using FluentMigrator.Expressions;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Builders.Update
{
    [TestFixture]
    [Category("Builder")]
    [Category("UpdateData")]
    public class UpdateDataExpressionBuilderTests
    {
        [Test]
        public void SetGetPopulatedWhenSetWithAnonymousObjectIsCalled()
        {
            var expression = new UpdateDataExpression();

            var builder = new UpdateDataExpressionBuilder(expression);
            builder.Set(new { Column1 = "Value1", Column2 = "Value2" });

            expression.Set.Count.ShouldBe(2);

            expression.Set[0].Key.ShouldBe("Column1");
            expression.Set[0].Value.ShouldBe("Value1");

            expression.Set[1].Key.ShouldBe("Column2");
            expression.Set[1].Value.ShouldBe("Value2");
        }

        [Test]
        public void SetGetPopulatedWhenSetWithDictionaryIsCalled()
        {
            var expression = new UpdateDataExpression();

            var builder = new UpdateDataExpressionBuilder(expression);
            builder.Set(new Dictionary<string, object>
            {
                ["Column1"] = "Value1",
                ["Column2"] = "Value2"
            });

            expression.Set.Count.ShouldBe(2);

            expression.Set.Any(kvp => kvp.Key == "Column1" && kvp.Value.Equals("Value1")).ShouldBeTrue();
            expression.Set.Any(kvp => kvp.Key == "Column2" && kvp.Value.Equals("Value2")).ShouldBeTrue();
        }

        [Test]
        public void WhereGetPopulatedWhenWhereWithAnonymousObjectIsCalled()
        {
            var expression = new UpdateDataExpression();

            var builder = new UpdateDataExpressionBuilder(expression);
            builder.Where(new { Id = 1, Name = "Test" });

            expression.Where.Count.ShouldBe(2);

            expression.Where[0].Key.ShouldBe("Id");
            expression.Where[0].Value.ShouldBe(1);

            expression.Where[1].Key.ShouldBe("Name");
            expression.Where[1].Value.ShouldBe("Test");
        }

        [Test]
        public void WhereGetPopulatedWhenWhereWithDictionaryIsCalled()
        {
            var expression = new UpdateDataExpression();

            var builder = new UpdateDataExpressionBuilder(expression);
            builder.Where(new Dictionary<string, object>
            {
                ["Id"] = 1,
                ["Name"] = "Test"
            });

            expression.Where.Count.ShouldBe(2);

            expression.Where.Any(kvp => kvp.Key == "Id" && kvp.Value.Equals(1)).ShouldBeTrue();
            expression.Where.Any(kvp => kvp.Key == "Name" && kvp.Value.Equals("Test")).ShouldBeTrue();
        }

        [Test]
        public void SetAndWhereWorkTogetherWithDictionaries()
        {
            var expression = new UpdateDataExpression();

            var builder = new UpdateDataExpressionBuilder(expression);
            builder
                .Set(new Dictionary<string, object>
                {
                    ["Column1"] = "NewValue1",
                    ["Column2"] = "NewValue2"
                })
                .Where(new Dictionary<string, object>
                {
                    ["Id"] = 123
                });

            expression.Set.Count.ShouldBe(2);
            expression.Where.Count.ShouldBe(1);

            expression.Set.Any(kvp => kvp.Key == "Column1" && kvp.Value.Equals("NewValue1")).ShouldBeTrue();
            expression.Set.Any(kvp => kvp.Key == "Column2" && kvp.Value.Equals("NewValue2")).ShouldBeTrue();
            expression.Where.Any(kvp => kvp.Key == "Id" && kvp.Value.Equals(123)).ShouldBeTrue();
        }

        [Test]
        public void SetAndWhereWorkTogetherWithMixedTypesAnonymousObjectAndDictionary()
        {
            var expression = new UpdateDataExpression();

            var builder = new UpdateDataExpressionBuilder(expression);
            builder
                .Set(new { Column1 = "NewValue1", Column2 = "NewValue2" })
                .Where(new Dictionary<string, object>
                {
                    ["Id"] = 123
                });

            expression.Set.Count.ShouldBe(2);
            expression.Where.Count.ShouldBe(1);

            expression.Set[0].Key.ShouldBe("Column1");
            expression.Set[0].Value.ShouldBe("NewValue1");
            expression.Set[1].Key.ShouldBe("Column2");
            expression.Set[1].Value.ShouldBe("NewValue2");

            expression.Where.Any(kvp => kvp.Key == "Id" && kvp.Value.Equals(123)).ShouldBeTrue();
        }

        [Test]
        public void SetAndWhereWorkTogetherWithMixedTypesDictionaryAndAnonymousObject()
        {
            var expression = new UpdateDataExpression();

            var builder = new UpdateDataExpressionBuilder(expression);
            builder
                .Set(new Dictionary<string, object>
                {
                    ["Column1"] = "NewValue1",
                    ["Column2"] = "NewValue2"
                })
                .Where(new { Id = 123 });

            expression.Set.Count.ShouldBe(2);
            expression.Where.Count.ShouldBe(1);

            expression.Set.Any(kvp => kvp.Key == "Column1" && kvp.Value.Equals("NewValue1")).ShouldBeTrue();
            expression.Set.Any(kvp => kvp.Key == "Column2" && kvp.Value.Equals("NewValue2")).ShouldBeTrue();

            expression.Where[0].Key.ShouldBe("Id");
            expression.Where[0].Value.ShouldBe(123);
        }
    }
}