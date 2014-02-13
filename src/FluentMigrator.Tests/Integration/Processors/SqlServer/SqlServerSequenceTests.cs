using System.Data.SqlClient;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.SqlServer;
using FluentMigrator.Tests.Helpers;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors.SqlServer
{
    [TestFixture]
    [Category("Integration")]
    public class SqlServerSequenceTests : BaseSequenceTests
    {
        public SqlConnection Connection { get; set; }
        public SqlServerProcessor Processor { get; set; }

        [SetUp]
        public void SetUp()
        {
            if (!IntegrationTestOptions.SqlServer2012.IsEnabled)
            {
                Assert.Ignore("Only MS Sql Server 2012 has support for sequences. Ignoring these tests.");
            }

            Connection = new SqlConnection(IntegrationTestOptions.SqlServer2012.ConnectionString);
            Processor = new SqlServerProcessor(Connection, new SqlServer2012Generator(), new TextWriterAnnouncer(System.Console.Out), new ProcessorOptions(), new SqlServerDbFactory());
            Connection.Open();
            Processor.BeginTransaction();
        }

        [TearDown]
        public void TearDown()
        {
            if (TestHasFailed())
            {
                Processor.Dispose();
                return;
            }

            if (IntegrationTestOptions.SqlServer2012.IsEnabled)
            {
                Processor.CommitTransaction();
                Processor.Dispose();
            }
        }

        private static bool TestHasFailed()
        {
            return TestContext.CurrentContext.Result.Status == TestStatus.Failed;
        }

        [Test]
        public override void CallingSequenceExistsReturnsFalseIfSequenceDoesNotExist()
        {
            Processor.SequenceExists(null, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingSequenceExistsReturnsFalseIfSequenceDoesNotExistWithSchema()
        {
            Processor.SequenceExists("test_schema", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingSequenceExistsReturnsTrueIfSequenceExists()
        {
            using(var testSequence = new SqlServerTestSequence(Processor, null, "test_sequence"))
            {
                testSequence.Create();
                Processor.SequenceExists(null, "test_sequence").ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingSequenceExistsReturnsTrueIfSequenceExistsWithSchema()
        {
            using (var testSequence = new SqlServerTestSequence(Processor, "test_schema", "test_sequence"))
            {
                testSequence.Create();
                Processor.SequenceExists("test_schema", "test_sequence").ShouldBeTrue();                
            }
        }
    }
}