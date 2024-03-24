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
using System.Data;
using FluentMigrator.Builders.Create.ForeignKey;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using Moq;
using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Builders.Create
{
    [TestFixture]
    [Category("Builder")]
    [Category("CreateForeignKey")]
    public class CreateForeignKeyExpressionBuilderTests
    {
        [Test]
        public void CallingFromTableSetsForeignTableName()
        {
            var foreignKeyMock = new Mock<ForeignKeyDefinition>();

            var expressionMock = new Mock<CreateForeignKeyExpression>();
            expressionMock.SetupGet(e => e.ForeignKey).Returns(foreignKeyMock.Object);

            var builder = new CreateForeignKeyExpressionBuilder(expressionMock.Object);
            builder.FromTable("Bacon");

            foreignKeyMock.VerifySet(f => f.ForeignTable = "Bacon");
            expressionMock.VerifyGet(e => e.ForeignKey);
        }

        [Test]
        public void CallingToTableSetsPrimaryTableName()
        {
            var foreignKeyMock = new Mock<ForeignKeyDefinition>();

            var expressionMock = new Mock<CreateForeignKeyExpression>();
            expressionMock.Setup(e => e.ForeignKey).Returns(foreignKeyMock.Object);

            var builder = new CreateForeignKeyExpressionBuilder(expressionMock.Object);
            builder.ToTable("Bacon");

            foreignKeyMock.VerifySet(f => f.PrimaryTable = "Bacon");
            expressionMock.VerifyGet(e => e.ForeignKey);
        }

        [Test]
        public void CallingForeignColumnAddsColumnNameToForeignColumnCollection()
        {
            var collectionMock = new Mock<IList<string>>();

            var foreignKeyMock = new Mock<ForeignKeyDefinition>();
            foreignKeyMock.SetupGet(f => f.ForeignColumns).Returns(collectionMock.Object);

            var expressionMock = new Mock<CreateForeignKeyExpression>();
            expressionMock.SetupGet(e => e.ForeignKey).Returns(foreignKeyMock.Object);

            var builder = new CreateForeignKeyExpressionBuilder(expressionMock.Object);
            builder.ForeignColumn("BaconId");

            collectionMock.Verify(x => x.Add("BaconId"));
            foreignKeyMock.VerifyGet(f => f.ForeignColumns);
            expressionMock.VerifyGet(e => e.ForeignKey);
        }

        [Test]
        public void CallingForeignColumnsAddsColumnNamesToForeignColumnCollection()
        {
            var collectionMock = new Mock<IList<string>>();

            var foreignKeyMock = new Mock<ForeignKeyDefinition>();
            foreignKeyMock.Setup(f => f.ForeignColumns).Returns(collectionMock.Object);

            var expressionMock = new Mock<CreateForeignKeyExpression>();
            expressionMock.Setup(e => e.ForeignKey).Returns(foreignKeyMock.Object);

            var builder = new CreateForeignKeyExpressionBuilder(expressionMock.Object);
            builder.ForeignColumns("BaconId", "EggsId");

            collectionMock.Verify(x => x.Add("BaconId"));
            collectionMock.Verify(x => x.Add("EggsId"));
            foreignKeyMock.VerifyGet(f => f.ForeignColumns);
            expressionMock.VerifyGet(e => e.ForeignKey);
        }

        [Test]
        public void CallingPrimaryColumnAddsColumnNameToPrimaryColumnCollection()
        {
            var collectionMock = new Mock<IList<string>>();

            var foreignKeyMock = new Mock<ForeignKeyDefinition>();
            foreignKeyMock.Setup(f => f.PrimaryColumns).Returns(collectionMock.Object);

            var expressionMock = new Mock<CreateForeignKeyExpression>();
            expressionMock.Setup(e => e.ForeignKey).Returns(foreignKeyMock.Object);

            var builder = new CreateForeignKeyExpressionBuilder(expressionMock.Object);
            builder.PrimaryColumn("BaconId");

            collectionMock.Verify(x => x.Add("BaconId"));
            foreignKeyMock.VerifyGet(f => f.PrimaryColumns);
            expressionMock.VerifyGet(e => e.ForeignKey);
        }

        [Test]
        public void CallingPrimaryColumnsAddsColumnNamesToForeignColumnCollection()
        {
            var collectionMock = new Mock<IList<string>>();

            var foreignKeyMock = new Mock<ForeignKeyDefinition>();
            foreignKeyMock.Setup(f => f.PrimaryColumns).Returns(collectionMock.Object);

            var expressionMock = new Mock<CreateForeignKeyExpression>();
            expressionMock.Setup(e => e.ForeignKey).Returns(foreignKeyMock.Object);

            var builder = new CreateForeignKeyExpressionBuilder(expressionMock.Object);
            builder.PrimaryColumns("BaconId", "EggsId");

            collectionMock.Verify(x => x.Add("BaconId"));
            collectionMock.Verify(x => x.Add("EggsId"));
            foreignKeyMock.VerifyGet(f => f.PrimaryColumns);
            expressionMock.VerifyGet(e => e.ForeignKey);
        }

        [TestCase(Rule.Cascade), TestCase(Rule.SetDefault), TestCase(Rule.SetNull), TestCase(Rule.None)]
        public void CallingOnUpdateSetsOnUpdateToSpecifiedRule(Rule rule)
        {
            var expression = new CreateForeignKeyExpression();
            var builder = new CreateForeignKeyExpressionBuilder(expression);
            builder.OnUpdate(rule);
            Assert.Multiple(() =>
            {
                Assert.That(expression.ForeignKey.OnUpdate, Is.EqualTo(rule));
                Assert.That(expression.ForeignKey.OnDelete, Is.EqualTo(Rule.None));
            });
        }

        [TestCase(Rule.Cascade), TestCase(Rule.SetDefault), TestCase(Rule.SetNull), TestCase(Rule.None)]
        public void CallingOnDeleteSetsOnDeleteToSpecifiedRule(Rule rule)
        {
            var expression = new CreateForeignKeyExpression();
            var builder = new CreateForeignKeyExpressionBuilder(expression);
            builder.OnDelete(rule);
            Assert.Multiple(() =>
            {
                Assert.That(expression.ForeignKey.OnUpdate, Is.EqualTo(Rule.None));
                Assert.That(expression.ForeignKey.OnDelete, Is.EqualTo(rule));
            });
        }

        [TestCase(Rule.Cascade), TestCase(Rule.SetDefault), TestCase(Rule.SetNull), TestCase(Rule.None)]
        public void CallingOnDeleteOrUpdateSetsBothOnDeleteAndOnUpdateToSpecifiedRule(Rule rule)
        {
            var expression = new CreateForeignKeyExpression();
            var builder = new CreateForeignKeyExpressionBuilder(expression);
            builder.OnDeleteOrUpdate(rule);
            Assert.Multiple(() =>
            {
                Assert.That(expression.ForeignKey.OnUpdate, Is.EqualTo(rule));
                Assert.That(expression.ForeignKey.OnDelete, Is.EqualTo(rule));
            });
        }
    }
}
