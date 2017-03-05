using FluentMigrator.Runner.Generators.DB2;
using FluentMigrator.Runner.Processors.DB2;

using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors.Db2
{

    [TestFixture]
    [Category("Integration")]
    public class Db2ColumnTests : BaseColumnTests
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
        public override void CallingColumnExistsCanAcceptColumnNameWithSingleQuote()
        {
            var columnName = new Db2Quoter().Quote("I'D") + " INT";
            using (var table = new Db2TestTable(Processor, null, columnName))
            {
                Processor.ColumnExists(null, table.Name, "I'D").ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingColumnExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new Db2TestTable("Test'Table", Processor, null, "ID INT"))
            {
                Processor.ColumnExists(null, table.Name, "ID").ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingColumnExistsReturnsFalseIfColumnDoesNotExist()
        {
            using (var table = new Db2TestTable(Processor, null, "ID INT"))
            {
                Processor.ColumnExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
            }
        }

        [Test]
        public override void CallingColumnExistsReturnsFalseIfColumnDoesNotExistWithSchema()
        {
            using (var table = new Db2TestTable(Processor, "TstSchma", "ID INT"))
            {
                Processor.ColumnExists("TstSchma", table.Name, "DoesNotExist").ShouldBeFalse();
            }
        }

        [Test]
        public override void CallingColumnExistsReturnsFalseIfTableDoesNotExist()
        {
            Processor.ColumnExists(null, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingColumnExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            Processor.ColumnExists("TstSchma", "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingColumnExistsReturnsTrueIfColumnExists()
        {
            using (var table = new Db2TestTable(Processor, null, "ID INT"))
            {
                Processor.ColumnExists(null, table.Name, "ID").ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingColumnExistsReturnsTrueIfColumnExistsWithSchema()
        {
            using (var table = new Db2TestTable(Processor, "TstSchma", "ID INT"))
            {
                Processor.ColumnExists("TstSchma", table.Name, "ID").ShouldBeTrue();
            }
        }
    }
}