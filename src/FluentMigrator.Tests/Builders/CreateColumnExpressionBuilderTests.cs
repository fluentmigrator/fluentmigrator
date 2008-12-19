using System;
using System.Data;
using System.Linq.Expressions;
using FluentMigrator.Builders.Create.Column;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using Moq;
using Moq.Stub;
using Xunit;

namespace FluentMigrator.Tests.Builders
{
	public class CreateColumnExpressionBuilderTests
	{
		[Fact]
		public void CallingOnTableSetsTableName()
		{
			var expressionMock = new Mock<CreateColumnExpression>();
			expressionMock.ExpectSet(x => x.TableName, "Bacon").AtMostOnce();

			var builder = new CreateColumnExpressionBuilder(expressionMock.Object);
			builder.OnTable("Bacon");

			expressionMock.VerifyAll();
		}

		[Fact]
		public void CallingAsAnsiStringSetsColumnDbTypeToAnsiString()
		{
			VerifyColumnDbType(DbType.AnsiString, b => b.AsAnsiString());
		}

		[Fact]
		public void CallingAsAnsiStringWithSizeSetsColumnDbTypeToAnsiString()
		{
			VerifyColumnDbType(DbType.AnsiString, b => b.AsAnsiString(42));
		}

		[Fact]
		public void CallingAsAnsiStringWithSizeSetsColumnSizeToSpecifiedValue()
		{
			VerifyColumnSize(42, b => b.AsAnsiString(42));
		}

		[Fact]
		public void CallingAsBinarySetsColumnDbTypeToBinary()
		{
			VerifyColumnDbType(DbType.Binary, b => b.AsBinary());
		}

		[Fact]
		public void CallingAsBinaryWithSizeSetsColumnDbTypeToBinary()
		{
			VerifyColumnDbType(DbType.Binary, b => b.AsBinary(42));
		}

		[Fact]
		public void CallingAsBinaryWithSizeSetsColumnSizeToSpecifiedValue()
		{
			VerifyColumnSize(42, b => b.AsBinary(42));
		}

		[Fact]
		public void CallingAsBooleanSetsColumnDbTypeToBoolean()
		{
			VerifyColumnDbType(DbType.Boolean, b => b.AsBoolean());
		}

		[Fact]
		public void CallingAsByteSetsColumnDbTypeToByte()
		{
			VerifyColumnDbType(DbType.Byte, b => b.AsByte());
		}

		[Fact]
		public void CallingAsCurrencySetsColumnDbTypeToCurrency()
		{
			VerifyColumnDbType(DbType.Currency, b => b.AsCurrency());
		}

		[Fact]
		public void CallingAsDateSetsColumnDbTypeToDate()
		{
			VerifyColumnDbType(DbType.Date, b => b.AsDate());
		}

		[Fact]
		public void CallingAsDateTimeSetsColumnDbTypeToDateTime()
		{
			VerifyColumnDbType(DbType.DateTime, b => b.AsDateTime());
		}

		[Fact]
		public void CallingAsDecimalSetsColumnDbTypeToDecimal()
		{
			VerifyColumnDbType(DbType.Decimal, b => b.AsDecimal());
		}

		[Fact]
		public void CallingAsDecimalWithSizeAndPrecisionSetsColumnDbTypeToDecimal()
		{
			VerifyColumnDbType(DbType.Decimal, b => b.AsDecimal(1, 2));
		}

		[Fact]
		public void CallingAsDecimalStringSetsColumnSizeToSpecifiedValue()
		{
			VerifyColumnSize(1, b => b.AsDecimal(1, 2));
		}

		[Fact]
		public void CallingAsDecimalStringSetsColumnPrecisionToSpecifiedValue()
		{
			VerifyColumnPrecision(2, b => b.AsDecimal(1, 2));
		}

		[Fact]
		public void CallingAsDoubleSetsColumnDbTypeToDouble()
		{
			VerifyColumnDbType(DbType.Double, b => b.AsDouble());
		}

		[Fact]
		public void CallingAsGuidSetsColumnDbTypeToGuid()
		{
			VerifyColumnDbType(DbType.Guid, b => b.AsGuid());
		}

		[Fact]
		public void CallingAsFixedLengthStringSetsColumnDbTypeToStringFixedLength()
		{
			VerifyColumnDbType(DbType.StringFixedLength, e => e.AsFixedLengthString(255));
		}

		[Fact]
		public void CallingAsFixedLengthStringSetsColumnSizeToSpecifiedValue()
		{
			VerifyColumnSize(255, b => b.AsFixedLengthString(255));
		}

		[Fact]
		public void CallingAsFixedLengthAnsiStringSetsColumnDbTypeToAnsiStringFixedLength()
		{
			VerifyColumnDbType(DbType.AnsiStringFixedLength, b => b.AsFixedLengthAnsiString(255));
		}

		[Fact]
		public void CallingAsFixedLengthAnsiStringSetsColumnSizeToSpecifiedValue()
		{
			VerifyColumnSize(255, b => b.AsFixedLengthAnsiString(255));
		}

		[Fact]
		public void CallingAsFloatSetsColumnDbTypeToSingle()
		{
			VerifyColumnDbType(DbType.Single, b => b.AsFloat());
		}

		[Fact]
		public void CallingAsInt16SetsColumnDbTypeToInt16()
		{
			VerifyColumnDbType(DbType.Int16, b => b.AsInt16());
		}

		[Fact]
		public void CallingAsInt32SetsColumnDbTypeToInt32()
		{
			VerifyColumnDbType(DbType.Int32, b => b.AsInt32());
		}

