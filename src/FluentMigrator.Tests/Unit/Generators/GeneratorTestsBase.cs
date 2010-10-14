using System;
using System.Collections.Generic;
using System.Data;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators
{
	[TestFixture]
	public abstract class GeneratorTestsBase<Generator,Expected> 
		where Generator : IMigrationGenerator, new()
		where Expected : IExpectedTestResults, new()
	{
		protected IMigrationGenerator generator;
		protected IExpectedTestResults expected;

		[SetUp]
		public void SetUp()
		{
			generator = new Generator();
			expected = new Expected();
		}

		[Test]
		public void CanAddColumn()
		{
			var tableName = "NewTable";

			var columnDefinition = new ColumnDefinition();
			columnDefinition.Name = "NewColumn";
			columnDefinition.Size = 5;
			columnDefinition.Type = DbType.String;

			var expression = new CreateColumnExpression();
			expression.Column = columnDefinition;
			expression.TableName = tableName;

			var sql = generator.Generate(expression);
			sql.ShouldBe(expected.AddColumn());
		}

		[Test]
		public void CanAddDecimalColumn()
		{
			var tableName = "NewTable";

			var columnDefinition = new ColumnDefinition();
			columnDefinition.Name = "NewColumn";
			columnDefinition.Size = 19;
			columnDefinition.Precision = 2;
			columnDefinition.Type = DbType.Decimal;

			var expression = new CreateColumnExpression();
			expression.Column = columnDefinition;
			expression.TableName = tableName;

			var sql = generator.Generate(expression);
			sql.ShouldBe(expected.AddDecimalColumn());
		}

		[Test]
		public void CanCreateForeignKey()
		{
			var expression = new CreateForeignKeyExpression();
			expression.ForeignKey.Name = "FK_Test";
			expression.ForeignKey.PrimaryTable = "TestPrimaryTable";
			expression.ForeignKey.ForeignTable = "TestForeignTable";
			expression.ForeignKey.PrimaryColumns = new[] { "Column1", "Column2" };
			expression.ForeignKey.ForeignColumns = new[] { "Column3", "Column4" };

			var sql = generator.Generate(expression);
			sql.ShouldBe(expected.CreateForeignKey());
		}

		[Test]
		public void CanCreateIndex()
		{
			var expression = new CreateIndexExpression();
			expression.Index.Name = "IX_TEST";
			expression.Index.TableName = "TEST_TABLE";
			expression.Index.IsUnique = true;
			expression.Index.IsClustered = true;
			expression.Index.Columns.Add(new IndexColumnDefinition { Direction = Direction.Ascending, Name = "Column1" });
			expression.Index.Columns.Add(new IndexColumnDefinition { Direction = Direction.Descending, Name = "Column2" });

			var sql = generator.Generate(expression);
			sql.ShouldBe(expected.CreateUniqueClusteredIndex());
		}

		[Test]
		public void CanDropForeignKey()
		{
			var expression = new DeleteForeignKeyExpression();
			expression.ForeignKey.Name = "FK_Test";
			expression.ForeignKey.ForeignTable = "TestPrimaryTable";

			var sql = generator.Generate(expression);
			sql.ShouldBe(expected.DropForeignKey());
		}

		[Test]
		public void CanDropTable()
		{
			var tableName = "NewTable";
			var expression = new DeleteTableExpression { TableName = tableName };
			var sql = generator.Generate(expression);
			sql.ShouldBe(expected.DropTable());
		}

		[Test]
		public void CanInsertData()
		{
			var expression = new InsertDataExpression();
			expression.TableName = "TestTable";
			expression.Rows.Add(new InsertionDataDefinition
									{
										new KeyValuePair<string, object>("Id", 1),
										new KeyValuePair<string, object>("Name", "Just'in"),
										new KeyValuePair<string, object>("Website", "codethinked.com")
									});
			expression.Rows.Add(new InsertionDataDefinition
									{
										new KeyValuePair<string, object>("Id", 2),
										new KeyValuePair<string, object>("Name", @"Na\te"),
										new KeyValuePair<string, object>("Website", "kohari.org")
									});

			var sql = generator.Generate(expression);

			sql.ShouldBe(expected.InsertData());
		}

		[Test]
		public void CanInsertGuidData()
		{
			var gid = new Guid("12345678-1234-1234-1234-123456789012");
			var expression = new InsertDataExpression { TableName = "TestTable" };
			expression.Rows.Add(new InsertionDataDefinition { new KeyValuePair<string, object>("guid", gid) });

			var sql = generator.Generate(expression);

			sql.ShouldBe(expected.InsertGuidData());
		}

		[Test]
		public void CanRenameColumn()
		{
			var expression = new RenameColumnExpression();
			expression.TableName = "Table1";
			expression.OldName = "Column1";
			expression.NewName = "Column2";

			var sql = generator.Generate(expression);
			sql.ShouldBe(expected.RenameColumn());
		}

		[Test]
		public void CanRenameTable()
		{
			var expression = new RenameTableExpression();
			expression.OldName = "Table1";
			expression.NewName = "Table2";

			var sql = generator.Generate(expression);
			sql.ShouldBe(expected.RenameTable());
		}
	}
}