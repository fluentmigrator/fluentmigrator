using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators.Oracle;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Oracle;

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Processors
{
    [TestFixture]
    public class OracleProcessorFactoryTests
    {
        private OracleProcessorFactory factory;
        private string connectionString;
        private IAnnouncer announcer;
        private ProcessorOptions options;

        [SetUp]
        public void SetUp()
        {
            factory = new OracleProcessorFactory();
            connectionString = "Data Source=localhost/XE;User Id=Something;Password=Something";
            announcer = new NullAnnouncer();
            options = new ProcessorOptions();
        }

        [TestCase("")]
        [TestCase(null)]
        public void CreateProcessorWithNoProviderSwitchesShouldUseOracleQuoter(string providerSwitches)
        {
            options.ProviderSwitches = providerSwitches;
            var processor = factory.Create(connectionString, announcer, options);
            Assert.That(((OracleProcessor)processor).Quoter, Is.InstanceOf<OracleQuoter>());
        }

        [TestCase("QuotedIdentifiers=true")]
        [TestCase("QuotedIdentifiers=TRUE;")]
        [TestCase("QuotedIDENTIFIERS=TRUE;")]
        [TestCase("QuotedIdentifiers=true;somethingelse=1")]
        [TestCase("somethingelse=1;QuotedIdentifiers=true")]
        [TestCase("somethingelse=1;QuotedIdentifiers=true;sometingOther='special thingy'")]
        public void CreateProcessorWithProviderSwitchIndicatingQuotedShouldUseOracleQuoterQuotedIdentifier(string providerSwitches)
        {
            options.ProviderSwitches = providerSwitches;
            var processor = factory.Create(connectionString, announcer, options);
            Assert.That(((OracleProcessor)processor).Quoter, Is.InstanceOf<OracleQuoterQuotedIdentifier>());
        }
    }
}