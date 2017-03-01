namespace FluentMigrator.Tests.Integration.Processors.Db2
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using FluentMigrator.Runner.Announcers;
    using FluentMigrator.Runner.Generators.DB2;
    using FluentMigrator.Runner.Processors;
    using FluentMigrator.Runner.Processors.DB2;
    using FluentMigrator.Tests.Helpers;

    using Xunit;

    [Trait("Category", "Integration")]
    public class Db2SchemaTests : BaseSchemaTests, IDisposable
    {
        #region Properties

        public System.Data.IDbConnection Connection
        {
            get;
            set;
        }

        public Db2DbFactory Factory
        {
            get;
            set;
        }

        public Db2Processor Processor
        {
            get;
            set;
        }

        public Db2Quoter Quoter
        {
            get;
            set;
        }

        #endregion Properties

        #region Methods

        [Fact]
        public override void CallingSchemaExistsReturnsFalseIfSchemaDoesNotExist()
        {
            Processor.SchemaExists("DNE").ShouldBeFalse();
        }

        [Fact]
        public override void CallingSchemaExistsReturnsTrueIfSchemaExists()
        {
            using (var table = new Db2TestTable(Processor, "TstSchma", "ID INT"))
            {
                Processor.SchemaExists("TstSchma").ShouldBeTrue();
            }
        }

        public Db2SchemaTests()
        {
            Factory = new Db2DbFactory();
            Connection = Factory.CreateConnection(IntegrationTestOptions.Db2.ConnectionString);
            Quoter = new Db2Quoter();
            Processor = new Db2Processor(Connection, new Db2Generator(), new TextWriterAnnouncer(System.Console.Out), new ProcessorOptions(), Factory);
            Connection.Open();
        }

        public void Dispose()
        {
            Processor.Dispose();
        }

        #endregion Methods
    }
}
