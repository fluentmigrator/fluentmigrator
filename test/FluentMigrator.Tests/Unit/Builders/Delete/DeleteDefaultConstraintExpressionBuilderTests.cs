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

using FluentMigrator.Builders.Delete.DefaultConstraint;
using FluentMigrator.Expressions;
using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Builders.Delete
{
    [TestFixture]
    [Category("Builder")]
    [Category("DeleteDefaultConstraint")]
    public class DeleteDefaultConstraintExpressionBuilderTests
    {
        [SetUp]
        public void Setup()
        {
            _expression = new DeleteDefaultConstraintExpression();
            _builder = new DeleteDefaultConstraintExpressionBuilder(_expression);
        }

        private DeleteDefaultConstraintExpressionBuilder _builder;
        private DeleteDefaultConstraintExpression _expression;

        [Test]
        public void OnColumnShouldSetColumnNameOnExpression()
        {
            _builder.OnColumn("column");
            _expression.ColumnName.ShouldBe("column");
        }

        [Test]
        public void OnSchemaShouldSetSchemaNameOnExpression()
        {
            _builder.InSchema("Shema");
            _expression.SchemaName.ShouldBe("Shema");
        }

        [Test]
        public void OnTableShouldSetTableNameOnExpression()
        {
            _builder.OnTable("ThaTable");
            _expression.TableName.ShouldBe("ThaTable");
        }
    }
}
