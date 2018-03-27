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

    using NUnit.Framework;
    using NUnit.Should;

    [TestFixture]
    [Category("Integration")]
    public class Db2TableTests : BaseTableTests
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

        [Test]
        public override void CallingTableExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new Db2TestTable("Test'Table", Processor, null, "ID INT"))
            {
                Processor.TableExists(null, table.Name).ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingTableExistsReturnsFalseIfTableDoesNotExist()
        {
            Processor.TableExists(null, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingTableExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            Processor.TableExists("TstSchma", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingTableExistsReturnsTrueIfTableExists()
        {
            using (var table = new Db2TestTable(Processor, null, "ID INT"))
            {
                Processor.TableExists(null, table.Name).ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingTableExistsReturnsTrueIfTableExistsWithSchema()
        {
            using (var table = new Db2TestTable(Processor, "TstSchma", "ID INT"))
            {
                Processor.TableExists("TstSchma", table.Name).ShouldBeTrue();
            }
        }

        [SetUp]
        public void SetUp()
        {
            Factory = new Db2DbFactory();
            Connection = Factory.CreateConnection(IntegrationTestOptions.Db2.ConnectionString);
            Quoter = new Db2Quoter();
            Processor = new Db2Processor(Connection, new Db2Generator(), new TextWriterAnnouncer(System.Console.Out), new ProcessorOptions(), Factory);
            Connection.Open();
        }

        [TearDown]
        public void TearDown()
        {
            Processor.Dispose();
        }

        #endregion Methods
    }
}