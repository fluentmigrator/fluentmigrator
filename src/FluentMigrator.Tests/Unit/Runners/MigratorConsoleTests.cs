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
using FluentMigrator.Console;
using FluentMigrator.Runner.Processors.Sqlite;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Runners
{
	[TestFixture]
	public class MigratorConsoleTests
	{
		string database = "Sqlite";
		string connection = "Data Source=:memory:;Version=3;New=True;";
		string target = "FluentMigrator.Tests.dll";

		[Test]
		public void MustInitializeConsoleWithDatabaseArgument()
		{
			string[] args = { "/connection", connection, "/log" };
			new MigratorConsole(args);
			Assert.That(Environment.ExitCode == 1);
		}

		[Test]
		public void MustInitializeConsoleWithConnectionArgument()
		{
			string[] args = { "/db", database, "/log" };
			new MigratorConsole(args);
			Assert.That(Environment.ExitCode == 1);
		}

		[Test]
		public void CanInitMigratorConsoleWithValidArguments()
		{
			string[] args = { "/db", database, "/connection", connection, "/log", "/target", target, 
								"/namespace", "FluentMigrator.Tests.Integration.Migrations", 
								"/task", "migrate:up",
								"/version", "1"};

			MigratorConsole console = new MigratorConsole(args);

			console.Processor.ShouldBeOfType<SqliteProcessor>();
			console.Connection.ShouldBe(connection);
			console.Namespace.ShouldBe("FluentMigrator.Tests.Integration.Migrations");
			console.Log.ShouldBeTrue();
			console.Task.ShouldBe("migrate:up");
			console.Version.ShouldBe(1);
		}

	}
}
