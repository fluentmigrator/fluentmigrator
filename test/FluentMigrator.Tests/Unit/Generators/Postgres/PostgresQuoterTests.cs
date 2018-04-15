using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.Postgres;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.Postgres
{
    [TestFixture]
    public class PostgresQuotesTests
    {
        [SetUp]
        public void SetUp()
        {
            quoter = new PostgresQuoter();
        }

        private IQuoter quoter = default(PostgresQuoter);

        [Test]
        public void ByteArrayIsFormattedWithQuotes()
        {
            quoter.QuoteValue(new byte[] { 0, 254, 13, 18, 125, 17 })
                .ShouldBe(@"E'\\x00FE0D127D11'");
        }
    }
}