using System.Data.SqlClient;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.Hana;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Hana;
using FluentMigrator.Tests.Helpers;
using NUnit.Framework;
using NUnit.Should;
using Sap.Data.Hana;

namespace FluentMigrator.Tests.Integration.Processors.Hana
{
    [TestFixture]
    [Category("Integration")]
    public class HanaSchemaExtensionsTests : BaseSchemaExtensionsTests
    {
        public HanaConnection Connection { get; set; }
        public HanaProcessor Processor { get; set; }
        public IQuoter Quoter { get; set; }

        [SetUp]
        public void SetUp()
        {
            Connection = new HanaConnection(IntegrationTestOptions.Hana.ConnectionString);
            Processor = new HanaProcessor(Connection, new HanaGenerator(), new TextWriterAnnouncer(System.Console.Out), new ProcessorOptions(), new HanaDbFactory());
            Quoter = new HanaQuoter();
            Connection.Open();
            Processor.BeginTransaction();
        }

        [TearDown]
        public void TearDown()
        {
            Processor.CommitTransaction();
            Processor.Dispose();
        }

        [Test]
        public override void CallingColumnExistsCanAcceptSchemaNameWithSingleQuote()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            using (var table = new HanaTestTable(Processor, "test'schema", "id int"))
                Processor.ColumnExists("test'schema", table.Name, "id").ShouldBeTrue();
        }

        [Test]
        public override void CallingConstraintExistsCanAcceptSchemaNameWithSingleQuote()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            using (var table = new HanaTestTable(Processor, "test'schema", "id int", "wibble int CONSTRAINT c1 CHECK(wibble > 0)"))
                Processor.ConstraintExists("test'schema", table.Name, "c1").ShouldBeTrue();
        }

        [Test]
        public override void CallingIndexExistsCanAcceptSchemaNameWithSingleQuote()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            using (var table = new HanaTestTable(Processor, "test'schema", "id int"))
            {
                var indexName = table.WithIndexOn("id");
                Processor.IndexExists("test'schema", table.Name, indexName).ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingSchemaExistsCanAcceptSchemaNameWithSingleQuote()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            using (new HanaTestTable(Processor, "test'schema", Quoter.QuoteColumnName("id") + " int"))
                Processor.SchemaExists("test'schema").ShouldBeTrue();
        }

        [Test]
        public override void CallingTableExistsCanAcceptSchemaNameWithSingleQuote()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            using (var table = new HanaTestTable(Processor, "test'schema", "id int"))
                Processor.TableExists("test'schema", table.Name).ShouldBeTrue();
        }

        [Test]
        public void CallingDefaultValueExistsCanAcceptSchemaNameWithSingleQuote()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            using (var table = new HanaTestTable(Processor, "test'schema", "id int"))
            {
                table.WithDefaultValueOn("id");
                Processor.DefaultValueExists("test'schema", table.Name, "id", 1).ShouldBeTrue();
            }
        }
    }
}