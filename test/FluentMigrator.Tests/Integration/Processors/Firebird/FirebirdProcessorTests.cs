#region License
//
// Copyright (c) 2018, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System.Data;

using FluentMigrator.Expressions;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators.Firebird;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors.Firebird;
using FluentMigrator.Tests.Helpers;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Integration.Processors.Firebird
{
    [TestFixture]
    [Category("Integration")]
    [Category("Firebird")]
    public class FirebirdProcessorTests
    {
        private readonly FirebirdLibraryProber _prober = new FirebirdLibraryProber();
        private TemporaryDatabase _temporaryDatabase;

        private ServiceProvider ServiceProvider { get; set; }
        private IServiceScope ServiceScope { get; set; }
        private FirebirdProcessor Processor { get; set; }
        private FirebirdQuoter Quoter {get; set; }

        [Test]
        public void CanReadData()
        {
            using (var table = new FirebirdTestTable(Processor, "id int"))
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
            using (var table = new FirebirdTestTable(Processor, "id int"))
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
            if (table.Connection.State != ConnectionState.Open)
                table.Connection.Open();

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
            using (var table = new FirebirdTestTable(Processor, "id int"))
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
            using (var table = new FirebirdTestTable(Processor, "id int"))
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
            using (new FirebirdTestTable(Processor, "id int"))
            {
                Processor.Process(new CreateSequenceExpression
                {
                    Sequence = { Name = "Sequence" }
                });

                Processor.SequenceExists("", "\"Sequence\"").ShouldBeTrue();
                Processor.SequenceExists("", "Sequence").ShouldBeTrue();

                Processor.Process(new DeleteSequenceExpression { SequenceName = "Sequence" });

                Processor.SequenceExists("", "\"Sequence\"").ShouldBeFalse();
                Processor.SequenceExists("", "Sequence").ShouldBeFalse();
            }
        }

        [Test]
        public void CanAlterSequence()
        {
            using (new FirebirdTestTable(Processor, "id int"))
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

                Processor.SequenceExists(string.Empty, "\"Sequence\"").ShouldBeFalse();
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
            using (var table = new FirebirdTestTable(Processor, "id int"))
            {
                Processor.Process(Processor.CreateTriggerExpression(table.Name, "TestTrigger", true, TriggerEvent.Insert, "as begin end"));
                Processor.TriggerExists(string.Empty, table.Name, "TestTrigger").ShouldBeTrue();
            }
        }

        [Test]
        public void CanDropTrigger()
        {
            using (var table = new FirebirdTestTable(Processor, "id int"))
            {
                Processor.Process(Processor.CreateTriggerExpression(table.Name, "TestTrigger", true, TriggerEvent.Insert, "as begin end"));
                Processor.TriggerExists(string.Empty, table.Name, "TestTrigger").ShouldBeTrue();

                Processor.Process(Processor.DeleteTriggerExpression(table.Name, "TestTrigger"));
                Processor.TriggerExists(string.Empty, table.Name, "TestTrigger").ShouldBeFalse();
            }
        }

        [Test]
        public void IdentityCanCreateIdentityColumn()
        {
            using (var table = new FirebirdTestTable(Processor, "bogus int"))
            {
                Processor.Process(new CreateColumnExpression
                {
                    TableName = table.Name,
                    Column = { Name = "id", IsIdentity = true, Type = DbType.Int64 }
                });
                Processor.ColumnExists(string.Empty, table.Name, "id").ShouldBeTrue();
                Processor.SequenceExists(string.Empty, string.Format("gen_{0}_id", table.Name)).ShouldBeTrue();
                Processor.TriggerExists(string.Empty, table.Name, string.Format("gen_id_{0}_id", table.Name)).ShouldBeTrue();
            }
        }

        [Test]
        public void IdentityCanDropIdentityColumn()
        {
            using (var table = new FirebirdTestTable(Processor, "bogus int"))
            {
                Processor.Process(new CreateColumnExpression
                {
                    TableName = table.Name,
                    Column = { Name = "id", IsIdentity = true, Type = DbType.Int64 }
                });
                Processor.ColumnExists(string.Empty, table.Name, "id").ShouldBeTrue();
                Processor.SequenceExists(string.Empty, string.Format("gen_{0}_id", table.Name)).ShouldBeTrue();
                Processor.TriggerExists(string.Empty, table.Name, string.Format("gen_id_{0}_id", table.Name)).ShouldBeTrue();

                Processor.Process(new DeleteColumnExpression
                {
                    TableName = table.Name,
                    ColumnNames = { "id" }
                });
                Processor.ColumnExists(string.Empty, table.Name, "id").ShouldBeFalse();
                Processor.SequenceExists(string.Empty, string.Format("gen_{0}_id", table.Name)).ShouldBeFalse();
                Processor.TriggerExists(string.Empty, table.Name, string.Format("gen_id_{0}_id", table.Name)).ShouldBeFalse();
            }
        }

        [Test]
        public void IdentityCanAlterColumnToIdentity()
        {
            using (var table = new FirebirdTestTable(Processor, "bogus int"))
            {
                Processor.Process(new CreateColumnExpression
                {
                    TableName = table.Name,
                    Column = { Name = "id", IsIdentity = false, Type = DbType.Int64 }
                });
                Processor.ColumnExists(string.Empty, table.Name, "id").ShouldBeTrue();
                Processor.SequenceExists(string.Empty, string.Format("gen_{0}_id", table.Name)).ShouldBeFalse();
                Processor.TriggerExists(string.Empty, table.Name, string.Format("gen_id_{0}_id", table.Name)).ShouldBeFalse();

                Processor.Process(new AlterColumnExpression
                {
                    TableName = table.Name,
                    Column = { Name = "id", IsIdentity = true, Type = DbType.Int64 }
                });
                Processor.ColumnExists(string.Empty, table.Name, "id").ShouldBeTrue();
                Processor.SequenceExists(string.Empty, string.Format("gen_{0}_id", table.Name)).ShouldBeTrue();
                Processor.TriggerExists(string.Empty, table.Name, string.Format("gen_id_{0}_id", table.Name)).ShouldBeTrue();
            }
        }

        [Test]
        public void IdentityCanAlterColumnToNotIdentity()
        {
            using (var table = new FirebirdTestTable(Processor, "bogus int"))
            {
                Processor.Process(new CreateColumnExpression
                {
                    TableName = table.Name,
                    Column = { Name = "id", IsIdentity = true, Type = DbType.Int64 }
                });
                Processor.ColumnExists(string.Empty, table.Name, "id").ShouldBeTrue();
                Processor.SequenceExists(string.Empty, string.Format("gen_{0}_id", table.Name)).ShouldBeTrue();
                Processor.TriggerExists(string.Empty, table.Name, string.Format("gen_id_{0}_id", table.Name)).ShouldBeTrue();

                Processor.Process(new AlterColumnExpression
                {
                    TableName = table.Name,
                    Column = { Name = "id", IsIdentity = false, Type = DbType.Int64 }
                });
                Processor.ColumnExists(string.Empty, table.Name, "id").ShouldBeTrue();
                Processor.SequenceExists(string.Empty, string.Format("gen_{0}_id", table.Name)).ShouldBeFalse();
                Processor.TriggerExists(string.Empty, table.Name, string.Format("gen_id_{0}_id", table.Name)).ShouldBeFalse();
            }
        }

        [Test]
        public void IdentityCanInsert()
        {
            using (var table = new FirebirdTestTable(Processor, "bogus int"))
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

                using (DataSet ds = Processor.ReadTableData(string.Empty, table.Name))
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
            using (var table = new FirebirdTestTable(Processor, "bogus int"))
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

                using (DataSet ds = Processor.ReadTableData(string.Empty, table.Name))
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

        [SetUp]
        public void SetUp()
        {
            if (!IntegrationTestOptions.Firebird.IsEnabled)
                Assert.Ignore();

            _temporaryDatabase = new TemporaryDatabase(
                IntegrationTestOptions.Firebird,
                _prober);

            var serivces = ServiceCollectionExtensions.CreateServices()
                .ConfigureRunner(builder => builder.AddFirebird())
                .AddScoped<IConnectionStringReader>(
                    _ => new PassThroughConnectionStringReader(_temporaryDatabase.ConnectionString));

            ServiceProvider = serivces.BuildServiceProvider();
            ServiceScope = ServiceProvider.CreateScope();
            Processor = ServiceScope.ServiceProvider.GetRequiredService<FirebirdProcessor>();
            Quoter = ServiceScope.ServiceProvider.GetRequiredService<FirebirdQuoter>();
        }

        [TearDown]
        public void TearDown()
        {
            ServiceScope?.Dispose();
            ServiceProvider?.Dispose();
            Processor?.Dispose();
            if (_temporaryDatabase != null)
            {
                var connString = _temporaryDatabase.ConnectionString;
                _temporaryDatabase = null;
                FbDatabase.DropDatabase(connString);
            }
            _temporaryDatabase?.Dispose();
        }
    }
}
