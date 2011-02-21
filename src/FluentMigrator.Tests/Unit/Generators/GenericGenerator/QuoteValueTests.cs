using System;
using FluentMigrator.Runner.Generators;
using NUnit.Framework;
using NUnit.Should;
using FluentMigrator.Runner.Generators.Base;
using FluentMigrator.Runner.Generators.Generic;

namespace FluentMigrator.Tests.Unit.Generators
{
	[TestFixture]
	public class ConstantFormatterTests
	{
		private IQuoter formatter;

		[SetUp]
		public void SetUp()
		{
			formatter = new GenericQuoter();
		}

		[Test]
		public void NullIsFormattedAsLiteral()
		{
			formatter.QuoteValue(null)
				.ShouldBe("NULL");
		}

		[Test]
		public void StringIsFormattedWithQuotes()
		{
            formatter.QuoteValue("value")
				.ShouldBe("'value'");
		}

		[Test]
		public void StringWithQuoteIsFormattedWithDoubleQuote()
		{
            formatter.QuoteValue("val'ue")
				.ShouldBe("'val''ue'");
		}

		[Test]
		public void CharIsFormattedWithQuotes()
		{
            formatter.QuoteValue('A')
				.ShouldBe("'A'");
		}

		[Test]
		public void TrueIsFormattedAsOne()
		{
            formatter.QuoteValue(true)
				.ShouldBe("1");
		}

		[Test]
		public void FalseIsFormattedAsZero()
		{
            formatter.QuoteValue(false)
				.ShouldBe("0");
		}

		[Test]
		public void GuidIsFormattedWithQuotes()
		{
			Guid guid = new Guid("00000000-0000-0000-0000-000000000000");
            formatter.QuoteValue(guid)
				.ShouldBe("'00000000-0000-0000-0000-000000000000'");
		}

		[Test]
		public void DateTimeIsFormattedIso8601WithQuotes()
		{
			DateTime date = new DateTime(2010,1,2,18,4,5,123);
            formatter.QuoteValue(date)
				.ShouldBe("'2010-01-02T18:04:05'");
		}

		[Test]
		public void Int32IsBare()
		{
            formatter.QuoteValue(1234)
				.ShouldBe("1234");
		}

		[Test]
		public void CustomTypeIsBare()
		{
            formatter.QuoteValue(new CustomClass())
				.ShouldBe("CustomClass");
		}

	    [Test]
	    public void EnumIsFormattedAsString()
	    {
            formatter.QuoteValue(Foo.Bar)
                .ShouldBe("'Bar'");
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
	}
}
