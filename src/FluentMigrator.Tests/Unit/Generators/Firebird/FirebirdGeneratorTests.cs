using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Should;
using FluentMigrator.Runner.Processors.Firebird;
using FluentMigrator.Runner.Generators.Firebird;
using FluentMigrator.Expressions;
using System.Data;
using FluentMigrator.Model;

namespace FluentMigrator.Tests.Unit.Generators.Firebird
{
    [TestFixture]
    public class FirebirdGeneratorTests
    {
        private FirebirdGenerator generator;

        public FirebirdGeneratorTests()
        {
            generator = new FirebirdGenerator(FirebirdOptions.StandardBehaviour());
        }

        [Test]
        public void CanCreateTable()
        {
            string tableName = "NewTable";
            CreateTableExpression expression = GetCreateTableExpression(tableName);
            string sql = generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE \"NewTable\" (\"ColumnName1\" BLOB SUB_TYPE TEXT NOT NULL, \"ColumnName2\" INTEGER NOT NULL)");
        }

        [Test]
        public void CanCreateTableInSchema()
        {
            string tableName = "NewTable";
            CreateTableExpression expression = GetCreateTableExpression(tableName);
            expression.SchemaName = "wibble";
            string sql = generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE \"NewTable\" (\"ColumnName1\" BLOB SUB_TYPE TEXT NOT NULL, \"ColumnName2\" INTEGER NOT NULL)");
        }

        [Test]
        public void CanCreateTableWithPrimaryKey()
        {
            string tableName = "NewTable";
            CreateTableExpression expression = GetCreateTableExpression(tableName);
            expression.Columns[0].IsPrimaryKey = true;
            string sql = generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE \"NewTable\" (\"ColumnName1\" BLOB SUB_TYPE TEXT NOT NULL, \"ColumnName2\" INTEGER NOT NULL, PRIMARY KEY (\"ColumnName1\"))");
        }

        [Test]
        public void CanCreateTableWithPrimaryKeyNamed()
        {
            string tableName = "NewTable";
            CreateTableExpression expression = GetCreateTableExpression(tableName);
            expression.Columns[0].IsPrimaryKey = true;
            expression.Columns[0].PrimaryKeyName = "PK_NewTable";
            string sql = generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE \"NewTable\" (\"ColumnName1\" BLOB SUB_TYPE TEXT NOT NULL, \"ColumnName2\" INTEGER NOT NULL, CONSTRAINT \"PK_NewTable\" PRIMARY KEY (\"ColumnName1\"))");
        }

        [Test]
        public void CanCreateTableWithDefaultValue()
        {
            string tableName = "NewTable";
            CreateTableExpression expression = GetCreateTableExpression(tableName);
            expression.Columns[0].DefaultValue = "abc";
            string sql = generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE \"NewTable\" (\"ColumnName1\" BLOB SUB_TYPE TEXT DEFAULT 'abc' NOT NULL, \"ColumnName2\" INTEGER NOT NULL)");
        }

        [Test]
        public void CanCreateTableWithDefaultValueExplicitlySetToNull()
        {
            string tableName = "NewTable";
            var expression = GetCreateTableExpression(tableName);
            expression.Columns[0].DefaultValue = null;
            var sql = generator.Generate(expression);
            sql.ShouldBe(
                "CREATE TABLE \"NewTable\" (\"ColumnName1\" BLOB SUB_TYPE TEXT DEFAULT NULL NOT NULL, \"ColumnName2\" INTEGER NOT NULL)");

        }

        [Test]
        public void CanCreateTableWithMultiColumnPrimaryKey()
        {
            string tableName = "NewTable";
            CreateTableExpression expression = GetCreateTableExpression(tableName);
            expression.Columns[0].IsPrimaryKey = true;
            expression.Columns[1].IsPrimaryKey = true;
            string sql = generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE \"NewTable\" (\"ColumnName1\" BLOB SUB_TYPE TEXT NOT NULL, \"ColumnName2\" INTEGER NOT NULL, PRIMARY KEY (\"ColumnName1\", \"ColumnName2\"))");
        }

        [Test]
        public void CanCreateTableWithMultiColumnPrimaryKeyNamed()
        {
            string tableName = "NewTable";
            CreateTableExpression expression = GetCreateTableExpression(tableName);
            expression.Columns[0].IsPrimaryKey = true;
            expression.Columns[0].PrimaryKeyName = "wibble";
            expression.Columns[1].IsPrimaryKey = true;
            string sql = generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE \"NewTable\" (\"ColumnName1\" BLOB SUB_TYPE TEXT NOT NULL, \"ColumnName2\" INTEGER NOT NULL, CONSTRAINT \"wibble\" PRIMARY KEY (\"ColumnName1\", \"ColumnName2\"))");
        }

        [Test]
        public void CanDropTable()
        {
            string tableName = "NewTable";
            DeleteTableExpression expression = GetDeleteTableExpression(tableName);
            string sql = generator.Generate(expression);
            sql.ShouldBe("DROP TABLE \"NewTable\"");
        }

        [Test]
        public void CanDropTableInSchema()
        {
            string tableName = "NewTable";
            DeleteTableExpression expression = GetDeleteTableExpression(tableName);
            expression.SchemaName = "wibble";
            string sql = generator.Generate(expression);
            sql.ShouldBe("DROP TABLE \"NewTable\"");
        }

        [Test]
        public void CanDropColumn()
        {
            string tableName = "NewTable";
            string columnName = "NewColumn";

            var expression = new DeleteColumnExpression();
            expression.TableName = tableName;
            expression.ColumnNames.Add(columnName);

            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"NewTable\" DROP \"NewColumn\"");
        }

        [Test]
        public void CanDropMultipleColumns()
        {
            var expression = new DeleteColumnExpression();
            expression.TableName = "NewTable";
            expression.ColumnNames.Add("NewColumn");
            expression.ColumnNames.Add("OtherColumn");

            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"NewTable\" DROP \"NewColumn\";\r\n" + 
                "ALTER TABLE \"NewTable\" DROP \"OtherColumn\"");
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
            sql.ShouldBe("ALTER TABLE \"NewTable\" ADD \"NewColumn\" VARCHAR(5) NOT NULL");
        }
        
        [Test]
        public void CanAddDecimalColumn()
        {
            string tableName = "NewTable";

            var columnDefinition = new ColumnDefinition();
            columnDefinition.Name = "NewColumn";
            columnDefinition.Size = 5;
            columnDefinition.Precision = 2;
            columnDefinition.Type = DbType.Decimal;

            var expression = new CreateColumnExpression();
            expression.Column = columnDefinition;
            expression.TableName = tableName;

            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"NewTable\" ADD \"NewColumn\" DECIMAL(2,5) NOT NULL");
        }

        [Test]
        public void CanRenameTable()
        {
            var expression = new RenameTableExpression();
            expression.OldName = "Table1";
            expression.NewName = "Table2";

            string sql = generator.Generate(expression);
            sql.ShouldBe(String.Empty);
        }

        [Test]
        public void CanRenameColumn()
        {
            var expression = new RenameColumnExpression();
            expression.TableName = "Table1";
            expression.OldName = "Column1";
            expression.NewName = "Column2";

            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"Table1\" ALTER COLUMN \"Column1\" TO \"Column2\"");
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
            sql.ShouldBe("CREATE ASC INDEX \"IX_TEST\" ON \"TEST_TABLE\" (\"Column1\", \"Column2\")");
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
            sql.ShouldBe("CREATE UNIQUE ASC INDEX \"IX_TEST\" ON \"TEST_TABLE\" (\"Column1\", \"Column2\")");
        }

        [Test]
        public void CanDropIndex()
        {
            var expression = new DeleteIndexExpression();
            expression.Index.Name = "IX_TEST";
            expression.Index.TableName = "TEST_TABLE";

            string sql = generator.Generate(expression);
            sql.ShouldBe("DROP INDEX \"IX_TEST\"");
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
            sql.ShouldBe("ALTER TABLE \"TestForeignTable\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"Column3\", \"Column4\") REFERENCES \"TestPrimaryTable\" (\"Column1\", \"Column2\")");
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
            sql.ShouldBe("ALTER TABLE \"TestForeignTable\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"Column3\", \"Column4\") REFERENCES \"TestPrimaryTable\" (\"Column1\", \"Column2\")");
        }

        [TestCase(Rule.SetDefault, "SET DEFAULT"), TestCase(Rule.SetNull, "SET NULL"), TestCase(Rule.Cascade, "CASCADE")]
        public void CanCreateForeignKeyWithOnUpdateOptions(Rule rule, string output) 
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
            expression.ForeignKey.OnUpdate = rule;
            var sql = generator.Generate(expression);
            sql.ShouldBe(
                string.Format("ALTER TABLE \"TestTable1\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"TestColumn1\") REFERENCES \"TestTable2\" (\"TestColumn2\") ON UPDATE {0}", output));
        }

        [TestCase(Rule.SetDefault, "SET DEFAULT"), TestCase(Rule.SetNull, "SET NULL"), TestCase(Rule.Cascade, "CASCADE")]
        public void CanCreateForeignKeyWithOnDeleteOptions(Rule rule, string output) 
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
            expression.ForeignKey.OnDelete = rule;
            var sql = generator.Generate(expression);
            sql.ShouldBe(
                string.Format(
                    "ALTER TABLE \"TestTable1\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"TestColumn1\") REFERENCES \"TestTable2\" (\"TestColumn2\") ON DELETE {0}",
                    output));
        }

        [Test]
        public void CanCreateForeignKeyWithOnDeleteAndOnUpdateOptions() 
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
            expression.ForeignKey.OnDelete = Rule.Cascade;
            expression.ForeignKey.OnUpdate = Rule.SetDefault;
            var sql = generator.Generate(expression);
            sql.ShouldBe(
                "ALTER TABLE \"TestTable1\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"TestColumn1\") REFERENCES \"TestTable2\" (\"TestColumn2\") ON DELETE CASCADE ON UPDATE SET DEFAULT");
        }

        [Test]
        public void CanCreateForeignKeyWithMultipleColumns() 
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
            expression.ForeignKey.PrimaryColumns = new[] { "Column1", "Column2" };
            expression.ForeignKey.ForeignColumns = new[] { "Column3", "Column4" };

            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"TestTable1\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"Column3\", \"Column4\") REFERENCES \"TestTable2\" (\"Column1\", \"Column2\")");
        }

        [Test]
        public void CanDropForeignKey()
        {
            var expression = new DeleteForeignKeyExpression();
            expression.ForeignKey.Name = "FK_Test";
            expression.ForeignKey.ForeignTable = "TestPrimaryTable";

            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"TestPrimaryTable\" DROP CONSTRAINT \"FK_Test\"");
        }

        [Test]
        public void CanInsertData()
        {
            var expression = new InsertDataExpression();
            expression.TableName = "TestTable";
            expression.Rows.Add(new ExplicitDataDefinition
                                    (
                                        new DataValue("Id", 1),
                                        new DataValue("Name", "Just'in"),
                                        new DataValue("Website", "codethinked.com")
                                    ));
            expression.Rows.Add(new ExplicitDataDefinition
                                    (
                                        new DataValue("Id", 2),
                                        new DataValue("Name", "Na\\te"),
                                        new DataValue("Website", "kohari.org")
                                    ));

            var sql = generator.Generate(expression);

            var expected = "INSERT INTO \"TestTable\" (\"Id\", \"Name\", \"Website\") VALUES (1, 'Just''in', 'codethinked.com');";
            expected += "\r\nINSERT INTO \"TestTable\" (\"Id\", \"Name\", \"Website\") VALUES (2, 'Na\\te', 'kohari.org')";

            sql.ShouldBe(expected);
        }

        [Test]
        public void CanInsertGuidData()
        {
            var gid = Guid.NewGuid();
            var expression = new InsertDataExpression { TableName = "TestTable" };
            expression.Rows.Add(new ExplicitDataDefinition(new DataValue("guid", gid)));

            var sql = generator.Generate(expression);

            var expected = String.Format("INSERT INTO \"TestTable\" (\"guid\") VALUES ('{0}')", gid);

            sql.ShouldBe(expected);
        }

        [Test]
        public void CanAlterColumn()
        {
            var expression = new AlterColumnExpression
                                 {
                                     Column = new ColumnDefinition { Type = DbType.String, Name = "Col1" },
                                     SchemaName = "Schema1",
                                     TableName = "Table1"
                                 };
            var sql = generator.Generate(expression);
            sql.ShouldBe(String.Empty);
        }

        [Test]
        public void CanDeleteAllData()
        {
            var expression = new DeleteDataExpression
                                 {
                                     IsAllRows = true,
                                     TableName = "Table1"
                                 };

            var sql = generator.Generate(expression);
            sql.ShouldBe("DELETE FROM \"Table1\" WHERE 1 = 1");

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

            expression.Rows.Add(new ExplicitDataDefinition(new DataValue("description", "wibble")));

            var sql = generator.Generate(expression);
            sql.ShouldBe("DELETE FROM \"Table1\" WHERE \"description\" = 'wibble'");
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

            expression.Rows.Add(new ExplicitDataDefinition(new DataValue("description", null)));

            var sql = generator.Generate(expression);
            sql.ShouldBe("DELETE FROM \"Table1\" WHERE \"description\" IS NULL");
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

            expression.Rows.Add(new ExplicitDataDefinition(new DataValue("description", null), new DataValue("id", 10)));

            var sql = generator.Generate(expression);
            sql.ShouldBe("DELETE FROM \"Table1\" WHERE \"description\" IS NULL AND \"id\" = 10");
        }

        [Test]
        public void CanCreateSequence()
        {
            var expression = new CreateSequenceExpression
                             {
                                 Sequence =
                                         {
                                             Cache = 10,
                                             Cycle = true,
                                             Increment = 2,
                                             MaxValue = 100,
                                             MinValue = 0,
                                             Name = "Sequence",
                                             SchemaName = "Schema",
                                             StartWith = 2
                                         }
                             };
            var sql = generator.Generate(expression);
            sql.ShouldBe("CREATE SEQUENCE \"Sequence\"");
        }

        [Test]
        public void CanAlterSequence()
        {
            var expression = new CreateSequenceExpression
            {
                Sequence =
                {
                    Cache = 10,
                    Cycle = true,
                    Increment = 2,
                    MaxValue = 100,
                    MinValue = 0,
                    Name = "Sequence",
                    SchemaName = "Schema",
                    StartWith = 2
                }
            };
            var sql = generator.GenerateAlterSequence(expression.Sequence);
            sql.ShouldBe("ALTER SEQUENCE \"Sequence\" RESTART WITH 2");
        }

        [Test]
        public void CanDeleteSequence()
        {
            var expression = new DeleteSequenceExpression { SchemaName = "Schema", SequenceName = "Sequence" };
            var sql = generator.Generate(expression);
            sql.ShouldBe("DROP SEQUENCE \"Sequence\"");
        }

        private DeleteTableExpression GetDeleteTableExpression(string tableName)
        {
            return new DeleteTableExpression { TableName = tableName };
        }

        private CreateTableExpression GetCreateTableExpression(string tableName)
        {
            string columnName1 = "ColumnName1";
            string columnName2 = "ColumnName2";

            var column1 = new ColumnDefinition { Name = columnName1, Type = DbType.String, TableName = tableName };
            var column2 = new ColumnDefinition { Name = columnName2, Type = DbType.Int32, TableName = tableName };

            var expression = new CreateTableExpression { TableName = tableName };
            expression.Columns.Add(column1);
            expression.Columns.Add(column2);
            return expression;
        }
    }
}
