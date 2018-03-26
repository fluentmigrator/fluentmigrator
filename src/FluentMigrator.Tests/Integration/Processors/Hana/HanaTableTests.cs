using FluentMigrator.Runner.Processors.Hana;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors.Hana
{
    [TestFixture]
    [Category("Integration")]
    public class HanaTableTests : BaseTableTests
    {
        public HanaProcessor Processor { get; set; }

        [SetUp]
        public void SetUp()
        {
            if (ConfiguredProcessor.IsAssignableFrom(typeof(HanaProcessor)))
            {
                Processor = CreateProcessor() as HanaProcessor;
                Processor.BeginTransaction();
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
        public override void CallingTableExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new HanaTestTable("Test'Table", Processor, null, "id integer"))
                Processor.TableExists(null, table.Name).ShouldBeTrue();
        }

        [Test]
        public override void CallingTableExistsReturnsFalseIfTableDoesNotExist()
        {
            Processor.TableExists(null, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingTableExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            Processor.TableExists("test_schema", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingTableExistsReturnsTrueIfTableExists()
        {
            using (var table = new HanaTestTable(Processor, null, "id int"))
                Processor.TableExists(null, table.Name).ShouldBeTrue();
        }

        [Test]
        public override void CallingTableExistsReturnsTrueIfTableExistsWithSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            using (var table = new HanaTestTable(Processor, "test_schema", "id int"))
                Processor.TableExists("test_schema", table.Name).ShouldBeTrue();
        }
    }
}