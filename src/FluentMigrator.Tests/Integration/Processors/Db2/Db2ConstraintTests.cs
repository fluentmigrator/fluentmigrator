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
    public class Db2ConstraintTests : BaseConstraintTests, IDisposable
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

        [Fact]
        public override void CallingConstraintExistsCanAcceptConstraintNameWithSingleQuote()
        {
            using (var table = new Db2TestTable(Processor, null, "ID INT"))
            {
                table.WithUniqueConstraintOn("ID", "C'1");
                Processor.ConstraintExists(null, table.Name, "C'1").ShouldBeTrue();
            }
        }

        [Fact]
        public override void CallingConstraintExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new Db2TestTable("Test'Table", Processor, null, "ID INT"))
            {
                table.WithUniqueConstraintOn("ID", "C'1");
                Processor.ConstraintExists(null, table.Name, "C'1").ShouldBeTrue();
            }
        }

        [Fact]
        public override void CallingConstraintExistsReturnsFalseIfConstraintDoesNotExist()
        {
            using (var table = new Db2TestTable(Processor, null, "ID INT"))
            {
                Processor.ConstraintExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
            }
        }

        [Fact]
        public override void CallingConstraintExistsReturnsFalseIfConstraintDoesNotExistWithSchema()
        {
            using (var table = new Db2TestTable(Processor, "TstSchma", "ID INT"))
            {
                Processor.ConstraintExists("TstSchma", table.Name, "DoesNotExist").ShouldBeFalse();
            }
        }

        [Fact]
        public override void CallingConstraintExistsReturnsFalseIfTableDoesNotExist()
        {
            Processor.ConstraintExists(null, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Fact]
        public override void CallingConstraintExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            Processor.ConstraintExists("TstSchma", "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Fact]
        public override void CallingConstraintExistsReturnsTrueIfConstraintExists()
        {
            using (var table = new Db2TestTable(Processor, null, "ID INT"))
            {
                table.WithUniqueConstraintOn("ID", "C1");
                Processor.ConstraintExists(null, table.Name, "C1").ShouldBeTrue();
            }
        }

        [Fact]
        public override void CallingConstraintExistsReturnsTrueIfConstraintExistsWithSchema()
        {
            using (var table = new Db2TestTable(Processor, "TstSchma", "ID INT"))
            {
                table.WithUniqueConstraintOn("ID", "C1");
                Processor.ConstraintExists("TstSchma", table.Name, "C1").ShouldBeTrue();
            }
        }

        public Db2ConstraintTests()
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
