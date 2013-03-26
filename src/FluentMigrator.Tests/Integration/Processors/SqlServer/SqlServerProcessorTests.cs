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
    public class SqlServerProcessorTests
    {
        public SqlConnection Connection { get; set; }
        public SqlServerProcessor Processor { get; set; }

        [SetUp]
        public void SetUp()
        {
            Connection = new SqlConnection(IntegrationTestOptions.SqlServer2012.ConnectionString);
            Processor = new SqlServerProcessor(Connection, new SqlServer2012Generator(), new TextWriterAnnouncer(System.Console.Out), new ProcessorOptions(), new SqlServerDbFactory());
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

            var connection = new SqlConnection(IntegrationTestOptions.SqlServer2012.ConnectionString);

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