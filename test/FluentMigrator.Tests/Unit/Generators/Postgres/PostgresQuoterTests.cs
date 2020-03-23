using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.Postgres;
using FluentMigrator.Runner.Processors.Postgres;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.Postgres
{
    [TestFixture]
    public class PostgresQuotesTests
    {
        [SetUp]
        public void SetUp()
        {
            _quoter = new PostgresQuoter(new PostgresOptions());
        }

        private IQuoter _quoter = default(PostgresQuoter);

        [Test]
        public void ByteArrayIsFormattedWithQuotes()
        {
            _quoter.QuoteValue(new byte[] { 0, 254, 13, 18, 125, 17 })
                .ShouldBe(@"E'\\x00FE0D127D11'");
        }

        [Test]
        public void DisableForceQuoteRemovesQuotes()
        {
            _quoter = new PostgresQuoter(new PostgresOptions() { ForceQuote = false });
            _quoter.Quote("TableName").ShouldBe("TableName");
        }

        [Test]
        public void DisableForceQuoteQuotesReservedKeyword()
        {
            _quoter = new PostgresQuoter(new PostgresOptions() { ForceQuote = false });

            _quoter.Quote("between").ShouldBe(@"""between""");
            _quoter.Quote("BETWEEN").ShouldBe(@"""BETWEEN""");
        }
    }
}
