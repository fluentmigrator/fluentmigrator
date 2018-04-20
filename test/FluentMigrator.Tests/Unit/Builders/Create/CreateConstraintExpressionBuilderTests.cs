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
using System.Collections.Generic;

using FluentMigrator.Builders.Create.Constraint;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.SqlServer;

using Moq;
using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Builders.Create
{
    [TestFixture]
    public class CreateConstraintExpressionBuilderTests
    {
        private const string TableName = "Bacon";
        private const string Column1 = "BaconId";
        private const string Column2 = "EggsId";

        private Mock<ConstraintDefinition> CreateMockOfConstraint(ConstraintType constraintType,
            Action<CreateConstraintExpressionBuilder> expressionBuilderAction)
        {
            var constraintMock = new Mock<ConstraintDefinition>(constraintType);

            var expressionMock = new Mock<CreateConstraintExpression>(constraintType);
            expressionMock.SetupProperty(e => e.Constraint);

            var expression = expressionMock.Object;
            expression.Constraint = constraintMock.Object;

            expressionBuilderAction(new CreateConstraintExpressionBuilder(expression));

            return constraintMock;
        }

        private Mock<IList<string>> CreateMockOfConstraintColumns(ConstraintType constraintType,
            Action<CreateConstraintExpressionBuilder> expressionBuilderAction)
        {
            var collectionMock = new Mock<IList<string>>();

            var constraintMock = new Mock<ConstraintDefinition>(constraintType);
            constraintMock.Setup(f => f.Columns).Returns(collectionMock.Object);

            var expressionMock = new Mock<CreateConstraintExpression>(constraintType);
            expressionMock.SetupProperty(e => e.Constraint);

            var expression = expressionMock.Object;
            expression.Constraint = constraintMock.Object;

            expressionBuilderAction(new CreateConstraintExpressionBuilder(expression));

            return collectionMock;
        }

        [Test]
        public void CallingOnTableSetsTableNameForPrimaryKey()
        {
            var constraintMock = CreateMockOfConstraint(ConstraintType.PrimaryKey, b => b.OnTable(TableName));

            constraintMock.VerifySet(x => x.TableName = TableName);
        }

        [Test]
        public void CallingOnTableSetsTableNamesForUnique()
        {
            var constraintMock = CreateMockOfConstraint(ConstraintType.Unique, b => b.OnTable(TableName));

            constraintMock.VerifySet(x => x.TableName = TableName);
        }

        [Test]
        public void CallingColumnAddsColumnNameForPrimaryKey()
        {
            var collectionMock = CreateMockOfConstraintColumns(ConstraintType.PrimaryKey, b => b.Column(Column1));

            collectionMock.Verify(x => x.Add(Column1));
        }

        [Test]
        public void CallingColumnAddsColumnNameForUnique()
        {
            var collectionMock = CreateMockOfConstraintColumns(ConstraintType.Unique, b => b.Column(Column1));

            collectionMock.Verify(x => x.Add(Column1));
        }

        [Test]
        public void CallingColumnsAddsColumnNamesForPrimaryKey()
        {
            var collectionMock = CreateMockOfConstraintColumns(ConstraintType.PrimaryKey, b => b.Columns(new[] { Column1, Column2 }));

            collectionMock.Verify(x => x.Add(Column1));
            collectionMock.Verify(x => x.Add(Column2));
        }

        [Test]
        public void CallingColumnsAddsColumnNamesForUnique()
        {
            var collectionMock = CreateMockOfConstraintColumns(ConstraintType.Unique, b => b.Columns(new[] { Column1, Column2 }));

            collectionMock.Verify(x => x.Add(Column1));
            collectionMock.Verify(x => x.Add(Column2));
        }

        [Test]
        public void CallingColumnsWithDuplicateNamesAddsSetOfColumnNamesForPrimaryKey()
        {
            var expression = new CreateConstraintExpression(ConstraintType.PrimaryKey);

            var builder = new CreateConstraintExpressionBuilder(expression);
            builder.Columns(new[] { Column1, Column2, Column1 });

            Assert.That(expression.Constraint.Columns.Count, Is.EqualTo(2));
        }

        [Test]
        public void CallingColumnsWithDuplicateNamesAddsSetOfColumnNamesForUnique()
        {
            var expression = new CreateConstraintExpression(ConstraintType.Unique);

            var builder = new CreateConstraintExpressionBuilder(expression);
            builder.Columns(new[] { Column1, Column2, Column1 });

            Assert.That(expression.Constraint.Columns.Count, Is.EqualTo(2));
        }

        [Test]
        public void CallingClusteredSetsAdditionalPropertiesForPrimaryKey()
        {
            var constraintMock = CreateMockOfConstraint(ConstraintType.PrimaryKey, b => b.Clustered());

            constraintMock.Object.AdditionalFeatures.ShouldContain(
                new KeyValuePair<string, object>(SqlServerExtensions.ConstraintType,
                    SqlServerConstraintType.Clustered));
        }

        [Test]
        public void CallingClusteredSetsAdditionalPropertiesForUnique()
        {
            var constraintMock = CreateMockOfConstraint(ConstraintType.Unique, b => b.Clustered());

            constraintMock.Object.AdditionalFeatures.ShouldContain(
                new KeyValuePair<string, object>(SqlServerExtensions.ConstraintType,
                    SqlServerConstraintType.Clustered));
        }

        [Test]
        public void CallingNonClusteredSetsAdditionalPropertiesForPrimaryKey()
        {
            var constraintMock = CreateMockOfConstraint(ConstraintType.PrimaryKey, b => b.NonClustered());

            constraintMock.Object.AdditionalFeatures.ShouldContain(
                new KeyValuePair<string, object>(SqlServerExtensions.ConstraintType,
                    SqlServerConstraintType.NonClustered));
        }

        [Test]
        public void CallingNonClusteredSetsAdditionalPropertiesForUnique()
        {
            var constraintMock = CreateMockOfConstraint(ConstraintType.Unique, b => b.NonClustered());

            constraintMock.Object.AdditionalFeatures.ShouldContain(
                new KeyValuePair<string, object>(SqlServerExtensions.ConstraintType,
                    SqlServerConstraintType.NonClustered));
        }
    }
}
