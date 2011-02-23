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

using System;
using System.Data;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Postgres;
using FluentMigrator.Tests.Helpers;
using Npgsql;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors
{
	[TestFixture]
	public class PostgresProcessorTests
	{
		public NpgsqlConnection Connection { get; set; }
		public PostgresProcessor Processor { get; set; }

		[SetUp]
		public void SetUp()
		{
            Connection = new NpgsqlConnection(IntegrationTestOptions.Postgres.ConnectionString);
            Processor = new PostgresProcessor(Connection, new PostgresGenerator(), new TextWriterAnnouncer(System.Console.Out), new ProcessorOptions());
		}

		[TearDown]
		public void TearDown()
		{
			Processor.CommitTransaction();
		}

        [Test]
        public void CallingSchemaExistsReturnsTrueIfSchemaExists()
        {
            Processor.SchemaExists("public").ShouldBeTrue();
        }

        [Test]
        public void CallingSchemaExistsReturnsFalseIfSchemaDoesNotExist()
        {
            Processor.SchemaExists("DoesNotExist").ShouldBeFalse();
        }

		[Test]
		public void CallingTableExistsReturnsTrueIfTableExists()
		{
            using (var table = new PostgresTestTable(Processor, "id int"))
                Processor.TableExists(table.Name).ShouldBeTrue();
		}

		[Test]
		public void CallingTableExistsReturnsFalseIfTableDoesNotExist()
		{
			Processor.TableExists("DoesNotExist").ShouldBeFalse();
		}

		[Test]
		public void CallingColumnExistsReturnsTrueIfColumnExists()
		{
            using (var table = new PostgresTestTable(Processor, "id int"))
				Processor.ColumnExists(table.Name, "id").ShouldBeTrue();
		}

		[Test]
		public void CallingColumnExistsReturnsFalseIfTableDoesNotExist()
		{
			Processor.ColumnExists("DoesNotExist", "DoesNotExist").ShouldBeFalse();
		}

		[Test]
		public void CallingColumnExistsReturnsFalseIfColumnDoesNotExist()
		{
            using (var table = new PostgresTestTable(Processor, "id int"))
				Processor.ColumnExists(table.Name, "DoesNotExist").ShouldBeFalse();
		}

        [Test]
        public void CallingContraintExistsReturnsTrueIfConstraintExists()
        {
            using (var table = new PostgresTestTable(Processor, "id int", "wibble int CONSTRAINT c1 CHECK(wibble > 0)"))
                Processor.ConstraintExists(table.Name,"c1").ShouldBeTrue();
        }

        [Test]
        public void CallingConstraintExistsReturnsFalseIfTableDoesNotExist()
        {
            Processor.ConstraintExists("DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public void CallingConstraintExistsReturnsFalseIfConstraintDoesNotExist()
        {
            using (var table = new PostgresTestTable(Processor, "id int"))
                Processor.ConstraintExists(table.Name, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public void CallingIndexExistsReturnsTrueIfIndexExists()
        {
            using (var table = new PostgresTestTable(Processor, "id int"))
            {
                var cmd = table.Connection.CreateCommand();
                cmd.Transaction = table.Transaction;
                cmd.CommandText = string.Format("CREATE INDEX \"idx_{0}\" ON \"{0}\" (id)", table.Name);
                cmd.ExecuteNonQuery();

                Processor.IndexExists(table.Name, string.Format("idx_{0}", table.Name)).ShouldBeTrue();
            }
        }

        [Test]
        public void CallingIndexExistsReturnsFalseIfTableDoesNotExist()
        {
            Processor.IndexExists("DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public void CallingIndexExistsReturnsFalseIfIndexDoesNotExist()
        {
            using (var table = new PostgresTestTable(Processor, "id int"))
                Processor.IndexExists(table.Name, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public void CanReadData()
        {
            using (var table = new PostgresTestTable(Processor, "id int"))
            {
                AddTestData(table);

                DataSet ds = Processor.Read("SELECT * FROM \"{0}\"", table.Name);

                ds.ShouldNotBeNull();
                ds.Tables.Count.ShouldBe(1);
                ds.Tables[0].Rows.Count.ShouldBe(3);
                ds.Tables[0].Rows[2][0].ShouldBe(2);
            }
        }

        [Test]
        public void CanReadTableData()
        {
            using (var table = new PostgresTestTable(Processor, "id int"))
            {
                AddTestData(table);

                DataSet ds = Processor.ReadTableData(table.Name);

                ds.ShouldNotBeNull();
                ds.Tables.Count.ShouldBe(1);
                ds.Tables[0].Rows.Count.ShouldBe(3);
                ds.Tables[0].Rows[2][0].ShouldBe(2);
            }
        }

	    private void AddTestData(PostgresTestTable table)
	    {
	        for (int i = 0; i < 3; i++)
	        {
                var cmd = table.Connection.CreateCommand();
                cmd.Transaction = table.Transaction;
                cmd.CommandText = string.Format("INSERT INTO \"{0}\" (id) VALUES ({1})", table.Name, i);
                cmd.ExecuteNonQuery();
	        }
	    }
	}
}