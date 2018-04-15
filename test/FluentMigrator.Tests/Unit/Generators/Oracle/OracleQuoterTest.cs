using System;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.Oracle;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.Oracle
{
    [TestFixture]
    public class OracleQuoterTest
    {
        private IQuoter _quoter;

        [SetUp]
        public void SetUp()
        {
            _quoter = new OracleQuoter();
        }

        [Test]
        public void TimeSpanIsFormattedQuotes()
        {
            _quoter.QuoteValue(new TimeSpan(1, 2, 13, 65))
                   .ShouldBe("'1 2:14:5.0'");
        }

        [Test]
        public void GuidIsFormattedAsOracleAndQuoted()
        {
            Guid givenValue = new Guid("CC28B6C7-9260-4800-9C1F-A5243960C087");

            _quoter.QuoteValue(givenValue)
                   .ShouldBe("'C7B628CC609200489C1FA5243960C087'");
        }
    }
}