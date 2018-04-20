using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.Postgres;
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
            _quoter = new PostgresQuoter();
        }

        private IQuoter _quoter = default(PostgresQuoter);

        [Test]
        public void ByteArrayIsFormattedWithQuotes()
        {
            _quoter.QuoteValue(new byte[] { 0, 254, 13, 18, 125, 17 })
                .ShouldBe(@"E'\\x00FE0D127D11'");
        }
    }
}
