using System.Data;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Oracle;
using FluentMigrator.Tests.Helpers;

using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors.Oracle
{
    [Category("Integration")]
    public abstract class OracleTableTestsBase : BaseTableTests
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
        public override void CallingTableExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new OracleTestTable("Test'Table", this.Connection, null, this.Factory, "id int"))
                this.Processor.TableExists(null, table.Name).ShouldBeTrue();
        }

        [Test]
        public override void CallingTableExistsReturnsFalseIfTableDoesNotExist()
        {
            this.Processor.TableExists(null, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingTableExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            this.Processor.TableExists(SchemaName, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingTableExistsReturnsTrueIfTableExists()
        {
            using (var table = new OracleTestTable(this.Connection, null, this.Factory, "id int"))
                this.Processor.TableExists(null, table.Name).ShouldBeTrue();
        }

        [Test]
        public override void CallingTableExistsReturnsTrueIfTableExistsWithSchema()
        {
            using (var table = new OracleTestTable(this.Connection, SchemaName, this.Factory, "id int"))
                this.Processor.TableExists(SchemaName, table.Name).ShouldBeTrue();
        }
    }
}