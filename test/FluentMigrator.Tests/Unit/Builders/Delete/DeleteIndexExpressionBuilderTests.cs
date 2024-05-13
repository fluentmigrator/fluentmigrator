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

using FluentMigrator.Builders.Create;
using FluentMigrator.Builders.Delete;
using FluentMigrator.Builders.Delete.Index;
using FluentMigrator.Builders.Schema;
using FluentMigrator.Builders.Schema.Index;
using FluentMigrator.Builders.Schema.Schema;
using FluentMigrator.Builders.Schema.Table;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;
using Moq;
using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Builders.Delete
{
    [TestFixture]
    [Category("Builder")]
    [Category("DeleteIndex")]
    public class DeleteIndexExpressionBuilderTests
    {
        [Test]
        public void CallingOnTableSetsTableNameToSpecifiedValue()
        {
            var indexMock = new Mock<IndexDefinition>();

            var expressionMock = new Mock<DeleteIndexExpression>();
            expressionMock.SetupGet(e => e.Index).Returns(indexMock.Object);

            var migrationContextMock = new Mock<IMigrationContext>().Object;
            var migrationMock = new Mock<IMigration>().Object;

            var builder = new DeleteIndexExpressionBuilder(expressionMock.Object, migrationContextMock, migrationMock);
            builder.OnTable("Bacon");

            indexMock.VerifySet(x => x.TableName = "Bacon");
            expressionMock.VerifyGet(e => e.Index);
        }

        [Test]
        public void CallingOnColumnAddsNewColumnToExpression()
        {
            var collectionMock = new Mock<IList<IndexColumnDefinition>>();

            var indexMock = new Mock<IndexDefinition>();
            indexMock.SetupGet(x => x.Columns).Returns(collectionMock.Object);

            var expressionMock = new Mock<DeleteIndexExpression>();
            expressionMock.SetupGet(e => e.Index).Returns(indexMock.Object);

            var migrationContextMock = new Mock<IMigrationContext>().Object;
            var migrationMock = new Mock<IMigration>().Object;

            var builder = new DeleteIndexExpressionBuilder(expressionMock.Object, migrationContextMock, migrationMock);
            builder.OnColumn("BaconId");

            collectionMock.Verify(x => x.Add(It.Is<IndexColumnDefinition>(c => c.Name.Equals("BaconId"))));
            indexMock.VerifyGet(x => x.Columns);
            expressionMock.VerifyGet(e => e.Index);
        }

        [Test]
        public void CallingOnColumnsAddsMultipleNewColumnsToExpression()
        {
            var collectionMock = new Mock<IList<IndexColumnDefinition>>();

            var indexMock = new Mock<IndexDefinition>();
            indexMock.SetupGet(x => x.Columns).Returns(collectionMock.Object);

            var expressionMock = new Mock<DeleteIndexExpression>();
            expressionMock.SetupGet(e => e.Index).Returns(indexMock.Object);

            var migrationContextMock = new Mock<IMigrationContext>().Object;
            var migrationMock = new Mock<IMigration>().Object;

            var builder = new DeleteIndexExpressionBuilder(expressionMock.Object, migrationContextMock, migrationMock);
            builder.OnColumns("BaconId", "EggsId");

            collectionMock.Verify(x => x.Add(It.Is<IndexColumnDefinition>(c => c.Name.Equals("BaconId"))));
            collectionMock.Verify(x => x.Add(It.Is<IndexColumnDefinition>(c => c.Name.Equals("EggsId"))));
            indexMock.VerifyGet(x => x.Columns);
            expressionMock.VerifyGet(e => e.Index);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void CallingIfNotExists(bool isIndexExist)
        {
            var expressions = new List<IMigrationExpression>();
            var migrationContextMock = new Mock<IMigrationContext>();
            migrationContextMock.SetupGet(x => x.Expressions).Returns(expressions);
            var migrationContext = migrationContextMock.Object;

            var schemaIndexSyntaxMock = new Mock<ISchemaIndexSyntax>();
            schemaIndexSyntaxMock.Setup(x => x.Exists()).Returns(isIndexExist);
            var schemaIndexSyntax = schemaIndexSyntaxMock.Object;

            var schemaTableSyntaxMock = new Mock<ISchemaTableSyntax>();
            schemaTableSyntaxMock.Setup(x => x.Index(It.IsAny<string>())).Returns(schemaIndexSyntax);
            var schemaTableSyntax = schemaTableSyntaxMock.Object;

            var schemaSchemaSyntaxMock = new Mock<ISchemaSchemaSyntax>();
            schemaSchemaSyntaxMock.Setup(x => x.Table(It.IsAny<string>())).Returns(schemaTableSyntax);
            var schemaSchemaSyntax = schemaSchemaSyntaxMock.Object;

            var schemaExpressionRootMock = new Mock<ISchemaExpressionRoot>();
            schemaExpressionRootMock.Setup(x => x.Schema(It.IsAny<string>())).Returns(schemaSchemaSyntax);
            var schemaExpressionRoot = schemaExpressionRootMock.Object;

            var migrationMock = new Mock<IMigration>();
            migrationMock.SetupGet(x => x.Schema).Returns(schemaExpressionRoot);
            var migration = migrationMock.Object;

            var createExpressionRoot = new DeleteExpressionRoot(migrationContext, migration);

            createExpressionRoot
                .Index("IX_Users_CreateDate")
                .OnTable("Users")
                .WithOptions().IfExists();

            if (isIndexExist) migrationContext.Expressions.ShouldNotBeEmpty();
            if (!isIndexExist) migrationContext.Expressions.ShouldBeEmpty();
        }
    }
}
