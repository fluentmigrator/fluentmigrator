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
using FluentMigrator.Builders.Create.Column;
using FluentMigrator.Builders.Create.Constraint;
using FluentMigrator.Builders.Create.ForeignKey;
using FluentMigrator.Builders.Create.Index;
using FluentMigrator.Builders.Create.Table;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using Moq;
using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Builders.Create
{
    using FluentMigrator.Builders.Create.Sequence;

    [TestFixture]
    [Category("Builder")]
    [Category("RootCreate")]
    public class CreateExpressionRootTests
    {
        [Test]
        public void CallingTableAddsCreateTableExpressionToContextWithSpecifiedNameSet()
        {
            var collectionMock = new Mock<ICollection<IMigrationExpression>>();

            var contextMock = new Mock<IMigrationContext>();
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);

            var root = new CreateExpressionRoot(contextMock.Object);
            root.Table("Bacon");

            collectionMock.Verify(x => x.Add(It.Is<CreateTableExpression>(e => e.TableName.Equals("Bacon"))));
            contextMock.VerifyGet(x => x.Expressions);
        }

        [Test]
        public void CallingTableReturnsCreateTableExpressionBuilder()
        {
            var collectionMock = new Mock<ICollection<IMigrationExpression>>();
            var contextMock = new Mock<IMigrationContext>();
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);

            var root = new CreateExpressionRoot(contextMock.Object);
            var builder = root.Table("Bacon");

            builder.ShouldBeOfType<CreateTableExpressionBuilder>();
            contextMock.VerifyGet(x => x.Expressions);
        }

        [Test]
        public void CallingColumnAddsCreateColumnExpressionToContextWithSpecifiedNameSet()
        {
            var collectionMock = new Mock<ICollection<IMigrationExpression>>();

            var contextMock = new Mock<IMigrationContext>();
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);

            var root = new CreateExpressionRoot(contextMock.Object);
            root.Column("Bacon");

            collectionMock.Verify(x => x.Add(It.Is<CreateColumnExpression>(e => e.Column.Name.Equals("Bacon"))));
            contextMock.VerifyGet(x => x.Expressions);
        }

        [Test]
        public void CallingColumnReturnsCreateColumnExpression()
        {
            var collectionMock = new Mock<ICollection<IMigrationExpression>>();
            var contextMock = new Mock<IMigrationContext>();
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);

            var root = new CreateExpressionRoot(contextMock.Object);
            var builder = root.Column("Bacon");

            builder.ShouldBeOfType<CreateColumnExpressionBuilder>();
            contextMock.VerifyGet(x => x.Expressions);
        }

        [Test]
        public void CallingForeignKeyWithoutNameAddsCreateForeignKeyExpressionToContext()
        {
            var collectionMock = new Mock<ICollection<IMigrationExpression>>();

            var contextMock = new Mock<IMigrationContext>();
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);

            var root = new CreateExpressionRoot(contextMock.Object);
            root.ForeignKey();

            collectionMock.Verify(x => x.Add(It.IsAny<CreateForeignKeyExpression>()));
            contextMock.VerifyGet(x => x.Expressions);
        }

        [Test]
        public void CallingForeignKeyAddsCreateForeignKeyExpressionToContextWithSpecifiedNameSet()
        {
            var collectionMock = new Mock<ICollection<IMigrationExpression>>();

            var contextMock = new Mock<IMigrationContext>();
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);

            var root = new CreateExpressionRoot(contextMock.Object);
            root.ForeignKey("FK_Bacon");

            collectionMock.Verify(x => x.Add(It.Is<CreateForeignKeyExpression>(e => e.ForeignKey.Name.Equals("FK_Bacon"))));
            contextMock.VerifyGet(x => x.Expressions);
        }

        [Test]
        public void CallingForeignKeyWithoutNameReturnsCreateForeignKeyExpression()
        {
            var collectionMock = new Mock<ICollection<IMigrationExpression>>();
            var contextMock = new Mock<IMigrationContext>();
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);

            var root = new CreateExpressionRoot(contextMock.Object);
            var builder = root.ForeignKey();

            builder.ShouldBeOfType<CreateForeignKeyExpressionBuilder>();
            contextMock.VerifyGet(x => x.Expressions);
        }

        [Test]
        public void CallingForeignKeyCreatesCreateForeignKeyExpression()
        {
            var collectionMock = new Mock<ICollection<IMigrationExpression>>();
            var contextMock = new Mock<IMigrationContext>();
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);

            var root = new CreateExpressionRoot(contextMock.Object);
            var builder = root.ForeignKey("FK_Bacon");

            builder.ShouldBeOfType<CreateForeignKeyExpressionBuilder>();
            contextMock.VerifyGet(x => x.Expressions);
        }

        [Test]
        public void CallingIndexWithoutNameAddsCreateIndexExpressionToContext()
        {
            var collectionMock = new Mock<ICollection<IMigrationExpression>>();

            var contextMock = new Mock<IMigrationContext>();
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);

            var root = new CreateExpressionRoot(contextMock.Object);
            root.Index();

            collectionMock.Verify(x => x.Add(It.IsAny<CreateIndexExpression>()));
            contextMock.VerifyGet(x => x.Expressions);
        }

        [Test]
        public void CallingIndexAddsCreateIndexExpressionToContextWithSpecifiedNameSet()
        {
            var collectionMock = new Mock<ICollection<IMigrationExpression>>();

            var contextMock = new Mock<IMigrationContext>();
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);

            var root = new CreateExpressionRoot(contextMock.Object);
            root.Index("IX_Bacon");

            collectionMock.Verify(x => x.Add(It.Is<CreateIndexExpression>(e => e.Index.Name.Equals("IX_Bacon"))));
            contextMock.VerifyGet(x => x.Expressions);
        }

        [Test]
        public void CallingIndexWithoutNameReturnsCreateIndexExpression()
        {
            var collectionMock = new Mock<ICollection<IMigrationExpression>>();
            var contextMock = new Mock<IMigrationContext>();
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);

            var root = new CreateExpressionRoot(contextMock.Object);
            var builder = root.Index();

            builder.ShouldBeOfType<CreateIndexExpressionBuilder>();
            contextMock.VerifyGet(x => x.Expressions);
        }

        [Test]
        public void CallingIndexCreatesCreateIndexExpression()
        {
            var collectionMock = new Mock<ICollection<IMigrationExpression>>();
            var contextMock = new Mock<IMigrationContext>();
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);

            var root = new CreateExpressionRoot(contextMock.Object);
            var builder = root.Index("IX_Bacon");

            builder.ShouldBeOfType<CreateIndexExpressionBuilder>();
            contextMock.VerifyGet(x => x.Expressions);
        }

        [Test]
        public void CallingPrimaryKeyAddsCreateColumnExpressionToContextWithSpecifiedNameSet()
        {
            var collectionMock = new Mock<ICollection<IMigrationExpression>>();

            var contextMock = new Mock<IMigrationContext>();
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);

            var root = new CreateExpressionRoot(contextMock.Object);
            root.PrimaryKey("PK_Bacon");

            collectionMock.Verify(x => x.Add(It.Is<CreateConstraintExpression>(
                e => e.Constraint.ConstraintName.Equals("PK_Bacon")
                     && e.Constraint.IsPrimaryKeyConstraint
            )));
            contextMock.VerifyGet(x => x.Expressions);
        }

        [Test]
        public void CallingPrimaryKeyReturnsCreateColumnExpression()
        {
            var collectionMock = new Mock<ICollection<IMigrationExpression>>();
            var contextMock = new Mock<IMigrationContext>();
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);

            var root = new CreateExpressionRoot(contextMock.Object);
            var builder = root.PrimaryKey("PK_Bacon");

            builder.ShouldBeOfType<CreateConstraintExpressionBuilder>();
            contextMock.VerifyGet(x => x.Expressions);
        }

        [Test]
        public void CallingSequenceAddsCreateSequenceExpressionToContextWithSpecifiedNameSet()
        {
            var collectionMock = new Mock<ICollection<IMigrationExpression>>();

            var contextMock = new Mock<IMigrationContext>();
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);

            var root = new CreateExpressionRoot(contextMock.Object);
            root.Sequence("Bacon");

            collectionMock.Verify(x => x.Add(It.Is<CreateSequenceExpression>(e => e.Sequence.Name.Equals("Bacon"))));
            contextMock.VerifyGet(x => x.Expressions);
        }

        [Test]
        public void CallingSequenceReturnsCreateSequenceExpression()
        {
            var collectionMock = new Mock<ICollection<IMigrationExpression>>();
            var contextMock = new Mock<IMigrationContext>();
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);

            var root = new CreateExpressionRoot(contextMock.Object);
            var builder = root.Sequence("Bacon");

            builder.ShouldBeOfType<CreateSequenceExpressionBuilder>();
            contextMock.VerifyGet(x => x.Expressions);
        }

        [Test]
        public void CallingUniqueConstraintCreateColumnExpressionToContextWithSpecifiedNameSet()
        {
            var collectionMock = new Mock<ICollection<IMigrationExpression>>();

            var contextMock = new Mock<IMigrationContext>();
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);

            var root = new CreateExpressionRoot(contextMock.Object);
            root.UniqueConstraint("UC_Bacon");

            collectionMock.Verify(x => x.Add(It.Is<CreateConstraintExpression>(
                e => e.Constraint.ConstraintName.Equals("UC_Bacon")
                     && e.Constraint.IsUniqueConstraint
            )));
            contextMock.VerifyGet(x => x.Expressions);
        }

        [Test]
        public void CallingUniqueConstraintReturnsCreateColumnExpression()
        {
            var collectionMock = new Mock<ICollection<IMigrationExpression>>();
            var contextMock = new Mock<IMigrationContext>();
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);

            var root = new CreateExpressionRoot(contextMock.Object);
            var builder = root.UniqueConstraint("UC_Bacon");

            builder.ShouldBeOfType<CreateConstraintExpressionBuilder>();
            contextMock.VerifyGet(x => x.Expressions);
        }
    }
}