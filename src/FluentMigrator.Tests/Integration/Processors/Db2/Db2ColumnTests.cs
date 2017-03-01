namespace FluentMigrator.Tests.Integration.Processors.Db2
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using FluentMigrator.Runner.Announcers;
    using FluentMigrator.Runner.Generators;
    using FluentMigrator.Runner.Generators.DB2;
    using FluentMigrator.Runner.Processors;
    using FluentMigrator.Runner.Processors.DB2;
    using FluentMigrator.Tests.Helpers;

    using Xunit;

    [Trait("Category", "Integration")]
    public class Db2ColumnTests : BaseColumnTests, IDisposable
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
        public override void CallingColumnExistsCanAcceptColumnNameWithSingleQuote()
        {
            var columnName = Quoter.Quote("I'D") + " INT";
            using (var table = new Db2TestTable(Processor, null, columnName))
            {
                Processor.ColumnExists(null, table.Name, "I'D").ShouldBeTrue();
            }
        }

        [Fact]
        public override void CallingColumnExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new Db2TestTable("Test'Table", Processor, null, "ID INT"))
            {
                Processor.ColumnExists(null, table.Name, "ID").ShouldBeTrue();
            }
        }

        [Fact]
        public override void CallingColumnExistsReturnsFalseIfColumnDoesNotExist()
        {
            using (var table = new Db2TestTable(Processor, null, "ID INT"))
            {
                Processor.ColumnExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
            }
        }

        [Fact]
        public override void CallingColumnExistsReturnsFalseIfColumnDoesNotExistWithSchema()
        {
            using (var table = new Db2TestTable(Processor, "TstSchma", "ID INT"))
            {
                Processor.ColumnExists("TstSchma", table.Name, "DoesNotExist").ShouldBeFalse();
            }
        }

        [Fact]
        public override void CallingColumnExistsReturnsFalseIfTableDoesNotExist()
        {
            Processor.ColumnExists(null, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Fact]
        public override void CallingColumnExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            Processor.ColumnExists("TstSchma", "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Fact]
        public override void CallingColumnExistsReturnsTrueIfColumnExists()
        {
            using (var table = new Db2TestTable(Processor, null, "ID INT"))
            {
                Processor.ColumnExists(null, table.Name, "ID").ShouldBeTrue();
            }
        }

        [Fact]
        public override void CallingColumnExistsReturnsTrueIfColumnExistsWithSchema()
        {
            using (var table = new Db2TestTable(Processor, "TstSchma", "ID INT"))
            {
                Processor.ColumnExists("TstSchma", table.Name, "ID").ShouldBeTrue();
            }
        }

        public Db2ColumnTests()
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
