using System;
using System.Globalization;
using System.Threading;

using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.MySql;

using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.MySql4
{
    [TestFixture]
    public class MySql4QuoterTest
    {
        private IQuoter quoter = default(MySqlQuoter);
        private readonly CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;

        [SetUp]
        public void SetUp()
        {
            quoter = new MySqlQuoter();
        }

        [Test]
        public void TimeSpanIsFormattedQuotes()
        {
            quoter.QuoteValue(new TimeSpan(1,2, 13, 65))
                .ShouldBe("'26:14:05'");
        }
    }
}
