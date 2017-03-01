using System;
using System.Data.SqlClient;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.SqlServer;
using FluentMigrator.Tests.Helpers;
using Xunit;

namespace FluentMigrator.Tests.Integration.Processors.SqlServer
{
    [Trait("Category", "Integration")]
    public class SqlServerSchemaExtensionsTests : BaseSchemaExtensionsTests, IDisposable
    {
        public SqlConnection Connection { get; set; }
        public SqlServerProcessor Processor { get; set; }
        public IQuoter Quoter { get; set; }

        public SqlServerSchemaExtensionsTests()
        {
            Connection = new SqlConnection(IntegrationTestOptions.SqlServer2012.ConnectionString);
            Processor = new SqlServerProcessor(Connection, new SqlServer2012Generator(), new TextWriterAnnouncer(System.Console.Out), new ProcessorOptions(), new SqlServerDbFactory());
            Quoter = new SqlServerQuoter();
            Connection.Open();
            Processor.BeginTransaction();
        }

        public void Dispose()
        {
            Processor.CommitTransaction();
            Processor.Dispose();
        }

        [Fact]
        public override void CallingColumnExistsCanAcceptSchemaNameWithSingleQuote()
        {
            using (var table = new SqlServerTestTable(Processor, "test'schema", "id int"))
                Processor.ColumnExists("test'schema", table.Name, "id").ShouldBeTrue();
        }

        [Fact]
        public override void CallingConstraintExistsCanAcceptSchemaNameWithSingleQuote()
        {
            using (var table = new SqlServerTestTable(Processor, "test'schema", "id int", "wibble int CONSTRAINT c1 CHECK(wibble > 0)"))
                Processor.ConstraintExists("test'schema", table.Name, "c1").ShouldBeTrue();
        }

        [Fact]
        public override void CallingIndexExistsCanAcceptSchemaNameWithSingleQuote()
        {
            using (var table = new SqlServerTestTable(Processor, "test'schema", "id int"))
            {
                var indexName = table.WithIndexOn("id");
                Processor.IndexExists("test'schema", table.Name, indexName).ShouldBeTrue();
            }
        }

        [Fact]
        public override void CallingSchemaExistsCanAcceptSchemaNameWithSingleQuote()
        {
            using (new SqlServerTestTable(Processor, "test'schema", Quoter.QuoteColumnName("id") + " int"))
                Processor.SchemaExists("test'schema").ShouldBeTrue();
        }

        [Fact]
        public override void CallingTableExistsCanAcceptSchemaNameWithSingleQuote()
        {
            using (var table = new SqlServerTestTable(Processor, "test'schema", "id int"))
                Processor.TableExists("test'schema", table.Name).ShouldBeTrue();
        }

        [Fact]
        public void CallingDefaultValueExistsCanAcceptSchemaNameWithSingleQuote()
        {
            using (var table = new SqlServerTestTable(Processor, "test'schema", "id int"))
            {
                table.WithDefaultValueOn("id");
                Processor.DefaultValueExists("test'schema", table.Name, "id", 1).ShouldBeTrue();
            }
        }
    }
}
