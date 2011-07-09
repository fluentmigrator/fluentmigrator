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

using System;
using System.Collections.Generic;
using FluentMigrator.Builders.Create.ForeignKey;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using Moq;
using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Builders.Create
{
	[TestFixture]
	public class CreateForeignKeyExpressionBuilderTests
	{
		[Test]
		public void CallingFromTableSetsForeignTableName()
		{
			var foreignKeyMock = new Mock<ForeignKeyDefinition>();
			foreignKeyMock.VerifySet(f => f.TableContainingForeignKey = "Bacon", Times.AtMostOnce());

			var expressionMock = new Mock<CreateForeignKeyExpression>();
			expressionMock.SetupGet(e => e.ForeignKey).Returns(foreignKeyMock.Object);
            expressionMock.VerifyGet(e => e.ForeignKey, Times.AtMostOnce());

			var builder = new CreateForeignKeyExpressionBuilder(expressionMock.Object);
			builder.FromTable("Bacon");

			foreignKeyMock.VerifyAll();
			expressionMock.VerifyAll();
		}

		[Test]
		public void CallingToTableSetsPrimaryTableName()
		{
			var foreignKeyMock = new Mock<ForeignKeyDefinition>();
            foreignKeyMock.VerifySet(f => f.TableContainingPrimayKey = "Bacon", Times.AtMostOnce());

			var expressionMock = new Mock<CreateForeignKeyExpression>();
            expressionMock.Setup(e => e.ForeignKey).Returns(foreignKeyMock.Object);
			expressionMock.VerifyGet(e => e.ForeignKey, Times.AtMostOnce());

			var builder = new CreateForeignKeyExpressionBuilder(expressionMock.Object);
			builder.ToTable("Bacon");

			foreignKeyMock.VerifyAll();
			expressionMock.VerifyAll();
		}

		[Test]
		public void CallingForeignColumnAddsColumnNameToForeignColumnCollection()
		{
			var collectionMock = new Mock<IList<string>>();
            collectionMock.Verify(x => x.Add("BaconId"), Times.AtMostOnce());

			var foreignKeyMock = new Mock<ForeignKeyDefinition>();
            foreignKeyMock.SetupGet(f => f.ColumnsInForeignKeyTableToInclude).Returns(collectionMock.Object);
			foreignKeyMock.VerifyGet(f => f.ColumnsInForeignKeyTableToInclude, Times.AtMostOnce());

			var expressionMock = new Mock<CreateForeignKeyExpression>();
            expressionMock.SetupGet(e => e.ForeignKey).Returns(foreignKeyMock.Object);
			expressionMock.VerifyGet(e => e.ForeignKey, Times.AtMostOnce());

			var builder = new CreateForeignKeyExpressionBuilder(expressionMock.Object);
			builder.ForeignColumn("BaconId");

			collectionMock.VerifyAll();
			foreignKeyMock.VerifyAll();
			expressionMock.VerifyAll();
		}

		[Test]
		public void CallingForeignColumnsAddsColumnNamesToForeignColumnCollection()
		{
			var collectionMock = new Mock<IList<string>>();
            collectionMock.Verify(x => x.Add("BaconId"), Times.AtMostOnce());
            collectionMock.Verify(x => x.Add("EggsId"), Times.AtMostOnce());

			var foreignKeyMock = new Mock<ForeignKeyDefinition>();
            foreignKeyMock.Setup(f => f.ColumnsInForeignKeyTableToInclude).Returns(collectionMock.Object);
			foreignKeyMock.VerifyGet(f => f.ColumnsInForeignKeyTableToInclude, Times.AtMost(2));

			var expressionMock = new Mock<CreateForeignKeyExpression>();
            expressionMock.Setup(e => e.ForeignKey).Returns(foreignKeyMock.Object);
			expressionMock.VerifyGet(e => e.ForeignKey, Times.AtMost(2));

			var builder = new CreateForeignKeyExpressionBuilder(expressionMock.Object);
			builder.ForeignColumns("BaconId", "EggsId");

			collectionMock.VerifyAll();
			foreignKeyMock.VerifyAll();
			expressionMock.VerifyAll();
		}

		[Test]
		public void CallingPrimaryColumnAddsColumnNameToPrimaryColumnCollection()
		{
			var collectionMock = new Mock<IList<string>>();
            collectionMock.Verify(x => x.Add("BaconId"), Times.AtMostOnce());

			var foreignKeyMock = new Mock<ForeignKeyDefinition>();
            foreignKeyMock.Setup(f => f.ColumnsInPrimaryKeyTableToInclude).Returns(collectionMock.Object);
			foreignKeyMock.VerifyGet(f => f.ColumnsInPrimaryKeyTableToInclude, Times.AtMostOnce());

			var expressionMock = new Mock<CreateForeignKeyExpression>();
            expressionMock.Setup(e => e.ForeignKey).Returns(foreignKeyMock.Object);
			expressionMock.VerifyGet(e => e.ForeignKey, Times.AtMostOnce());

			var builder = new CreateForeignKeyExpressionBuilder(expressionMock.Object);
			builder.PrimaryColumn("BaconId");

			collectionMock.VerifyAll();
			foreignKeyMock.VerifyAll();
			expressionMock.VerifyAll();
		}

		[Test]
		public void CallingPrimaryColumnsAddsColumnNamesToForeignColumnCollection()
		{
			var collectionMock = new Mock<IList<string>>();
            collectionMock.Verify(x => x.Add("BaconId"), Times.AtMostOnce());
            collectionMock.Verify(x => x.Add("EggsId"), Times.AtMostOnce());

			var foreignKeyMock = new Mock<ForeignKeyDefinition>();
            foreignKeyMock.Setup(f => f.ColumnsInPrimaryKeyTableToInclude).Returns(collectionMock.Object);
			foreignKeyMock.VerifyGet(f => f.ColumnsInPrimaryKeyTableToInclude, Times.AtMost(2));

			var expressionMock = new Mock<CreateForeignKeyExpression>();
            expressionMock.Setup(e => e.ForeignKey).Returns(foreignKeyMock.Object);
			expressionMock.VerifyGet(e => e.ForeignKey, Times.AtMost(2));

			var builder = new CreateForeignKeyExpressionBuilder(expressionMock.Object);
			builder.PrimaryColumns("BaconId", "EggsId");

			collectionMock.VerifyAll();
			foreignKeyMock.VerifyAll();
			expressionMock.VerifyAll();
		}
	}
}