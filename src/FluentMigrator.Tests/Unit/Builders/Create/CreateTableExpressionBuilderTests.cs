using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using FluentMigrator.Builders.Create.Table;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using Moq;
using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Builders.Create
{
	[TestFixture]
	public class CreateTableExpressionBuilderTests
	{
		[Test]
		public void CallingAsAnsiStringSetsColumnDbTypeToAnsiString()
		{
			VerifyColumnDbType(DbType.AnsiString, b => b.AsAnsiString());
		}

		[Test]
		public void CallingAsAnsiStringWithSizeSetsColumnDbTypeToAnsiString()
		{
			VerifyColumnDbType(DbType.AnsiString, b => b.AsAnsiString(255));
		}

		[Test]
		public void CallingAsAnsiStringSetsColumnSizeToSpecifiedValue()
		{
			VerifyColumnSize(255, b => b.AsAnsiString(255));
		}

		[Test]
		public void CallingAsBinarySetsColumnDbTypeToBinary()
		{
			VerifyColumnDbType(DbType.Binary, b => b.AsBinary(255));
		}

		[Test]
		public void CallingAsBinarySetsColumnSizeToSpecifiedValue()
		{
			VerifyColumnSize(255, b => b.AsBinary(255));
		}

		[Test]
		public void CallingAsBooleanSetsColumnDbTypeToBoolean()
		{
			VerifyColumnDbType(DbType.Boolean, b => b.AsBoolean());
		}

		[Test]
		public void CallingAsByteSetsColumnDbTypeToByte()
		{
			VerifyColumnDbType(DbType.Byte, b => b.AsByte());
		}

		[Test]
		public void CallingAsCurrencySetsColumnDbTypeToCurrency()
		{
			VerifyColumnDbType(DbType.Currency, b => b.AsCurrency());
		}

		[Test]
		public void CallingAsDateSetsColumnDbTypeToDate()
		{
			VerifyColumnDbType(DbType.Date, b => b.AsDate());
		}

		[Test]
		public void CallingAsDateTimeSetsColumnDbTypeToDateTime()
		{
			VerifyColumnDbType(DbType.DateTime, b => b.AsDateTime());
		}

		[Test]
		public void CallingAsDecimalSetsColumnDbTypeToDecimal()
		{
			VerifyColumnDbType(DbType.Decimal, b => b.AsDecimal());
		}

		[Test]
		public void CallingAsDecimalWithSizeSetsColumnDbTypeToDecimal()
		{
			VerifyColumnDbType(DbType.Decimal, b => b.AsDecimal(1, 2));
		}

		[Test]
		public void CallingAsDecimalStringSetsColumnSizeToSpecifiedValue()
		{
			VerifyColumnSize(1, b => b.AsDecimal(1, 2));
		}

		[Test]
		public void CallingAsDecimalStringSetsColumnPrecisionToSpecifiedValue()
		{
			VerifyColumnPrecision(2, b => b.AsDecimal(1, 2));
		}

		[Test]
		public void CallingAsDoubleSetsColumnDbTypeToDouble()
		{
			VerifyColumnDbType(DbType.Double, b => b.AsDouble());
		}

		[Test]
		public void CallingAsGuidSetsColumnDbTypeToGuid()
		{
			VerifyColumnDbType(DbType.Guid, b => b.AsGuid());
		}

		[Test]
		public void CallingAsFixedLengthStringSetsColumnDbTypeToStringFixedLength()
		{
			VerifyColumnDbType(DbType.StringFixedLength, e => e.AsFixedLengthString(255));
		}

		[Test]
		public void CallingAsFixedLengthStringSetsColumnSizeToSpecifiedValue()
		{
			VerifyColumnSize(255, b => b.AsFixedLengthString(255));
		}

		[Test]
		public void CallingAsFixedLengthAnsiStringSetsColumnDbTypeToAnsiStringFixedLength()
		{
			VerifyColumnDbType(DbType.AnsiStringFixedLength, b => b.AsFixedLengthAnsiString(255));
		}

		[Test]
		public void CallingAsFixedLengthAnsiStringSetsColumnSizeToSpecifiedValue()
		{
			VerifyColumnSize(255, b => b.AsFixedLengthAnsiString(255));
		}

		[Test]
		public void CallingAsFloatSetsColumnDbTypeToSingle()
		{
			VerifyColumnDbType(DbType.Single, b => b.AsFloat());
		}

		[Test]
		public void CallingAsInt16SetsColumnDbTypeToInt16()
		{
			VerifyColumnDbType(DbType.Int16, b => b.AsInt16());
		}

		[Test]
		public void CallingAsInt32SetsColumnDbTypeToInt32()
		{
			VerifyColumnDbType(DbType.Int32, b => b.AsInt32());
		}

		[Test]
		public void CallingAsInt64SetsColumnDbTypeToInt64()
		{
			VerifyColumnDbType(DbType.Int64, b => b.AsInt64());
		}

		[Test]
		public void CallingAsStringSetsColumnDbTypeToString()
		{
			VerifyColumnDbType(DbType.String, b => b.AsString());
		}

		[Test]
		public void CallingAsStringWithSizeSetsColumnDbTypeToString()
		{
			VerifyColumnDbType(DbType.String, b => b.AsString(255));
		}

		[Test]
		public void CallingAsStringSetsColumnSizeToSpecifiedValue()
		{
			VerifyColumnSize(255, b => b.AsFixedLengthAnsiString(255));
		}

		[Test]
		public void CallingAsTimeSetsColumnDbTypeToTime()
		{
			VerifyColumnDbType(DbType.Time, b => b.AsTime());
		}

		[Test]
		public void CallingAsXmlSetsColumnDbTypeToXml()
		{
			VerifyColumnDbType(DbType.Xml, b => b.AsXml());
		}

		[Test]
		public void CallingAsXmlWithSizeSetsColumnDbTypeToXml()
		{
			VerifyColumnDbType(DbType.Xml, b => b.AsXml(255));
		}

		[Test]
		public void CallingAsXmlSetsColumnSizeToSpecifiedValue()
		{
			VerifyColumnSize(255, b => b.AsXml(255));
		}

        [Test]
        public void CallingAsCustomSetsTypeToNullAndSetsCustomType()
        {
            this.VerifyColumnProperty(null, c => c.Type, b => b.AsCustom("Test"));
            this.VerifyColumnProperty("Test", c => c.CustomType, b => b.AsCustom("Test"));
        }

		[Test]
		public void CallingWithDefaultValueSetsDefaultValue()
		{
			const int value = 42;

			var columnMock = new Mock<ColumnDefinition>();
			columnMock.SetupSet(c => c.DefaultValue = value).AtMostOnce();

			var expressionMock = new Mock<CreateTableExpression>();

			var builder = new CreateTableExpressionBuilder(expressionMock.Object);
			builder.CurrentColumn = columnMock.Object;
			builder.WithDefaultValue(42);

			columnMock.VerifyAll();
		}

		[Test]
		public void CallingForeignKeySetsIsForeignKeyToTrue()
		{
			VerifyColumnProperty(true, c => c.IsForeignKey, b => b.ForeignKey());
		}

		[Test]
		public void CallingIdentitySetsIsIdentityToTrue()
		{
			VerifyColumnProperty(true, c => c.IsIdentity, b => b.Identity());
		}

		[Test]
		public void CallingIndexedSetsIsIndexedToTrue()
		{
			VerifyColumnProperty(true, c => c.IsIndexed, b => b.Indexed());
		}

		[Test]
		public void CallingPrimaryKeySetsIsPrimaryKeyToTrue()
		{
			VerifyColumnProperty(true, c => c.IsPrimaryKey, b => b.PrimaryKey());
		}

		[Test]
		public void CallingNullableSetsIsNullableToTrue()
		{
			VerifyColumnProperty(true, c => c.IsNullable, b => b.Nullable());
		}

		[Test]
		public void CallingNotNullableSetsIsNullableToFalse()
		{
			VerifyColumnProperty(false, c => c.IsNullable, b => b.NotNullable());
		}

		[Test]
		public void CallingUniqueSetsIsUniqueToTrue()
		{
			VerifyColumnProperty(true, c => c.IsUnique, b => b.Unique());
		}

		[Test]
		public void CallingWithColumnAddsNewColumnToExpression()
		{
			const string name = "BaconId";

			var collectionMock = new Mock<IList<ColumnDefinition>>();
			collectionMock.Setup(x => x.Add(It.Is<ColumnDefinition>(c => c.Name.Equals(name)))).AtMostOnce();

			var expressionMock = new Mock<CreateTableExpression>();
			expressionMock.SetupGet(e => e.Columns).Returns(collectionMock.Object).AtMostOnce();

			var builder = new CreateTableExpressionBuilder(expressionMock.Object);
			builder.WithColumn(name);

			collectionMock.VerifyAll();
			expressionMock.VerifyAll();
		}

		private void VerifyColumnProperty<T>(T expected, Expression<Func<ColumnDefinition, T>> columnExpression, Action<CreateTableExpressionBuilder> callToTest)
		{
			var columnMock = new Mock<ColumnDefinition>();
			columnMock.SetupSet(columnExpression).AtMostOnce();

			var expressionMock = new Mock<CreateTableExpression>();

			var builder = new CreateTableExpressionBuilder(expressionMock.Object);
			builder.CurrentColumn = columnMock.Object;

			callToTest(builder);

			columnMock.VerifyAll();
			expressionMock.VerifyAll();
		}

		private void VerifyColumnDbType(DbType expected, Action<CreateTableExpressionBuilder> callToTest)
		{
			var columnMock = new Mock<ColumnDefinition>();
			columnMock.SetupSet(c => c.Type = expected).AtMostOnce();

			var expressionMock = new Mock<CreateTableExpression>();

			var builder = new CreateTableExpressionBuilder(expressionMock.Object);
			builder.CurrentColumn = columnMock.Object;

			callToTest(builder);

			columnMock.VerifyAll();
			expressionMock.VerifyAll();
		}

		private void VerifyColumnSize(int expected, Action<CreateTableExpressionBuilder> callToTest)
		{
			var columnMock = new Mock<ColumnDefinition>();
			columnMock.SetupSet(c => c.Size = expected).AtMostOnce();

			var expressionMock = new Mock<CreateTableExpression>();

			var builder = new CreateTableExpressionBuilder(expressionMock.Object);
			builder.CurrentColumn = columnMock.Object;

			callToTest(builder);

			columnMock.VerifyAll();
		}

		private void VerifyColumnPrecision(int expected, Action<CreateTableExpressionBuilder> callToTest)
		{
			var columnMock = new Mock<ColumnDefinition>();
			columnMock.SetupSet(c => c.Precision = expected).AtMostOnce();

			var expressionMock = new Mock<CreateTableExpression>();

			var builder = new CreateTableExpressionBuilder(expressionMock.Object);
			builder.CurrentColumn = columnMock.Object;

			callToTest(builder);

			columnMock.VerifyAll();
		}
	}
}