using System.Data;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Oracle;
using FluentMigrator.Tests.Helpers;

using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors.Oracle
{
    [Category("Integration")]
    public abstract class OracleIndexTestsBase : BaseIndexTests
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
        public override void CallingIndexExistsCanAcceptIndexNameWithSingleQuote()
        {
            using (var table = new OracleTestTable(this.Connection, null, this.Factory, "id int"))
            {
                table.WithIndexOn("ID", "UI'id");
                this.Processor.IndexExists(null, table.Name, "UI'id").ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingIndexExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new OracleTestTable("Test'Table", this.Connection, null, this.Factory, "id int"))
            {
                table.WithIndexOn("ID");
                this.Processor.IndexExists(null, table.Name, "UI_id").ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingIndexExistsReturnsFalseIfIndexDoesNotExist()
        {
            using (var table = new OracleTestTable(this.Connection, null, this.Factory, "id int"))
            {
                table.WithIndexOn("ID");
                this.Processor.IndexExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
            }
        }

        [Test]
        public override void CallingIndexExistsReturnsFalseIfIndexDoesNotExistWithSchema()
        {
            using (var table = new OracleTestTable(this.Connection, SchemaName, this.Factory, "id int"))
            {
                table.WithIndexOn("ID");
                this.Processor.IndexExists(SchemaName, table.Name, "DoesNotExist").ShouldBeFalse();
            }
        }

        [Test]
        public override void CallingIndexExistsReturnsFalseIfTableDoesNotExist()
        {
            this.Processor.IndexExists(null, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingIndexExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            this.Processor.IndexExists(SchemaName, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingIndexExistsReturnsTrueIfIndexExists()
        {
            using (var table = new OracleTestTable(this.Connection, null, this.Factory, "id int"))
            {
                table.WithIndexOn("ID");
                this.Processor.IndexExists(null, table.Name, "UI_id").ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingIndexExistsReturnsTrueIfIndexExistsWithSchema()
        {
            using (var table = new OracleTestTable(this.Connection, SchemaName, this.Factory, "id int"))
            {
                table.WithIndexOn("ID");
                this.Processor.IndexExists(SchemaName, table.Name, "UI_id").ShouldBeTrue();
            }
        }
    }
}