using System.Data;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Oracle;

using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors.Oracle
{
    [Category("Integration")]
    public abstract class OracleSchemaTestsBase : BaseSchemaTests
    {
        private const string SchemaName = "test";
        private IDbConnection Connection { get; set; }
        private OracleProcessor Processor { get; set; }
        private IDbFactory Factory { get; set; }

        protected void SetUp(IDbFactory dbFactory)
        {
            if (ConfiguredProcessor.IsAssignableFrom(typeof(OracleProcessor)))
            {
                this.Processor = CreateProcessor() as OracleProcessor;
                this.Connection = Processor.Connection;
                this.Factory = Processor.Factory;
                this.Connection.Open();
            }
            else
                Assert.Ignore("Test is intended to run against Oracle. Current configuration: {0}", ConfiguredDbEngine);
        }

        [TearDown]
        public void TearDown()
        {
            if (ConfiguredProcessor.IsAssignableFrom(typeof(OracleProcessor)))
                this.Processor.Dispose();
        }

        [Test]
        public override void CallingSchemaExistsReturnsFalseIfSchemaDoesNotExist()
        {
            this.Processor.SchemaExists("DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingSchemaExistsReturnsTrueIfSchemaExists()
        {
            this.Processor.SchemaExists(SchemaName).ShouldBeTrue();
        }
    }
}