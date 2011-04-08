using System;
using System.Collections.Generic;
using System.Data;
using FluentMigrator.Builders.Insert;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators
{
	[TestFixture]
	public class PostgresGeneratorTests
	{
        private PostgresGenerator generator;

        public PostgresGeneratorTests()
		{
			generator = new PostgresGenerator();
		}

        [Test]
        public void CanCreateSchema()
        {
            var expression = new CreateSchemaExpression {SchemaName = "Schema1"};
            var sql = generator.Generate(expression);
            sql.ShouldBe("CREATE SCHEMA \"Schema1\"");
        }

        [Test]
        public void CanDropSchema()
        {
            var expression = new DeleteSchemaExpression() { SchemaName = "Schema1" };
            var sql = generator.Generate(expression);
            sql.ShouldBe("DROP SCHEMA \"Schema1\"");
        }

		[Test]
		public void CanCreateTable()
		{
			string tableName = "NewTable";
			CreateTableExpression expression = GetCreateTableExpression(tableName);
			string sql = generator.Generate(expression);
			sql.ShouldBe("CREATE TABLE \"public\".\"NewTable\" (\"ColumnName1\" text NOT NULL, \"ColumnName2\" integer NOT NULL)");
		}

        [Test]
        public void CanCreateTableInSchema()
        {
            string tableName = "NewTable";
            CreateTableExpression expression = GetCreateTableExpression(tableName);
            expression.SchemaName = "wibble";
            string sql = generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE \"wibble\".\"NewTable\" (\"ColumnName1\" text NOT NULL, \"ColumnName2\" integer NOT NULL)");
        }
        
		[Test]
		public void CanCreateTableWithPrimaryKey()
		{
			string tableName = "NewTable";
			CreateTableExpression expression = GetCreateTableExpression(tableName);
			expression.Columns[0].IsPrimaryKey = true;
			string sql = generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE \"public\".\"NewTable\" (\"ColumnName1\" text NOT NULL , CONSTRAINT \"PK_NewTable\" PRIMARY KEY (\"ColumnName1\"), \"ColumnName2\" integer NOT NULL)");
		}

		[Test]
		public void CanCreateTableWithDefaultValue()
		{
			string tableName = "NewTable";
			CreateTableExpression expression = GetCreateTableExpression(tableName);
			expression.Columns[0].DefaultValue = "abc";
			string sql = generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE \"public\".\"NewTable\" (\"ColumnName1\" text NOT NULL DEFAULT 'abc', \"ColumnName2\" integer NOT NULL)");
		}

        [Test]
        public void CanCreateTableWithBoolDefaultValue()
        {
            string tableName = "NewTable";
            CreateTableExpression expression = GetCreateTableExpression(tableName);
            expression.Columns[0].DefaultValue = true;
            string sql = generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE \"public\".\"NewTable\" (\"ColumnName1\" text NOT NULL DEFAULT true, \"ColumnName2\" integer NOT NULL)");
        }

        [Test]
        public void CanCreateTableWithDefaultValueExplicitlySetToNull()
        {
            string tableName = "NewTable";
            var expression = GetCreateTableExpression(tableName);
            expression.Columns[0].DefaultValue = null;
            var sql = generator.Generate(expression);
            sql.ShouldBe(
                "CREATE TABLE \"public\".\"NewTable\" (\"ColumnName1\" text NOT NULL DEFAULT NULL, \"ColumnName2\" integer NOT NULL)");

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
            sql.ShouldBe("CREATE TABLE \"public\".\"NewTable\" (\"ColumnName1\" text NOT NULL, \"ColumnName2\" integer NOT NULL, CONSTRAINT \"ColumnName1_ColumnName2_PK\" PRIMARY KEY (\"ColumnName1\",\"ColumnName2\"))");
		}

		[Test]
		public void CanDropTable()
		{
			string tableName = "NewTable";
			DeleteTableExpression expression = GetDeleteTableExpression(tableName);
			string sql = generator.Generate(expression);
			sql.ShouldBe("DROP TABLE \"public\".\"NewTable\"");
		}

        [Test]
        public void CanDropTableInSchema()
        {
            string tableName = "NewTable";
            DeleteTableExpression expression = GetDeleteTableExpression(tableName);
            expression.SchemaName = "wibble";
            string sql = generator.Generate(expression);
            sql.ShouldBe("DROP TABLE \"wibble\".\"NewTable\"");
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
			sql.ShouldBe("ALTER TABLE \"public\".\"NewTable\" DROP COLUMN \"NewColumn\"");
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
			sql.ShouldBe("ALTER TABLE \"public\".\"NewTable\" ADD \"NewColumn\" varchar(5) NOT NULL");
		}

        [Test]
        public void CanAddIdentityColumn()
        {
            string tableName = "NewTable";

            var columnDefinition = new ColumnDefinition();
            columnDefinition.Name = "id";
            columnDefinition.IsIdentity=true;
            columnDefinition.Type = DbType.Int32;

            var expression = new CreateColumnExpression();
            expression.Column = columnDefinition;
            expression.TableName = tableName;

            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"public\".\"NewTable\" ADD \"id\" serial NOT NULL");
        }

        [Test]
        public void CanAddIdentityColumnForInt64()
        {
            string tableName = "NewTable";

            var columnDefinition = new ColumnDefinition();
            columnDefinition.Name = "id";
            columnDefinition.IsIdentity = true;
            columnDefinition.Type = DbType.Int64;

            var expression = new CreateColumnExpression();
            expression.Column = columnDefinition;
            expression.TableName = tableName;

            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"public\".\"NewTable\" ADD \"id\" bigserial NOT NULL");
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
			sql.ShouldBe("ALTER TABLE \"public\".\"NewTable\" ADD \"NewColumn\" decimal(2,19) NOT NULL");
		}

		[Test]
		public void CanRenameTable()
		{
			var expression = new RenameTableExpression();
			expression.OldName = "Table1";
			expression.NewName = "Table2";

			string sql = generator.Generate(expression);
			sql.ShouldBe("ALTER TABLE \"public\".\"Table1\" RENAME TO \"Table2\"");
		}

		[Test]
		public void CanRenameColumn()
		{
			var expression = new RenameColumnExpression();
			expression.TableName = "Table1";
			expression.OldName = "Column1";
			expression.NewName = "Column2";

			string sql = generator.Generate(expression);
			sql.ShouldBe("ALTER TABLE \"public\".\"Table1\" RENAME COLUMN \"Column1\" TO \"Column2\"");
		}

		[Test]
		public void CanCreateIndex()
		{
			var expression = new CreateIndexExpression();
			expression.Index.Name = "IX_TEST";
			expression.Index.TableName = "TEST_TABLE";
			expression.Index.Columns.Add(new IndexColumnDefinition { Direction = Direction.Ascending, Name = "Column1" });
			expression.Index.Columns.Add(new IndexColumnDefinition { Direction = Direction.Descending, Name = "Column2" });

			string sql = generator.Generate(expression);
			sql.ShouldBe("CREATE INDEX \"IX_TEST\" ON \"public\".\"TEST_TABLE\" (\"Column1\" ASC,\"Column2\" DESC)");
		}

        [Test]
        public void CanCreateUniqueIndex()
        {
            var expression = new CreateIndexExpression();
            expression.Index.Name = "IX_TEST";
            expression.Index.TableName = "TEST_TABLE";
            expression.Index.IsUnique = true;
            expression.Index.Columns.Add(new IndexColumnDefinition { Direction = Direction.Ascending, Name = "Column1" });
            expression.Index.Columns.Add(new IndexColumnDefinition { Direction = Direction.Descending, Name = "Column2" });

            string sql = generator.Generate(expression);
            sql.ShouldBe("CREATE UNIQUE INDEX \"IX_TEST\" ON \"public\".\"TEST_TABLE\" (\"Column1\" ASC,\"Column2\" DESC)");
        }

        [Test]
        public void CanDropIndex()
        {
            var expression = new DeleteIndexExpression();
            expression.Index.Name = "IX_TEST";
            expression.Index.TableName = "TEST_TABLE";
         
            string sql = generator.Generate(expression);
            sql.ShouldBe("DROP INDEX \"public\".\"IX_TEST\"");
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
			sql.ShouldBe("ALTER TABLE \"public\".\"TestForeignTable\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"Column3\",\"Column4\") REFERENCES \"public\".\"TestPrimaryTable\" (\"Column1\",\"Column2\")");
		}

        [Test]
        public void CanCreateForeignKeyToDifferentSchema()
        {
            var expression = new CreateForeignKeyExpression();
            expression.ForeignKey.Name = "FK_Test";
            expression.ForeignKey.PrimaryTable = "TestPrimaryTable";
            expression.ForeignKey.ForeignTable = "TestForeignTable";
            expression.ForeignKey.PrimaryColumns = new[] { "Column1", "Column2" };
            expression.ForeignKey.ForeignColumns = new[] { "Column3", "Column4" };
            expression.ForeignKey.PrimaryTableSchema = "wibble";

            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"public\".\"TestForeignTable\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"Column3\",\"Column4\") REFERENCES \"wibble\".\"TestPrimaryTable\" (\"Column1\",\"Column2\")");
        }

        [Test]
        public void CanCreateForeignKeyWithDeleteCascade()
        {
            var expression = new CreateForeignKeyExpression();
            expression.ForeignKey.Name = "FK_Test";
            expression.ForeignKey.PrimaryTable = "TestPrimaryTable";
            expression.ForeignKey.ForeignTable = "TestForeignTable";
            expression.ForeignKey.PrimaryColumns = new[] { "Column1", "Column2" };
            expression.ForeignKey.ForeignColumns = new[] { "Column3", "Column4" };
            expression.ForeignKey.OnDelete = Rule.Cascade;

            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"public\".\"TestForeignTable\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"Column3\",\"Column4\") REFERENCES \"public\".\"TestPrimaryTable\" (\"Column1\",\"Column2\") ON DELETE CASCADE");
        }

        [Test]
        public void CanCreateForeignKeyWithUpdateSetNull()
        {
            var expression = new CreateForeignKeyExpression();
            expression.ForeignKey.Name = "FK_Test";
            expression.ForeignKey.PrimaryTable = "TestPrimaryTable";
            expression.ForeignKey.ForeignTable = "TestForeignTable";
            expression.ForeignKey.PrimaryColumns = new[] { "Column1", "Column2" };
            expression.ForeignKey.ForeignColumns = new[] { "Column3", "Column4" };
            expression.ForeignKey.OnUpdate = Rule.SetNull;

            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"public\".\"TestForeignTable\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"Column3\",\"Column4\") REFERENCES \"public\".\"TestPrimaryTable\" (\"Column1\",\"Column2\") ON UPDATE SET NULL");
        }

        [Test]
        public void CanCreateForeignKeyWithUpdateAndDelete()
        {
            var expression = new CreateForeignKeyExpression();
            expression.ForeignKey.Name = "FK_Test";
            expression.ForeignKey.PrimaryTable = "TestPrimaryTable";
            expression.ForeignKey.ForeignTable = "TestForeignTable";
            expression.ForeignKey.PrimaryColumns = new[] { "Column1", "Column2" };
            expression.ForeignKey.ForeignColumns = new[] { "Column3", "Column4" };
            expression.ForeignKey.OnUpdate = Rule.SetNull;
            expression.ForeignKey.OnDelete = Rule.SetDefault;

            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"public\".\"TestForeignTable\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"Column3\",\"Column4\") REFERENCES \"public\".\"TestPrimaryTable\" (\"Column1\",\"Column2\") ON DELETE SET DEFAULT ON UPDATE SET NULL");
        }

		[Test]
		public void CanDropForeignKey()
		{
			var expression = new DeleteForeignKeyExpression();
			expression.ForeignKey.Name = "FK_Test";
			expression.ForeignKey.ForeignTable = "TestPrimaryTable";

			string sql = generator.Generate(expression);
			sql.ShouldBe("ALTER TABLE \"public\".\"TestPrimaryTable\" DROP CONSTRAINT \"FK_Test\"");
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
										new KeyValuePair<string, object>("Name", "Na\\te"),
										new KeyValuePair<string, object>("Website", "kohari.org")
									});

            var sql = generator.Generate(expression);

            var expected = "INSERT INTO \"public\".\"TestTable\" (\"Id\",\"Name\",\"Website\") VALUES (1,'Just''in','codethinked.com');";
            expected += "INSERT INTO \"public\".\"TestTable\" (\"Id\",\"Name\",\"Website\") VALUES (2,'Na\\te','kohari.org');";

            sql.ShouldBe(expected);
        }

        [Test]
        public void CanInsertGuidData()
        {
            var gid = Guid.NewGuid();
            var expression = new InsertDataExpression { TableName = "TestTable" };
            expression.Rows.Add(new InsertionDataDefinition { new KeyValuePair<string, object>("guid", gid) });

            var sql = generator.Generate(expression);

            var expected = String.Format("INSERT INTO \"public\".\"TestTable\" (\"guid\") VALUES ('{0}');", gid);

            sql.ShouldBe(expected);
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

            var sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"SOURCE\".\"TABLE\" SET SCHEMA \"DEST\"");
        }

        [Test]
        public void CanAlterColumn()
        {
            var expression = new AlterColumnExpression
                                 {
                                     Column = new ColumnDefinition {Type = DbType.String, Name = "Col1"},
                                     SchemaName = "Schema1",
                                     TableName = "Table1"
                                 };
            var sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"Schema1\".\"Table1\" ALTER \"Col1\" TYPE text");
        }

        [Test]
        public void CanDeleteAllData()
        {
            var expression = new DeleteDataExpression
                                 {
                                     IsAllRows=true, TableName = "Table1"
                                 };

            var sql = generator.Generate(expression);
            sql.ShouldBe("DELETE FROM \"public\".\"Table1\";");

        }

        [Test]
        public void CanDeleteAllDataWithCondition()
        {
            var expression = new DeleteDataExpression
            {
                IsAllRows = false,
                SchemaName = "public",
                TableName = "Table1"
            };
            expression.Rows.Add(
                new DeletionDataDefinition
                    {
                        new KeyValuePair<string, object>("description", "wibble")
                    });

            var sql = generator.Generate(expression);
            sql.ShouldBe("DELETE FROM \"public\".\"Table1\" WHERE \"description\" = 'wibble';");
        }

        [Test]
        public void CanDeleteAllDataWithNullCondition()
        {
            var expression = new DeleteDataExpression
            {
                IsAllRows = false,
                SchemaName = "public",
                TableName = "Table1"
            };
            expression.Rows.Add(
                new DeletionDataDefinition
                    {
                        new KeyValuePair<string, object>("description", null)
                    });

            var sql = generator.Generate(expression);
            sql.ShouldBe("DELETE FROM \"public\".\"Table1\" WHERE \"description\" IS NULL;");
        }

        [Test]
        public void CanDeleteAllDataWithMultipleConditions()
        {
            var expression = new DeleteDataExpression
            {
                IsAllRows = false,
                SchemaName = "public",
                TableName = "Table1"
            };
            expression.Rows.Add(new DeletionDataDefinition
                                    {
                                        new KeyValuePair<string, object>("description", null),
                                        new KeyValuePair<string, object>("id", 10)
                                    });

            var sql = generator.Generate(expression);
            sql.ShouldBe("DELETE FROM \"public\".\"Table1\" WHERE \"description\" IS NULL AND \"id\" = 10;");
        }

		private DeleteTableExpression GetDeleteTableExpression(string tableName)
		{
			return new DeleteTableExpression { TableName = tableName };
		}

		private CreateTableExpression GetCreateTableExpression(string tableName)
		{
			string columnName1 = "ColumnName1";
			string columnName2 = "ColumnName2";

			var column1 = new ColumnDefinition { Name = columnName1, Type = DbType.String, TableName = tableName};
            var column2 = new ColumnDefinition { Name = columnName2, Type = DbType.Int32, TableName = tableName };

			var expression = new CreateTableExpression { TableName = tableName };
			expression.Columns.Add(column1);
			expression.Columns.Add(column2);
			return expression;
		}
	}
}