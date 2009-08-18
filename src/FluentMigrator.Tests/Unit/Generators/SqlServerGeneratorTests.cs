using System.Collections.Generic;
using System.Data;
using FluentMigrator.Builders.Insert;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators;
using Xunit;

namespace FluentMigrator.Tests.Unit.Generators
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
			Assert.Equal("CREATE TABLE [NewTable] (ColumnName1 NVARCHAR(255) NOT NULL, ColumnName2 INT NOT NULL)", sql);
		}

		[Fact]
		public void CanCreateTableWithPrimaryKey()
		{
			string tableName = "NewTable";
			CreateTableExpression expression = GetCreateTableExpression(tableName);
			expression.Columns[0].IsPrimaryKey = true;
			string sql = generator.Generate(expression);
            Assert.Equal("CREATE TABLE [NewTable] (ColumnName1 NVARCHAR(255) NOT NULL PRIMARY KEY CLUSTERED, ColumnName2 INT NOT NULL)", sql);
		}

		[Fact]
		public void CanDropTable()
		{
			string tableName = "NewTable";
			DeleteTableExpression expression = GetDeleteTableExpression(tableName);
			string sql = generator.Generate(expression);
			Assert.Equal("DROP TABLE [NewTable]", sql);
		}

		[Fact]
		public void CanDropColumn()
		{
			string tableName = "NewTable";
			string columnName = "NewColumn";

			var expression = new DeleteColumnExpression();
			expression.TableName = tableName;
			expression.ColumnName = columnName;

			string sql = generator.Generate(expression);
			Assert.Equal("ALTER TABLE [NewTable] DROP COLUMN NewColumn", sql);
		}

		[Fact]
		public void CanAddColumn()
		{
			string tableName = "NewTable";

			var columnDefinition = new ColumnDefinition();
			columnDefinition.Name = "NewColumn";
			columnDefinition.Size = 5;
			columnDefinition.Type = DbType.String;

			var expression = new CreateColumnExpression();
			expression.Column = columnDefinition;
			expression.TableName = tableName;

			string sql = generator.Generate(expression);
			Assert.Equal("ALTER TABLE [NewTable] ADD NewColumn NVARCHAR(5) NOT NULL", sql);
		}

		[Fact]
		public void CanRenameTable()
		{
			var expression = new RenameTableExpression();
			expression.OldName = "Table1";
			expression.NewName = "Table2";

			string sql = generator.Generate(expression);
			Assert.Equal("sp_rename [Table1], [Table2]", sql);
		}

		[Fact]
		public void CanRenameColumn()
		{
			var expression = new RenameColumnExpression();
			expression.TableName = "Table1";
			expression.OldName = "Column1";
			expression.NewName = "Column2";

			string sql = generator.Generate(expression);
			Assert.Equal("sp_rename '[Table1].[Column1]', [Column2]", sql);
		}

		[Fact]
		public void CanCreateIndex()
		{
			var expression = new CreateIndexExpression();
			expression.Index.Name = "IX_TEST";
			expression.Index.TableName = "TEST_TABLE";
			expression.Index.IsUnique = true;
			expression.Index.IsClustered = true;
			expression.Index.Columns.Add(new IndexColumnDefinition { Direction = Direction.Ascending, Name = "Column1" });
			expression.Index.Columns.Add(new IndexColumnDefinition { Direction = Direction.Descending, Name = "Column2" });

			string sql = generator.Generate(expression);
			Assert.Equal("CREATE UNIQUE CLUSTERED INDEX IX_TEST ON TEST_TABLE (Column1 ASC,Column2 DESC)", sql);
		}

		[Fact]
		public void CanCreateForeignKey()
		{
			var expression = new CreateForeignKeyExpression();
			expression.ForeignKey.Name = "FK_Test";
			expression.ForeignKey.PrimaryTable = "TestPrimaryTable";
			expression.ForeignKey.ForeignTable = "TestForeignTable";
			expression.ForeignKey.PrimaryColumns = new[] {"Column1", "Column2"};
			expression.ForeignKey.ForeignColumns = new[] { "Column3", "Column4" };

			string sql = generator.Generate(expression);
            Assert.Equal("ALTER TABLE [TestForeignTable] ADD CONSTRAINT FK_Test FOREIGN KEY (Column3,Column4) REFERENCES [TestPrimaryTable] (Column1,Column2)", sql);
		}

		[Fact]
		public void CanDropForeignKey()
		{
			var expression = new DeleteForeignKeyExpression();
			expression.ForeignKey.Name = "FK_Test";
			expression.ForeignKey.PrimaryTable = "TestPrimaryTable";

			string sql = generator.Generate(expression);
			Assert.Equal("ALTER TABLE [TestPrimaryTable] DROP CONSTRAINT FK_Test", sql);
		}

        [Fact]
        public void CanInsertData()
        {
            var expression = new InsertDataExpression();
            expression.TableName = "TestTable";
            expression.Rows.Add(new InsertionData { new KeyValuePair<string, object>("Id", 1), 
                                                    new KeyValuePair<string, object>("Name", "Justin"),
                                                    new KeyValuePair<string, object>("Website", "codethinked.com") });
            expression.Rows.Add(new InsertionData { new KeyValuePair<string, object>("Id", 2), 
                                                    new KeyValuePair<string, object>("Name", "Nate"),
                                                    new KeyValuePair<string, object>("Website", "kohari.org") });

            string sql = generator.Generate(expression);

            string expected = "INSERT INTO [TestTable] (Id,Name,Website) VALUES (1,'Justin','codethinked.com');";
            expected += "INSERT INTO [TestTable] (Id,Name,Website) VALUES (2,'Nate','kohari.org');";

            Assert.Equal(expected, sql);
        }

		private DeleteTableExpression GetDeleteTableExpression(string tableName)
		{
			return new DeleteTableExpression { TableName = tableName };
		}

		private CreateTableExpression GetCreateTableExpression(string tableName)
		{
			string columnName1 = "ColumnName1";
			string columnName2 = "ColumnName2";

			var column1 = new ColumnDefinition { Name = columnName1, Type = DbType.String };
			var column2 = new ColumnDefinition { Name = columnName2, Type = DbType.Int32 };

			var expression = new CreateTableExpression { TableName = tableName };
			expression.Columns.Add(column1);
			expression.Columns.Add(column2);
			return expression;
		}
	}
}