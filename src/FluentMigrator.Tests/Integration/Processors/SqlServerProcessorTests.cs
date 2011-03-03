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

using System.Data.SqlClient;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.SqlServer;
using FluentMigrator.Tests.Helpers;
using NUnit.Framework;
using NUnit.Should;
using FluentMigrator.Runner.Generators.SqlServer;

namespace FluentMigrator.Tests.Integration.Processors
{
	[TestFixture]
	public class SqlServerProcessorTests
	{
		public SqlConnection Connection { get; set; }
		public SqlServerProcessor Processor { get; set; }

		[SetUp]
		public void SetUp()
		{
			Connection = new SqlConnection(IntegrationTestOptions.SqlServer.ConnectionString);
			Processor = new SqlServerProcessor(Connection, new SqlServer2000Generator(), new TextWriterAnnouncer(System.Console.Out), new ProcessorOptions());
		}

		[TearDown]
		public void TearDown()
		{
			Processor.CommitTransaction();
		}

		[Test]
		public void CallingTableExistsReturnsTrueIfTableExists()
		{
			using (var table = new SqlServerTestTable(Processor, null, "id int"))
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
			using (var table = new SqlServerTestTable(Processor, null, "id int"))
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
			using (var table = new SqlServerTestTable(Processor, null, "id int"))
				Processor.ColumnExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
		}

        [Test]
        public void CallingTableExistsReturnsTrueIfTableExistsWithSchema()
        {
            using (var table = new SqlServerTestTable(Processor, "test_schema", "id int"))
                Processor.TableExists("test_schema", table.Name).ShouldBeTrue();
        }

        [Test]
        public void CallingTableExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            Processor.TableExists("test_schema", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public void CallingColumnExistsReturnsTrueIfColumnExistsWithSchema()
        {
            using (var table = new SqlServerTestTable(Processor, "test_schema", "id int"))
                Processor.ColumnExists("test_schema", table.Name, "id").ShouldBeTrue();
        }

        [Test]
        public void CallingColumnExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            Processor.ColumnExists("test_schema", "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public void CallingColumnExistsReturnsFalseIfColumnDoesNotExistWithSchema()
        {
            using (var table = new SqlServerTestTable(Processor, "test_schema", "id int"))
                Processor.ColumnExists("test_schema", table.Name, "DoesNotExist").ShouldBeFalse();
        }
	}
}