using System.Data;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.CreateTable
{
	[TestFixture]
	public abstract class GeneratorCreateTableTestsBase<Generator,Expected> 
		where Generator : IMigrationGenerator, new()
		where Expected : IExpectedCreateTableTestResults, new()
	{
		protected IMigrationGenerator generator;
		protected IExpectedCreateTableTestResults expected;

		[SetUp]
		public void SetUp()
		{
			generator = new Generator();
			expected = new Expected();
		}

		private string tableName = "NewTable";

		[Test]
		public void CanCreateTable()
		{
			var expression = GetCreateTableExpression(tableName);
			var sql = generator.Generate(expression);
			sql.ShouldBe(expected.CreateTable());
		}

		[Test]
		public void CanCreateTableWithCustomColumnType()
		{
			var expression = GetCreateTableExpression(tableName);
			expression.Columns[0].IsPrimaryKey = true;
			expression.Columns[1].Type = null;
			expression.Columns[1].CustomType = "[timestamp]";
			var sql = generator.Generate(expression);
			sql.ShouldBe(expected.CreateTableWithCustomColumnType());
		}

		[Test]
		public void CanCreateTableWithPrimaryKey()
		{
			var expression = GetCreateTableExpression(tableName);
			expression.Columns[0].IsPrimaryKey = true;
			var sql = generator.Generate(expression);
			sql.ShouldBe(expected.CreateTableWithPrimaryKey());
		}

		[Test]
		public void CanCreateTableWithIdentity()
		{
			var expression = GetCreateTableExpression(tableName);
			expression.Columns[0].IsIdentity = true;
			var sql = generator.Generate(expression);
			sql.ShouldBe(expected.CreateTableWithIdentity());
		}

		[Test]
		public void CanCreateTableWithNullField()
		{
			var expression = GetCreateTableExpression(tableName);
			expression.Columns[0].IsNullable = true;
			var sql = generator.Generate(expression);
			sql.ShouldBe(expected.CreateTableWithNullField());
		}

		[Test]
		public void CanCreateTableWithDefaultValue()
		{
			var expression = GetCreateTableExpression(tableName);
			expression.Columns[0].DefaultValue = "Default";
			expression.Columns[1].DefaultValue = 0;
			var sql = generator.Generate(expression);
			sql.ShouldBe(expected.CreateTableWithDefaultValue());
		}

		[Test]
		public void CanCreateTableWithDefaultValueExplicitlySetToNull()
		{
			var expression = GetCreateTableExpression(tableName);
			expression.Columns[0].DefaultValue = null;
			var sql = generator.Generate(expression);
			sql.ShouldBe(expected.CreateTableWithDefaultValueExplicitlySetToNull());

		}


		protected CreateTableExpression GetCreateTableExpression(string tableName)
		{
			var columnName1 = "ColumnName1";
			var columnName2 = "ColumnName2";

			var column1 = new ColumnDefinition { Name = columnName1, Type = DbType.String };
			var column2 = new ColumnDefinition { Name = columnName2, Type = DbType.Int32 };

			var expression = new CreateTableExpression { TableName = tableName };
			expression.Columns.Add(column1);
			expression.Columns.Add(column2);
			return expression;
		}
	}
}