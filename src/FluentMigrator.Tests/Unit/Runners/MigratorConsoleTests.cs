using System;
using FluentMigrator;
using FluentMigrator.Runner.Processors;
using Xunit;

namespace FluentMigrator.Tests.Unit.Runners
{
	public class MigratorConsoleTests
	{
		string database = "Sqlite";
		string connection = "Data Source=:memory:;Version=3;New=True;";
		string log = "1";

		[Fact]
		public void MustInitializeConsoleWithDatabaseArgument()
		{
			string[] args = { "/connection", connection, "/log" };
			Assert.Throws<ArgumentException>(() => new MigratorConsole(args));
		}

		[Fact]
		public void MustInitializeConsoleWithConnectionArgument()
		{
			string[] args = { "/db", database, "/log" };
			Assert.Throws<ArgumentException>(() => new MigratorConsole(args));
		}

		[Fact]
		public void CanInitMigratorConsoleWithValidArguments()
		{
			string[] args = { "/db", database, "/connection", connection, "/log" };

			MigratorConsole console = new MigratorConsole(args);
			Assert.Equal(typeof(SqliteProcessor), console.Processor.GetType());
			Assert.Equal(connection, console.Connection);
			Assert.True(console.Log);
		}
	}
}
