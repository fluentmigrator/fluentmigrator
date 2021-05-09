using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.Postgres;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors.Postgres;

using Microsoft.Extensions.Options;

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
            _quoterOptions = new OptionsWrapper<QuoterOptions>(new QuoterOptions());
            _quoter = new PostgresQuoter(_quoterOptions, new PostgresOptions());
        }

        private IQuoter _quoter = default(PostgresQuoter);
        private IOptions<QuoterOptions> _quoterOptions;

        [Test]
        public void ByteArrayIsFormattedWithQuotes()
        {
            _quoter.QuoteValue(new byte[] { 0, 254, 13, 18, 125, 17 })
                .ShouldBe(@"E'\\x00FE0D127D11'");
        }

        [Test]
        public void DisableForceQuoteRemovesQuotes()
        {
            _quoter = new PostgresQuoter(_quoterOptions, new PostgresOptions() { ForceQuote = false });
            _quoter.Quote("TableName").ShouldBe("TableName");
        }

        [Test]
        public void DisableForceQuoteQuotesReservedKeyword()
        {
            _quoter = new PostgresQuoter(_quoterOptions, new PostgresOptions() { ForceQuote = false });

            _quoter.Quote("between").ShouldBe(@"""between""");
            _quoter.Quote("BETWEEN").ShouldBe(@"""BETWEEN""");
        }
    }
}
