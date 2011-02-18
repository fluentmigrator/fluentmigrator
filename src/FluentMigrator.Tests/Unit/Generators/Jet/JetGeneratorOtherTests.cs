

namespace FluentMigrator.Tests.Unit.Generators
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using FluentMigrator.Expressions;
    using FluentMigrator.Model;
    using NUnit.Framework;
    using NUnit.Should;

	public class JetGeneratorOtherTests : JetGeneratorTestBase
	{
		

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
			sql.ShouldBe("ALTER TABLE [NewTable] ADD COLUMN [NewColumn] VARCHAR(5) NOT NULL");
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
			sql.ShouldBe("ALTER TABLE [NewTable] ADD COLUMN [NewColumn] DECIMAL(19,2) NOT NULL");
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
			sql.ShouldBe(
				"ALTER TABLE [TestForeignTable] ADD CONSTRAINT FK_Test FOREIGN KEY ([Column3],[Column4]) REFERENCES [TestPrimaryTable] ([Column1],[Column2])");
		}

		[Test]
		public void CanCreateIndex()
		{
			var expression = new CreateIndexExpression();
			expression.Index.Name = "IX_TEST";
			expression.Index.TableName = "TEST_TABLE";
			expression.Index.IsUnique = true;
			expression.Index.Columns.Add(new IndexColumnDefinition { Direction = Direction.Ascending, Name = "Column1" });
			expression.Index.Columns.Add(new IndexColumnDefinition { Direction = Direction.Descending, Name = "Column2" });

			var sql = generator.Generate(expression);
			sql.ShouldBe("CREATE UNIQUE INDEX IX_TEST ON [TEST_TABLE] (Column1 ASC,Column2 DESC)");
		}

		[Test]
		public void CanDeleteIndex()
		{
			var expression = new DeleteIndexExpression();
			expression.Index.Name = "IX_TEST";
			expression.Index.TableName = "TEST_TABLE";

			var sql = generator.Generate(expression);
			sql.ShouldBe("DROP INDEX IX_TEST ON [TEST_TABLE]");
		}

		[Test]
		public void CanDropColumn()
		{
			string tableName = "NewTable";
			string columnName = "NewColumn";

			var expression = new DeleteColumnExpression();
			expression.TableName = tableName;
			expression.ColumnName = columnName;

			string sql = generator.Generate(expression);
			sql.ShouldBe("ALTER TABLE [NewTable] DROP COLUMN NewColumn");
		}

		[Test]
		public void CanDropForeignKey()
		{
			var expression = new DeleteForeignKeyExpression();
			expression.ForeignKey.Name = "FK_Test";
			expression.ForeignKey.ForeignTable = "TestPrimaryTable";

			var sql = generator.Generate(expression);
			sql.ShouldBe("ALTER TABLE [TestPrimaryTable] DROP CONSTRAINT FK_Test");
		}

		[Test]
		public void CanDropTable()
		{
			var tableName = "NewTable";
			var expression = GetDeleteTableExpression();
			var sql = generator.Generate(expression);
			sql.ShouldBe("DROP TABLE [NewTable]");
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

			var expected = "INSERT INTO [TestTable] ([Id],[Name],[Website]) VALUES (1,'Just''in','codethinked.com');";
			expected += @"INSERT INTO [TestTable] ([Id],[Name],[Website]) VALUES (2,'Na\te','kohari.org');";

			sql.ShouldBe(expected);
		}

		[Test]
		public void CanDeleteData()
		{
			var expression = new DeleteDataExpression();
			expression.TableName = "TestTable";
			expression.Rows.Add(new DeletionDataDefinition
									{
										new KeyValuePair<string, object>("Name", "Just'in"),
										new KeyValuePair<string, object>("Website", null)
									});

			var sql = generator.Generate(expression);

			sql.ShouldBe("DELETE FROM [TestTable] WHERE [Name] = 'Just''in' AND [Website] IS NULL;");
		}

		[Test]
		public void CanInsertGuidData()
		{
			var gid = Guid.NewGuid();
			var expression = new InsertDataExpression { TableName = "TestTable" };
			expression.Rows.Add(new InsertionDataDefinition { new KeyValuePair<string, object>("guid", gid) });

			var sql = generator.Generate(expression);

			var expected = String.Format("INSERT INTO [TestTable] ([guid]) VALUES ('{0}');", gid);

			sql.ShouldBe(expected);
		}

		[Test, Ignore("Is not supported.")]
		public void CanRenameColumn()
		{
			var expression = new RenameColumnExpression();
			expression.TableName = "Table1";
			expression.OldName = "Column1";
			expression.NewName = "Column2";

			var sql = generator.Generate(expression);
			sql.ShouldBe("sp_rename '[Table1].[Column1]', [Column2]");
		}

		[Test, Ignore("Is not supported.")]
		public void CanRenameTable()
		{
			var expression = new RenameTableExpression();
			expression.OldName = "Table1";
			expression.NewName = "Table2";

			var sql = generator.Generate(expression);
			sql.ShouldBe("sp_rename [Table1], [Table2]");
		}

		// Xml Columns are not supported

		[Test]
		public void CanAlterColumn()
		{
			var expression = new AlterColumnExpression();
			expression.TableName = "Table1";

			expression.Column = new ColumnDefinition();
			expression.Column.Name = "Column1";
			expression.Column.Type = DbType.String;
			expression.Column.Size = 20;
			expression.Column.IsNullable = false;

			var sql = generator.Generate(expression);
			sql.ShouldBe("ALTER TABLE [Table1] ALTER COLUMN [Column1] VARCHAR(20) NOT NULL");
		}

		[Test]
		public void CanUpdateData()
		{
			var expression = new UpdateDataExpression();
			expression.TableName = "Table1";

			expression.Set = new List<KeyValuePair<string, object>>
								 {
									 new KeyValuePair<string, object>("Name", "Just'in"),
									 new KeyValuePair<string, object>("Age", 25)
								 };

			expression.Where = new List<KeyValuePair<string, object>>
								   {
									   new KeyValuePair<string, object>("Id", 9),
									   new KeyValuePair<string, object>("Homepage", null)
								   };

			var sql = generator.Generate(expression);
			sql.ShouldBe("UPDATE [Table1] SET [Name] = 'Just''in', [Age] = 25 WHERE [Id] = 9 AND [Homepage] IS NULL");
		}
	}
}
