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
    public class Db2TableTests : BaseTableTests, IDisposable
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
            get; set;
        }

        public Db2Quoter Quoter
        {
            get;
            set;
        }

        #endregion Properties

        #region Methods

        [Fact]
        public override void CallingTableExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new Db2TestTable("Test'Table", Processor, null, "ID INT"))
            {
                Processor.TableExists(null, table.Name).ShouldBeTrue();
            }
        }

        [Fact]
        public override void CallingTableExistsReturnsFalseIfTableDoesNotExist()
        {
            Processor.TableExists(null, "DoesNotExist").ShouldBeFalse();
        }

        [Fact]
        public override void CallingTableExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            Processor.TableExists("TstSchma", "DoesNotExist").ShouldBeFalse();
        }

        [Fact]
        public override void CallingTableExistsReturnsTrueIfTableExists()
        {
            using (var table = new Db2TestTable(Processor, null, "ID INT"))
            {
                Processor.TableExists(null, table.Name).ShouldBeTrue();
            }
        }

        [Fact]
        public override void CallingTableExistsReturnsTrueIfTableExistsWithSchema()
        {
            using (var table = new Db2TestTable(Processor, "TstSchma", "ID INT"))
            {
                Processor.TableExists("TstSchma", table.Name).ShouldBeTrue();
            }
        }

        public Db2TableTests()
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
