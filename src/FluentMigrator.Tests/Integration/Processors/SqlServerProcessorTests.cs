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
using FluentMigrator.Runner.Processors.SqlServer;
using FluentMigrator.Tests.Helpers;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors
{
	[TestFixture]
	public class SqlServerProcessorTests
	{
		public SqlConnection Connection { get; set; }
		public SqlServerProcessor Processor { get; set; }

		public SqlServerProcessorTests()
		{
			Connection = new SqlConnection(@"server=(local)\sqlexpress;uid=;pwd=;Trusted_Connection=yes;database=FluentMigrator");
			Connection.Open();

			Processor = new SqlServerProcessor(Connection, new SqlServer2000Generator(), new TextWriterAnnouncer(System.Console.Out));
		}

		[Test]
		public void CallingTableExistsReturnsTrueIfTableExists()
		{
			using (var table = new SqlServerTestTable(Processor, "id int"))
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
			using (var table = new SqlServerTestTable(Processor, "id int"))
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
			using (var table = new SqlServerTestTable(Processor, "id int"))
				Processor.ColumnExists(table.Name, "DoesNotExist").ShouldBeFalse();
		}
	}
}