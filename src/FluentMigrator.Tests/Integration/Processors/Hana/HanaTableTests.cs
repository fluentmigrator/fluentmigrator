using System;
using System.Data.SqlClient;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators.Hana;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Hana;
using FluentMigrator.Tests.Helpers;
using Xunit;
using Sap.Data.Hana;

namespace FluentMigrator.Tests.Integration.Processors.Hana
{
    [Trait("Category", "Integration")]
    public class HanaTableTests : BaseTableTests, IDisposable
    {
        public HanaConnection Connection { get; set; }
        public HanaProcessor Processor { get; set; }

        public HanaTableTests()
        {
            Connection = new HanaConnection(IntegrationTestOptions.Hana.ConnectionString);
            Processor = new HanaProcessor(Connection, new HanaGenerator(), new TextWriterAnnouncer(System.Console.Out), new ProcessorOptions(), new HanaDbFactory());
            Connection.Open();
            Processor.BeginTransaction();
        }

        public void Disposable()
        {
            Processor.CommitTransaction();
            Processor.Dispose();
        }

        [Fact]
        public override void CallingTableExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new HanaTestTable("Test'Table", Processor, null, "id integer"))
                Processor.TableExists(null, table.Name).ShouldBeTrue();
        }

        [Fact]
        public override void CallingTableExistsReturnsFalseIfTableDoesNotExist()
        {
            Processor.TableExists(null, "DoesNotExist").ShouldBeFalse();
        }

        [Fact]
        public override void CallingTableExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            Processor.TableExists("test_schema", "DoesNotExist").ShouldBeFalse();
        }

        [Fact]
        public override void CallingTableExistsReturnsTrueIfTableExists()
        {
            using (var table = new HanaTestTable(Processor, null, "id int"))
                Processor.TableExists(null, table.Name).ShouldBeTrue();
        }
        
        [Fact]
        public override void CallingTableExistsReturnsTrueIfTableExistsWithSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            using (var table = new HanaTestTable(Processor, "test_schema", "id int"))
                Processor.TableExists("test_schema", table.Name).ShouldBeTrue();
        }
    }
}
