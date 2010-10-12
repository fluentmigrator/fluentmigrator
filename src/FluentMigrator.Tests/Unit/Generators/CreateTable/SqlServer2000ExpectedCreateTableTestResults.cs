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
			return "CREATE TABLE [NewTable] (ColumnName1 NVARCHAR(255) NOT NULL, ColumnName2 INT NOT NULL)";
		}

		public string CreateTableWithCustomColumnType()
		{
			return
				"CREATE TABLE [NewTable] (ColumnName1 NVARCHAR(255) NOT NULL PRIMARY KEY CLUSTERED, ColumnName2 [timestamp] NOT NULL)";
		}

		public string CreateTableWithPrimaryKey()
		{
			return "CREATE TABLE [NewTable] (ColumnName1 NVARCHAR(255) NOT NULL PRIMARY KEY CLUSTERED, ColumnName2 INT NOT NULL)";
		}

		public string CreateTableWithIdentity()
		{
			return "CREATE TABLE [NewTable] (ColumnName1 NVARCHAR(255) NOT NULL IDENTITY(1,1), ColumnName2 INT NOT NULL)";
		}

		public string CreateTableWithNullField()
		{
			return "CREATE TABLE [NewTable] (ColumnName1 NVARCHAR(255), ColumnName2 INT NOT NULL)";
		}

		public string CreateTableWithDefaultValue()
		{
			return
				"CREATE TABLE [NewTable] (ColumnName1 NVARCHAR(255) NOT NULL DEFAULT 'Default', ColumnName2 INT NOT NULL DEFAULT 0)";
		}

		public string CreateTableWithDefaultValueExplicitlySetToNull()
		{
			return "CREATE TABLE [NewTable] (ColumnName1 NVARCHAR(255) NOT NULL DEFAULT NULL, ColumnName2 INT NOT NULL)";
		}
	}
}