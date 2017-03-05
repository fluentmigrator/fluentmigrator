using FluentMigrator.Runner.Processors.DB2;

using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors.Db2
{

    [TestFixture]
    [Category("Integration")]
    public class Db2SchemaTests : BaseSchemaTests
    {
        public Db2Processor Processor
        {
            get;
            set;
        }

        [SetUp]
        public void SetUp()
        {
            if (ConfiguredProcessor.IsAssignableFrom(typeof(Db2Processor)))
                Processor = CreateProcessor() as Db2Processor;
            else
                Assert.Ignore("Test is intended to run against Db2 server. Current configuration: {0}", ConfiguredDbEngine);
        }

        [TearDown]
        public void TearDown()
        {
            if (ConfiguredProcessor.IsAssignableFrom(typeof(Db2Processor)))
                Processor.Dispose();
        }

        [Test]
        public override void CallingSchemaExistsReturnsFalseIfSchemaDoesNotExist()
        {
            Processor.SchemaExists("DNE").ShouldBeFalse();
        }

        [Test]
        public override void CallingSchemaExistsReturnsTrueIfSchemaExists()
        {
            using (var table = new Db2TestTable(Processor, "TstSchma", "ID INT"))
            {
                Processor.SchemaExists("TstSchma").ShouldBeTrue();
            }
        }
    }
}