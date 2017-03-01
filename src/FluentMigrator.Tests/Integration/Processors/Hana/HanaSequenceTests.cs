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
    public class HanaSequenceTests : BaseSequenceTests, IDisposable
    {
        public HanaConnection Connection { get; set; }
        public HanaProcessor Processor { get; set; }

        public HanaSequenceTests()
        {
            Connection = new HanaConnection(IntegrationTestOptions.Hana.ConnectionString);
            Processor = new HanaProcessor(Connection, new HanaGenerator(), new TextWriterAnnouncer(System.Console.Out), new ProcessorOptions(), new HanaDbFactory());
            Connection.Open();
            Processor.BeginTransaction();
        }

        public void Dispose()
        {
            Processor.CommitTransaction();
            Processor.Dispose();
        }


        [Fact]
        public override void CallingSequenceExistsReturnsFalseIfSequenceDoesNotExist()
        {
            Processor.SequenceExists(null, "DoesNotExist").ShouldBeFalse();
        }

        [Fact]
        public override void CallingSequenceExistsReturnsFalseIfSequenceDoesNotExistWithSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            Processor.SequenceExists("test_schema", "DoesNotExist").ShouldBeFalse();
        }

        [Fact]
        public override void CallingSequenceExistsReturnsTrueIfSequenceExists()
        {
            using (new HanaTestSequence(Processor, null, "test_sequence"))
                Processor.SequenceExists(null, "test_sequence").ShouldBeTrue();
        }

        [Fact]
        public override void CallingSequenceExistsReturnsTrueIfSequenceExistsWithSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            using (new HanaTestSequence(Processor, "test_schema", "test_sequence"))
                Processor.SequenceExists("test_schema", "test_sequence").ShouldBeTrue();
        }
    }
}
