using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.Runner.Processors.SqlServer;
using FluentMigrator.Tests.Helpers;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors.SqlServer
{
    [TestFixture]
    [Category("Integration")]
    public class SqlServerConstraintTests : BaseConstraintTests
    {
        public SqlServerProcessor Processor { get; set; }

        [SetUp]
        public void SetUp()
        {
            if (ConfiguredDbEngine == "SqlServer2012")
            {
                Processor = CreateProcessor() as SqlServerProcessor;
                Processor.Connection.Open();
                Processor.BeginTransaction();
            }
            else
                Assert.Ignore("Test is intended to run against SqlServer2012. Current configuration: {0}", ConfiguredDbEngine);
        }

        [TearDown]
        public void TearDown()
        {
            if (ConfiguredDbEngine == "SqlServer2012")
            {
                Processor.CommitTransaction();
                Processor.Dispose();
            }
        }

        [Test]
        public override void CallingConstraintExistsCanAcceptConstraintNameWithSingleQuote()
        {
            var constraintName = new SqlServerQuoter().QuoteConstraintName("c'1");

            using (var table = new SqlServerTestTable(Processor, null, "id int", string.Format("wibble int CONSTRAINT {0} CHECK(wibble > 0)", constraintName)))
                Processor.ConstraintExists(null, table.Name, "c'1").ShouldBeTrue();
        }

        [Test]
        public override void CallingConstraintExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new SqlServerTestTable("Test'Table", Processor, null, "id int", "wibble int CONSTRAINT c1 CHECK(wibble > 0)"))
                Processor.ConstraintExists(null, table.Name, "c1").ShouldBeTrue();
        }

        [Test]
        public override void CallingConstraintExistsReturnsFalseIfConstraintDoesNotExist()
        {
            using (var table = new SqlServerTestTable(Processor, null, "id int"))
                Processor.ConstraintExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingConstraintExistsReturnsFalseIfConstraintDoesNotExistWithSchema()
        {
            using (var table = new SqlServerTestTable(Processor, "test_schema", "id int"))
                Processor.ConstraintExists("test_schema", table.Name, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingConstraintExistsReturnsFalseIfTableDoesNotExist()
        {
            Processor.ConstraintExists(null, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingConstraintExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            Processor.ConstraintExists("test_schema", "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingConstraintExistsReturnsTrueIfConstraintExists()
        {
            using (var table = new SqlServerTestTable(Processor, null, "id int", "wibble int CONSTRAINT c1 CHECK(wibble > 0)"))
                Processor.ConstraintExists(null, table.Name, "c1").ShouldBeTrue();
        }

        [Test]
        public override void CallingConstraintExistsReturnsTrueIfConstraintExistsWithSchema()
        {
            using (var table = new SqlServerTestTable(Processor, "test_schema", "id int", "wibble int CONSTRAINT c1 CHECK(wibble > 0)"))
                Processor.ConstraintExists("test_schema", table.Name, "c1").ShouldBeTrue();
        }
    }
}