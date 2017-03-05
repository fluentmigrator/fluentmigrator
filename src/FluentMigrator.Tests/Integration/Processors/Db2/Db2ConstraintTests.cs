using FluentMigrator.Runner.Processors.DB2;

using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors.Db2
{

    [TestFixture]
    [Category("Integration")]
    public class Db2ConstraintTests : BaseConstraintTests
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
        public override void CallingConstraintExistsCanAcceptConstraintNameWithSingleQuote()
        {
            using (var table = new Db2TestTable(Processor, null, "ID INT"))
            {
                table.WithUniqueConstraintOn("ID", "C'1");
                Processor.ConstraintExists(null, table.Name, "C'1").ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingConstraintExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new Db2TestTable("Test'Table", Processor, null, "ID INT"))
            {
                table.WithUniqueConstraintOn("ID", "C'1");
                Processor.ConstraintExists(null, table.Name, "C'1").ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingConstraintExistsReturnsFalseIfConstraintDoesNotExist()
        {
            using (var table = new Db2TestTable(Processor, null, "ID INT"))
            {
                Processor.ConstraintExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
            }
        }

        [Test]
        public override void CallingConstraintExistsReturnsFalseIfConstraintDoesNotExistWithSchema()
        {
            using (var table = new Db2TestTable(Processor, "TstSchma", "ID INT"))
            {
                Processor.ConstraintExists("TstSchma", table.Name, "DoesNotExist").ShouldBeFalse();
            }
        }

        [Test]
        public override void CallingConstraintExistsReturnsFalseIfTableDoesNotExist()
        {
            Processor.ConstraintExists(null, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingConstraintExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            Processor.ConstraintExists("TstSchma", "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingConstraintExistsReturnsTrueIfConstraintExists()
        {
            using (var table = new Db2TestTable(Processor, null, "ID INT"))
            {
                table.WithUniqueConstraintOn("ID", "C1");
                Processor.ConstraintExists(null, table.Name, "C1").ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingConstraintExistsReturnsTrueIfConstraintExistsWithSchema()
        {
            using (var table = new Db2TestTable(Processor, "TstSchma", "ID INT"))
            {
                table.WithUniqueConstraintOn("ID", "C1");
                Processor.ConstraintExists("TstSchma", table.Name, "C1").ShouldBeTrue();
            }
        }
    }
}