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
using NUnit.Framework;
using FluentMigrator.Expressions;
using Moq;
using FluentMigrator.Infrastructure;
using FluentMigrator.Builders.Delete;
using FluentMigrator.Builders.Schema;
using FluentMigrator.Builders.Schema.Constraint;
using FluentMigrator.Builders.Schema.Schema;
using FluentMigrator.Builders.Schema.Table;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Builders.Delete
{
    [TestFixture]
    [Category("Builder")]
    [Category("DeleteConstraint")]
    public class DeleteConstraintExpressionBuilderTest
    {
        [Test]
        public void CallingDeletePrimaryKeyCreatesADeleteConstraintExpression()
        {
            var collectionMock = new Mock<ICollection<IMigrationExpression>>();

            var contextMock = new Mock<IMigrationContext>();
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);

            var migrationMock = new Mock<IMigration>().Object;

            var root = new DeleteExpressionRoot(contextMock.Object, migrationMock);
            root.PrimaryKey("TestKey");

            collectionMock.Verify(x => x.Add(It.Is<DeleteConstraintExpression>(e => e.Constraint.IsPrimaryKeyConstraint)));
            collectionMock.Verify(x => x.Add(It.Is<DeleteConstraintExpression>(e => e.Constraint.ConstraintName == "TestKey")));
            contextMock.VerifyGet(x => x.Expressions);
        }

        [Test]
        public void CallingDeleteUniqueConstraintCreatesADeleteConstraintExpression()
        {
            var collectionMock = new Mock<ICollection<IMigrationExpression>>();

            var contextMock = new Mock<IMigrationContext>();
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);

            var migrationMock = new Mock<IMigration>().Object;

            var root = new DeleteExpressionRoot(contextMock.Object, migrationMock);
            root.UniqueConstraint("TestUniqueConstraintName");

            collectionMock.Verify(x => x.Add(It.Is<DeleteConstraintExpression>(e => e.Constraint.IsUniqueConstraint)));
            collectionMock.Verify(x => x.Add(It.Is<DeleteConstraintExpression>(e => e.Constraint.ConstraintName == "TestUniqueConstraintName")));
            contextMock.VerifyGet(x => x.Expressions);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void CallingIfNotExists(bool isConstraintExist)
        {
            var expressions = new List<IMigrationExpression>();
            var migrationContextMock = new Mock<IMigrationContext>();
            migrationContextMock.SetupGet(x => x.Expressions).Returns(expressions);
            var migrationContext = migrationContextMock.Object;

            var schemaConstraintSyntaxMock = new Mock<ISchemaConstraintSyntax>();
            schemaConstraintSyntaxMock.Setup(x => x.Exists()).Returns(isConstraintExist);
            var schemaConstraintSyntax = schemaConstraintSyntaxMock.Object;

            var schemaTableSyntaxMock = new Mock<ISchemaTableSyntax>();
            schemaTableSyntaxMock.Setup(x => x.Constraint(It.IsAny<string>())).Returns(schemaConstraintSyntax);
            var schemaTableSyntax = schemaTableSyntaxMock.Object;

            var schemaSchemaSyntaxMock = new Mock<ISchemaSchemaSyntax>();
            schemaSchemaSyntaxMock.Setup(x => x.Table(It.IsAny<string>())).Returns(schemaTableSyntax);
            var schemaSchemaSyntax = schemaSchemaSyntaxMock.Object;

            var schemaExpressionRootMock = new Mock<ISchemaExpressionRoot>();
            schemaExpressionRootMock.Setup(x => x.Schema(It.IsAny<string>())).Returns(schemaSchemaSyntax);
            var schemaExpressionRoot = schemaExpressionRootMock.Object;

            var migrationMock = new Mock<IMigration>();
            migrationMock.SetupGet(x => x.Schema).Returns(schemaExpressionRoot);
            var migration= migrationMock.Object;

            var createExpressionRoot = new DeleteExpressionRoot(migrationContext, migration);

            createExpressionRoot
                .PrimaryKey("PK_Users_Id")
                .FromTable("Users")
                .WithOptions().IfExists();

            if (isConstraintExist) migrationContext.Expressions.ShouldNotBeEmpty();
            if (!isConstraintExist) migrationContext.Expressions.ShouldBeEmpty();
        }
    }
}
