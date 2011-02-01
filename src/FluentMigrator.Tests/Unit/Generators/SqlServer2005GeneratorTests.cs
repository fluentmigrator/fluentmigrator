using System;
using System.Data;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators
{
    [TestFixture]
	public class SqlServer2005GeneratorTests
	{
		private SqlServer2005Generator generator;

		[SetUp]
		public void SetUp()
		{
			generator = new SqlServer2005Generator();
		}

		[Test]
		public void CanCreateTableWithNvarcharMax()
		{
			var expression = GetCreateTableExpression(tableName);
			expression.Columns[0].Type = DbType.String;
			expression.Columns[0].Size = Int32.MaxValue;
			var sql = generator.Generate(expression);
			sql.ShouldBe(
				"CREATE TABLE [dbo].[NewTable] (ColumnName1 NVARCHAR(MAX) NOT NULL)");
		}

    [Test]
    public void CanAlterSchema()
    {
      var expression = new AlterSchemaExpression
      {
        DestinationSchemaName = "DEST",
        SourceSchemaName = "SOURCE",
        TableName = "TABLE"
      };

      var sql = generator.Generate( expression );
      sql.ShouldBe(
        "ALTER SCHEMA [DEST] TRANSFER [SOURCE].[TABLE]" );
    }

		private CreateTableExpression GetCreateTableExpression(string tableName)
		{
			var columnName1 = "ColumnName1";

			var column1 = new ColumnDefinition { Name = columnName1, Type = DbType.String };

			var expression = new CreateTableExpression { TableName = tableName };
			expression.Columns.Add(column1);
			return expression;
		}

		private string tableName = "NewTable";
	}
}