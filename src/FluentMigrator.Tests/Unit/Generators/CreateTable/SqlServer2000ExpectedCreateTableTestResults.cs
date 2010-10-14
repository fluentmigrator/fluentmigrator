using System;
using FluentMigrator.Runner.Generators;

namespace FluentMigrator.Tests.Unit.Generators.CreateTable
{
	public class SqlServer2000GeneratorCreateTableTests : GeneratorCreateTableTestsBase<SqlServer2000Generator, SqlServer2000ExpectedCreateTableTestResults>
	{
	}

	public class SqlServer2000ExpectedCreateTableTestResults : IExpectedCreateTableTestResults	
	{
		
		public string CreateTable()
		{
			return "CREATE TABLE [NewTable] (ColumnName1 NVARCHAR(255) NOT NULL)";
		}

		public string CreateTableWithMultipleColumns()
		{
			return "CREATE TABLE [NewTable] (ColumnName1 NVARCHAR(255) NOT NULL, ColumnName2 INT NOT NULL, ColumnName3 NVARCHAR(255) NOT NULL)";
		}


		public string CreateTableWithCustomColumnType()
		{
			return
				"CREATE TABLE [NewTable] (ColumnName1 [timestamp] NOT NULL)";
		}

		public string CreateTableWithPrimaryKey()
		{
			return "CREATE TABLE [NewTable] (ColumnName1 NVARCHAR(255) NOT NULL PRIMARY KEY CLUSTERED)";
		}

		public string CreateTableWithIdentity()
		{
			return "CREATE TABLE [NewTable] (ColumnName1 NVARCHAR(255) NOT NULL IDENTITY(1,1))";
		}

		public string CreateTableWithNullField()
		{
			return "CREATE TABLE [NewTable] (ColumnName1 NVARCHAR(255))";
		}

		public string CreateTableWithDefaultValue()
		{
			return
				"CREATE TABLE [NewTable] (ColumnName1 NVARCHAR(255) NOT NULL DEFAULT 'Default', ColumnName2 INT NOT NULL DEFAULT 0)";
		}

		public string CreateTableWithDefaultValueExplicitlySetToNull()
		{
			return "CREATE TABLE [NewTable] (ColumnName1 NVARCHAR(255) NOT NULL DEFAULT NULL)";
		}
	}
}