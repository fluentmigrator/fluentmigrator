using System;
using FluentMigrator.Runner.Generators;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators
{
	[TestFixture]
	public class ConstantFormatterTests
	{
		private ConstantFormatter formatter;

		[SetUp]
		public void SetUp()
		{
			formatter = new ConstantFormatter();
		}

		[Test]
		public void NullIsFormattedAsLiteral()
		{
			formatter.Format(null)
				.ShouldBe("NULL");
		}

		[Test]
		public void StringIsFormattedWithQuotes()
		{
			formatter.Format("value")
				.ShouldBe("'value'");
		}

		[Test]
		public void StringWithQuoteIsFormattedWithDoubleQuote()
		{
			formatter.Format("val'ue")
				.ShouldBe("'val''ue'");
		}

		[Test]
		public void CharIsFormattedWithQuotes()
		{
			formatter.Format('A')
				.ShouldBe("'A'");
		}

		[Test]
		public void TrueIsFormattedAsOne()
		{
			formatter.Format(true)
				.ShouldBe("1");
		}

		[Test]
		public void FalseIsFormattedAsZero()
		{
			formatter.Format(false)
				.ShouldBe("0");
		}

		[Test]
		public void GuidIsFormattedWithQuotes()
		{
			Guid guid = new Guid("00000000-0000-0000-0000-000000000000");
			formatter.Format(guid)
				.ShouldBe("'00000000-0000-0000-0000-000000000000'");
		}

		[Test]
		public void DateTimeIsFormattedIso8601WithQuotes()
		{
			DateTime date = new DateTime(2010,1,2,18,4,5,123);
			formatter.Format(date)
				.ShouldBe("'2010-01-02T18:04:05'");
		}

		[Test]
		public void Int32IsBare()
		{
			formatter.Format(1234)
				.ShouldBe("1234");
		}

		[Test]
		public void CustomTypeIsBare()
		{
			formatter.Format(new CustomClass())
				.ShouldBe("CustomClass");
		}

	    [Test]
	    public void EnumIsFormattedAsString()
	    {
	        formatter.Format(Foo.Bar)
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
