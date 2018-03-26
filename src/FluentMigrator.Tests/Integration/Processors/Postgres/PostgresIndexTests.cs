using FluentMigrator.Runner.Generators.Postgres;
using FluentMigrator.Runner.Processors.Postgres;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors.Postgres
{
    [TestFixture]
    [Category("Integration")]
    public class PostgresIndexTests : BaseIndexTests
    {
        public PostgresProcessor Processor { get; set; }

        [SetUp]
        public void SetUp()
        {
            if (ConfiguredProcessor.IsAssignableFrom(typeof(PostgresProcessor)))
            {
                Processor = CreateProcessor() as PostgresProcessor;
            }
            else
                Assert.Ignore("Test is intended to run against Postgres. Current configuration: {0}", ConfiguredDbEngine);
        }

        [TearDown]
        public void TearDown()
        {
            if (ConfiguredProcessor.IsAssignableFrom(typeof(PostgresProcessor)))
            {
                Processor.CommitTransaction();
                Processor.Dispose();
            }
        }

        [Test]
        public override void CallingIndexExistsCanAcceptIndexNameWithSingleQuote()
        {
            using (var table = new PostgresTestTable(Processor, null, "id int"))
            {
                var idxName = string.Format("\"id'x_{0}\"", new PostgresQuoter().UnQuote(table.Name));

                var cmd = table.Connection.CreateCommand();
                cmd.Transaction = table.Transaction;
                cmd.CommandText = string.Format("CREATE INDEX {0} ON {1} (id)", idxName, table.Name);
                cmd.ExecuteNonQuery();

                Processor.IndexExists(null, table.Name, idxName).ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingIndexExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new PostgresTestTable("Test'Table", Processor, null, "id int"))
            {
                var idxName = string.Format("\"idx_{0}\"", new PostgresQuoter().UnQuote(table.Name));

                var cmd = table.Connection.CreateCommand();
                cmd.Transaction = table.Transaction;
                cmd.CommandText = string.Format("CREATE INDEX {0} ON {1} (id)", idxName, table.Name);
                cmd.ExecuteNonQuery();

                Processor.IndexExists(null, table.Name, idxName).ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingIndexExistsReturnsFalseIfIndexDoesNotExist()
        {
            using (var table = new PostgresTestTable(Processor, null, "id int"))
                Processor.IndexExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingIndexExistsReturnsFalseIfIndexDoesNotExistWithSchema()
        {
            using (var table = new PostgresTestTable(Processor, "TestSchema", "id int"))
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
            using (var table = new PostgresTestTable(Processor, null, "id int"))
            {
                var idxName = string.Format("\"idx_{0}\"", new PostgresQuoter().UnQuote(table.Name));

                var cmd = table.Connection.CreateCommand();
                cmd.Transaction = table.Transaction;
                cmd.CommandText = string.Format("CREATE INDEX {0} ON {1} (id)", idxName, table.Name);
                cmd.ExecuteNonQuery();

                Processor.IndexExists(null, table.Name, idxName).ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingIndexExistsReturnsTrueIfIndexExistsWithSchema()
        {
            using (var table = new PostgresTestTable(Processor, "TestSchema", "id int"))
            {
                var idxName = string.Format("\"idx_{0}\"", new PostgresQuoter().UnQuote(table.Name));

                var cmd = table.Connection.CreateCommand();
                cmd.Transaction = table.Transaction;
                cmd.CommandText = string.Format("CREATE INDEX {0} ON {1} (id)", idxName, table.NameWithSchema);
                cmd.ExecuteNonQuery();

                Processor.IndexExists("TestSchema", table.Name, idxName).ShouldBeTrue();
            }
        }
    }
}