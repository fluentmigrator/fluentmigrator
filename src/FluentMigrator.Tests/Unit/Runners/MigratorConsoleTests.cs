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
		[ExpectedException(typeof(ArgumentException))]
		public void MustInitializeConsoleWithDatabaseArgument()
		{
			string[] args = { "/connection", connection, "/log" };
			new MigratorConsole(args);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void MustInitializeConsoleWithConnectionArgument()
		{
			string[] args = { "/db", database, "/log" };
			new MigratorConsole(args);
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
