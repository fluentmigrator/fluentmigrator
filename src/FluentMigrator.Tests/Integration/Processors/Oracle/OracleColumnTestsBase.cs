using System.Data;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Oracle;
using FluentMigrator.Tests.Helpers;

using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors.Oracle
{
    [Category("Integration")]
    public abstract class OracleColumnTestsBase : BaseColumnTests
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
        public override void CallingColumnExistsCanAcceptColumnNameWithSingleQuote()
        {
            using (var table = new OracleTestTable(this.Connection, null, this.Factory, "\"i'd\" int"))
                this.Processor.ColumnExists(null, table.Name, "i'd").ShouldBeTrue();
        }

        [Test]
        public override void CallingColumnExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new OracleTestTable("Test'Table", this.Connection, null, this.Factory, "id int"))
                this.Processor.ColumnExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingColumnExistsReturnsFalseIfColumnDoesNotExist()
        {
            using (var table = new OracleTestTable(this.Connection, null, this.Factory, "id int"))
                this.Processor.ColumnExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingColumnExistsReturnsFalseIfColumnDoesNotExistWithSchema()
        {
            using (var table = new OracleTestTable(this.Connection, SchemaName, this.Factory, "id int"))
                this.Processor.ColumnExists(SchemaName, table.Name, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingColumnExistsReturnsFalseIfTableDoesNotExist()
        {
            this.Processor.ColumnExists(null, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingColumnExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            this.Processor.ColumnExists(SchemaName, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingColumnExistsReturnsTrueIfColumnExists()
        {
            using (var table = new OracleTestTable(this.Connection, null, this.Factory, "id int"))
                this.Processor.ColumnExists(null, table.Name, "id").ShouldBeTrue();
        }

        [Test]
        public override void CallingColumnExistsReturnsTrueIfColumnExistsWithSchema()
        {
            using (var table = new OracleTestTable(this.Connection, SchemaName, this.Factory, "id int"))
                this.Processor.ColumnExists(SchemaName, table.Name, "id").ShouldBeTrue();
        }
    }
}