using System.Data;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators;
using Moq;
using Xunit;

namespace FluentMigrator.Tests.Generators
{
	public class SqlServerGeneratorTests
	{
		private SqlServerGenerator generator;
		
		public SqlServerGeneratorTests()
		{
			generator = new SqlServerGenerator();
		}

		[Fact]
		public void CanCreateTable()
		{
			string tableName = "NewTable";
			CreateTableExpression expression = GetCreateTableExpression(tableName);
			string sql = generator.Generate(expression);
			Assert.Equal("CREATE TABLE NewTable (ColumnName1 NVARCHAR(255), ColumnName2 INT)", sql);			
		}

		[Fact]
		public void CanDropTable()
		{
			string tableName = "NewTable";
			DeleteTableExpression expression = GetDeleteTableExpression(tableName);
			string sql = generator.Generate(expression);
			Assert.Equal("DROP TABLE NewTable", sql);
		}

		private DeleteTableExpression GetDeleteTableExpression(string tableName)
		{
			return new DeleteTableExpression { TableName = tableName };
		}

		private CreateTableExpression GetCreateTableExpression(string tableName)
		{
			string columnName1 = "ColumnName1";
			string columnName2 = "ColumnName2";

			var column1 = new Mock<ColumnDefinition>();
			column1.ExpectGet(c => c.Name).Returns(columnName1);
			column1.ExpectGet(c => c.Type).Returns(DbType.String);

			var column2 = new Mock<ColumnDefinition>();
			column2.ExpectGet(c => c.Name).Returns(columnName2);
			column2.ExpectGet(c => c.Type).Returns(DbType.Int32);

			var expression = new CreateTableExpression { TableName = tableName };
			expression.Columns.Add(column1.Object);
			expression.Columns.Add(column2.Object);
			return expression;
		}	    
	}
}