using FirebirdSql.Data.FirebirdClient;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators.Firebird;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Firebird;
using FluentMigrator.Tests.Helpers;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors.Firebird
{
    [TestFixture]
    [Category("Integration")]
    [Category("Firebird")]
    public class FirebirdTableTests : BaseTableTests
    {
        private readonly FirebirdLibraryProber _prober = new FirebirdLibraryProber();
        private TemporaryDatabase _temporaryDatabase;

        public FbConnection Connection { get; set; }
        public FirebirdProcessor Processor { get; set; }

        [SetUp]
        public void SetUp()
        {
            _temporaryDatabase = new TemporaryDatabase(
                IntegrationTestOptions.Firebird.ConnectionString,
                _prober);
            Connection = new FbConnection(_temporaryDatabase.ConnectionString);
            var options = FirebirdOptions.AutoCommitBehaviour();
            Processor = new FirebirdProcessor(Connection, new FirebirdGenerator(options), new TextWriterAnnouncer(TestContext.Out), new ProcessorOptions(), new FirebirdDbFactory(), options);
            Connection.Open();
            Processor.BeginTransaction();
        }

        [TearDown]
        public void TearDown()
        {
            if (!Processor.WasCommitted)
                Processor.CommitTransaction();
            Connection.Close();

            FbDatabase.DropDatabase(_temporaryDatabase.ConnectionString);
        }

        [Test]
        public override void CallingTableExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new FirebirdTestTable("\"Test'Table\"", Processor, null, "id int"))
                Processor.TableExists(null, table.Name).ShouldBeTrue();
        }

        [Test]
        public override void CallingTableExistsReturnsFalseIfTableDoesNotExist()
        {
            Processor.TableExists(null, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingTableExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            Processor.TableExists("TestSchema", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingTableExistsReturnsTrueIfTableExists()
        {
            using (var table = new FirebirdTestTable(Processor, null, "id int"))
                Processor.TableExists(null, table.Name).ShouldBeTrue();
        }

        [Test]
        public override void CallingTableExistsReturnsTrueIfTableExistsWithSchema()
        {
            using (var table = new FirebirdTestTable(Processor, "TestSchema", "id int"))
                Processor.TableExists("TestSchema", table.Name).ShouldBeTrue();
        }


    }
}
