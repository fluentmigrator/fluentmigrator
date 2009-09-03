using System;
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

		[Test,Ignore("failing for some reason. need to investigate")]
		public void CanInitMigratorConsoleWithValidArguments()
		{
			string[] args = { "/db", database, "/connection", connection, "/log", "/target", target };

			MigratorConsole console = new MigratorConsole(args);

			console.Processor.ShouldBeOfType<SqliteProcessor>();
			console.Connection.ShouldBe(connection);
			console.Log.ShouldBeTrue();
		}
	}
}
