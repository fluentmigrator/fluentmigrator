#region License
//
// Copyright (c) 2018, Fluent Migrator Project
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
using System.Globalization;
using System.Threading;

using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.Generic;
using FluentMigrator.Runner.Generators.Jet;
using FluentMigrator.Runner.Generators.MySql;
using FluentMigrator.Runner.Generators.Oracle;
using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.Runner.Generators.SQLite;
using FluentMigrator.Runner.Initialization;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.GenericGenerator
{
    [TestFixture]
    public class ConstantFormatterTests
    {
        private readonly CultureInfo _currentCulture = Thread.CurrentThread.CurrentCulture;

        private static IQuoter CreateFixture(QuoterOptions options = null) =>
            new GenericQuoter(options);

        private void RestoreCulture()
        {
            Thread.CurrentThread.CurrentCulture = _currentCulture;
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
            CreateFixture().Quote("Test\"String").ShouldBe("\"Test\"\"String\"");
        }

        [Test]
        public void CanHandleAnAlreadyQuotedColumnName()
        {
            CreateFixture().QuoteColumnName("\"ColumnName\"").ShouldBe("\"ColumnName\"");
        }

        [Test]
        public void CanHandleAnAlreadyQuotedSchemaName()
        {
            CreateFixture().QuoteColumnName("\"SchemaName\"").ShouldBe("\"SchemaName\"");
        }

        [Test]
        public void CanHandleAnAlreadyQuotedTableName()
        {
            CreateFixture().QuoteColumnName("\"TableName\"").ShouldBe("\"TableName\"");
        }

        [Test]
        public void CanHandleAnUnQuotedColumnName()
        {
            CreateFixture().QuoteColumnName("ColumnName").ShouldBe("\"ColumnName\"");
        }

        [Test]
        public void CanHandleAnUnQuotedSchemaName()
        {
            CreateFixture().QuoteColumnName("SchemaName").ShouldBe("\"SchemaName\"");
        }

        [Test]
        public void CanHandleAnUnQuotedTableName()
        {
            CreateFixture().QuoteColumnName("TableName").ShouldBe("\"TableName\"");
        }

        [Test]
        public void CanQuoteAString()
        {
            CreateFixture().Quote("TestString").ShouldBe("\"TestString\"");
        }

        [Test]
        public void CanRecogniseAQuotedString()
        {
            CreateFixture().IsQuoted("\"QuotedString\"").ShouldBeTrue();
        }

        [Test]
        public void CanRecogniseAnUnQuotedString()
        {
            CreateFixture().IsQuoted("UnQuotedString").ShouldBeFalse();
        }

        [Test]
        public void CharIsFormattedWithQuotes()
        {
            CreateFixture().QuoteValue('A')
                .ShouldBe("'A'");
        }

        [Test]
        public void CustomTypeIsBare()
        {
            CreateFixture().QuoteValue(new CustomClass())
                .ShouldBe("CustomClass");
        }

        [Test]
        public void DateTimeIsFormattedIso8601WithQuotes()
        {
            ChangeCulture();
            DateTime date = new DateTime(2010, 1, 2, 18, 4, 5, 123);
            CreateFixture().QuoteValue(date)
                .ShouldBe("'2010-01-02T18:04:05'");
        }

        [Test]
        public void DateTimeIsFormattedIso8601WithQuotes_WithItalyAsCulture()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("it-IT");
            DateTime date = new DateTime(2010, 1, 2, 18, 4, 5, 123);
            CreateFixture().QuoteValue(date)
                .ShouldBe("'2010-01-02T18:04:05'");
        }

        [Test]
        public void DateTimeOffsetIsFormattedIso8601WithQuotes()
        {
            ChangeCulture();
            DateTimeOffset date = new DateTimeOffset(2010, 1, 2, 18, 4, 5, 123, TimeSpan.FromHours(-4));
            CreateFixture().QuoteValue(date).ShouldBe("'2010-01-02T18:04:05-04:00'");
        }

        [Test]
        public void DateTimeOffsetIsFormattedIso8601WithQuotes_WithItalyAsCulture()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("it-IT");
            DateTimeOffset date = new DateTimeOffset(2010, 1, 2, 18, 4, 5, 123, TimeSpan.FromHours(-4));
            CreateFixture().QuoteValue(date)
                .ShouldBe("'2010-01-02T18:04:05-04:00'");
        }

        [Test]
        public void EnumIsFormattedAsString()
        {
            CreateFixture().QuoteValue(Foo.Bar)
                .ShouldBe("'Bar'");
        }

        [Test]
        public void EnumIsFormatterAsUnterlyingType()
        {
            var options = new QuoterOptions
            {
                EnumAsString = false
            };

            CreateFixture(options).QuoteValue(Foo.Baz)
                .ShouldBe("1");
        }

        [Test]
        public void FalseIsFormattedAsZero()
        {
            CreateFixture().QuoteValue(false)
                .ShouldBe("0");
        }

        [Test]
        public void GuidIsFormattedWithQuotes()
        {
            Guid guid = new Guid("00000000-0000-0000-0000-000000000000");
            CreateFixture().QuoteValue(guid)
                .ShouldBe("'00000000-0000-0000-0000-000000000000'");
        }

        [Test]
        public void Int32IsBare()
        {
            CreateFixture().QuoteValue(1234)
                .ShouldBe("1234");
        }

        [Test]
        public void NullIsFormattedAsLiteral()
        {
            CreateFixture().QuoteValue(null)
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
            SqlServer2000Quoter quoter = new SqlServer2000Quoter();
            quoter.Quote("[Table]Name").ShouldBe("[[Table]]Name]");
        }

        [Test]
        public void ShouldEscapeSqliteObjectNames()
        {
            SQLiteQuoter quoter = new SQLiteQuoter();
            quoter.Quote("Table\"Name").ShouldBe("\"Table\"\"Name\"");
        }

        [Test]
        public void ShouldHandleDecimalToStringConversionInAnyCulture()
        {
            ChangeCulture();
            CreateFixture().QuoteValue(new decimal(123.4d)).ShouldBe("123.4");
            RestoreCulture();
        }

        [Test]
        public void ShouldHandleDoubleToStringConversionInAnyCulture()
        {
            ChangeCulture();
            CreateFixture().QuoteValue(123.4d).ShouldBe("123.4");
            RestoreCulture();
        }

        [Test]
        public void ShouldHandleFloatToStringConversionInAnyCulture()
        {
            ChangeCulture();
            CreateFixture().QuoteValue(123.4f).ShouldBe("123.4");
            RestoreCulture();
        }

        [Test]
        public void StringIsFormattedWithQuotes()
        {
            CreateFixture().QuoteValue("value")
                .ShouldBe("'value'");
        }

        [Test]
        public void StringWithQuoteIsFormattedWithDoubleQuote()
        {
            CreateFixture().QuoteValue("val'ue")
                .ShouldBe("'val''ue'");
        }

        [Test]
        public void TrueIsFormattedAsOne()
        {
            CreateFixture().QuoteValue(true)
                .ShouldBe("1");
        }

        [Test]
        public void ByteArrayIsFormattedWithQuotes()
        {
            CreateFixture().QuoteValue(new byte[] { 0, 254, 13, 18, 125, 17 })
                .ShouldBe("0x00fe0d127d11");
        }

        [Test]
        public void TimeSpanIsFormattedQuotes()
        {
            CreateFixture().QuoteValue(new TimeSpan(2, 13, 65))
                .ShouldBe("'02:14:05'");
        }

        [Test]
        public void NonUnicodeStringIsFormattedAsNormalString()
        {
            CreateFixture().QuoteValue(new NonUnicodeString("Test String")).ShouldBe("'Test String'");
        }

        [Test]
        public void NonUnicodeStringIsFormattedAsNormalStringQuotes()
        {
            CreateFixture().QuoteValue(new NonUnicodeString("Test ' String")).ShouldBe("'Test '' String'");
        }

        [Test]
        [Obsolete]
        public void ExplicitUnicodeStringIsFormattedAsNormalString()
        {
            CreateFixture().QuoteValue(new ExplicitUnicodeString("Test String")).ShouldBe("'Test String'");
        }

        [Test]
        [Obsolete]
        public void ExplicitUnicodeStringIsFormattedAsNormalStringQuotes()
        {
            CreateFixture().QuoteValue(new ExplicitUnicodeString("Test ' String")).ShouldBe("'Test '' String'");
        }
    }
}
