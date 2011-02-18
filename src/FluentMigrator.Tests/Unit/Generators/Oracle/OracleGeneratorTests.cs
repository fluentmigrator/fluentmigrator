

namespace FluentMigrator.Tests.Unit.Generators
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using FluentMigrator.Expressions;
    using FluentMigrator.Model;
    using NUnit.Framework;
    using NUnit.Should;
    using FluentMigrator.Runner.Generators.Oracle;

	[TestFixture]
	public class OracleGeneratorTests
	{
		private OracleGenerator generator;

		public OracleGeneratorTests()
		{
			generator = new OracleGenerator();
		}

		[Test]
		public void CanCreateTable()
		{
			string tableName = "NewTable";
			CreateTableExpression expression = GetCreateTableExpression(tableName);
			string sql = generator.Generate(expression);
			sql.ShouldBe("CREATE TABLE NewTable (ColumnName1 NVARCHAR2(255) NOT NULL, ColumnName2 NUMBER(10,0) NOT NULL)");
		}

		[Test]
		public void CanCreateTableWithPrimaryKey()
		{
			string tableName = "NewTable";
			CreateTableExpression expression = GetCreateTableExpression(tableName);
			expression.Columns[0].IsPrimaryKey = true;
			string sql = generator.Generate(expression);
			sql.ShouldBe("CREATE TABLE NewTable (ColumnName1 NVARCHAR2(255) NOT NULL PRIMARY KEY, ColumnName2 NUMBER(10,0) NOT NULL)");
		}

		[Test]
		public void CanCreateTableWithDefaultValue()
		{
			string tableName = "NewTable";
			CreateTableExpression expression = GetCreateTableExpression(tableName);
			expression.Columns[0].DefaultValue = "abc";
			string sql = generator.Generate(expression);
			// Oracle requires the DEFAULT clause to appear before the NOT NULL clause
			sql.ShouldBe("CREATE TABLE NewTable (ColumnName1 NVARCHAR2(255) DEFAULT 'abc' NOT NULL, ColumnName2 NUMBER(10,0) NOT NULL)");
		}

		[Test]
		public void CanCreateTableWithMultipartKey()
		{
			string tableName = "NewTable";
			CreateTableExpression expression = GetCreateTableExpression(tableName);
			expression.Columns[0].IsPrimaryKey = true;
			expression.Columns[1].IsPrimaryKey = true;
			string sql = generator.Generate(expression);
			// See the note in OracleColumn about why the PK should not be named
			sql.ShouldBe("CREATE TABLE NewTable (ColumnName1 NVARCHAR2(255) NOT NULL, ColumnName2 NUMBER(10,0) NOT NULL,  PRIMARY KEY (ColumnName1,ColumnName2))");
		}

		[Test]
		public void CanDropTable()
		{
			string tableName = "NewTable";
			DeleteTableExpression expression = GetDeleteTableExpression(tableName);
			string sql = generator.Generate(expression);
			sql.ShouldBe("DROP TABLE NewTable");
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
			sql.ShouldBe("ALTER TABLE NewTable DROP COLUMN NewColumn");
		}

		[Test]
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
			sql.ShouldBe("ALTER TABLE NewTable ADD NewColumn NVARCHAR2(5) NOT NULL");
		}

		[Test]
		public void CanAddDecimalColumn()
		{
			string tableName = "NewTable";

			var columnDefinition = new ColumnDefinition();
			columnDefinition.Name = "NewColumn";
			columnDefinition.Size = 19;
			columnDefinition.Precision = 2;
			columnDefinition.Type = DbType.Decimal;

			var expression = new CreateColumnExpression();
			expression.Column = columnDefinition;
			expression.TableName = tableName;

			string sql = generator.Generate(expression);
			sql.ShouldBe("ALTER TABLE NewTable ADD NewColumn NUMBER(19,2) NOT NULL");
		}

		[Test]
		public void CanRenameTable()
		{
			var expression = new RenameTableExpression();
			expression.OldName = "Table1";
			expression.NewName = "Table2";

			string sql = generator.Generate(expression);
			sql.ShouldBe("ALTER TABLE Table1 RENAME TO Table2");
		}

		[Test]
		public void CanRenameColumn()
		{
			var expression = new RenameColumnExpression();
			expression.TableName = "Table1";
			expression.OldName = "Column1";
			expression.NewName = "Column2";

			string sql = generator.Generate(expression);
			sql.ShouldBe("ALTER TABLE Table1 RENAME COLUMN Column1 TO Column2");
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

			string sql = generator.Generate(expression);
			sql.ShouldBe("CREATE UNIQUE INDEX IX_TEST ON TEST_TABLE (Column1 ASC,Column2 DESC)");
		}

        [Test]
        public void CanDropIndex()
        {
            var expression = new DeleteIndexExpression();
            expression.Index.Name = "IX_TEST";
            expression.Index.TableName = "TEST_TABLE";
         
            string sql = generator.Generate(expression);
            sql.ShouldBe("DROP INDEX IX_TEST");
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

			string sql = generator.Generate(expression);
			sql.ShouldBe("ALTER TABLE TestForeignTable ADD CONSTRAINT FK_Test FOREIGN KEY (Column3,Column4) REFERENCES TestPrimaryTable (Column1,Column2)");
		}

		[Test]
		public void CanDropForeignKey()
		{
			var expression = new DeleteForeignKeyExpression();
			expression.ForeignKey.Name = "FK_Test";
			expression.ForeignKey.ForeignTable = "TestPrimaryTable";

			string sql = generator.Generate(expression);
			sql.ShouldBe("ALTER TABLE TestPrimaryTable DROP CONSTRAINT FK_Test");
		}

		[Test]
		public void CanInsertData()
		{
			var expression = new InsertDataExpression();
			expression.TableName = "TestTable";
			expression.Rows.Add(new InsertionDataDefinition { new KeyValuePair<string, object>("Id", 1), 
													new KeyValuePair<string, object>("Name", "Justin"),
													new KeyValuePair<string, object>("Website", "codethinked.com") });
			expression.Rows.Add(new InsertionDataDefinition { new KeyValuePair<string, object>("Id", 2), 
													new KeyValuePair<string, object>("Name", "Nate"),
													new KeyValuePair<string, object>("Website", "kohari.org") });

			string sql = generator.Generate(expression);

			string expected = "INSERT ALL INTO TestTable (Id,Name,Website) VALUES (1,'Justin','codethinked.com')";
			expected += " INTO TestTable (Id,Name,Website) VALUES (2,'Nate','kohari.org')";
			expected += " SELECT 1 FROM DUAL";

			sql.ShouldBe(expected);
		}

		[Test]
		[Ignore("Oracle can not insert GUID data using string representation")]
		public void CanInsertGuidData()
		{
			var gid = Guid.NewGuid();
			var expression = new InsertDataExpression() { TableName = "TestTable" };
			expression.Rows.Add(new InsertionDataDefinition { new KeyValuePair<string, object>("guid", gid) });

			string sql = generator.Generate(expression);

			string expected = String.Format("INSERT ALL INTO TestTable (guid) VALUES ('{0}') SELECT 1 FROM DUAL", gid.ToString());

			sql.ShouldBe(expected);
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