using System;
using System.Data;
using FirebirdSql.Data.FirebirdClient;
using FluentMigrator.Expressions;
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
    public class FirebirdProcessorTests
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
        public void CanReadData()
        {
            using (var table = new FirebirdTestTable(Processor, null, "id int"))
            {
                Processor.CheckTable(table.Name);
                AddTestData(table);

                using (DataSet ds = Processor.Read("SELECT * FROM {0}", Quoter.QuoteTableName(table.Name)))
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
                    cmd.CommandText = string.Format("INSERT INTO {0} (id) VALUES ({1})", Quoter.QuoteTableName(table.Name), i);
                    cmd.ExecuteNonQuery();
                }
            }

            Processor.AutoCommit();
        }

        [Test]
        public void CanReadDataWithSchema()
        {
            using (var table = new FirebirdTestTable(Processor, "TestSchema", "id int"))
            {
                Processor.CheckTable(table.Name);
                AddTestData(table);

                using (DataSet ds = Processor.Read("SELECT * FROM {0}", Quoter.QuoteTableName(table.Name)))
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

                using (var ds = Processor.ReadTableData("TestSchema", table.Name))
                {
                    ds.ShouldNotBeNull();
                    ds.Tables.Count.ShouldBe(1);
                    ds.Tables[0].Rows.Count.ShouldBe(3);
                    ds.Tables[0].Rows[2][0].ShouldBe(2);
                }
            }
        }

        [Test]
        public void CanCreateAndDropSequenceWithExistCheck()
        {
            Processor.SequenceExists("", "Sequence").ShouldBeFalse();
            using (new FirebirdTestTable(Processor, null, "id int"))
            {
                Processor.Process(new CreateSequenceExpression
                {
                    Sequence = { Name = "Sequence" }
                });

                Processor.SequenceExists("", "\"Sequence\"").ShouldBeTrue();
                Processor.SequenceExists("", "Sequence").ShouldBeFalse();
                
                Processor.Process(new DeleteSequenceExpression { SequenceName = "Sequence" });

                Processor.SequenceExists("", "\"Sequence\"").ShouldBeFalse();
                Processor.SequenceExists("", "Sequence").ShouldBeFalse();
            }
        }

        [Test]
        public void CanAlterSequence()
        {
            using (new FirebirdTestTable(Processor, null, "id int"))
            {
                Processor.Process(new CreateSequenceExpression
                {
                    Sequence = { Name = "Sequence", StartWith = 6 }
                });

                using (DataSet ds = Processor.Read("SELECT GEN_ID(\"Sequence\", 1) as generated_value FROM RDB$DATABASE"))
                {
                    ds.Tables[0].ShouldNotBeNull();
                    ds.Tables[0].Rows[0].ShouldNotBeNull();
                    ds.Tables[0].Rows[0]["generated_value"].ShouldBe(7);
                }

                Processor.Process(new DeleteSequenceExpression { SequenceName = "Sequence" });

                Processor.SequenceExists(String.Empty, "\"Sequence\"").ShouldBeFalse();
                Processor.SequenceExists("", "Sequence").ShouldBeFalse();
            }
        }

        [Test]
        public void CallingSequenceExistsReturnsFalseIfSequenceDoesNotExist()
        {
            Processor.SequenceExists("", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public void CanCreateTrigger()
        {
            using (var table = new FirebirdTestTable(Processor, null, "id int"))
            {
                Processor.Process(Processor.CreateTriggerExpression(table.Name, "TestTrigger", true, TriggerEvent.Insert, "as begin end"));
                Processor.TriggerExists(String.Empty, table.Name, "TestTrigger").ShouldBeTrue();
            }
        }

        [Test]
        public void CanDropTrigger()
        {
            using (var table = new FirebirdTestTable(Processor, null, "id int"))
            {
                Processor.Process(Processor.CreateTriggerExpression(table.Name, "TestTrigger", true, TriggerEvent.Insert, "as begin end"));
                Processor.TriggerExists(String.Empty, table.Name, "TestTrigger").ShouldBeTrue();

                Processor.Process(Processor.DeleteTriggerExpression(table.Name, "TestTrigger"));
                Processor.TriggerExists(String.Empty, table.Name, "TestTrigger").ShouldBeFalse();
            }
        }

        [Test]
        public void IdentityCanCreateIdentityColumn()
        {
            using (var table = new FirebirdTestTable(Processor, null, "bogus int"))
            {
                Processor.Process(new CreateColumnExpression
                {
                    TableName = table.Name,
                    Column = { Name = "id", IsIdentity = true, Type = DbType.Int64 }
                });
                Processor.ColumnExists(String.Empty, table.Name, "id").ShouldBeTrue();
                Processor.SequenceExists(String.Empty, String.Format("gen_{0}_id", table.Name)).ShouldBeTrue();
                Processor.TriggerExists(String.Empty, table.Name, String.Format("gen_id_{0}_id", table.Name)).ShouldBeTrue();
            }
        }

        [Test]
        public void IdentityCanDropIdentityColumn()
        {
            using (var table = new FirebirdTestTable(Processor, null, "bogus int"))
            {
                Processor.Process(new CreateColumnExpression
                {
                    TableName = table.Name,
                    Column = { Name = "id", IsIdentity = true, Type = DbType.Int64 }
                });
                Processor.ColumnExists(String.Empty, table.Name, "id").ShouldBeTrue();
                Processor.SequenceExists(String.Empty, String.Format("gen_{0}_id", table.Name)).ShouldBeTrue();
                Processor.TriggerExists(String.Empty, table.Name, String.Format("gen_id_{0}_id", table.Name)).ShouldBeTrue();

                Processor.Process(new DeleteColumnExpression
                {
                    TableName = table.Name,
                    ColumnNames = { "id" }
                });
                Processor.ColumnExists(String.Empty, table.Name, "id").ShouldBeFalse();
                Processor.SequenceExists(String.Empty, String.Format("gen_{0}_id", table.Name)).ShouldBeFalse();
                Processor.TriggerExists(String.Empty, table.Name, String.Format("gen_id_{0}_id", table.Name)).ShouldBeFalse();
            }
        }

        [Test]
        public void IdentityCanAlterColumnToIdentity()
        {
            using (var table = new FirebirdTestTable(Processor, null, "bogus int"))
            {
                Processor.Process(new CreateColumnExpression
                {
                    TableName = table.Name,
                    Column = { Name = "id", IsIdentity = false, Type = DbType.Int64 }
                });
                Processor.ColumnExists(String.Empty, table.Name, "id").ShouldBeTrue();
                Processor.SequenceExists(String.Empty, String.Format("gen_{0}_id", table.Name)).ShouldBeFalse();
                Processor.TriggerExists(String.Empty, table.Name, String.Format("gen_id_{0}_id", table.Name)).ShouldBeFalse();

                Processor.Process(new AlterColumnExpression
                {
                    TableName = table.Name,
                    Column = { Name = "id", IsIdentity = true, Type = DbType.Int64 }
                });
                Processor.ColumnExists(String.Empty, table.Name, "id").ShouldBeTrue();
                Processor.SequenceExists(String.Empty, String.Format("gen_{0}_id", table.Name)).ShouldBeTrue();
                Processor.TriggerExists(String.Empty, table.Name, String.Format("gen_id_{0}_id", table.Name)).ShouldBeTrue();
            }
        }

        [Test]
        public void IdentityCanAlterColumnToNotIdentity()
        {
            using (var table = new FirebirdTestTable(Processor, null, "bogus int"))
            {
                Processor.Process(new CreateColumnExpression
                {
                    TableName = table.Name,
                    Column = { Name = "id", IsIdentity = true, Type = DbType.Int64 }
                });
                Processor.ColumnExists(String.Empty, table.Name, "id").ShouldBeTrue();
                Processor.SequenceExists(String.Empty, String.Format("gen_{0}_id", table.Name)).ShouldBeTrue();
                Processor.TriggerExists(String.Empty, table.Name, String.Format("gen_id_{0}_id", table.Name)).ShouldBeTrue();

                Processor.Process(new AlterColumnExpression
                {
                    TableName = table.Name,
                    Column = { Name = "id", IsIdentity = false, Type = DbType.Int64 }
                });
                Processor.ColumnExists(String.Empty, table.Name, "id").ShouldBeTrue();
                Processor.SequenceExists(String.Empty, String.Format("gen_{0}_id", table.Name)).ShouldBeFalse();
                Processor.TriggerExists(String.Empty, table.Name, String.Format("gen_id_{0}_id", table.Name)).ShouldBeFalse();
            }
        }

        [Test]
        public void IdentityCanInsert()
        {
            using (var table = new FirebirdTestTable(Processor, null, "bogus int"))
            {
                Processor.Process(new CreateColumnExpression
                {
                    TableName = table.Name,
                    Column = { Name = "id", IsIdentity = true, Type = DbType.Int64, IsPrimaryKey = true }
                });

                var insert = new InsertDataExpression { TableName = table.Name };
                var item = new Model.InsertionDataDefinition();
                item.Add(new System.Collections.Generic.KeyValuePair<string, object>("BOGUS", 0));
                insert.Rows.Add(item);
                Processor.Process(insert);

                using (DataSet ds = Processor.ReadTableData(String.Empty, table.Name))
                {
                    ds.Tables.Count.ShouldBe(1);
                    ds.Tables[0].Rows.Count.ShouldBe(1);
                    ds.Tables[0].Rows[0]["BOGUS"].ShouldBe(0);
                    ds.Tables[0].Rows[0]["id"].ShouldBe(1);
                }
                
            }
        }

        [Test]
        public void IdentityCanInsertMultiple()
        {
            using (var table = new FirebirdTestTable(Processor, null, "bogus int"))
            {
                Processor.Process(new CreateColumnExpression
                {
                    TableName = table.Name,
                    Column = { Name = "id", IsIdentity = true, Type = DbType.Int64, IsPrimaryKey = true }
                });

                var insert = new InsertDataExpression { TableName = table.Name };
                var item = new Model.InsertionDataDefinition();
                item.Add(new System.Collections.Generic.KeyValuePair<string, object>("BOGUS", 0));
                insert.Rows.Add(item);

                //Process 5 times = insert 5 times
                Processor.Process(insert);
                Processor.Process(insert);
                Processor.Process(insert);
                Processor.Process(insert);
                Processor.Process(insert);

                using (DataSet ds = Processor.ReadTableData(String.Empty, table.Name))
                {
                    ds.Tables.Count.ShouldBe(1);
                    ds.Tables[0].Rows.Count.ShouldBe(5);
                    ds.Tables[0].Rows[0]["BOGUS"].ShouldBe(0);
                    ds.Tables[0].Rows[0]["id"].ShouldBe(1);
                    ds.Tables[0].Rows[1]["id"].ShouldBe(2);
                    ds.Tables[0].Rows[2]["id"].ShouldBe(3);
                    ds.Tables[0].Rows[3]["id"].ShouldBe(4);
                    ds.Tables[0].Rows[4]["id"].ShouldBe(5);
                }

            }
        }


    }
}