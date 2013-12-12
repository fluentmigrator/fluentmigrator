﻿using System;
using System.Globalization;
using System.Threading;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.Generic;
using FluentMigrator.Runner.Generators.Jet;
using FluentMigrator.Runner.Generators.MySql;
using FluentMigrator.Runner.Generators.Oracle;
using FluentMigrator.Runner.Generators.SQLite;
using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators
{
    [TestFixture]
    public class ConstantFormatterTests
    {
        [SetUp]
        public void SetUp()
        {
            quoter = new GenericQuoter();
        }

        private IQuoter quoter = default(GenericQuoter);
        private readonly CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;

        private void RestoreCulture()
        {
            Thread.CurrentThread.CurrentCulture = currentCulture;
        }

        private void ChangeCulture()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("nb-NO");
        }

        private enum Foo
        {
            Bar,
            Baz
        }

        private class CustomClass
        {
            public override string ToString()
            {
                return "CustomClass";
            }
        }

        [Test]
        public void CanEscapeAString()
        {
            quoter.Quote("Test\"String").ShouldBe("\"Test\"\"String\"");
        }

        [Test]
        public void CanHandleAnAlreadyQuotedColumnName()
        {
            quoter.QuoteColumnName("\"ColumnName\"").ShouldBe("\"ColumnName\"");
        }

        [Test]
        public void CanHandleAnAlreadyQuotedSchemaName()
        {
            quoter.QuoteColumnName("\"SchemaName\"").ShouldBe("\"SchemaName\"");
        }

        [Test]
        public void CanHandleAnAlreadyQuotedTableName()
        {
            quoter.QuoteColumnName("\"TableName\"").ShouldBe("\"TableName\"");
        }

        [Test]
        public void CanHandleAnUnQuotedColumnName()
        {
            quoter.QuoteColumnName("ColumnName").ShouldBe("\"ColumnName\"");
        }

        [Test]
        public void CanHandleAnUnQuotedSchemaName()
        {
            quoter.QuoteColumnName("SchemaName").ShouldBe("\"SchemaName\"");
        }

        [Test]
        public void CanHandleAnUnQuotedTableName()
        {
            quoter.QuoteColumnName("TableName").ShouldBe("\"TableName\"");
        }

        [Test]
        public void CanQuoteAString()
        {
            quoter.Quote("TestString").ShouldBe("\"TestString\"");
        }

        [Test]
        public void CanRecogniseAQuotedString()
        {
            quoter.IsQuoted("\"QuotedString\"").ShouldBeTrue();
        }

        [Test]
        public void CanRecogniseAnUnQuotedString()
        {
            quoter.IsQuoted("UnQuotedString").ShouldBeFalse();
        }

        [Test]
        public void CharIsFormattedWithQuotes()
        {
            quoter.QuoteValue('A')
                .ShouldBe("'A'");
        }

        [Test]
        public void CustomTypeIsBare()
        {
            quoter.QuoteValue(new CustomClass())
                .ShouldBe("CustomClass");
        }

        [Test]
        public void DateTimeIsFormattedIso8601WithQuotes()
        {
            ChangeCulture();
            DateTime date = new DateTime(2010, 1, 2, 18, 4, 5, 123);
            quoter.QuoteValue(date)
                .ShouldBe("'2010-01-02T18:04:05'");
        } 
        
        [Test]
        public void DateTimeIsFormattedIso8601WithQuotes_WithItalyAsCulture()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("it-IT");
            DateTime date = new DateTime(2010, 1, 2, 18, 4, 5, 123);
            quoter.QuoteValue(date)
                .ShouldBe("'2010-01-02T18:04:05'");
        }

        [Test]
        public void EnumIsFormattedAsString()
        {
            quoter.QuoteValue(Foo.Bar)
                .ShouldBe("'Bar'");
        }

        [Test]
        public void FalseIsFormattedAsZero()
        {
            quoter.QuoteValue(false)
                .ShouldBe("0");
        }

        [Test]
        public void GuidIsFormattedWithQuotes()
        {
            Guid guid = new Guid("00000000-0000-0000-0000-000000000000");
            quoter.QuoteValue(guid)
                .ShouldBe("'00000000-0000-0000-0000-000000000000'");
        }

        [Test]
        public void Int32IsBare()
        {
            quoter.QuoteValue(1234)
                .ShouldBe("1234");
        }

        [Test]
        public void NullIsFormattedAsLiteral()
        {
            quoter.QuoteValue(null)
                .ShouldBe("NULL");
        }

        [Test]
        public void ShouldEscapeJetObjectNames()
        {
            //This will throw and error on the Jet Engine if special characters are used.
            //We do nothing.
            JetQuoter quoter = new JetQuoter();
            quoter.Quote("[Table]Name").ShouldBe("[[Table]Name]");
        }

        [Test]
        public void ShouldEscapeMySqlObjectNames()
        {
            MySqlQuoter quoter = new MySqlQuoter();
            quoter.Quote("`Table`Name").ShouldBe("```Table``Name`");
        }

        [Test]
        public void ShouldEscapeOracleObjectNames()
        {
            //Do Nothing at the moment due to case sensitivity issues with oracle
            OracleQuoterQuotedIdentifier quoter = new OracleQuoterQuotedIdentifier();
            quoter.Quote("Table\"Name").ShouldBe("\"Table\"\"Name\"");
        }

        [Test]
        public void ShouldEscapeSqlServerObjectNames()
        {
            SqlServerQuoter quoter = new SqlServerQuoter();
            quoter.Quote("[Table]Name").ShouldBe("[[Table]]Name]");
        }

        [Test]
        public void ShouldEscapeSqliteObjectNames()
        {
            SqliteQuoter quoter = new SqliteQuoter();
            quoter.Quote("Table\"Name").ShouldBe("\"Table\"\"Name\"");
        }

        [Test]
        public void ShouldHandleDecimalToStringConversionInAnyCulture()
        {
            ChangeCulture();
            quoter.QuoteValue(new Decimal(123.4d)).ShouldBe("123.4");
            RestoreCulture();
        }

        [Test]
        public void ShouldHandleDoubleToStringConversionInAnyCulture()
        {
            ChangeCulture();
            quoter.QuoteValue(123.4d).ShouldBe("123.4");
            RestoreCulture();
        }

        [Test]
        public void ShouldHandleFloatToStringConversionInAnyCulture()
        {
            ChangeCulture();
            quoter.QuoteValue(123.4f).ShouldBe("123.4");
            RestoreCulture();
        }

        [Test]
        public void StringIsFormattedWithQuotes()
        {
            quoter.QuoteValue("value")
                .ShouldBe("'value'");
        }

        [Test]
        public void StringWithQuoteIsFormattedWithDoubleQuote()
        {
            quoter.QuoteValue("val'ue")
                .ShouldBe("'val''ue'");
        }

        [Test]
        public void TrueIsFormattedAsOne()
        {
            quoter.QuoteValue(true)
                .ShouldBe("1");
        }

        [Test]
        public void ByteArrayIsFormattedWithQuotes()
        {
            quoter.QuoteValue(new byte[] { 0, 254, 13, 18, 125, 17 })
                .ShouldBe("0x00fe0d127d11");
        }

        [Test]
        public void ExplicitUnicodeStringIsFormattedAsNormalString()
        {
            quoter.QuoteValue(new ExplicitUnicodeString("Test String")).ShouldBe("'Test String'");
        }

        [Test]
        public void ExplicitUnicodeStringIsFormattedAsNormalStringQuotes()
        {
            quoter.QuoteValue(new ExplicitUnicodeString("Test ' String")).ShouldBe("'Test '' String'");
        }
    }
}