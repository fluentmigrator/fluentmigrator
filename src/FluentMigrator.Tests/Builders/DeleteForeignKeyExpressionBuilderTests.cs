using System;
using System.Collections.Generic;
using FluentMigrator.Builders.Delete.ForeignKey;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using Moq;
using Xunit;

namespace FluentMigrator.Tests.Builders
{
	public class DeleteForeignKeyExpressionBuilderTests
	{
		[Fact]
		public void CallingFromTableSetsForeignTableName()
		{
			var foreignKeyMock = new Mock<ForeignKeyDefinition>();
			foreignKeyMock.ExpectSet(f => f.ForeignTable, "Bacon").AtMostOnce();

			var expressionMock = new Mock<DeleteForeignKeyExpression>();
			expressionMock.ExpectGet(e => e.ForeignKey).Returns(foreignKeyMock.Object).AtMostOnce();

			var builder = new DeleteForeignKeyExpressionBuilder(expressionMock.Object);
			builder.FromTable("Bacon");

			foreignKeyMock.VerifyAll();
			expressionMock.VerifyAll();
		}

		[Fact]
		public void CallingToTableSetsPrimaryTableName()
		{
			var foreignKeyMock = new Mock<ForeignKeyDefinition>();
			foreignKeyMock.ExpectSet(f => f.PrimaryTable, "Bacon").AtMostOnce();

			var expressionMock = new Mock<DeleteForeignKeyExpression>();
			expressionMock.ExpectGet(e => e.ForeignKey).Returns(foreignKeyMock.Object).AtMostOnce();

			var builder = new DeleteForeignKeyExpressionBuilder(expressionMock.Object);
			builder.ToTable("Bacon");

			foreignKeyMock.VerifyAll();
			expressionMock.VerifyAll();
		}

		[Fact]
		public void CallingForeignColumnAddsColumnNameToForeignColumnCollection()
		{
			var collectionMock = new Mock<IList<string>>();
			collectionMock.Expect(x => x.Add("BaconId")).AtMostOnce();

			var foreignKeyMock = new Mock<ForeignKeyDefinition>();
			foreignKeyMock.ExpectGet(f => f.ForeignColumns).Returns(collectionMock.Object).AtMostOnce();

			var expressionMock = new Mock<DeleteForeignKeyExpression>();
			expressionMock.ExpectGet(e => e.ForeignKey).Returns(foreignKeyMock.Object).AtMostOnce();

			var builder = new DeleteForeignKeyExpressionBuilder(expressionMock.Object);
			builder.ForeignColumn("BaconId");

			collectionMock.VerifyAll();
			foreignKeyMock.VerifyAll();
			expressionMock.VerifyAll();
		}

		[Fact]
		public void CallingForeignColumnsAddsColumnNamesToForeignColumnCollection()
		{
			var collectionMock = new Mock<IList<string>>();
			collectionMock.Expect(x => x.Add("BaconId")).AtMostOnce();
			collectionMock.Expect(x => x.Add("EggsId")).AtMostOnce();

			var foreignKeyMock = new Mock<ForeignKeyDefinition>();
			foreignKeyMock.ExpectGet(f => f.ForeignColumns).Returns(collectionMock.Object).AtMost(2);

			var expressionMock = new Mock<DeleteForeignKeyExpression>();
			expressionMock.ExpectGet(e => e.ForeignKey).Returns(foreignKeyMock.Object).AtMost(2);

			var builder = new DeleteForeignKeyExpressionBuilder(expressionMock.Object);
			builder.ForeignColumns("BaconId", "EggsId");

			collectionMock.VerifyAll();
			foreignKeyMock.VerifyAll();
			expressionMock.VerifyAll();
		}

		[Fact]
		public void CallingPrimaryColumnAddsColumnNameToPrimaryColumnCollection()
		{
			var collectionMock = new Mock<IList<string>>();
			collectionMock.Expect(x => x.Add("BaconId")).AtMostOnce();

			var foreignKeyMock = new Mock<ForeignKeyDefinition>();
			foreignKeyMock.ExpectGet(f => f.PrimaryColumns).Returns(collectionMock.Object).AtMostOnce();

			var expressionMock = new Mock<DeleteForeignKeyExpression>();
			expressionMock.ExpectGet(e => e.ForeignKey).Returns(foreignKeyMock.Object).AtMostOnce();

			var builder = new DeleteForeignKeyExpressionBuilder(expressionMock.Object);
			builder.PrimaryColumn("BaconId");

			collectionMock.VerifyAll();
			foreignKeyMock.VerifyAll();
			expressionMock.VerifyAll();
		}

		[Fact]
		public void CallingPrimaryColumnsAddsColumnNamesToForeignColumnCollection()
		{
			var collectionMock = new Mock<IList<string>>();
			collectionMock.Expect(x => x.Add("BaconId")).AtMostOnce();
			collectionMock.Expect(x => x.Add("EggsId")).AtMostOnce();

			var foreignKeyMock = new Mock<ForeignKeyDefinition>();
			foreignKeyMock.ExpectGet(f => f.PrimaryColumns).Returns(collectionMock.Object).AtMost(2);

			var expressionMock = new Mock<DeleteForeignKeyExpression>();
			expressionMock.ExpectGet(e => e.ForeignKey).Returns(foreignKeyMock.Object).AtMost(2);

			var builder = new DeleteForeignKeyExpressionBuilder(expressionMock.Object);
			builder.PrimaryColumns("BaconId", "EggsId");

			collectionMock.VerifyAll();
			foreignKeyMock.VerifyAll();
			expressionMock.VerifyAll();
		}
	}
}