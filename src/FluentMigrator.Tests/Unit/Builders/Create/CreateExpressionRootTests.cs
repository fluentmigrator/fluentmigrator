#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
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
using FluentMigrator.Builders.Create.ForeignKey;
using FluentMigrator.Builders.Create.Index;
using FluentMigrator.Builders.Create.Table;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using Moq;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Builders.Create
{
	[TestFixture]
	public class CreateExpressionRootTests
	{
		[Test]
		public void CallingTableAddsCreateTableExpressionToContextWithSpecifiedNameSet()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
            collectionMock.Verify(x => x.Add(It.Is<CreateTableExpression>(e => e.TableName.Equals("Bacon"))), Times.AtMostOnce());

			var contextMock = new Mock<IMigrationContext>();
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);
            contextMock.VerifyGet(x => x.Expressions, Times.AtMostOnce());

			var root = new CreateExpressionRoot(contextMock.Object);
			root.Table("Bacon");

			collectionMock.VerifyAll();
			contextMock.VerifyAll();
		}

		[Test]
		public void CallingTableReturnsCreateTableExpressionBuilder()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			var contextMock = new Mock<IMigrationContext>();
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);
            contextMock.VerifyGet(x => x.Expressions, Times.AtMostOnce());

			var root = new CreateExpressionRoot(contextMock.Object);
			var builder = root.Table("Bacon");

			builder.ShouldBeOfType<CreateTableExpressionBuilder>();
			contextMock.VerifyAll();
		}

		[Test]
		public void CallingColumnAddsCreateColumnExpressionToContextWithSpecifiedNameSet()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
            collectionMock.Verify(x => x.Add(It.Is<CreateColumnExpression>(e => e.Column.Name.Equals("Bacon"))), Times.AtMostOnce());

			var contextMock = new Mock<IMigrationContext>();
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);
			contextMock.VerifyGet(x => x.Expressions, Times.AtMostOnce());

			var root = new CreateExpressionRoot(contextMock.Object);
			root.Column("Bacon");

			collectionMock.VerifyAll();
			contextMock.VerifyAll();
		}

		[Test]
		public void CallingColumnReturnsCreateColumnExpression()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			var contextMock = new Mock<IMigrationContext>();
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);
			contextMock.VerifyGet(x => x.Expressions, Times.AtMostOnce());

			var root = new CreateExpressionRoot(contextMock.Object);
			var builder = root.Column("Bacon");

			builder.ShouldBeOfType<CreateColumnExpressionBuilder>();
			contextMock.VerifyAll();
		}

		[Test]
		public void CallingForeignKeyWithoutNameAddsCreateForeignKeyExpressionToContext()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
            collectionMock.Verify(x => x.Add(It.IsAny<CreateForeignKeyExpression>()), Times.AtMostOnce());

			var contextMock = new Mock<IMigrationContext>();
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);
			contextMock.VerifyGet(x => x.Expressions, Times.AtMostOnce());

			var root = new CreateExpressionRoot(contextMock.Object);
			root.ForeignKey();

			collectionMock.VerifyAll();
			contextMock.VerifyAll();
		}

		[Test]
		public void CallingForeignKeyAddsCreateForeignKeyExpressionToContextWithSpecifiedNameSet()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
            collectionMock.Verify(x => x.Add(It.Is<CreateForeignKeyExpression>(e => e.ForeignKey.Name.Equals("FK_Bacon"))), Times.AtMostOnce());

			var contextMock = new Mock<IMigrationContext>();
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);
			contextMock.VerifyGet(x => x.Expressions, Times.AtMostOnce());

			var root = new CreateExpressionRoot(contextMock.Object);
			root.ForeignKey("FK_Bacon");

			collectionMock.VerifyAll();
			contextMock.VerifyAll();
		}

		[Test]
		public void CallingForeignKeyWithoutNameReturnsCreateForeignKeyExpression()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			var contextMock = new Mock<IMigrationContext>();
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);
			contextMock.VerifyGet(x => x.Expressions, Times.AtMostOnce());

			var root = new CreateExpressionRoot(contextMock.Object);
			var builder = root.ForeignKey();

			builder.ShouldBeOfType<CreateForeignKeyExpressionBuilder>();
			contextMock.VerifyAll();
		}

		[Test]
		public void CallingForeignKeyCreatesCreateForeignKeyExpression()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			var contextMock = new Mock<IMigrationContext>();
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);
			contextMock.VerifyGet(x => x.Expressions, Times.AtMostOnce());

			var root = new CreateExpressionRoot(contextMock.Object);
			var builder = root.ForeignKey("FK_Bacon");

			builder.ShouldBeOfType<CreateForeignKeyExpressionBuilder>();
			contextMock.VerifyAll();
		}

		[Test]
		public void CallingIndexWithoutNameAddsCreateIndexExpressionToContext()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
            collectionMock.Verify(x => x.Add(It.IsAny<CreateIndexExpression>()), Times.AtMostOnce());

			var contextMock = new Mock<IMigrationContext>();
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);
			contextMock.VerifyGet(x => x.Expressions, Times.AtMostOnce());

			var root = new CreateExpressionRoot(contextMock.Object);
			root.Index();

			collectionMock.VerifyAll();
			contextMock.VerifyAll();
		}

		[Test]
		public void CallingIndexAddsCreateIndexExpressionToContextWithSpecifiedNameSet()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
            collectionMock.Verify(x => x.Add(It.Is<CreateIndexExpression>(e => e.Index.Name.Equals("IX_Bacon"))), Times.AtMostOnce());

			var contextMock = new Mock<IMigrationContext>();
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);
			contextMock.VerifyGet(x => x.Expressions, Times.AtMostOnce());

			var root = new CreateExpressionRoot(contextMock.Object);
			root.Index("IX_Bacon");

			collectionMock.VerifyAll();
			contextMock.VerifyAll();
		}

		[Test]
		public void CallingIndexWithoutNameReturnsCreateIndexExpression()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			var contextMock = new Mock<IMigrationContext>();
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);
			contextMock.VerifyGet(x => x.Expressions, Times.AtMostOnce());

			var root = new CreateExpressionRoot(contextMock.Object);
			var builder = root.Index();

			builder.ShouldBeOfType<CreateIndexExpressionBuilder>();
			contextMock.VerifyAll();
		}

		[Test]
		public void CallingIndexCreatesCreateIndexExpression()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			var contextMock = new Mock<IMigrationContext>();
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);
			contextMock.VerifyGet(x => x.Expressions, Times.AtMostOnce());

			var root = new CreateExpressionRoot(contextMock.Object);
			var builder = root.Index("IX_Bacon");

			builder.ShouldBeOfType<CreateIndexExpressionBuilder>();
			contextMock.VerifyAll();
		}

        [Test]
        public void CallingUniqueConstraintShouldCreateAConstraintExpressionWithATypeOfUnique()
        {
            var contextMock = new Mock<IMigrationContext>();
            var root = new CreateExpressionRoot(contextMock.Object);
            var builder = root.UniqueConstraint();
            builder.ShouldBeOfType<CreateConstraintExpression>();
        }

	}
}