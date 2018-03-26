using System.Data.SqlClient;
using System.IO;
using FluentMigrator.Builders.Execute;
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
    public class SqlServerProcessorTests : IntegrationTestBase
    {
        private string _connectionString;
        public SqlServerProcessor Processor { get; set; }

        [SetUp]
        public void SetUp()
        {
            if (ConfiguredDbEngine == "SqlServer2012")
            {
                Processor = CreateProcessor() as SqlServerProcessor;
                Processor.Connection.Open();
                Processor.BeginTransaction();
                _connectionString = Processor.Connection.ConnectionString;
            }
            else
                Assert.Ignore("Test is intended to run against SqlServer2012. Current configuration: {0}", ConfiguredDbEngine);
        }

        [TearDown]
        public void TearDown()
        {
            if (ConfiguredDbEngine == "SqlServer2012")
            {
                Processor.CommitTransaction();
                Processor.Dispose();
            }
        }

        [Test]
        public void CallingColumnExistsReturnsFalseIfColumnExistsInDifferentSchema()
        {
            using (var table = new SqlServerTestTable(Processor, "test_schema", "id int"))
                Processor.ColumnExists("test_schema2", table.Name, "id").ShouldBeFalse();
        }

        [Test]
        public void CallingConstraintExistsReturnsFalseIfConstraintExistsInDifferentSchema()
        {
            using (var table = new SqlServerTestTable(Processor, "test_schema", "id int", "wibble int CONSTRAINT c1 CHECK(wibble > 0)"))
                Processor.ConstraintExists("test_schema2", table.Name, "c1").ShouldBeFalse();
        }

        [Test]
        public void CallingTableExistsReturnsFalseIfTableExistsInDifferentSchema()
        {
            using (var table = new SqlServerTestTable(Processor, "test_schema", "id int"))
                Processor.TableExists("test_schema2", table.Name).ShouldBeFalse();
        }

        [Test]
        public void CallingProcessWithPerformDbOperationExpressionWhenInPreviewOnlyModeWillNotMakeDbChanges()
        {
            var output = new StringWriter();

            var connection = new SqlConnection(_connectionString);

            var processor = new SqlServerProcessor(
                connection,
                new SqlServer2012Generator(),
                new TextWriterAnnouncer(output),
                new ProcessorOptions { PreviewOnly = true },
                new SqlServerDbFactory());

            bool tableExists;

            try
            {
                var expression =
                    new PerformDBOperationExpression
                    {
                        Operation = (con, trans) =>
                        {
                            var command = con.CreateCommand();
                            command.CommandText = "CREATE TABLE ProcessTestTable (test int NULL) ";
                            command.Transaction = trans;

                            command.ExecuteNonQuery();
                        }
                    };

                processor.Process(expression);

                tableExists = processor.TableExists("", "ProcessTestTable");
            }
            finally
            {
                processor.RollbackTransaction();

            }

            tableExists.ShouldBeFalse();

            string fmOutput = output.ToString();
            Assert.That(fmOutput, Is.StringContaining("/* Performing DB Operation */"));
        }
    }
}