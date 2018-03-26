using FluentMigrator.Runner.Processors.Hana;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors.Hana
{
    [TestFixture]
    [Category("Integration")]
    public class HanaSchemaTests : BaseSchemaTests
    {
        public HanaProcessor Processor { get; set; }

        [SetUp]
        public void SetUp()
        {
            if (ConfiguredProcessor.IsAssignableFrom(typeof(HanaProcessor)))
            {
                Processor = CreateProcessor() as HanaProcessor;
            }
            else
                Assert.Ignore("Test is intended to run against Hana server. Current configuration: {0}", ConfiguredDbEngine);
        }

        [TearDown]
        public void TearDown()
        {
            if (ConfiguredProcessor.IsAssignableFrom(typeof(HanaProcessor)))
            {
                Processor.CommitTransaction();
                Processor.Dispose();
            }
        }

        [Test]
        public override void CallingSchemaExistsReturnsFalseIfSchemaDoesNotExist()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            Processor.SchemaExists("DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingSchemaExistsReturnsTrueIfSchemaExists()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            Processor.SchemaExists("dbo").ShouldBeTrue();
        }
    }
}