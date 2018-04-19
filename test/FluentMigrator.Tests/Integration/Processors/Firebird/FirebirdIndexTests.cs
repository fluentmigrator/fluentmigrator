using FirebirdSql.Data.FirebirdClient;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators;
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
    public class FirebirdIndexTests : BaseIndexTests
    {
        private readonly FirebirdLibraryProber _prober = new FirebirdLibraryProber();
        private TemporaryDatabase _temporaryDatabase;

        public FbConnection Connection { get; set; }
        public FirebirdProcessor Processor { get; set; }
        public IQuoter Quoter { get; set; }

        [SetUp]
        public void SetUp()
        {
            if (!IntegrationTestOptions.Firebird.IsEnabled)
                Assert.Ignore();
            _temporaryDatabase = new TemporaryDatabase(
                IntegrationTestOptions.Firebird,
                _prober);
            Connection = new FbConnection(_temporaryDatabase.ConnectionString);
            var options = FirebirdOptions.AutoCommitBehaviour();
            Processor = new FirebirdProcessor(Connection, new FirebirdGenerator(options), new TextWriterAnnouncer(TestContext.Out), new ProcessorOptions(), new FirebirdDbFactory(serviceProvider: null), options);
            Quoter = new FirebirdQuoter(false);
            Connection.Open();
            Processor.BeginTransaction();
        }

        [TearDown]
        public void TearDown()
        {
            if (Processor == null)
                return;

            if (!Processor.WasCommitted)
                Processor.CommitTransaction();
            Connection.Close();

            FbDatabase.DropDatabase(_temporaryDatabase.ConnectionString);
        }

        [Test]
        public override void CallingIndexExistsCanAcceptIndexNameWithSingleQuote()
        {
            using (var table = new FirebirdTestTable(Processor, null, "id int"))
            {
                Processor.CheckTable(table.Name);
                Processor.LockTable(table.Name);
                var idxName = string.Format("\"id'x_{0}\"", table.Name);

                using (var cmd = table.Connection.CreateCommand())
                {
                    cmd.Transaction = table.Transaction;
                    cmd.CommandText = string.Format("CREATE INDEX {0} ON {1} (id)", idxName, table.Name);
                    cmd.ExecuteNonQuery();
                }

                Processor.AutoCommit();

                Processor.IndexExists(null, table.Name, idxName).ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingIndexExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new FirebirdTestTable("\"Test'Table\"", Processor, null, "id int"))
            {
                Processor.CheckTable(table.Name);
                Processor.LockTable(table.Name);
                var idxName = "\"idx_Test'Table\"";

                using (var cmd = table.Connection.CreateCommand())
                {
                    cmd.Transaction = table.Transaction;
                    cmd.CommandText = string.Format("CREATE INDEX {0} ON {1} (id)", idxName, table.Name);
                    cmd.ExecuteNonQuery();
                }

                Processor.AutoCommit();

                Processor.IndexExists(null, table.Name, idxName).ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingIndexExistsReturnsFalseIfIndexDoesNotExist()
        {
            using (var table = new FirebirdTestTable(Processor, null, "id int"))
                Processor.IndexExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingIndexExistsReturnsFalseIfIndexDoesNotExistWithSchema()
        {
            using (var table = new FirebirdTestTable(Processor, "TestSchema", "id int"))
                Processor.IndexExists("TestSchema", table.Name, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingIndexExistsReturnsFalseIfTableDoesNotExist()
        {
            Processor.IndexExists(null, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingIndexExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            Processor.IndexExists("TestSchema", "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingIndexExistsReturnsTrueIfIndexExists()
        {
            using (var table = new FirebirdTestTable(Processor, null, "id int"))
            {
                Processor.CheckTable(table.Name);
                Processor.LockTable(table.Name);
                var idxName = string.Format("idx_{0}", table.Name);

                using (var cmd = table.Connection.CreateCommand())
                {
                    cmd.Transaction = table.Transaction;
                    cmd.CommandText = string.Format("CREATE INDEX {0} ON {1} (id)", Quoter.QuoteIndexName(idxName), Quoter.QuoteTableName(table.Name));
                    cmd.ExecuteNonQuery();
                }

                Processor.AutoCommit();

                Processor.IndexExists(null, table.Name, idxName).ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingIndexExistsReturnsTrueIfIndexExistsWithSchema()
        {
            using (var table = new FirebirdTestTable(Processor, "TestSchema", "id int"))
            {
                Processor.CheckTable(table.Name);
                Processor.LockTable(table.Name);
                var idxName = string.Format("idx_{0}", table.Name);

                using (var cmd = table.Connection.CreateCommand())
                {
                    cmd.Transaction = table.Transaction;
                    cmd.CommandText = string.Format("CREATE INDEX {0} ON {1} (id)", Quoter.QuoteIndexName(idxName), Quoter.QuoteTableName(table.Name));
                    cmd.ExecuteNonQuery();
                }

                Processor.AutoCommit();

                Processor.IndexExists("TestSchema", table.Name, idxName).ShouldBeTrue();
            }
        }
    }
}
