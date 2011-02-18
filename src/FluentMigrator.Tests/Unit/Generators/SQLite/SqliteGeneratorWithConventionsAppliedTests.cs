

namespace FluentMigrator.Tests.Unit.Generators
{
    using System.Collections.Generic;
    using System.Data;
    using FluentMigrator.Expressions;
    using FluentMigrator.Model;
    using NUnit.Framework;
    using FluentMigrator.Runner.Generators.SQLite;
    using NUnit.Should;

    [TestFixture]
    public class SqliteGeneratorWithConventionsAppliedTests
    {
        SqliteGenerator generator;
		string table = "Table";
		private string oldTable = "OldTable";
		string newTable = "NewTable";

		string column = "Column";
        string newColumn = "NewColumn";

		string indexName = "indexed-column";
		string indexColumn = "IndexColumn";

        public SqliteGeneratorWithConventionsAppliedTests()
		{
			generator = new SqliteGenerator();
		}

		[Test]
		public void CanCreateTable()
		{
			var expression = GetCreateTableExpression();
		    ApplyDefaultConventions(expression);
			var sql = generator.Generate(expression);
			sql.ShouldBe(string.Format("CREATE TABLE [{0}] (NewColumn TEXT NOT NULL)", table));
		}

        [Test]
        public void CanCreateTableWithAutoIncrementPrimaryKey()
        {
            var expression = GetCreateTableWithPrimaryKeyIdentityExpression();
            ApplyDefaultConventions(expression);
            var sql = generator.Generate(expression);
            sql.ShouldBe(string.Format("CREATE TABLE [{0}] (NewColumn INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT)", table));
        }

        [Test]
        public void CanCreateTableWithMultipartPrimaryKey()
        {
            var expression = GetCreateTableExpression();
            expression.Columns.Add(new ColumnDefinition{Name = "OtherColumn",Type = DbType.String, TableName = expression.TableName});
            expression.Columns[0].IsPrimaryKey = true;
            expression.Columns[1].IsPrimaryKey = true;

            ApplyDefaultConventions(expression);

            var sql = generator.Generate(expression);
            sql.ShouldBe(string.Format("CREATE TABLE [{0}] (NewColumn TEXT NOT NULL, OtherColumn TEXT NOT NULL, CONSTRAINT PK_{0} PRIMARY KEY (NewColumn,OtherColumn))", table));
        }

	    [Test]
		public void CanRenameTable()
		{
			var expression = new RenameTableExpression { OldName = oldTable, NewName = newTable };
            ApplyDefaultConventions(expression);
			var sql = generator.Generate(expression);
			sql.ShouldBe(string.Format("ALTER TABLE [{0}] RENAME TO [{1}]", oldTable, newTable));
		}

		[Test]
		public void CanDeleteTable()
		{
			var expression = new DeleteTableExpression { TableName = table };
            ApplyDefaultConventions(expression);
			var sql = generator.Generate(expression);
			sql.ShouldBe(string.Format("DROP TABLE [{0}]", table));
		}

		[Test]
		public void CanCreateColumn()
		{
			var expression = GetCreateColumnExpression();
            ApplyDefaultConventions(expression);
			var sql = generator.Generate(expression);
			sql.ShouldBe(string.Format("ALTER TABLE [{0}] ADD COLUMN {1} TEXT NOT NULL", table, newColumn));
		}

		[Test]
		public void CanAddDecimalColumn()
		{
			var tableName = "NewTable";
			var columnDefinition = new ColumnDefinition {Name = "NewColumn", Size = 19, Precision = 2, Type = DbType.Decimal};
		    var expression = new CreateColumnExpression {Column = columnDefinition, TableName = tableName};

            ApplyDefaultConventions(expression);
		    var sql = generator.Generate(expression);
			sql.ShouldBe("ALTER TABLE [NewTable] ADD COLUMN NewColumn NUMERIC NOT NULL");
		}

		[Test]
		public void CanCreateAutoIncrementColumn()
		{
			var expression = GetCreateAutoIncrementColumnExpression();
            ApplyDefaultConventions(expression);
			var sql = generator.Generate(expression);
			sql.ShouldBe(string.Format("ALTER TABLE [{0}] ADD COLUMN {1} INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT", table, newColumn));
		}

		[Test]
		public void CanDeleteColumn()
		{
			var expression = new DeleteColumnExpression { TableName = table, ColumnName = column };
            ApplyDefaultConventions(expression);
			var sql = generator.Generate(expression);
			sql.ShouldBe(string.Format("ALTER TABLE [{0}] DROP COLUMN {1}", table, column));
		}

		[Test]
		public void CanCreateBasicIndex()
		{
			var expression = GetCreateIndexExpression();
            ApplyDefaultConventions(expression);
			var sql = generator.Generate(expression);
			sql.ShouldBe(string.Format("CREATE INDEX IF NOT EXISTS {0} ON {1} ({2})", indexName, table, indexColumn));
		}

        [Test]
		public void CanInsertData()
		{
			var expression = new InsertDataExpression {TableName = "TestTable"};
		    expression.Rows.Add(new InsertionDataDefinition { new KeyValuePair<string, object>("Id", 1), 
													new KeyValuePair<string, object>("Name", "Justin"),
													new KeyValuePair<string, object>("Website", "codethinked.com") });
			expression.Rows.Add(new InsertionDataDefinition { new KeyValuePair<string, object>("Id", 2), 
													new KeyValuePair<string, object>("Name", "Nate"),
													new KeyValuePair<string, object>("Website", "kohari.org") });
            
            ApplyDefaultConventions(expression);
			
            var sql = generator.Generate(expression);

			var expected = "INSERT INTO [TestTable] (Id,Name,Website) VALUES (1,'Justin','codethinked.com');";
			expected += "INSERT INTO [TestTable] (Id,Name,Website) VALUES (2,'Nate','kohari.org');";

			sql.ShouldBe(expected);
		}
        
        private void ApplyDefaultConventions(IMigrationExpression expression)
        {
            expression.ApplyConventions(new MigrationConventions());
        }
        
        private CreateIndexExpression GetCreateIndexExpression()
		{
			IndexColumnDefinition indexColumnDefinition = new IndexColumnDefinition { Name = indexColumn };
			IndexDefinition indexDefinition = new IndexDefinition { TableName = table, Name = indexName, Columns = new List<IndexColumnDefinition> { indexColumnDefinition } };
			return new CreateIndexExpression { Index = indexDefinition };
		}

		private CreateColumnExpression GetCreateColumnExpression()
		{
			ColumnDefinition column = new ColumnDefinition { Name = newColumn, Type = DbType.String };
			return new CreateColumnExpression { TableName = table, Column = column };
		}

		private CreateColumnExpression GetCreateAutoIncrementColumnExpression()
		{
			var column = new ColumnDefinition { Name = newColumn, IsIdentity = true, IsPrimaryKey = true, Type = DbType.String };
			return new CreateColumnExpression { TableName = table, Column = column };
		}

        private CreateTableExpression GetCreateTableWithPrimaryKeyIdentityExpression()
        {
            var expression = new CreateTableExpression {TableName = table};
            expression.Columns.Add(new ColumnDefinition { Name = newColumn, IsIdentity = true, IsPrimaryKey = true, Type = DbType.String, TableName = table});
            return expression;  
        }

		private CreateTableExpression GetCreateTableExpression()
		{
			CreateTableExpression expression = new CreateTableExpression() { TableName = table, };
			expression.Columns.Add(new ColumnDefinition { Name = newColumn, Type = DbType.String, TableName = table});
			return expression;
		}
    }
}