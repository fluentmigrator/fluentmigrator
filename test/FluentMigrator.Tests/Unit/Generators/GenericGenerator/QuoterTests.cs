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

using FluentMigrator.Generation;
using FluentMigrator.Runner.Generators.Generic;
using FluentMigrator.Runner.Generators.Jet;
using FluentMigrator.Runner.Generators.MySql;
using FluentMigrator.Runner.Generators.Oracle;
using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.Runner.Generators.SQLite;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.GenericGenerator
{
    [TestFixture]
    [Category("Generator")]
    [Category("Quoter")]
    [Category("Generic")]
    public class ConstantFormatterTests
    {
        [SetUp]
        public void SetUp()
        {
            _quoter = new GenericQuoter();
        }

        private IQuoter _quoter;
        private readonly CultureInfo _currentCulture = Thread.CurrentThread.CurrentCulture;

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
            // ReSharper disable once UnusedMember.Local
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
            _quoter.Quote("Test\"String").ShouldBe("\"Test\"\"String\"");
        }

        [Test]
        public void CanHandleAnAlreadyQuotedColumnName()
        {
            _quoter.QuoteColumnName("\"ColumnName\"").ShouldBe("\"ColumnName\"");
        }

        [Test]
        public void CanHandleAnAlreadyQuotedSchemaName()
        {
            _quoter.QuoteColumnName("\"SchemaName\"").ShouldBe("\"SchemaName\"");
        }

        [Test]
        public void CanHandleAnAlreadyQuotedTableName()
        {
            _quoter.QuoteColumnName("\"TableName\"").ShouldBe("\"TableName\"");
        }

        [Test]
        public void CanHandleAnUnQuotedColumnName()
        {
            _quoter.QuoteColumnName("ColumnName").ShouldBe("\"ColumnName\"");
        }

        [Test]
        public void CanHandleAnUnQuotedSchemaName()
        {
            _quoter.QuoteColumnName("SchemaName").ShouldBe("\"SchemaName\"");
        }

        [Test]
        public void CanHandleAnUnQuotedTableName()
        {
            _quoter.QuoteColumnName("TableName").ShouldBe("\"TableName\"");
        }

        [Test]
        public void CanQuoteAString()
        {
            _quoter.Quote("TestString").ShouldBe("\"TestString\"");
        }

        [Test]
        public void CanRecogniseAQuotedString()
        {
            _quoter.IsQuoted("\"QuotedString\"").ShouldBeTrue();
        }

        [Test]
        public void CanRecogniseAnUnQuotedString()
        {
            _quoter.IsQuoted("UnQuotedString").ShouldBeFalse();
        }

        [Test]
        public void CharIsFormattedWithQuotes()
        {
            _quoter.QuoteValue('A')
                .ShouldBe("'A'");
        }

        [Test]
        public void CustomTypeIsBare()
        {
            _quoter.QuoteValue(new CustomClass())
                .ShouldBe("CustomClass");
        }

        [Test]
        public void DateTimeIsFormattedIso8601WithQuotes()
        {
            ChangeCulture();
            DateTime date = new DateTime(2010, 1, 2, 18, 4, 5, 123);
            _quoter.QuoteValue(date)
                .ShouldBe("'2010-01-02T18:04:05'");
        }

        [Test]
        public void DateTimeIsFormattedIso8601WithQuotes_WithItalyAsCulture()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("it-IT");
            DateTime date = new DateTime(2010, 1, 2, 18, 4, 5, 123);
            _quoter.QuoteValue(date)
                .ShouldBe("'2010-01-02T18:04:05'");
        }

        [Test]
        public void DateTimeOffsetIsFormattedIso8601WithQuotes()
        {
            ChangeCulture();
            DateTimeOffset date = new DateTimeOffset(2010, 1, 2, 18, 4, 5, 123, TimeSpan.FromHours(-4));
            _quoter.QuoteValue(date).ShouldBe("'2010-01-02T18:04:05-04:00'");
        }

        [Test]
        public void DateTimeOffsetIsFormattedIso8601WithQuotes_WithItalyAsCulture()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("it-IT");
            DateTimeOffset date = new DateTimeOffset(2010, 1, 2, 18, 4, 5, 123, TimeSpan.FromHours(-4));
            _quoter.QuoteValue(date)
                .ShouldBe("'2010-01-02T18:04:05-04:00'");
        }

        [Test]
        public void EnumIsFormattedAsString()
        {
            _quoter.QuoteValue(Foo.Bar)
                .ShouldBe("'Bar'");
        }

        [Test]
        public void FalseIsFormattedAsZero()
        {
            _quoter.QuoteValue(false)
                .ShouldBe("0");
        }

        [Test]
        public void GuidIsFormattedWithQuotes()
        {
            Guid guid = new Guid("00000000-0000-0000-0000-000000000000");
            _quoter.QuoteValue(guid)
                .ShouldBe("'00000000-0000-0000-0000-000000000000'");
        }

        [Test]
        public void Int32IsBare()
        {
            _quoter.QuoteValue(1234)
                .ShouldBe("1234");
        }

        [Test]
        public void NullIsFormattedAsLiteral()
        {
            _quoter.QuoteValue(null)
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
            _quoter.QuoteValue(new decimal(123.4d)).ShouldBe("123.4");
            RestoreCulture();
        }

        [Test]
        public void ShouldHandleDoubleToStringConversionInAnyCulture()
        {
            ChangeCulture();
            _quoter.QuoteValue(123.4d).ShouldBe("123.4");
            RestoreCulture();
        }

        [Test]
        public void ShouldHandleFloatToStringConversionInAnyCulture()
        {
            ChangeCulture();
            _quoter.QuoteValue(123.4f).ShouldBe("123.4");
            RestoreCulture();
        }

        [Test]
        public void StringIsFormattedWithQuotes()
        {
            _quoter.QuoteValue("value")
                .ShouldBe("'value'");
        }

        [Test]
        public void StringWithQuoteIsFormattedWithDoubleQuote()
        {
            _quoter.QuoteValue("val'ue")
                .ShouldBe("'val''ue'");
        }

        [Test]
        public void TrueIsFormattedAsOne()
        {
            _quoter.QuoteValue(true)
                .ShouldBe("1");
        }

        [Test]
        public void ByteArrayIsFormattedWithQuotes()
        {
            _quoter.QuoteValue(new byte[] { 0, 254, 13, 18, 125, 17 })
                .ShouldBe("0x00fe0d127d11");
        }

        [Test]
        public void TimeSpanIsFormattedQuotes()
        {
            _quoter.QuoteValue(new TimeSpan(2, 13, 65))
                .ShouldBe("'02:14:05'");
        }

        [Test]
        public void NonUnicodeStringIsFormattedAsNormalString()
        {
            _quoter.QuoteValue(new NonUnicodeString("Test String")).ShouldBe("'Test String'");
        }

        [Test]
        public void NonUnicodeStringIsFormattedAsNormalStringQuotes()
        {
            _quoter.QuoteValue(new NonUnicodeString("Test ' String")).ShouldBe("'Test '' String'");
        }
    }
}
