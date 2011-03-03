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
using FluentMigrator.Builders;
using FluentMigrator.Builders.Delete.ForeignKey;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using Moq;
using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Builders.Delete
{
	[TestFixture]
	public class DeleteForeignKeyExpressionBuilderTests
	{
		[Test]
		public void CallingFromTableSetsForeignTableName()
		{
			var foreignKeyMock = new Mock<ForeignKeyDefinition>();
			foreignKeyMock.SetupSet(f => f.ForeignTable = "Bacon").AtMostOnce();

			var expressionMock = new Mock<DeleteForeignKeyExpression>();
			expressionMock.SetupGet(e => e.ForeignKey).Returns(foreignKeyMock.Object).AtMostOnce();

			var builder = new DeleteForeignKeyExpressionBuilder(expressionMock.Object);
			builder.FromTable("Bacon");

			foreignKeyMock.VerifyAll();
			expressionMock.VerifyAll();
		}

		[Test]
		public void CallingToTableSetsPrimaryTableName()
		{
			var foreignKeyMock = new Mock<ForeignKeyDefinition>();
			foreignKeyMock.SetupSet(f => f.PrimaryTable = "Bacon").AtMostOnce();

			var expressionMock = new Mock<DeleteForeignKeyExpression>();
			expressionMock.SetupGet(e => e.ForeignKey).Returns(foreignKeyMock.Object).AtMostOnce();

			var builder = new DeleteForeignKeyExpressionBuilder(expressionMock.Object);
			builder.ToTable("Bacon");

			foreignKeyMock.VerifyAll();
			expressionMock.VerifyAll();
		}

        [Test]
        public void CallingOnTableSetsForeignTableName()
        {
            var foreignKeyMock = new Mock<ForeignKeyDefinition>();
            foreignKeyMock.SetupSet(f => f.ForeignTable = "Bacon").AtMostOnce();

            var expressionMock = new Mock<DeleteForeignKeyExpression>();
            expressionMock.SetupGet(e => e.ForeignKey).Returns(foreignKeyMock.Object).AtMostOnce();

            var builder = new DeleteForeignKeyExpressionBuilder(expressionMock.Object);
            ((IDeleteForeignKeyOnTableSyntax)builder).OnTable(("Bacon"));

            foreignKeyMock.VerifyAll();
            expressionMock.VerifyAll();
        }

        [Test]
        public void CallingInSchemaSetsForeignTableSchemaName()
        {
            var foreignKeyMock = new Mock<ForeignKeyDefinition>();
            foreignKeyMock.SetupSet(f => f.ForeignTableSchema = "Bacon").AtMostOnce();

            var expressionMock = new Mock<DeleteForeignKeyExpression>();
            expressionMock.SetupGet(e => e.ForeignKey).Returns(foreignKeyMock.Object).AtMostOnce();

            var builder = new DeleteForeignKeyExpressionBuilder(expressionMock.Object);
            ((IInSchemaSyntax)builder).InSchema("Bacon");

            foreignKeyMock.VerifyAll();
            expressionMock.VerifyAll();
        }

		[Test]
		public void CallingForeignColumnAddsColumnNameToForeignColumnCollection()
		{
			var collectionMock = new Mock<IList<string>>();
			collectionMock.Setup(x => x.Add("BaconId")).AtMostOnce();

			var foreignKeyMock = new Mock<ForeignKeyDefinition>();
			foreignKeyMock.SetupGet(f => f.ForeignColumns).Returns(collectionMock.Object).AtMostOnce();

			var expressionMock = new Mock<DeleteForeignKeyExpression>();
			expressionMock.SetupGet(e => e.ForeignKey).Returns(foreignKeyMock.Object).AtMostOnce();

			var builder = new DeleteForeignKeyExpressionBuilder(expressionMock.Object);
			builder.ForeignColumn("BaconId");

			collectionMock.VerifyAll();
			foreignKeyMock.VerifyAll();
			expressionMock.VerifyAll();
		}

		[Test]
		public void CallingForeignColumnsAddsColumnNamesToForeignColumnCollection()
		{
			var collectionMock = new Mock<IList<string>>();
			collectionMock.Setup(x => x.Add("BaconId")).AtMostOnce();
			collectionMock.Setup(x => x.Add("EggsId")).AtMostOnce();

			var foreignKeyMock = new Mock<ForeignKeyDefinition>();
			foreignKeyMock.SetupGet(f => f.ForeignColumns).Returns(collectionMock.Object).AtMost(2);

			var expressionMock = new Mock<DeleteForeignKeyExpression>();
			expressionMock.SetupGet(e => e.ForeignKey).Returns(foreignKeyMock.Object).AtMost(2);

			var builder = new DeleteForeignKeyExpressionBuilder(expressionMock.Object);
			builder.ForeignColumns("BaconId", "EggsId");

			collectionMock.VerifyAll();
			foreignKeyMock.VerifyAll();
			expressionMock.VerifyAll();
		}

		[Test]
		public void CallingPrimaryColumnAddsColumnNameToPrimaryColumnCollection()
		{
			var collectionMock = new Mock<IList<string>>();
			collectionMock.Setup(x => x.Add("BaconId")).AtMostOnce();

			var foreignKeyMock = new Mock<ForeignKeyDefinition>();
			foreignKeyMock.SetupGet(f => f.PrimaryColumns).Returns(collectionMock.Object).AtMostOnce();

			var expressionMock = new Mock<DeleteForeignKeyExpression>();
			expressionMock.SetupGet(e => e.ForeignKey).Returns(foreignKeyMock.Object).AtMostOnce();

			var builder = new DeleteForeignKeyExpressionBuilder(expressionMock.Object);
			builder.PrimaryColumn("BaconId");

			collectionMock.VerifyAll();
			foreignKeyMock.VerifyAll();
			expressionMock.VerifyAll();
		}

		[Test]
		public void CallingPrimaryColumnsAddsColumnNamesToForeignColumnCollection()
		{
			var collectionMock = new Mock<IList<string>>();
			collectionMock.Setup(x => x.Add("BaconId")).AtMostOnce();
			collectionMock.Setup(x => x.Add("EggsId")).AtMostOnce();

			var foreignKeyMock = new Mock<ForeignKeyDefinition>();
			foreignKeyMock.SetupGet(f => f.PrimaryColumns).Returns(collectionMock.Object).AtMost(2);

			var expressionMock = new Mock<DeleteForeignKeyExpression>();
			expressionMock.SetupGet(e => e.ForeignKey).Returns(foreignKeyMock.Object).AtMost(2);

			var builder = new DeleteForeignKeyExpressionBuilder(expressionMock.Object);
			builder.PrimaryColumns("BaconId", "EggsId");

			collectionMock.VerifyAll();
			foreignKeyMock.VerifyAll();
			expressionMock.VerifyAll();
		}
	}
}