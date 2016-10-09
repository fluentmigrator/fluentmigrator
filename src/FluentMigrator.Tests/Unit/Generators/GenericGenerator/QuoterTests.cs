using System;
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
using Xunit;

namespace FluentMigrator.Tests.Unit.Generators
{
    public class ConstantFormatterTests
    {
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

        [Fact]
        public void CanEscapeAString()
        {
            quoter.Quote("Test\"String").ShouldBe("\"Test\"\"String\"");
        }

        [Fact]
        public void CanHandleAnAlreadyQuotedColumnName()
        {
            quoter.QuoteColumnName("\"ColumnName\"").ShouldBe("\"ColumnName\"");
        }

        [Fact]
        public void CanHandleAnAlreadyQuotedSchemaName()
        {
            quoter.QuoteColumnName("\"SchemaName\"").ShouldBe("\"SchemaName\"");
        }

        [Fact]
        public void CanHandleAnAlreadyQuotedTableName()
        {
            quoter.QuoteColumnName("\"TableName\"").ShouldBe("\"TableName\"");
        }

        [Fact]
        public void CanHandleAnUnQuotedColumnName()
        {
            quoter.QuoteColumnName("ColumnName").ShouldBe("\"ColumnName\"");
        }

        [Fact]
        public void CanHandleAnUnQuotedSchemaName()
        {
            quoter.QuoteColumnName("SchemaName").ShouldBe("\"SchemaName\"");
        }

        [Fact]
        public void CanHandleAnUnQuotedTableName()
        {
            quoter.QuoteColumnName("TableName").ShouldBe("\"TableName\"");
        }

        [Fact]
        public void CanQuoteAString()
        {
            quoter.Quote("TestString").ShouldBe("\"TestString\"");
        }

        [Fact]
        public void CanRecogniseAQuotedString()
        {
            quoter.IsQuoted("\"QuotedString\"").ShouldBeTrue();
        }

        [Fact]
        public void CanRecogniseAnUnQuotedString()
        {
            quoter.IsQuoted("UnQuotedString").ShouldBeFalse();
        }

        [Fact]
        public void CharIsFormattedWithQuotes()
        {
            quoter.QuoteValue('A')
                .ShouldBe("'A'");
        }

        [Fact]
        public void CustomTypeIsBare()
        {
            quoter.QuoteValue(new CustomClass())
                .ShouldBe("CustomClass");
        }

        [Fact]
        public void DateTimeIsFormattedIso8601WithQuotes()
        {
            ChangeCulture();
            DateTime date = new DateTime(2010, 1, 2, 18, 4, 5, 123);
            quoter.QuoteValue(date)
                .ShouldBe("'2010-01-02T18:04:05'");
        } 
        
        [Fact]
        public void DateTimeIsFormattedIso8601WithQuotes_WithItalyAsCulture()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("it-IT");
            DateTime date = new DateTime(2010, 1, 2, 18, 4, 5, 123);
            quoter.QuoteValue(date)
                .ShouldBe("'2010-01-02T18:04:05'");
        }

        [Fact]
        public void DateTimeOffsetIsFormattedIso8601WithQuotes() 
        {
            ChangeCulture();
            DateTimeOffset date = new DateTimeOffset(2010, 1, 2, 18, 4, 5, 123, TimeSpan.FromHours(-4));
            quoter.QuoteValue(date).ShouldBe("'2010-01-02T18:04:05 -04:00'");
        }

