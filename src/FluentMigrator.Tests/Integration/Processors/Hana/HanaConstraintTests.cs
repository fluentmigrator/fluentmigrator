using FluentMigrator.Runner.Processors.Hana;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors.Hana
{
    [TestFixture]
    [Category("Integration")]
    public class HanaConstraintTests : BaseConstraintTests
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
        public override void CallingConstraintExistsCanAcceptConstraintNameWithSingleQuote()
        {
            using (var table = new HanaTestTable(Processor, null, "id int"))
            {
                table.WithUniqueConstraintOn("ID", "UC'id");
                Processor.ConstraintExists(null, table.Name, "UC'id").ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingConstraintExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new HanaTestTable("Test'Table", Processor, null, "id int"))
            {
                table.WithUniqueConstraintOn("ID");
                Processor.ConstraintExists(null, table.Name, "UC_id").ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingConstraintExistsReturnsFalseIfConstraintDoesNotExist()
        {
            using (var table = new HanaTestTable(this.Processor, null, "id int"))
            {
                table.WithUniqueConstraintOn("ID");
                Processor.ConstraintExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
            }
        }

        [Test]
        public override void CallingConstraintExistsReturnsFalseIfConstraintDoesNotExistWithSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            using (var table = new HanaTestTable(Processor, "schemaName", "id int"))
            {
                table.WithUniqueConstraintOn("ID");
                Processor.ConstraintExists("schemaName", table.Name, "DoesNotExist").ShouldBeFalse();
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
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            Processor.ConstraintExists("SchemaName", "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingConstraintExistsReturnsTrueIfConstraintExists()
        {
            using (var table = new HanaTestTable(Processor, null, "id int"))
            {
                table.WithUniqueConstraintOn("ID");
                Processor.ConstraintExists(null, table.Name, "UC_id").ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingConstraintExistsReturnsTrueIfConstraintExistsWithSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            using (var table = new HanaTestTable(Processor, "schema", "id int"))
            {
                table.WithUniqueConstraintOn("ID");
                Processor.ConstraintExists("schema", table.Name, "UC_id").ShouldBeTrue();
            }
        }
    }
}