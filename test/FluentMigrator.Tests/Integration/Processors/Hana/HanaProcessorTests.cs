using System.IO;

using FluentMigrator.Expressions;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators.Hana;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Hana;

using NUnit.Framework;
using NUnit.Should;

using Sap.Data.Hana;

namespace FluentMigrator.Tests.Integration.Processors.Hana
{
    [TestFixture]
    [Category("Integration")]
    public class HanaProcessorTests
    {
        public HanaConnection Connection { get; set; }
        public HanaProcessor Processor { get; set; }

        [SetUp]
        public void SetUp()
        {
            Connection = new HanaConnection(IntegrationTestOptions.Hana.ConnectionString);
            Processor = new HanaProcessor(Connection, new HanaGenerator(), new TextWriterAnnouncer(TestContext.Out),
                new ProcessorOptions(), new HanaDbFactory());
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
        public void CallingProcessWithPerformDbOperationExpressionWhenInPreviewOnlyModeWillNotMakeDbChanges()
        {
            var output = new StringWriter();

            var connection = new HanaConnection(IntegrationTestOptions.Hana.ConnectionString);

            var processor = new HanaProcessor(
                connection,
                new HanaGenerator(),
                new TextWriterAnnouncer(output),
                new ProcessorOptions { PreviewOnly = true },
                new HanaDbFactory());

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

            var fmOutput = output.ToString();
            Assert.That(fmOutput, Does.Contain("/* Performing DB Operation */"));
        }
    }
}
