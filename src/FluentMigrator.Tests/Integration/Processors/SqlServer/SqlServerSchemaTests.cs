using FluentMigrator.Runner.Processors.SqlServer;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors.SqlServer
{
    [TestFixture]
    [Category("Integration")]
    public class SqlServerSchemaTests : BaseSchemaTests
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
        public override void CallingSchemaExistsReturnsFalseIfSchemaDoesNotExist()
        {
            Processor.SchemaExists("DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingSchemaExistsReturnsTrueIfSchemaExists()
        {
            Processor.SchemaExists("dbo").ShouldBeTrue();
        }
    }
}