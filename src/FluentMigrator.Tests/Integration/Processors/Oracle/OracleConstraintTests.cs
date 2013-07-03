using System.Data;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.Oracle;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Oracle;
using FluentMigrator.Tests.Helpers;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors.Oracle
{
    [TestFixture]
    [Category("Integration")]
    public class OracleConstraintTests : BaseConstraintTests
    {
        private const string SchemaName = "test";
        public IDbConnection Connection { get; set; }
        public OracleProcessor Processor { get; set; }
        public OracleDbFactory Factory { get; set; }
        public IQuoter Quoter { get; set; }

        [SetUp]
        public void SetUp()
        {
            Factory = new OracleDbFactory();
            Connection = Factory.CreateConnection(IntegrationTestOptions.Oracle.ConnectionString);
            Quoter = new OracleQuoter();
            Processor = new OracleProcessor(() => Connection, new OracleGenerator(), new TextWriterAnnouncer(System.Console.Out), new ProcessorOptions(), () => Factory);
            Connection.Open();
        }

        [TearDown]
        public void TearDown()
        {
            Processor.Dispose();
        }

        [Test]
        public override void CallingConstraintExistsCanAcceptConstraintNameWithSingleQuote()
        {
            using (var table = new OracleTestTable(Connection, null, Quoter.QuoteColumnName("id") + " int"))
            {
                table.WithUniqueConstraintOn("id","UC'id");
                Processor.ConstraintExists(null, table.Name, "UC'id").ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingConstraintExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new OracleTestTable("Test'Table", Connection, null, Quoter.QuoteColumnName("id") + " int"))
            {
                table.WithUniqueConstraintOn("id");
                Processor.ConstraintExists(null, table.Name, "UC_id").ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingConstraintExistsReturnsFalseIfConstraintDoesNotExist()
        {
            using (var table = new OracleTestTable(Connection, null, Quoter.QuoteColumnName("id") + " int"))
            {
                table.WithUniqueConstraintOn("id");
                Processor.ConstraintExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
            }
        }

        [Test]
        public override void CallingConstraintExistsReturnsFalseIfConstraintDoesNotExistWithSchema()
        {
            using (var table = new OracleTestTable(Connection, SchemaName, Quoter.QuoteColumnName("id") + " int"))
            {
                table.WithUniqueConstraintOn("id");
                Processor.ConstraintExists(SchemaName, table.Name, "DoesNotExist").ShouldBeFalse();
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
            Processor.ConstraintExists(SchemaName, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingConstraintExistsReturnsTrueIfConstraintExists()
        {
            using (var table = new OracleTestTable(Connection, null, Quoter.QuoteColumnName("id") + " int"))
            {
                table.WithUniqueConstraintOn("id");
                Processor.ConstraintExists(null, table.Name, "UC_id").ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingConstraintExistsReturnsTrueIfConstraintExistsWithSchema()
        {
            using (var table = new OracleTestTable(Connection, SchemaName, Quoter.QuoteColumnName("id") + " int"))
            {
                table.WithUniqueConstraintOn("id");
                Processor.ConstraintExists(SchemaName, table.Name, "UC_id").ShouldBeTrue();
            }
        }
    }
}
