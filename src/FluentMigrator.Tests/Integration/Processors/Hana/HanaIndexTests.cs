using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.Hana;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Hana;
using FluentMigrator.Tests.Helpers;
using Xunit;
using Sap.Data.Hana;

namespace FluentMigrator.Tests.Integration.Processors.Hana
{
    [Trait("Category", "Integration")]
    public class HanaIndexTests : BaseIndexTests
    {
        public HanaConnection Connection { get; set; }
        public HanaProcessor Processor { get; set; }
        public IQuoter Quoter { get; set; }

        public HanaIndexTests()
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

        [Fact]
        public override void CallingIndexExistsCanAcceptIndexNameWithSingleQuote()
        {
            const string columnSingleQuote = "i'd";
            using (var table = new HanaTestTable(Processor, null, Quoter.Quote(columnSingleQuote) +  " int"))
            {
                var indexName = table.WithIndexOn(columnSingleQuote);
                Processor.IndexExists(null, table.Name, indexName).ShouldBeTrue();
            }
        }

        [Fact]
        public override void CallingIndexExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new HanaTestTable("Test'Table", Processor, null, "\"id\" int"))
            {
                var indexName = table.WithIndexOn("id");
                Processor.IndexExists(null, table.Name, indexName).ShouldBeTrue();
            }
        }

        [Fact]
        public override void CallingIndexExistsReturnsFalseIfIndexDoesNotExist()
        {
            using (var table = new HanaTestTable(Processor, null, "id int"))
                Processor.IndexExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
        }

        [Fact]
        public override void CallingIndexExistsReturnsFalseIfIndexDoesNotExistWithSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            using (var table = new HanaTestTable(Processor, "test_schema", "id int"))
                Processor.IndexExists("test_schema", table.Name, "DoesNotExist").ShouldBeFalse();
        }

        [Fact]
        public override void CallingIndexExistsReturnsFalseIfTableDoesNotExist()
        {
            Processor.IndexExists(null, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Fact]
        public override void CallingIndexExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            Processor.IndexExists("test_schema", "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Fact]
        public override void CallingIndexExistsReturnsTrueIfIndexExists()
        {
            using (var table = new HanaTestTable(Processor, null, "\"id\" int"))
            {
                var indexName = table.WithIndexOn("id");
                Processor.IndexExists(null, table.Name, indexName).ShouldBeTrue();
            }
        }

        [Fact]
        public override void CallingIndexExistsReturnsTrueIfIndexExistsWithSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            using (var table = new HanaTestTable(Processor, "test_schema", "id int"))
            {
                var indexName = table.WithIndexOn("id");
                Processor.IndexExists("test_schema", table.Name, indexName).ShouldBeTrue();
            }
        }
    }
}
