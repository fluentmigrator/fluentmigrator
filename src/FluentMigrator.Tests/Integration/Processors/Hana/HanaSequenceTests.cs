using FluentMigrator.Runner.Processors.Hana;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors.Hana
{
    [TestFixture]
    [Category("Integration")]
    public class HanaSequenceTests : BaseSequenceTests
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
        public override void CallingSequenceExistsReturnsFalseIfSequenceDoesNotExist()
        {
            Processor.SequenceExists(null, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingSequenceExistsReturnsFalseIfSequenceDoesNotExistWithSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            Processor.SequenceExists("test_schema", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingSequenceExistsReturnsTrueIfSequenceExists()
        {
            using (new HanaTestSequence(Processor, null, "test_sequence"))
                Processor.SequenceExists(null, "test_sequence").ShouldBeTrue();
        }

        [Test]
        public override void CallingSequenceExistsReturnsTrueIfSequenceExistsWithSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            using (new HanaTestSequence(Processor, "test_schema", "test_sequence"))
                Processor.SequenceExists("test_schema", "test_sequence").ShouldBeTrue();
        }
    }
}