using FluentMigrator.Runner.Processors.SqlServer;
using FluentMigrator.Tests.Helpers;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors.SqlServer
{
    [TestFixture]
    [Category("Integration")]
    public class SqlServerTableTests : BaseTableTests
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
        public override void CallingTableExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new SqlServerTestTable("Test'Table", Processor, null, "id int"))
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
            Processor.TableExists("test_schema", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingTableExistsReturnsTrueIfTableExists()
        {
            using (var table = new SqlServerTestTable(Processor, null, "id int"))
                Processor.TableExists(null, table.Name).ShouldBeTrue();
        }

        [Test]
        public override void CallingTableExistsReturnsTrueIfTableExistsWithSchema()
        {
            using (var table = new SqlServerTestTable(Processor, "test_schema", "id int"))
                Processor.TableExists("test_schema", table.Name).ShouldBeTrue();
        }
    }
}