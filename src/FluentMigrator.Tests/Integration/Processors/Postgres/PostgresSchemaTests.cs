using FluentMigrator.Runner.Processors.Postgres;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors.Postgres
{
    [TestFixture]
    [Category("Integration")]
    public class PostgresSchemaTests : BaseSchemaTests
    {
        public PostgresProcessor Processor { get; set; }

        [SetUp]
        public void SetUp()
        {
            if (ConfiguredProcessor.IsAssignableFrom(typeof(PostgresProcessor)))
            {
                Processor = CreateProcessor() as PostgresProcessor;
            }
            else
                Assert.Ignore("Test is intended to run against Postgres. Current configuration: {0}", ConfiguredDbEngine);
        }

        [TearDown]
        public void TearDown()
        {
            if (ConfiguredProcessor.IsAssignableFrom(typeof(PostgresProcessor)))
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
            Processor.SchemaExists("public").ShouldBeTrue();
        }

    }
}