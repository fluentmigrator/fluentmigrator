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

    using NUnit.Framework;
    using NUnit.Should;

    [TestFixture]
    [Category("Integration")]
    public class Db2ConstraintTests : BaseConstraintTests
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

        [Test]
        public override void CallingConstraintExistsCanAcceptConstraintNameWithSingleQuote()
        {
            using (var table = new Db2TestTable(Processor, null, "ID INT"))
            {
                table.WithUniqueConstraintOn("ID", "C'1");
                Processor.ConstraintExists(null, table.Name, "C'1").ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingConstraintExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new Db2TestTable("Test'Table", Processor, null, "ID INT"))
            {
                table.WithUniqueConstraintOn("ID", "C'1");
                Processor.ConstraintExists(null, table.Name, "C'1").ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingConstraintExistsReturnsFalseIfConstraintDoesNotExist()
        {
            using (var table = new Db2TestTable(Processor, null, "ID INT"))
            {
                Processor.ConstraintExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
            }
        }

        [Test]
        public override void CallingConstraintExistsReturnsFalseIfConstraintDoesNotExistWithSchema()
        {
            using (var table = new Db2TestTable(Processor, "TstSchma", "ID INT"))
            {
                Processor.ConstraintExists("TstSchma", table.Name, "DoesNotExist").ShouldBeFalse();
            }
        }

        [Test]
        public override void CallingConstraintExistsReturnsFalseIfTableDoesNotExist()
        {
            Processor.ConstraintExists(null, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingConstraintExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            Processor.ConstraintExists("TstSchma", "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingConstraintExistsReturnsTrueIfConstraintExists()
        {
            using (var table = new Db2TestTable(Processor, null, "ID INT"))
            {
                table.WithUniqueConstraintOn("ID", "C1");
                Processor.ConstraintExists(null, table.Name, "C1").ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingConstraintExistsReturnsTrueIfConstraintExistsWithSchema()
        {
            using (var table = new Db2TestTable(Processor, "TstSchma", "ID INT"))
            {
                table.WithUniqueConstraintOn("ID", "C1");
                Processor.ConstraintExists("TstSchma", table.Name, "C1").ShouldBeTrue();
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