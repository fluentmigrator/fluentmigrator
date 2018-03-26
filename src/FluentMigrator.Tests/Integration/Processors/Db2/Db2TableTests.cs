using FluentMigrator.Runner.Processors.DB2;

using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors.Db2
{

    [TestFixture]
    [Category("Integration")]
    public class Db2TableTests : BaseTableTests
    {
        public Db2Processor Processor
        {
            get; set;
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
        public override void CallingTableExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new Db2TestTable("Test'Table", Processor, null, "ID INT"))
            {
                Processor.TableExists(null, table.Name).ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingTableExistsReturnsFalseIfTableDoesNotExist()
        {
            Processor.TableExists(null, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingTableExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            Processor.TableExists("TstSchma", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingTableExistsReturnsTrueIfTableExists()
        {
            using (var table = new Db2TestTable(Processor, null, "ID INT"))
            {
                Processor.TableExists(null, table.Name).ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingTableExistsReturnsTrueIfTableExistsWithSchema()
        {
            using (var table = new Db2TestTable(Processor, "TstSchma", "ID INT"))
            {
                Processor.TableExists("TstSchma", table.Name).ShouldBeTrue();
            }
        }
    }
}