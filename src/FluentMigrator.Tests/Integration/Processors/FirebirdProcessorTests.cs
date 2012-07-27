using System.Data;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Tests.Helpers;
using NUnit.Framework;
using NUnit.Should;
using FirebirdSql.Data.FirebirdClient;
using FluentMigrator.Runner.Generators.Firebird;
using FluentMigrator.Runner.Processors.Firebird;

namespace FluentMigrator.Tests.Integration.Processors
{
    [TestFixture]
    public class FirebirdProcessorTests
    {
        private readonly FirebirdQuoter quoter = new FirebirdQuoter();
        public FbConnection Connection { get; set; }
        public FirebirdProcessor Processor { get; set; }

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
        }

        [TearDown]
        public void TearDown()
        {
            Processor.CommitTransaction();
            Connection.Close();
        }
        
        [Test]
        public void CallingTableExistsReturnsTrueIfTableExists()
        {
            using (var table = new FirebirdTestTable(Processor, null, "id int"))
                Processor.TableExists(null, table.Name).ShouldBeTrue();
        }

        [Test]
        public void CallingTableExistsReturnsFalseIfTableDoesNotExist()
        {
            Processor.TableExists(null, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public void CallingColumnExistsReturnsTrueIfColumnExists()
        {
            using (var table = new FirebirdTestTable(Processor, null, "id int"))
                Processor.ColumnExists(null, table.Name, "ID").ShouldBeTrue();
        }

        [Test]
        public void CallingColumnExistsReturnsFalseIfTableDoesNotExist()
        {
            Processor.ColumnExists(null, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public void CallingColumnExistsReturnsFalseIfColumnDoesNotExist()
        {
            using (var table = new FirebirdTestTable(Processor, null, "id int"))
                Processor.ColumnExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public void CallingContraintExistsReturnsTrueIfConstraintExists()
        {
            using (var table = new FirebirdTestTable(Processor, null, "id int", "wibble int CONSTRAINT c1 CHECK(wibble > 0)"))
                Processor.ConstraintExists(null, table.Name, "C1").ShouldBeTrue();
        }

        [Test]
        public void CallingConstraintExistsReturnsFalseIfTableDoesNotExist()
        {
            Processor.ConstraintExists(null, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public void CallingConstraintExistsReturnsFalseIfConstraintDoesNotExist()
        {
            using (var table = new FirebirdTestTable(Processor, null, "id int"))
                Processor.ConstraintExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public void CallingIndexExistsReturnsTrueIfIndexExists()
        {
            using (var table = new FirebirdTestTable(Processor, null, "id int"))
            {
                Processor.CheckTable(table.Name);
                Processor.LockTable(table.Name);
                var idxName = string.Format("idx_{0}", table.Name);

                using (var cmd = table.Connection.CreateCommand())
                {
                    cmd.Transaction = table.Transaction;
                    cmd.CommandText = string.Format("CREATE INDEX {0} ON {1} (id)", quoter.QuoteIndexName(idxName), quoter.QuoteTableName(table.Name));
                    cmd.ExecuteNonQuery();
                }

                Processor.AutoCommit();

                Processor.IndexExists(null, table.Name, idxName).ShouldBeTrue();
            }
        }

        [Test]
        public void CallingIndexExistsReturnsFalseIfTableDoesNotExist()
        {
            Processor.IndexExists(null, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public void CallingIndexExistsReturnsFalseIfIndexDoesNotExist()
        {
            using (var table = new FirebirdTestTable(Processor, null, "id int"))
                Processor.IndexExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public void CallingIndexExistsCanAcceptIndexNameWithSingleQuote()
        {
            using (var table = new FirebirdTestTable(Processor, null, "id int"))
            {
                Processor.CheckTable(table.Name);
                Processor.LockTable(table.Name);
                var idxName = string.Format("id'x_{0}", table.Name);

                using (var cmd = table.Connection.CreateCommand())
                {
                    cmd.Transaction = table.Transaction;
                    cmd.CommandText = string.Format("CREATE INDEX {0} ON {1} (id)", quoter.QuoteIndexName(idxName), quoter.QuoteTableName(table.Name));
                    cmd.ExecuteNonQuery();
                }

                Processor.AutoCommit();

                Processor.IndexExists(null, table.Name, idxName).ShouldBeTrue();
            }
        }

        [Test]
        public void CallingIndexExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new FirebirdTestTable(null, "TestSingle'Quote", Processor, "id int"))
            {
                Processor.CheckTable(table.Name);
                Processor.LockTable(table.Name);
                var idxName = string.Format("idx_{0}", table.Name);

                using (var cmd = table.Connection.CreateCommand())
                {
                    cmd.Transaction = table.Transaction;
                    cmd.CommandText = string.Format("CREATE INDEX {0} ON {1} (id)", quoter.QuoteIndexName(idxName), quoter.QuoteTableName(table.Name));
                    cmd.ExecuteNonQuery();
                }

                Processor.AutoCommit();

                Processor.IndexExists(null, table.Name, idxName).ShouldBeTrue();
            }
        }
        
        [Test]
        public void CanReadData()
        {
            using (var table = new FirebirdTestTable(Processor, null, "id int"))
            {
                Processor.CheckTable(table.Name);
                AddTestData(table);

                using (DataSet ds = Processor.Read("SELECT * FROM {0}", quoter.QuoteTableName(table.Name)))
                {
                    ds.ShouldNotBeNull();
                    ds.Tables.Count.ShouldBe(1);
                    ds.Tables[0].Rows.Count.ShouldBe(3);
                    ds.Tables[0].Rows[2][0].ShouldBe(2);
                }
            }
        }

        [Test]
        public void CanReadTableData()
        {
            using (var table = new FirebirdTestTable(Processor, null, "id int"))
            {
                Processor.CheckTable(table.Name);
                AddTestData(table);

                using (DataSet ds = Processor.ReadTableData(null, table.Name))
                {
                    ds.ShouldNotBeNull();
                    ds.Tables.Count.ShouldBe(1);
                    ds.Tables[0].Rows.Count.ShouldBe(3);
                    ds.Tables[0].Rows[2][0].ShouldBe(2);
                }
            }
        }

        private void AddTestData(FirebirdTestTable table)
        {
            for (int i = 0; i < 3; i++)
            {
                using (var cmd = table.Connection.CreateCommand())
                {
                    cmd.Transaction = table.Transaction;
                    cmd.CommandText = string.Format("INSERT INTO {0} (id) VALUES ({1})", quoter.QuoteTableName(table.Name), i);
                    cmd.ExecuteNonQuery();
                }
            }

            Processor.AutoCommit();
        }

        [Test]
        public void CallingTableExistsReturnsTrueIfTableExistsWithSchema()
        {
            using (var table = new FirebirdTestTable(Processor, "TestSchema", "id int"))
                Processor.TableExists("TestSchema", table.Name).ShouldBeTrue();
        }

        [Test]
        public void CallingTableExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            Processor.TableExists("TestSchema", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public void CallingTableExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new FirebirdTestTable(null, "TestSingle'Quote", Processor, "id int"))
                Processor.TableExists(null, table.Name).ShouldBeTrue();
        }

        [Test]
        public void CallingTableExistsCanAcceptSchemaNameWithSingleQuote()
        {
            using (var table = new FirebirdTestTable(Processor, "Test'Schema", "id int"))
                Processor.TableExists("Test'Schema", table.Name).ShouldBeTrue();
        }

        [Test]
        public void CallingColumnExistsReturnsTrueIfColumnExistsWithSchema()
        {
            using (var table = new FirebirdTestTable(Processor, "TestSchema", "id int"))
                Processor.ColumnExists("TestSchema", table.Name, "ID").ShouldBeTrue();
        }

        [Test]
        public void CallingColumnExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            Processor.ColumnExists("TestSchema", "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public void CallingColumnExistsReturnsFalseIfColumnDoesNotExistWithSchema()
        {
            using (var table = new FirebirdTestTable(Processor, "TestSchema", "id int"))
                Processor.ColumnExists("TestSchema", table.Name, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public void CallingColumnExistsCanAcceptColumnNameWithSingleQuote()
        {
            var columnNameWithSingleQuote = quoter.QuoteColumnName("i'd");
            using (var table = new FirebirdTestTable(Processor, null, string.Format("{0} int", columnNameWithSingleQuote)))
                Processor.ColumnExists(null, table.Name, "i'd").ShouldBeTrue();
        }

        [Test]
        public void CallingColumnExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new FirebirdTestTable("TestSchema", "TestSingle'Quote", Processor, "id int"))
                Processor.ColumnExists("TestSchema", table.Name, "ID").ShouldBeTrue();
        }

        [Test]
        public void CallingColumnExistsCanAcceptSchemaNameWithSingleQuote()
        {
            using (var table = new FirebirdTestTable("Test'Schema", "TestSingle'Quote", Processor, "id int"))
                Processor.ColumnExists("Test'Schema", table.Name, "ID").ShouldBeTrue();
        }

        [Test]
        public void CallingConstraintExistsCanAcceptConstraintNameWithSingleQuote()
        {
            using (var table = new FirebirdTestTable(Processor, "TestSchema", "id int", @"wibble int CONSTRAINT ""c'1"" CHECK(wibble > 0)"))
                Processor.ConstraintExists("TestSchema", table.Name, "c'1").ShouldBeTrue();
        }

        [Test]
        public void CallingConstraintExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new FirebirdTestTable("TestSchema", "TestSingle'Quote", Processor, "id int", "wibble int CONSTRAINT c1 CHECK(wibble > 0)"))
                Processor.ConstraintExists("TestSchema", table.Name, "C1").ShouldBeTrue();
        }

        [Test]
        public void CallingConstraintExistsCanAcceptSchemaNameWithSingleQuote()
        {
            using (var table = new FirebirdTestTable(Processor, "Test'Schema", "id int", "wibble int CONSTRAINT c1 CHECK(wibble > 0)"))
                Processor.ConstraintExists("Test'Schema", table.Name, "C1").ShouldBeTrue();
        }

        [Test]
        public void CallingConstraintExistsReturnsTrueIfConstraintExistsWithSchema()
        {
            using (var table = new FirebirdTestTable(Processor, "TestSchema", "id int", "wibble int CONSTRAINT C1 CHECK(wibble > 0)"))
                Processor.ConstraintExists("TestSchema", table.Name, "C1").ShouldBeTrue();
        }

        [Test]
        public void CallingConstraintExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            Processor.ConstraintExists("TestSchema", "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public void CallingConstraintExistsReturnsFalseIfConstraintDoesNotExistWithSchema()
        {
            using (var table = new FirebirdTestTable(Processor, "TestSchema", "id int"))
                Processor.ConstraintExists("TestSchema", table.Name, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public void CallingIndexExistsReturnsTrueIfIndexExistsWithSchema()
        {
            using (var table = new FirebirdTestTable(Processor, "TestSchema", "id int"))
            {
                Processor.CheckTable(table.Name);
                Processor.LockTable(table.Name);
                var idxName = string.Format("idx_{0}", table.Name);

                using (var cmd = table.Connection.CreateCommand())
                {
                    cmd.Transaction = table.Transaction;
                    cmd.CommandText = string.Format("CREATE INDEX {0} ON {1} (id)", quoter.QuoteIndexName(idxName), quoter.QuoteTableName(table.Name));
                    cmd.ExecuteNonQuery();
                }

                Processor.AutoCommit();

                Processor.IndexExists("TestSchema", table.Name, idxName).ShouldBeTrue();
            }
        }

        [Test]
        public void CallingIndexExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            Processor.IndexExists("TestSchema", "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public void CallingIndexExistsReturnsFalseIfIndexDoesNotExistWithSchema()
        {
            using (var table = new FirebirdTestTable(Processor, "TestSchema", "id int"))
                Processor.IndexExists("TestSchema", table.Name, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public void CanReadDataWithSchema()
        {
            using (var table = new FirebirdTestTable(Processor, "TestSchema", "id int"))
            {
                Processor.CheckTable(table.Name);
                AddTestData(table);

                using (DataSet ds = Processor.Read("SELECT * FROM {0}", quoter.QuoteTableName(table.Name)))
                {
                    ds.ShouldNotBeNull();
                    ds.Tables.Count.ShouldBe(1);
                    ds.Tables[0].Rows.Count.ShouldBe(3);
                    ds.Tables[0].Rows[2][0].ShouldBe(2);
                }
            }
        }

        [Test]
        public void CanReadTableDataWithSchema()
        {
            using (var table = new FirebirdTestTable(Processor, "TestSchema", "id int"))
            {
                Processor.CheckTable(table.Name);
                AddTestData(table);

                using (DataSet ds = Processor.ReadTableData("TestSchema", table.Name))
                {
                    ds.ShouldNotBeNull();
                    ds.Tables.Count.ShouldBe(1);
                    ds.Tables[0].Rows.Count.ShouldBe(3);
                    ds.Tables[0].Rows[2][0].ShouldBe(2);
                }
            }
        }

    }
}