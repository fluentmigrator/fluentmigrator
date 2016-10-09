using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.Postgres;
using Xunit;

namespace FluentMigrator.Tests.Unit.Generators.Postgres
{
    public class PostgresQuotesTests
    {
        public void SetUp()
        {
            quoter = new PostgresQuoter();
        }

        private IQuoter quoter = default(PostgresQuoter);

        [Fact]
        public void ByteArrayIsFormattedWithQuotes()
        {
            quoter.QuoteValue(new byte[] { 0, 254, 13, 18, 125, 17 })
                .ShouldBe(@"E'\\x00FE0D127D11'");
        }
    }
}