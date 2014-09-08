#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
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
using System.IO;
using FluentMigrator.Builders.Execute;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators.Postgres;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.DotConnectPostgres;
using FluentMigrator.Tests.Helpers;
using NUnit.Framework;
using NUnit.Should;
using Devart.Data.PostgreSql;

namespace FluentMigrator.Tests.Integration.Processors.DotConnectPostgres
{
    [TestFixture]
    [Category("Integration")]
    public class DotConnectPostgresProcessorTests
    {
        public PgSqlConnection Connection { get; set; }
        public DotConnectPostgresProcessor Processor { get; set; }

        [SetUp]
        public void SetUp()
        {
            Connection = new PgSqlConnection(IntegrationTestOptions.DotConnectPostgres.ConnectionString);
            Processor = new DotConnectPostgresProcessor(Connection, new PostgresGenerator(), new TextWriterAnnouncer(System.Console.Out), new ProcessorOptions(), new PostgresDbFactory());
            Connection.Open();
        }

        [TearDown]
        public void TearDown()
        {
            Processor.CommitTransaction();
            Processor.Dispose();
        }

        [Test]
        public void CallingColumnExistsReturnsFalseIfColumnExistsInDifferentSchema()
        {
            using (var table = new DotConnectPostgresTestTable(Processor, "TestSchema1", "id int"))
                Processor.ColumnExists("TestSchema2", table.Name, "id").ShouldBeFalse();
        }

        [Test]
        public void CallingConstraintExistsReturnsFalseIfConstraintExistsInDifferentSchema()
        {
            using (var table = new DotConnectPostgresTestTable(Processor, "TestSchema1", "id int", "wibble int CONSTRAINT c1 CHECK(wibble > 0)"))
                Processor.ConstraintExists("TestSchema2", table.Name, "c1").ShouldBeFalse();
        }

        [Test]
        public void CallingTableExistsReturnsFalseIfTableExistsInDifferentSchema()
        {
            using (var table = new DotConnectPostgresTestTable(Processor, "TestSchema1", "id int"))
                Processor.TableExists("TestSchema2", table.Name).ShouldBeFalse();
        }

        [Test]
        public void CanReadData()
        {
            using (var table = new DotConnectPostgresTestTable(Processor, null, "id int"))
            {
                AddTestData(table);

                DataSet ds = Processor.Read("SELECT * FROM {0}", table.Name);

                ds.ShouldNotBeNull();
                ds.Tables.Count.ShouldBe(1);
                ds.Tables[0].Rows.Count.ShouldBe(3);
                ds.Tables[0].Rows[2][0].ShouldBe(2);
            }
        }

        [Test]
        public void CanReadTableData()
        {
            using (var table = new DotConnectPostgresTestTable(Processor, null, "id int"))
            {
                AddTestData(table);

                DataSet ds = Processor.ReadTableData(null, table.Name);

                ds.ShouldNotBeNull();
                ds.Tables.Count.ShouldBe(1);
                ds.Tables[0].Rows.Count.ShouldBe(3);
                ds.Tables[0].Rows[2][0].ShouldBe(2);
            }
        }

        private void AddTestData(DotConnectPostgresTestTable table)
        {
            for (int i = 0; i < 3; i++)
            {
                var cmd = table.Connection.CreateCommand();
                cmd.Transaction = table.Transaction;
                cmd.CommandText = string.Format("INSERT INTO {0} (id) VALUES ({1})", table.NameWithSchema, i);
                cmd.ExecuteNonQuery();
            }
        }



        [Test]
        public void CanReadDataWithSchema()
        {
            using (var table = new DotConnectPostgresTestTable(Processor, "TestSchema", "id int"))
            {
                AddTestData(table);

                DataSet ds = Processor.Read("SELECT * FROM {0}", table.NameWithSchema);

                ds.ShouldNotBeNull();
                ds.Tables.Count.ShouldBe(1);
                ds.Tables[0].Rows.Count.ShouldBe(3);
                ds.Tables[0].Rows[2][0].ShouldBe(2);
            }
        }

        [Test]
        public void CanReadTableDataWithSchema()
        {
            using (var table = new DotConnectPostgresTestTable(Processor, "TestSchema", "id int"))
            {
                AddTestData(table);

                DataSet ds = Processor.ReadTableData("TestSchema", table.Name);

                ds.ShouldNotBeNull();
                ds.Tables.Count.ShouldBe(1);
                ds.Tables[0].Rows.Count.ShouldBe(3);
                ds.Tables[0].Rows[2][0].ShouldBe(2);
            }
        }

        [Test]
        public void CallingProcessWithPerformDbOperationExpressionWhenInPreviewOnlyModeWillNotMakeDbChanges()
        {
            var output = new StringWriter();

            var connection = new PgSqlConnection(IntegrationTestOptions.DotConnectPostgres.ConnectionString);

            var processor = new DotConnectPostgresProcessor(
                connection,
                new PostgresGenerator(),
                new TextWriterAnnouncer(output),
                new ProcessorOptions { PreviewOnly = true },
                new DotConnectPostgresDbFactory());

            bool tableExists;

            try
            {
                var expression =
                    new PerformDBOperationExpression
                    {
                        Operation = (con, trans) =>
                        {
                            var command = con.CreateCommand();
                            command.CommandText = "CREATE TABLE processtesttable (test int NULL) ";
                            command.Transaction = trans;

                            command.ExecuteNonQuery();
                        }
                    };

                processor.BeginTransaction();
                processor.Process(expression);

                var com = connection.CreateCommand();
                com.CommandText = "";

                tableExists = processor.TableExists("public", "processtesttable");
            }
            finally
            {
                processor.RollbackTransaction();
            }

            tableExists.ShouldBeFalse();
            output.ToString().ShouldBe(
@"/* Beginning Transaction */
/* Performing DB Operation */
/* Rolling back transaction */
");
        }
    }
}