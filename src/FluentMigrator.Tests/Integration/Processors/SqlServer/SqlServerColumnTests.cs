using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.Runner.Processors.SqlServer;
using FluentMigrator.Tests.Helpers;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors.SqlServer
{
    [TestFixture]
    [Category("Integration")]
    public class SqlServerColumnTests : BaseColumnTests
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
        public override void CallingColumnExistsCanAcceptColumnNameWithSingleQuote()
        {
            var columnNameWithSingleQuote = new SqlServerQuoter().Quote("i'd");
            using (var table = new SqlServerTestTable(Processor, null, string.Format("{0} int", columnNameWithSingleQuote)))
                Processor.ColumnExists(null, table.Name, "i'd").ShouldBeTrue();
        }

        [Test]
        public override void CallingColumnExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new SqlServerTestTable("Test'Table", Processor, null, "id int"))
                Processor.ColumnExists(null, table.Name, "id").ShouldBeTrue();
        }

        [Test]
        public override void CallingColumnExistsReturnsFalseIfColumnDoesNotExist()
        {
            using (var table = new SqlServerTestTable(Processor, null, "id int"))
                Processor.ColumnExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingColumnExistsReturnsFalseIfColumnDoesNotExistWithSchema()
        {
            using (var table = new SqlServerTestTable(Processor, "test_schema", "id int"))
                Processor.ColumnExists("test_schema", table.Name, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingColumnExistsReturnsFalseIfTableDoesNotExist()
        {
            Processor.ColumnExists(null, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingColumnExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            Processor.ColumnExists("test_schema", "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingColumnExistsReturnsTrueIfColumnExists()
        {
            using (var table = new SqlServerTestTable(Processor, null, "id int"))
                Processor.ColumnExists(null, table.Name, "id").ShouldBeTrue();
        }

        [Test]
        public override void CallingColumnExistsReturnsTrueIfColumnExistsWithSchema()
        {
            using (var table = new SqlServerTestTable(Processor, "test_schema", "id int"))
                Processor.ColumnExists("test_schema", table.Name, "id").ShouldBeTrue();
        }
    }
}