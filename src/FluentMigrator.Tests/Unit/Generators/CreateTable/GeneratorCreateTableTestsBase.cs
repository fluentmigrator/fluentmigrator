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
			var expression = GetCreateTableExpression(tableName, 1);
			var sql = generator.Generate(expression);
			sql.ShouldBe(expected.CreateTable());
		}

		[Test]
		public void CanCreateTableWithMultipleColumns()
		{
			var expression = GetCreateTableExpression(tableName, 3);
			var sql = generator.Generate(expression);
			sql.ShouldBe(expected.CreateTableWithMultipleColumns());
		}

		[Test]
		public void CanCreateTableWithCustomColumnType()
		{
			var expression = GetCreateTableExpression(tableName, 1);
			//FIXME: It feels like a bug that CustomType is currently ignored when Type is already set - should throw ?
			expression.Columns[0].Type = null;
			expression.Columns[0].CustomType = "[timestamp]";
			var sql = generator.Generate(expression);
			sql.ShouldBe(expected.CreateTableWithCustomColumnType());
		}

		[Test]
		public void CanCreateTableWithPrimaryKey()
		{
			var expression = GetCreateTableExpression(tableName, 1);
			expression.Columns[0].IsPrimaryKey = true;
			var sql = generator.Generate(expression);
			sql.ShouldBe(expected.CreateTableWithPrimaryKey());
		}

		[Test]
		public void CanCreateTableWithIdentity()
		{
			var expression = GetCreateTableExpression(tableName, 1);
			expression.Columns[0].IsIdentity = true;
			var sql = generator.Generate(expression);
			sql.ShouldBe(expected.CreateTableWithIdentity());
		}

		[Test]
		public void CanCreateTableWithNullField()
		{
			var expression = GetCreateTableExpression(tableName, 1);
			expression.Columns[0].IsNullable = true;
			var sql = generator.Generate(expression);
			sql.ShouldBe(expected.CreateTableWithNullField());
		}

		[Test]
		public void CanCreateTableWithDefaultValue()
		{
			var expression = GetCreateTableExpression(tableName, 2);
			expression.Columns[0].DefaultValue = "Default";
			expression.Columns[1].DefaultValue = 0;
			var sql = generator.Generate(expression);
			sql.ShouldBe(expected.CreateTableWithDefaultValue());
		}

		[Test]
		public void CanCreateTableWithDefaultValueExplicitlySetToNull()
		{
			var expression = GetCreateTableExpression(tableName, 1);
			expression.Columns[0].DefaultValue = null;
			var sql = generator.Generate(expression);
			sql.ShouldBe(expected.CreateTableWithDefaultValueExplicitlySetToNull());

		}

		protected CreateTableExpression GetCreateTableExpression(string tableName, int numberColumns)
		{
			var types = new [] {DbType.String, DbType.Int32};

			var expression = new CreateTableExpression { TableName = tableName };

			for (int idx = 1; idx <= numberColumns; idx++)
			{
				var column = new ColumnDefinition { Name = "ColumnName" + idx, Type = types [(idx-1) % types.Length]};
				expression.Columns.Add(column);
			}
			return expression;
		}
	}
}