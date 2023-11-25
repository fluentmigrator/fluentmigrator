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

using System.Collections.Generic;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner;
using FluentMigrator.Tests.Helpers;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Expressions
{
    [TestFixture]
    [Category("Expression")]
    [Category("UpdateData")]
    public class UpdateDataExpressionTests {
        private UpdateDataExpression _expression;

        [SetUp]
        public void Initialize()
        {
            _expression =
                new UpdateDataExpression()
                {
                    TableName = "ExampleTable",
                    Set = new List<KeyValuePair<string, object>>
                    {
                        new KeyValuePair<string, object>("Column", "value")
                    },
                    IsAllRows = false
                };
        }

        [Test]
        public void NullUpdateTargetCausesErrorMessage()
        {
            // null is the default value, but it might not always be, so I'm codifying it here anyway
            _expression.Where = null;

            var errors = ValidationHelper.CollectErrors(_expression);
            errors.ShouldContain(ErrorMessages.UpdateDataExpressionMustSpecifyWhereClauseOrAllRows);
        }

        [Test]
        public void EmptyUpdateTargetCausesErrorMessage()
        {
            // The same should be true for an empty list
            _expression.Where = new List<KeyValuePair<string, object>>();

            var errors = ValidationHelper.CollectErrors(_expression);
            errors.ShouldContain(ErrorMessages.UpdateDataExpressionMustSpecifyWhereClauseOrAllRows);
        }

        [Test]
        public void DoesNotRequireWhereConditionWhenIsAllRowsIsSet()
        {
            _expression.IsAllRows = true;

            var errors = ValidationHelper.CollectErrors(_expression);
            errors.ShouldNotContain(ErrorMessages.UpdateDataExpressionMustSpecifyWhereClauseOrAllRows);
        }

        [Test]
        public void DoesNotAllowWhereConditionWhenIsAllRowsIsSet()
        {
            _expression.IsAllRows = true;
            _expression.Where = new List<KeyValuePair<string, object>> {new KeyValuePair<string, object>("key", "value")};

            var errors = ValidationHelper.CollectErrors(_expression);
            errors.ShouldContain(ErrorMessages.UpdateDataExpressionMustNotSpecifyBothWhereClauseAndAllRows);
        }

        [Test]
        public void WhenDefaultSchemaConventionIsAppliedAndSchemaIsNotSetThenSchemaShouldBeNull()
        {
            var processed = _expression.Apply(ConventionSets.NoSchemaName);

            Assert.That(processed.SchemaName, Is.Null);
        }

        [Test]
        public void WhenDefaultSchemaConventionIsAppliedAndSchemaIsSetThenSchemaShouldNotBeChanged()
        {
            _expression.SchemaName = "testschema";

            var processed = _expression.Apply(ConventionSets.WithSchemaName);

            Assert.That(processed.SchemaName, Is.EqualTo("testschema"));
        }

        [Test]
        public void WhenDefaultSchemaConventionIsChangedAndSchemaIsNotSetThenSetSchema()
        {
            var processed = _expression.Apply(ConventionSets.WithSchemaName);

            Assert.That(processed.SchemaName, Is.EqualTo("testdefault"));
        }
    }
}
