using FluentMigrator.Runner.Generators.Firebird;
using FluentMigrator.Runner.Processors.Firebird;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors.Firebird
{
    [TestFixture]
    [Category("Integration")]
    [Category("Firebird")]
    public class FirebirdIndexTests : BaseIndexTests
    {
        [Test]
        public override void CallingIndexExistsCanAcceptIndexNameWithSingleQuote()
        {
            ExecuteFor(FIREBIRD, processor =>
            {
                var fbProcessor = processor as FirebirdProcessor;
                using (var table = new FirebirdTestTable(fbProcessor, null, "id int"))
                {
                    fbProcessor.CheckTable(table.Name);
                    fbProcessor.LockTable(table.Name);
                    var idxName = string.Format("\"id'x_{0}\"", table.Name);

                    using (var cmd = table.Connection.CreateCommand())
                    {
                        cmd.Transaction = table.Transaction;
                        cmd.CommandText = string.Format("CREATE INDEX {0} ON {1} (id)", idxName, table.Name);
                        cmd.ExecuteNonQuery();
                    }

                    fbProcessor.AutoCommit();

                    fbProcessor.IndexExists(null, table.Name, idxName).ShouldBeTrue();
                }
            });
        }

        [Test]
        public override void CallingIndexExistsCanAcceptTableNameWithSingleQuote()
        {
            ExecuteFor(FIREBIRD, processor =>
            {
                var fbProcessor = processor as FirebirdProcessor;
                using (var table = new FirebirdTestTable("\"Test'Table\"", fbProcessor, null, "id int"))
                {
                    fbProcessor.CheckTable(table.Name);
                    fbProcessor.LockTable(table.Name);
                    var idxName = "\"idx_Test'Table\"";

                    using (var cmd = table.Connection.CreateCommand())
                    {
                        cmd.Transaction = table.Transaction;
                        cmd.CommandText = string.Format("CREATE INDEX {0} ON {1} (id)", idxName, table.Name);
                        cmd.ExecuteNonQuery();
                    }

                    fbProcessor.AutoCommit();

                    fbProcessor.IndexExists(null, table.Name, idxName).ShouldBeTrue();
                }
            });
        }

        [Test]
        public override void CallingIndexExistsReturnsFalseIfIndexDoesNotExist()
        {
            ExecuteFor(FIREBIRD, processor =>
            {
                var fbProcessor = processor as FirebirdProcessor;
                using (var table = new FirebirdTestTable(fbProcessor, null, "id int"))
                    fbProcessor.IndexExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
            });
        }

        [Test]
        public override void CallingIndexExistsReturnsFalseIfIndexDoesNotExistWithSchema()
        {
            ExecuteFor(FIREBIRD, processor =>
            {
                var fbProcessor = processor as FirebirdProcessor;
                using (var table = new FirebirdTestTable(fbProcessor, "TestSchema", "id int"))
                    fbProcessor.IndexExists("TestSchema", table.Name, "DoesNotExist").ShouldBeFalse();
            });
        }

        [Test]
        public override void CallingIndexExistsReturnsFalseIfTableDoesNotExist()
        {
            ExecuteFor(FIREBIRD, processor =>
            {
                processor.IndexExists(null, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
            });
        }

        [Test]
        public override void CallingIndexExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            ExecuteFor(FIREBIRD, processor =>
            {
                processor.IndexExists("TestSchema", "DoesNotExist", "DoesNotExist").ShouldBeFalse();
            });
        }

        [Test]
        public override void CallingIndexExistsReturnsTrueIfIndexExists()
        {
            ExecuteFor(FIREBIRD, processor =>
            {
                var fbProcessor = processor as FirebirdProcessor;
                using (var table = new FirebirdTestTable(fbProcessor, null, "id int"))
                {
                    fbProcessor.CheckTable(table.Name);
                    fbProcessor.LockTable(table.Name);
                    var idxName = string.Format("idx_{0}", table.Name);

                    using (var cmd = table.Connection.CreateCommand())
                    {
                        var quoter = new FirebirdQuoter();
                        cmd.Transaction = table.Transaction;
                        cmd.CommandText = string.Format("CREATE INDEX {0} ON {1} (id)", quoter.QuoteIndexName(idxName), quoter.QuoteTableName(table.Name));
                        cmd.ExecuteNonQuery();
                    }

                    fbProcessor.AutoCommit();

                    fbProcessor.IndexExists(null, table.Name, idxName).ShouldBeTrue();
                }
            });
        }

        [Test]
        public override void CallingIndexExistsReturnsTrueIfIndexExistsWithSchema()
        {
            ExecuteFor(FIREBIRD, processor =>
            {
                var fbProcessor = processor as FirebirdProcessor;
                using (var table = new FirebirdTestTable(fbProcessor, "TestSchema", "id int"))
                {
                    fbProcessor.CheckTable(table.Name);
                    fbProcessor.LockTable(table.Name);
                    var idxName = string.Format("idx_{0}", table.Name);

                    using (var cmd = table.Connection.CreateCommand())
                    {
                        var quoter = new FirebirdQuoter();
                        cmd.Transaction = table.Transaction;
                        cmd.CommandText = string.Format("CREATE INDEX {0} ON {1} (id)", quoter.QuoteIndexName(idxName), quoter.QuoteTableName(table.Name));
                        cmd.ExecuteNonQuery();
                    }

                    fbProcessor.AutoCommit();

                    fbProcessor.IndexExists("TestSchema", table.Name, idxName).ShouldBeTrue();
                }
            });
        }
    }
}