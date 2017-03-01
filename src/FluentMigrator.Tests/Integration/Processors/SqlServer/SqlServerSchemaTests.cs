using System;
using System.Data.SqlClient;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.SqlServer;
using Xunit;

namespace FluentMigrator.Tests.Integration.Processors.SqlServer
{
    [Trait("Category", "Integration")]
    public class SqlServerSchemaTests : BaseSchemaTests, IDisposable
    {
        public SqlConnection Connection { get; set; }
        public SqlServerProcessor Processor { get; set; }

        public SqlServerSchemaTests()
        {
            Connection = new SqlConnection(IntegrationTestOptions.SqlServer2012.ConnectionString);
            Processor = new SqlServerProcessor(Connection, new SqlServer2012Generator(), new TextWriterAnnouncer(System.Console.Out), new ProcessorOptions(), new SqlServerDbFactory());
            Connection.Open();
            Processor.BeginTransaction();
        }

        public void Dispose()
        {
            Processor.CommitTransaction();
            Processor.Dispose();
        }

        [Fact]
        public override void CallingSchemaExistsReturnsFalseIfSchemaDoesNotExist()
        {
            Processor.SchemaExists("DoesNotExist").ShouldBeFalse();
        }

        [Fact]
        public override void CallingSchemaExistsReturnsTrueIfSchemaExists()
        {
            Processor.SchemaExists("dbo").ShouldBeTrue();
        }
    }
}