        [Fact]
        public void DateTimeOffsetIsFormattedIso8601WithQuotes_WithItalyAsCulture() 
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("it-IT");
            DateTimeOffset date = new DateTimeOffset(2010, 1, 2, 18, 4, 5, 123, TimeSpan.FromHours(-4));
            quoter.QuoteValue(date)
                .ShouldBe("'2010-01-02T18:04:05 -04:00'");
        }

        [Fact]
        public void EnumIsFormattedAsString()
        {
            quoter.QuoteValue(Foo.Bar)
                .ShouldBe("'Bar'");
        }

        [Fact]
        public void FalseIsFormattedAsZero()
        {
            quoter.QuoteValue(false)
                .ShouldBe("0");
        }

        [Fact]
        public void GuidIsFormattedWithQuotes()
        {
            Guid guid = new Guid("00000000-0000-0000-0000-000000000000");
            quoter.QuoteValue(guid)
                .ShouldBe("'00000000-0000-0000-0000-000000000000'");
        }

        [Fact]
        public void Int32IsBare()
        {
            quoter.QuoteValue(1234)
                .ShouldBe("1234");
        }

        [Fact]
        public void NullIsFormattedAsLiteral()
        {
            quoter.QuoteValue(null)
                .ShouldBe("NULL");
        }

        [Fact]
        public void ShouldEscapeJetObjectNames()
        {
            //This will throw and error on the Jet Engine if special characters are used.
            //We do nothing.
            JetQuoter quoter = new JetQuoter();
            quoter.Quote("[Table]Name").ShouldBe("[[Table]Name]");
        }

        [Fact]
        public void ShouldEscapeMySqlObjectNames()
        {
            MySqlQuoter quoter = new MySqlQuoter();
            quoter.Quote("`Table`Name").ShouldBe("```Table``Name`");
        }

        [Fact]
        public void ShouldEscapeOracleObjectNames()
        {
            //Do Nothing at the moment due to case sensitivity issues with oracle
            OracleQuoterQuotedIdentifier quoter = new OracleQuoterQuotedIdentifier();
            quoter.Quote("Table\"Name").ShouldBe("\"Table\"\"Name\"");
        }

        [Fact]
        public void ShouldEscapeSqlServerObjectNames()
        {
            SqlServerQuoter quoter = new SqlServerQuoter();
            quoter.Quote("[Table]Name").ShouldBe("[[Table]]Name]");
        }

        [Fact]
        public void ShouldEscapeSqliteObjectNames()
        {
            SQLiteQuoter quoter = new SQLiteQuoter();
            quoter.Quote("Table\"Name").ShouldBe("\"Table\"\"Name\"");
        }

        [Fact]
        public void ShouldHandleDecimalToStringConversionInAnyCulture()
        {
            ChangeCulture();
            quoter.QuoteValue(new Decimal(123.4d)).ShouldBe("123.4");
            RestoreCulture();
        }

        [Fact]
        public void ShouldHandleDoubleToStringConversionInAnyCulture()
        {
            ChangeCulture();
            quoter.QuoteValue(123.4d).ShouldBe("123.4");
            RestoreCulture();
        }

        [Fact]
        public void ShouldHandleFloatToStringConversionInAnyCulture()
        {
            ChangeCulture();
            quoter.QuoteValue(123.4f).ShouldBe("123.4");
            RestoreCulture();
        }

        [Fact]
        public void StringIsFormattedWithQuotes()
        {
            quoter.QuoteValue("value")
                .ShouldBe("'value'");
        }

        [Fact]
        public void StringWithQuoteIsFormattedWithDoubleQuote()
        {
            quoter.QuoteValue("val'ue")
                .ShouldBe("'val''ue'");
        }

        [Fact]
        public void TrueIsFormattedAsOne()
        {
            quoter.QuoteValue(true)
                .ShouldBe("1");
        }

        [Fact]
        public void ByteArrayIsFormattedWithQuotes()
        {
            quoter.QuoteValue(new byte[] { 0, 254, 13, 18, 125, 17 })
                .ShouldBe("0x00fe0d127d11");
        }

        [Fact]
        public void TimeSpanIsFormattedQuotes()
        {
            quoter.QuoteValue(new TimeSpan(2, 13, 65))
                .ShouldBe("'02:14:05'");
        }

        [Fact]
        public void ExplicitUnicodeStringIsFormattedAsNormalString()
        {
            quoter.QuoteValue(new ExplicitUnicodeString("Test String")).ShouldBe("'Test String'");
        }

        [Fact]
        public void ExplicitUnicodeStringIsFormattedAsNormalStringQuotes()
        {
            quoter.QuoteValue(new ExplicitUnicodeString("Test ' String")).ShouldBe("'Test '' String'");
        }
    }
}