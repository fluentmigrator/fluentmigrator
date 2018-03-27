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

    using NUnit.Framework;
    using NUnit.Should;
    using FluentMigrator.Tests.Helpers;

    [TestFixture]
    [Category("Integration")]
    public class Db2ProcessorTests
    {
        #region Properties

        public System.Data.IDbConnection Connection
        {
            get; set;
        }

        public Db2DbFactory Factory
        {
            get; set;
        }

        public Db2Processor Processor
        {
            get; set;
        }

        public Db2Quoter Quoter
        {
            get; set;
        }

        #endregion Properties

        #region Methods

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

        [Test]
        public void CallingColumnExistsReturnsFalseIfColumnExistsInDifferentSchema()
        {
            using (var table = new Db2TestTable(Processor, "TstSchma", "ID INT"))
            {
                Processor.ColumnExists("DNE", table.Name, "ID").ShouldBeFalse();
            }
        }

        [Test]
        public void CallingConstraintExistsReturnsFalseIfConstraintExistsInDifferentSchema()
        {
            using (var table = new Db2TestTable(Processor, "TstSchma", "ID INT"))
            {
                table.WithUniqueConstraintOn("ID", "c1");
                Processor.ConstraintExists("DNE", table.Name, "c1").ShouldBeFalse();
            }
        }

        [Test]
        public void CallingTableExistsReturnsFalseIfTableExistsInDifferentSchema()
        {
            using (var table = new Db2TestTable(Processor, "TstSchma", "ID INT"))
            {
                Processor.TableExists("DNE", table.Name).ShouldBeFalse();
            }
        }

        #endregion Methods
    }
}