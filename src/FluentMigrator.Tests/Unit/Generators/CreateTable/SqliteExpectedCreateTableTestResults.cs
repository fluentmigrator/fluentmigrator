using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator.Runner.Generators;
using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Generators.CreateTable
{
	public class SqliteGeneratorCreateTableTests : GeneratorCreateTableTestsBase<SqliteGenerator,SqliteExpectedCreateTableTestResults>
	{
		[Ignore("Sqlite requires primary key to be set on identity column")]
		public override void CanCreateTableWithIdentity()
		{
			base.CanCreateTableWithIdentity();
		}
	}

	public class SqliteExpectedCreateTableTestResults : IExpectedCreateTableTestResults
	{
		public string CreateTable()
		{
			return "CREATE TABLE NewTable (ColumnName1 TEXT NOT NULL)";
		}

		public string CreateTableWithCustomColumnType()
		{
			return "CREATE TABLE NewTable (ColumnName1 timestamp NOT NULL)";
		}

		public string CreateTableWithPrimaryKey()
		{
			return "CREATE TABLE NewTable (ColumnName1 TEXT NOT NULL PRIMARY KEY)";
		}

		public string CreateTableWithIdentity()
		{
			return "CREATE TABLE NewTable (ColumnName1 INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT)";
		}

		public string CreateTableWithNullField()
		{
			return "CREATE TABLE NewTable (ColumnName1 TEXT)";
		}

		public string CreateTableWithDefaultValue()
		{
			return "CREATE TABLE NewTable (ColumnName1 TEXT NOT NULL DEFAULT 'Default', ColumnName2 INTEGER NOT NULL DEFAULT 0)";
		}

		public string CreateTableWithDefaultValueExplicitlySetToNull()
		{
			return "CREATE TABLE NewTable (ColumnName1 TEXT DEFAULT NULL)";
		}

		public string CreateTableWithMultipleColumns()
		{
			return "CREATE TABLE NewTable (ColumnName1 TEXT NOT NULL, ColumnName2 INTEGER NOT NULL, ColumnName3 TEXT NOT NULL)";
		}

		public string CreateTableWithMultipartPrimaryKey()
		{
			return "CREATE TABLE NewTable (ColumnName1 TEXT NOT NULL, ColumnName2 INTEGER NOT NULL, CONSTRAINT ColumnName1_ColumnName2_PK PRIMARY KEY (ColumnName1,ColumnName2))";
		}
	}
}
