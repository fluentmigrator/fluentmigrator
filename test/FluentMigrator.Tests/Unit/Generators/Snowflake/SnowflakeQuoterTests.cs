using FluentMigrator.Runner.Generators.Snowflake;
using FluentMigrator.Runner.Processors.Snowflake;
using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Generators.Snowflake
{
    [TestFixture]
    [Category("Snowflake")]
    [Category("Generator")]
    [Category("Quoter")]
    public class SnowflakeQuoterTests
    {
        private SnowflakeQuoter _quoterQuotingEnabled;
        private SnowflakeQuoter _quoterQuotingDisabled;

        [SetUp]
        public void Setup()
        {
            _quoterQuotingEnabled = new SnowflakeQuoter(true);
            _quoterQuotingDisabled = new SnowflakeQuoter(false);
        }

        [Test]
        public void FormatSystemMethods_CurrentDateTime_ReturnsSysdateWithTimestampNtz()
        {
            // Act
            var result = _quoterQuotingEnabled.FormatSystemMethods(SystemMethods.CurrentDateTime);

            // Assert
            Assert.That(result, Is.EqualTo("CURRENT_TIMESTAMP()"));
        }

        [Test]
        public void FormatSystemMethods_CurrentDateTimeOffset_ReturnsCurrentTimestamp()
        {
            // Act
            var result = _quoterQuotingEnabled.FormatSystemMethods(SystemMethods.CurrentDateTimeOffset);

            // Assert
            Assert.That(result, Is.EqualTo("CURRENT_TIMESTAMP()"));
        }

        [Test]
        public void FormatSystemMethods_CurrentUTCDateTime_ReturnsSysdate()
        {
            // Act
            var result = _quoterQuotingEnabled.FormatSystemMethods(SystemMethods.CurrentUTCDateTime);

            // Assert
            Assert.That(result, Is.EqualTo("SYSDATE()"));
        }

        [Test]
        public void FormatSystemMethods_QuotingDisabled_CurrentDateTime_ReturnsSysdateWithTimestampNtz()
        {
            // Act
            var result = _quoterQuotingDisabled.FormatSystemMethods(SystemMethods.CurrentDateTime);

            // Assert
            Assert.That(result, Is.EqualTo("CURRENT_TIMESTAMP()"));
        }

        [Test]
        public void FormatSystemMethods_QuotingDisabled_CurrentDateTimeOffset_ReturnsCurrentTimestamp()
        {
            // Act
            var result = _quoterQuotingDisabled.FormatSystemMethods(SystemMethods.CurrentDateTimeOffset);

            // Assert
            Assert.That(result, Is.EqualTo("CURRENT_TIMESTAMP()"));
        }
    }
}