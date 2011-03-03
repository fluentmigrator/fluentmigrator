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
using FluentMigrator.Runner.Generators.Postgres;
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
			using (var table = new PostgresTestTable(Processor, null, "id int"))
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
			using (var table = new PostgresTestTable(Processor, null, "id int"))
				Processor.ColumnExists(null, table.Name, "id").ShouldBeTrue();
		}

		[Test]
		public void CallingColumnExistsReturnsFalseIfTableDoesNotExist()
		{
			Processor.ColumnExists(null, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
		}

		[Test]
		public void CallingColumnExistsReturnsFalseIfColumnDoesNotExist()
		{
			using (var table = new PostgresTestTable(Processor, null, "id int"))
				Processor.ColumnExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
		}

		[Test]
		public void CallingContraintExistsReturnsTrueIfConstraintExists()
		{
			using (var table = new PostgresTestTable(Processor, null, "id int", "wibble int CONSTRAINT c1 CHECK(wibble > 0)"))
				Processor.ConstraintExists(null, table.Name,"c1").ShouldBeTrue();
		}

		[Test]
		public void CallingConstraintExistsReturnsFalseIfTableDoesNotExist()
		{
			Processor.ConstraintExists(null, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
		}

		[Test]
		public void CallingConstraintExistsReturnsFalseIfConstraintDoesNotExist()
		{
			using (var table = new PostgresTestTable(Processor, null, "id int"))
				Processor.ConstraintExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
		}

		[Test]
		public void CallingIndexExistsReturnsTrueIfIndexExists()
		{
			var quoter = new PostgresQuoter();

			using (var table = new PostgresTestTable(Processor, null, "id int"))
			{
				var idxName = string.Format("\"idx_{0}\"", quoter.UnQuote(table.Name));

				var cmd = table.Connection.CreateCommand();
				cmd.Transaction = table.Transaction;
				cmd.CommandText = string.Format("CREATE INDEX {0} ON {1} (id)", idxName, table.Name);
				cmd.ExecuteNonQuery();

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
			using (var table = new PostgresTestTable(Processor, null, "id int"))
				Processor.IndexExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
		}

		[Test]
		public void CanReadData()
		{
			using (var table = new PostgresTestTable(Processor, null, "id int"))
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
			using (var table = new PostgresTestTable(Processor, null, "id int"))
			{
				AddTestData(table);

				DataSet ds = Processor.ReadTableData(null, table.Name);

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
				cmd.CommandText = string.Format("INSERT INTO {0} (id) VALUES ({1})", table.NameWithSchema, i);
				cmd.ExecuteNonQuery();
			}
		}

		[Test]
		public void CallingTableExistsReturnsTrueIfTableExistsWithSchema()
		{
			using (var table = new PostgresTestTable(Processor, "TestSchema", "id int"))
				Processor.TableExists("TestSchema", table.Name).ShouldBeTrue();
		}

		[Test]
		public void CallingTableExistsReturnsFalseIfTableDoesNotExistWithSchema()
		{
			Processor.TableExists("TestSchema", "DoesNotExist").ShouldBeFalse();
		}

		[Test]
		public void CallingTableExistsReturnsFalseIfTableExistsInDifferentSchema()
		{
			using (var table = new PostgresTestTable(Processor, "TestSchema1", "id int"))
				Processor.TableExists("TestSchema2", table.Name).ShouldBeFalse();
		}

		[Test]
		public void CallingColumnExistsReturnsTrueIfColumnExistsWithSchema()
		{
			using (var table = new PostgresTestTable(Processor, "TestSchema", "id int"))
				Processor.ColumnExists("TestSchema", table.Name, "id").ShouldBeTrue();
		}

		[Test]
		public void CallingColumnExistsReturnsFalseIfColumnExistsInDifferentSchema()
		{
			using (var table = new PostgresTestTable(Processor, "TestSchema1", "id int"))
				Processor.ColumnExists("TestSchema2", table.Name, "id").ShouldBeFalse();
		}

		[Test]
		public void CallingColumnExistsReturnsFalseIfTableDoesNotExistWithSchema()
		{
			Processor.ColumnExists("TestSchema", "DoesNotExist", "DoesNotExist").ShouldBeFalse();
		}

		[Test]
		public void CallingColumnExistsReturnsFalseIfColumnDoesNotExistWithSchema()
		{
			using (var table = new PostgresTestTable(Processor, "TestSchema", "id int"))
				Processor.ColumnExists("TestSchema", table.Name, "DoesNotExist").ShouldBeFalse();
		}

		[Test]
		public void CallingContraintExistsReturnsTrueIfConstraintExistsWithSchema()
		{
			using (var table = new PostgresTestTable(Processor, "TestSchema", "id int", "wibble int CONSTRAINT c1 CHECK(wibble > 0)"))
				Processor.ConstraintExists("TestSchema", table.Name, "c1").ShouldBeTrue();
		}

		[Test]
		public void CallingConstraintExistsReturnsFalseIfTableDoesNotExistWithSchema()
		{
			Processor.ConstraintExists("TestSchema", "DoesNotExist", "DoesNotExist").ShouldBeFalse();
		}

		[Test]
		public void CallingContraintExistsReturnsFalseIfConstraintExistsInDifferentSchema()
		{
			using (var table = new PostgresTestTable(Processor, "TestSchema1", "id int", "wibble int CONSTRAINT c1 CHECK(wibble > 0)"))
				Processor.ConstraintExists("TestSchema2", table.Name, "c1").ShouldBeFalse();
		}

		[Test]
		public void CallingConstraintExistsReturnsFalseIfConstraintDoesNotExistWithSchema()
		{
			using (var table = new PostgresTestTable(Processor, "TestSchema", "id int"))
				Processor.ConstraintExists("TestSchema", table.Name, "DoesNotExist").ShouldBeFalse();
		}

		[Test]
		public void CallingIndexExistsReturnsTrueIfIndexExistsWithSchema()
		{
			var quoter = new PostgresQuoter();

			using (var table = new PostgresTestTable(Processor, "TestSchema", "id int"))
			{
				var idxName = string.Format("\"idx_{0}\"", quoter.UnQuote(table.Name));

				var cmd = table.Connection.CreateCommand();
				cmd.Transaction = table.Transaction;
				cmd.CommandText = string.Format("CREATE INDEX {0} ON {1} (id)", idxName,table.NameWithSchema);
				cmd.ExecuteNonQuery();

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
			using (var table = new PostgresTestTable(Processor, "TestSchema", "id int"))
				Processor.IndexExists("TestSchema", table.Name, "DoesNotExist").ShouldBeFalse();
		}

		[Test]
		public void CanReadDataWithSchema()
		{
			using (var table = new PostgresTestTable(Processor, "TestSchema", "id int"))
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
			using (var table = new PostgresTestTable(Processor, "TestSchema", "id int"))
			{
				AddTestData(table);

				DataSet ds = Processor.ReadTableData("TestSchema", table.Name);

				ds.ShouldNotBeNull();
				ds.Tables.Count.ShouldBe(1);
				ds.Tables[0].Rows.Count.ShouldBe(3);
				ds.Tables[0].Rows[2][0].ShouldBe(2);
			}
		}
	}
}