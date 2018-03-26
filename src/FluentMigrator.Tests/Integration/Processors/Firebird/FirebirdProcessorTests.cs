using System;
using System.Data;
using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators.Firebird;
using FluentMigrator.Runner.Processors.Firebird;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors.Firebird
{
    [TestFixture]
    [Category("Integration")]
    [Category("Firebird")]
    public class FirebirdProcessorTests : IntegrationTestBase
    {
        [Test]
        public void CanReadData()
        {
            ExecuteFor(FIREBIRD, processor =>
            {
                var fbProcessor = processor as FirebirdProcessor;
                var quoter = new FirebirdQuoter();
                using (var table = new FirebirdTestTable(fbProcessor, null, "id int"))
                {
                    fbProcessor.CheckTable(table.Name);
                    AddTestData(table);
                    fbProcessor.AutoCommit();

                    using (DataSet ds = fbProcessor.Read("SELECT * FROM {0}", quoter.QuoteTableName(table.Name)))
                    {
                        ds.ShouldNotBeNull();
                        ds.Tables.Count.ShouldBe(1);
                        ds.Tables[0].Rows.Count.ShouldBe(3);
                        ds.Tables[0].Rows[2][0].ShouldBe(2);
                    }
                }
            });
        }

        [Test]
        public void CanReadTableData()
        {
            ExecuteFor(FIREBIRD, processor =>
            {
                var fbProcessor = processor as FirebirdProcessor;
                using (var table = new FirebirdTestTable(fbProcessor, null, "id int"))
                {
                    fbProcessor.CheckTable(table.Name);
                    AddTestData(table);
                    fbProcessor.AutoCommit();

                    using (DataSet ds = fbProcessor.ReadTableData(null, table.Name))
                    {
                        ds.ShouldNotBeNull();
                        ds.Tables.Count.ShouldBe(1);
                        ds.Tables[0].Rows.Count.ShouldBe(3);
                        ds.Tables[0].Rows[2][0].ShouldBe(2);
                    }
                }
            });
        }

        [Test]
        public void CanReadDataWithSchema()
        {
            ExecuteFor(FIREBIRD, processor =>
            {
                var fbProcessor = processor as FirebirdProcessor;
                var quoter = new FirebirdQuoter();
                using (var table = new FirebirdTestTable(fbProcessor, "TestSchema", "id int"))
                {
                    fbProcessor.CheckTable(table.Name);
                    AddTestData(table);
                    fbProcessor.AutoCommit();

                    using (DataSet ds = fbProcessor.Read("SELECT * FROM {0}", quoter.QuoteTableName(table.Name)))
                    {
                        ds.ShouldNotBeNull();
                        ds.Tables.Count.ShouldBe(1);
                        ds.Tables[0].Rows.Count.ShouldBe(3);
                        ds.Tables[0].Rows[2][0].ShouldBe(2);
                    }
                }
            });
        }

        [Test]
        public void CanReadTableDataWithSchema()
        {
            ExecuteFor(FIREBIRD, processor =>
            {
                var fbProcessor = processor as FirebirdProcessor;
                using (var table = new FirebirdTestTable(fbProcessor, "TestSchema", "id int"))
                {
                    fbProcessor.CheckTable(table.Name);
                    AddTestData(table);
                    fbProcessor.AutoCommit();

                    using (var ds = fbProcessor.ReadTableData("TestSchema", table.Name))
                    {
                        ds.ShouldNotBeNull();
                        ds.Tables.Count.ShouldBe(1);
                        ds.Tables[0].Rows.Count.ShouldBe(3);
                        ds.Tables[0].Rows[2][0].ShouldBe(2);
                    }
                }
            });
        }

        [Test]
        public void CanCreateAndDropSequenceWithExistCheck()
        {
            ExecuteFor(FIREBIRD, processor =>
            {
                var fbProcessor = processor as FirebirdProcessor;
                processor.SequenceExists("", "Sequence").ShouldBeFalse();
                using (new FirebirdTestTable(fbProcessor, null, "id int"))
                {
                    processor.Process(new CreateSequenceExpression
                    {
                        Sequence = { Name = "Sequence" }
                    });

                    processor.SequenceExists("", "\"Sequence\"").ShouldBeTrue();
                    processor.SequenceExists("", "Sequence").ShouldBeTrue();

                    processor.Process(new DeleteSequenceExpression { SequenceName = "Sequence" });

                    processor.SequenceExists("", "\"Sequence\"").ShouldBeFalse();
                    processor.SequenceExists("", "Sequence").ShouldBeFalse();
                }
            });
        }

        [Test]
        public void CanAlterSequence()
        {
            ExecuteFor(FIREBIRD, processor =>
            {
                var fbProcessor = processor as FirebirdProcessor;
                using (new FirebirdTestTable(fbProcessor, null, "id int"))
                {
                    processor.Process(new CreateSequenceExpression
                    {
                        Sequence = { Name = "Sequence", StartWith = 6 }
                    });

                    using (DataSet ds = fbProcessor.Read("SELECT GEN_ID(\"Sequence\", 1) as generated_value FROM RDB$DATABASE"))
                    {
                        ds.Tables[0].ShouldNotBeNull();
                        ds.Tables[0].Rows[0].ShouldNotBeNull();
                        ds.Tables[0].Rows[0]["generated_value"].ShouldBe(7);
                    }

                    processor.Process(new DeleteSequenceExpression { SequenceName = "Sequence" });

                    processor.SequenceExists(String.Empty, "\"Sequence\"").ShouldBeFalse();
                    processor.SequenceExists("", "Sequence").ShouldBeFalse();
                }
            });
        }

        [Test]
        public void CallingSequenceExistsReturnsFalseIfSequenceDoesNotExist()
        {
            ExecuteFor(FIREBIRD, processor =>
            {
                processor.SequenceExists("", "DoesNotExist").ShouldBeFalse();
            });
        }

        [Test]
        public void CanCreateTrigger()
        {
            ExecuteFor(FIREBIRD, processor =>
            {
                var fbProcessor = processor as FirebirdProcessor;
                using (var table = new FirebirdTestTable(fbProcessor, null, "id int"))
                {
                    processor.Process(fbProcessor.CreateTriggerExpression(table.Name, "TestTrigger", true, TriggerEvent.Insert, "as begin end"));
                    fbProcessor.TriggerExists(String.Empty, table.Name, "TestTrigger").ShouldBeTrue();
                }
            });
        }

        [Test]
        public void CanDropTrigger()
        {
            ExecuteFor(FIREBIRD, processor =>
            {
                var fbProcessor = processor as FirebirdProcessor;
                using (var table = new FirebirdTestTable(fbProcessor, null, "id int"))
                {
                    processor.Process(fbProcessor.CreateTriggerExpression(table.Name, "TestTrigger", true, TriggerEvent.Insert, "as begin end"));
                    fbProcessor.TriggerExists(String.Empty, table.Name, "TestTrigger").ShouldBeTrue();

                    processor.Process(fbProcessor.DeleteTriggerExpression(table.Name, "TestTrigger"));
                    fbProcessor.TriggerExists(String.Empty, table.Name, "TestTrigger").ShouldBeFalse();
                }
            });
        }

        [Test]
        public void IdentityCanCreateIdentityColumn()
        {
            ExecuteFor(FIREBIRD, processor =>
            {
                var fbProcessor = processor as FirebirdProcessor;
                using (var table = new FirebirdTestTable(fbProcessor, null, "bogus int"))
                {
                    processor.Process(new CreateColumnExpression
                    {
                        TableName = table.Name,
                        Column = { Name = "id", IsIdentity = true, Type = DbType.Int64 }
                    });
                    fbProcessor.ColumnExists(String.Empty, table.Name, "id").ShouldBeTrue();
                    processor.SequenceExists(String.Empty, String.Format("gen_{0}_id", table.Name)).ShouldBeTrue();
                    fbProcessor.TriggerExists(String.Empty, table.Name, String.Format("gen_id_{0}_id", table.Name)).ShouldBeTrue();
                }
            });
        }

        [Test]
        public void IdentityCanDropIdentityColumn()
        {
            ExecuteFor(FIREBIRD, processor =>
            {
                var fbProcessor = processor as FirebirdProcessor;
                using (var table = new FirebirdTestTable(fbProcessor, null, "bogus int"))
                {
                    processor.Process(new CreateColumnExpression
                    {
                        TableName = table.Name,
                        Column = { Name = "id", IsIdentity = true, Type = DbType.Int64 }
                    });
                    processor.ColumnExists(String.Empty, table.Name, "id").ShouldBeTrue();
                    processor.SequenceExists(String.Empty, String.Format("gen_{0}_id", table.Name)).ShouldBeTrue();
                    fbProcessor.TriggerExists(String.Empty, table.Name, String.Format("gen_id_{0}_id", table.Name)).ShouldBeTrue();

                    processor.Process(new DeleteColumnExpression
                    {
                        TableName = table.Name,
                        ColumnNames = { "id" }
                    });
                    processor.ColumnExists(String.Empty, table.Name, "id").ShouldBeFalse();
                    processor.SequenceExists(String.Empty, String.Format("gen_{0}_id", table.Name)).ShouldBeFalse();
                    fbProcessor.TriggerExists(String.Empty, table.Name, String.Format("gen_id_{0}_id", table.Name)).ShouldBeFalse();
                }
            });
        }

        [Test]
        public void IdentityCanAlterColumnToIdentity()
        {
            ExecuteFor(FIREBIRD, processor =>
            {
                var fbProcessor = processor as FirebirdProcessor;
                using (var table = new FirebirdTestTable(fbProcessor, null, "bogus int"))
                {
                    processor.Process(new CreateColumnExpression
                    {
                        TableName = table.Name,
                        Column = { Name = "id", IsIdentity = false, Type = DbType.Int64 }
                    });
                    processor.ColumnExists(String.Empty, table.Name, "id").ShouldBeTrue();
                    processor.SequenceExists(String.Empty, String.Format("gen_{0}_id", table.Name)).ShouldBeFalse();
                    fbProcessor.TriggerExists(String.Empty, table.Name, String.Format("gen_id_{0}_id", table.Name)).ShouldBeFalse();

                    processor.Process(new AlterColumnExpression
                    {
                        TableName = table.Name,
                        Column = { Name = "id", IsIdentity = true, Type = DbType.Int64 }
                    });
                    processor.ColumnExists(String.Empty, table.Name, "id").ShouldBeTrue();
                    processor.SequenceExists(String.Empty, String.Format("gen_{0}_id", table.Name)).ShouldBeTrue();
                    fbProcessor.TriggerExists(String.Empty, table.Name, String.Format("gen_id_{0}_id", table.Name)).ShouldBeTrue();
                }
            });
        }

        [Test]
        public void IdentityCanAlterColumnToNotIdentity()
        {
            ExecuteFor(FIREBIRD, processor =>
            {
                var fbProcessor = processor as FirebirdProcessor;
                using (var table = new FirebirdTestTable(fbProcessor, null, "bogus int"))
                {
                    processor.Process(new CreateColumnExpression
                    {
                        TableName = table.Name,
                        Column = { Name = "id", IsIdentity = true, Type = DbType.Int64 }
                    });
                    processor.ColumnExists(String.Empty, table.Name, "id").ShouldBeTrue();
                    processor.SequenceExists(String.Empty, String.Format("gen_{0}_id", table.Name)).ShouldBeTrue();
                    fbProcessor.TriggerExists(String.Empty, table.Name, String.Format("gen_id_{0}_id", table.Name)).ShouldBeTrue();

                    processor.Process(new AlterColumnExpression
                    {
                        TableName = table.Name,
                        Column = { Name = "id", IsIdentity = false, Type = DbType.Int64 }
                    });
                    processor.ColumnExists(String.Empty, table.Name, "id").ShouldBeTrue();
                    processor.SequenceExists(String.Empty, String.Format("gen_{0}_id", table.Name)).ShouldBeFalse();
                    fbProcessor.TriggerExists(String.Empty, table.Name, String.Format("gen_id_{0}_id", table.Name)).ShouldBeFalse();
                }
            });
        }

        [Test]
        public void IdentityCanInsert()
        {
            ExecuteFor(FIREBIRD, processor =>
            {
                var fbProcessor = processor as FirebirdProcessor;
                using (var table = new FirebirdTestTable(fbProcessor, null, "bogus int"))
                {
                    processor.Process(new CreateColumnExpression
                    {
                        TableName = table.Name,
                        Column = { Name = "id", IsIdentity = true, Type = DbType.Int64, IsPrimaryKey = true }
                    });

                    var insert = new InsertDataExpression { TableName = table.Name };
                    var item = new Model.InsertionDataDefinition();
                    item.Add(new System.Collections.Generic.KeyValuePair<string, object>("BOGUS", 0));
                    insert.Rows.Add(item);
                    processor.Process(insert);

                    using (DataSet ds = processor.ReadTableData(String.Empty, table.Name))
                    {
                        ds.Tables.Count.ShouldBe(1);
                        ds.Tables[0].Rows.Count.ShouldBe(1);
                        ds.Tables[0].Rows[0]["BOGUS"].ShouldBe(0);
                        ds.Tables[0].Rows[0]["id"].ShouldBe(1);
                    }
                }
            });
        }

        [Test]
        public void IdentityCanInsertMultiple()
        {
            ExecuteFor(FIREBIRD, processor =>
            {
                var fbProcessor = processor as FirebirdProcessor;
                using (var table = new FirebirdTestTable(fbProcessor, null, "bogus int"))
                {
                    processor.Process(new CreateColumnExpression
                    {
                        TableName = table.Name,
                        Column = { Name = "id", IsIdentity = true, Type = DbType.Int64, IsPrimaryKey = true }
                    });

                    var insert = new InsertDataExpression { TableName = table.Name };
                    var item = new Model.InsertionDataDefinition();
                    item.Add(new System.Collections.Generic.KeyValuePair<string, object>("BOGUS", 0));
                    insert.Rows.Add(item);

                    //Process 5 times = insert 5 times
                    processor.Process(insert);
                    processor.Process(insert);
                    processor.Process(insert);
                    processor.Process(insert);
                    processor.Process(insert);

                    using (DataSet ds = processor.ReadTableData(String.Empty, table.Name))
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
            });
        }

        private void AddTestData(FirebirdTestTable table)
        {
            var quoter = new FirebirdQuoter();
            for (int i = 0; i < 3; i++)
            {
                using (var cmd = table.Connection.CreateCommand())
                {
                    cmd.Transaction = table.Transaction;
                    cmd.CommandText = string.Format("INSERT INTO {0} (id) VALUES ({1})", quoter.QuoteTableName(table.Name), i);
                    cmd.ExecuteNonQuery();
                }
            }
        }

    }
}