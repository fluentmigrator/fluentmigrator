using System;
using System.Data;
using FluentMigrator.Builders.Create.Column;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using Moq;
using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Builders.Create
{
	public class CreateColumnExpressionBuilderTests
	{
		[Test]
		public void CallingOnTableSetsTableName()
		{
			var expressionMock = new Mock<CreateColumnExpression>();
			expressionMock.SetupSet(x => x.TableName = "Bacon").AtMostOnce();

			var builder = new CreateColumnExpressionBuilder(expressionMock.Object);
			builder.OnTable("Bacon");

			expressionMock.VerifyAll();
		}

		[Test]
		public void CallingAsAnsiStringSetsColumnDbTypeToAnsiString()
		{
			VerifyColumnDbType(DbType.AnsiString, b => b.AsAnsiString());
		}

		[Test]
		public void CallingAsAnsiStringWithSizeSetsColumnDbTypeToAnsiString()
		{
			VerifyColumnDbType(DbType.AnsiString, b => b.AsAnsiString(42));
		}

		[Test]
		public void CallingAsAnsiStringWithSizeSetsColumnSizeToSpecifiedValue()
		{
			VerifyColumnSize(42, b => b.AsAnsiString(42));
		}

		[Test]
		public void CallingAsBinarySetsColumnDbTypeToBinary()
		{
			VerifyColumnDbType(DbType.Binary, b => b.AsBinary());
		}

		[Test]
		public void CallingAsBinaryWithSizeSetsColumnDbTypeToBinary()
		{
			VerifyColumnDbType(DbType.Binary, b => b.AsBinary(42));
		}

		[Test]
		public void CallingAsBinaryWithSizeSetsColumnSizeToSpecifiedValue()
		{
			VerifyColumnSize(42, b => b.AsBinary(42));
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
		public void CallingAsDecimalWithSizeAndPrecisionSetsColumnDbTypeToDecimal()
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
		public void CallingAsStringWithLengthSetsColumnDbTypeToString()
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
		public void CallingWithDefaultValueSetsDefaultValue()
		{
			const int value = 42;

			var columnMock = new Mock<ColumnDefinition>();
			columnMock.SetupSet(c => c.DefaultValue = value).AtMostOnce();

			var expressionMock = new Mock<CreateColumnExpression>();
			expressionMock.SetupProperty(e => e.Column);

			var expression = expressionMock.Object;
			expression.Column = columnMock.Object;

			var builder = new CreateColumnExpressionBuilder(expressionMock.Object);
			builder.WithDefaultValue(value);

			columnMock.VerifyAll();
		}

		[Test]
		public void CallingForeignKeySetsIsForeignKeyToTrue()
		{
			VerifyColumnProperty(c => c.IsForeignKey = true, b => b.ForeignKey());
		}

		[Test]
		public void CallingIdentitySetsIsIdentityToTrue()
		{
			VerifyColumnProperty(c => c.IsIdentity = true, b => b.Identity());
		}

		[Test]
		public void CallingIndexedSetsIsIndexedToTrue()
		{
			VerifyColumnProperty(c => c.IsIndexed = true, b => b.Indexed());
		}

		[Test]
		public void CallingPrimaryKeySetsIsPrimaryKeyToTrue()
		{
			VerifyColumnProperty(c => c.IsPrimaryKey = true, b => b.PrimaryKey());
		}

		[Test]
		public void CallingNullableSetsIsNullableToTrue()
		{
			VerifyColumnProperty(c => c.IsNullable = true, b => b.Nullable());
		}

		[Test]
		public void CallingNotNullableSetsIsNullableToFalse()
		{
			VerifyColumnProperty(c => c.IsNullable = false, b => b.NotNullable());
		}

		[Test]
		public void CallingUniqueSetsIsUniqueToTrue()
		{
			VerifyColumnProperty(c => c.IsUnique = true, b => b.Unique());
		}

		private void VerifyColumnProperty(Action<ColumnDefinition> columnExpression, Action<CreateColumnExpressionBuilder> callToTest)
		{
			var columnMock = new Mock<ColumnDefinition>();
			columnMock.SetupSet(columnExpression).AtMostOnce();

			var expressionMock = new Mock<CreateColumnExpression>();
			expressionMock.SetupProperty(e => e.Column);

			var expression = expressionMock.Object;
			expression.Column = columnMock.Object;

			callToTest(new CreateColumnExpressionBuilder(expression));

			columnMock.VerifyAll();
		}

		private void VerifyColumnDbType(DbType expected, Action<CreateColumnExpressionBuilder> callToTest)
		{
			var columnMock = new Mock<ColumnDefinition>();
			columnMock.SetupSet(c => c.Type = expected).AtMostOnce();

			var expressionMock = new Mock<CreateColumnExpression>();
			expressionMock.SetupProperty(e => e.Column);

			var expression = expressionMock.Object;
			expression.Column = columnMock.Object;

			callToTest(new CreateColumnExpressionBuilder(expression));

			columnMock.VerifyAll();
		}

		private void VerifyColumnSize(int expected, Action<CreateColumnExpressionBuilder> callToTest)
		{
			var columnMock = new Mock<ColumnDefinition>();
			columnMock.SetupSet(c => c.Size = expected).AtMostOnce();

			var expressionMock = new Mock<CreateColumnExpression>();
			expressionMock.SetupProperty(e => e.Column);

			var expression = expressionMock.Object;
			expression.Column = columnMock.Object;

			callToTest(new CreateColumnExpressionBuilder(expression));

			columnMock.VerifyAll();
		}

		private void VerifyColumnPrecision(int expected, Action<CreateColumnExpressionBuilder> callToTest)
		{
			var columnMock = new Mock<ColumnDefinition>();
			columnMock.SetupSet(c => c.Precision = expected).AtMostOnce();

			var expressionMock = new Mock<CreateColumnExpression>();
			expressionMock.SetupProperty(e => e.Column);

			var expression = expressionMock.Object;
			expression.Column = columnMock.Object;

			callToTest(new CreateColumnExpressionBuilder(expression));

			columnMock.VerifyAll();
		}
	}
}