		[Fact]
		public void CallingAsInt64SetsColumnDbTypeToInt64()
		{
			VerifyColumnDbType(DbType.Int64, b => b.AsInt64());
		}

		[Fact]
		public void CallingAsStringSetsColumnDbTypeToString()
		{
			VerifyColumnDbType(DbType.String, b => b.AsString());
		}

		[Fact]
		public void CallingAsStringWithLengthSetsColumnDbTypeToString()
		{
			VerifyColumnDbType(DbType.String, b => b.AsString(255));
		}

		[Fact]
		public void CallingAsStringSetsColumnSizeToSpecifiedValue()
		{
			VerifyColumnSize(255, b => b.AsFixedLengthAnsiString(255));
		}

		[Fact]
		public void CallingAsTimeSetsColumnDbTypeToTime()
		{
			VerifyColumnDbType(DbType.Time, b => b.AsTime());
		}

		[Fact]
		public void CallingAsXmlSetsColumnDbTypeToXml()
		{
			VerifyColumnDbType(DbType.Xml, b => b.AsXml());
		}

		[Fact]
		public void CallingAsXmlWithSizeSetsColumnDbTypeToXml()
		{
			VerifyColumnDbType(DbType.Xml, b => b.AsXml(255));
		}

		[Fact]
		public void CallingAsXmlSetsColumnSizeToSpecifiedValue()
		{
			VerifyColumnSize(255, b => b.AsXml(255));
		}

		[Fact]
		public void CallingWithDefaultValueSetsDefaultValue()
		{
			const int value = 42;

			var columnMock = new Mock<ColumnDefinition>();
			columnMock.ExpectSet(c => c.DefaultValue, value).AtMostOnce();

			var expressionMock = new Mock<CreateColumnExpression>();
			expressionMock.Stub(e => e.Column);

			var expression = expressionMock.Object;
			expression.Column = columnMock.Object;

			var builder = new CreateColumnExpressionBuilder(expressionMock.Object);
			builder.WithDefaultValue(value);

			columnMock.VerifyAll();
		}

		[Fact]
		public void CallingForeignKeySetsIsForeignKeyToTrue()
		{
			VerifyColumnProperty(true, c => c.IsForeignKey, b => b.ForeignKey());
		}

		[Fact]
		public void CallingIdentitySetsIsIdentityToTrue()
		{
			VerifyColumnProperty(true, c => c.IsIdentity, b => b.Identity());
		}

		[Fact]
		public void CallingIndexedSetsIsIndexedToTrue()
		{
			VerifyColumnProperty(true, c => c.IsIndexed, b => b.Indexed());
		}

		[Fact]
		public void CallingPrimaryKeySetsIsPrimaryKeyToTrue()
		{
			VerifyColumnProperty(true, c => c.IsPrimaryKey, b => b.PrimaryKey());
		}

		[Fact]
		public void CallingNullableSetsIsNullableToTrue()
		{
			VerifyColumnProperty(true, c => c.IsNullable, b => b.Nullable());
		}

		[Fact]
		public void CallingNotNullableSetsIsNullableToFalse()
		{
			VerifyColumnProperty(false, c => c.IsNullable, b => b.NotNullable());
		}

		[Fact]
		public void CallingUniqueSetsIsUniqueToTrue()
		{
			VerifyColumnProperty(true, c => c.IsUnique, b => b.Unique());
		}

		private void VerifyColumnProperty<T>(T expected, Expression<Func<ColumnDefinition, T>> columnExpression, Action<CreateColumnExpressionBuilder> callToTest)
		{
			var columnMock = new Mock<ColumnDefinition>();
			columnMock.ExpectSet(columnExpression, expected).AtMostOnce();

			var expressionMock = new Mock<CreateColumnExpression>();
			expressionMock.Stub(e => e.Column);

			var expression = expressionMock.Object;
			expression.Column = columnMock.Object;

			callToTest(new CreateColumnExpressionBuilder(expression));

			columnMock.VerifyAll();
		}

		private void VerifyColumnDbType(DbType expected, Action<CreateColumnExpressionBuilder> callToTest)
		{
			var columnMock = new Mock<ColumnDefinition>();
			columnMock.ExpectSet(c => c.Type, expected).AtMostOnce();

			var expressionMock = new Mock<CreateColumnExpression>();
			expressionMock.Stub(e => e.Column);

			var expression = expressionMock.Object;
			expression.Column = columnMock.Object;

			callToTest(new CreateColumnExpressionBuilder(expression));

			columnMock.VerifyAll();
		}

		private void VerifyColumnSize(int expected, Action<CreateColumnExpressionBuilder> callToTest)
		{
			var columnMock = new Mock<ColumnDefinition>();
			columnMock.ExpectSet(c => c.Size, expected).AtMostOnce();

			var expressionMock = new Mock<CreateColumnExpression>();
			expressionMock.Stub(e => e.Column);

			var expression = expressionMock.Object;
			expression.Column = columnMock.Object;

			callToTest(new CreateColumnExpressionBuilder(expression));

			columnMock.VerifyAll();
		}

		private void VerifyColumnPrecision(int expected, Action<CreateColumnExpressionBuilder> callToTest)
		{
			var columnMock = new Mock<ColumnDefinition>();
			columnMock.ExpectSet(c => c.Precision, expected).AtMostOnce();

			var expressionMock = new Mock<CreateColumnExpression>();
			expressionMock.Stub(e => e.Column);

			var expression = expressionMock.Object;
			expression.Column = columnMock.Object;

			callToTest(new CreateColumnExpressionBuilder(expression));

			columnMock.VerifyAll();
		}
	}
}