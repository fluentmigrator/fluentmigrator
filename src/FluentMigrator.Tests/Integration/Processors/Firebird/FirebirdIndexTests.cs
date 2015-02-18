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
    public class FirebirdIndexTests : BaseIndexTests
    {
        public FbConnection Connection { get; set; }
        public FirebirdProcessor Processor { get; set; }
        public IQuoter Quoter { get; set; }

        [SetUp]
        public void SetUp()
        {
            if (!System.IO.File.Exists("fbtest.fdb"))
            {
                FbConnection.CreateDatabase(IntegrationTestOptions.Firebird.ConnectionString);
            }
            Connection = new FbConnection(IntegrationTestOptions.Firebird.ConnectionString);
            var options = FirebirdOptions.AutoCommitBehaviour();
            Processor = new FirebirdProcessor(Connection, new FirebirdGenerator(options), new TextWriterAnnouncer(System.Console.Out), new ProcessorOptions(), new FirebirdDbFactory(), options);
            Quoter = new FirebirdQuoter();
            Connection.Open();
            Processor.BeginTransaction();
        }

        [TearDown]
        public void TearDown()
        {
            if (!Processor.WasCommitted)
                Processor.CommitTransaction();
            Connection.Close();
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