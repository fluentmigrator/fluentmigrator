using System;
using System.Collections.Generic;
using System.Data;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Postgres;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.Postgres
{
    [TestFixture]
    public class PostgresGeneratorTests
    {
        private readonly PostgresGenerator _generator;

        public PostgresGeneratorTests()
        {
            _generator = new PostgresGenerator();
        }

        [Test]
        public void CanDropSchema()
        {
            var expression = new DeleteSchemaExpression { SchemaName = "Schema1" };
            var sql = _generator.Generate(expression);
            sql.ShouldBe("DROP SCHEMA \"Schema1\"");
        }

        [Test]
        public void CanCreateTableInSchema()
        {
            const string tableName = "NewTable";
            CreateTableExpression expression = GetCreateTableExpression(tableName);
            expression.SchemaName = "wibble";
            string sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE \"wibble\".\"NewTable\" (\"ColumnName1\" text NOT NULL, \"ColumnName2\" integer NOT NULL)");
        }

        [Test]
        public void CanCreateTableWithBoolDefaultValue()
        {
            const string tableName = "NewTable";
            CreateTableExpression expression = GetCreateTableExpression(tableName);
            expression.Columns[0].DefaultValue = true;
            string sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE \"public\".\"NewTable\" (\"ColumnName1\" text NOT NULL DEFAULT true, \"ColumnName2\" integer NOT NULL)");
        }

        [Test]
        public void CanDropTable()
        {
            const string tableName = "NewTable";
            DeleteTableExpression expression = GetDeleteTableExpression(tableName);
            string sql = _generator.Generate(expression);
            sql.ShouldBe("DROP TABLE \"public\".\"NewTable\"");
        }

        [Test]
        public void CanDropTableInSchema()
        {
            const string tableName = "NewTable";
            DeleteTableExpression expression = GetDeleteTableExpression(tableName);
            expression.SchemaName = "wibble";
            string sql = _generator.Generate(expression);
            sql.ShouldBe("DROP TABLE \"wibble\".\"NewTable\"");
        }

        [Test]
        public void CanDropColumn()
        {
            const string tableName = "NewTable";
            const string columnName = "NewColumn";

            var expression = new DeleteColumnExpression {TableName = tableName, ColumnName = columnName};

            string sql = _generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"public\".\"NewTable\" DROP COLUMN \"NewColumn\"");
        }

        [Test]
        public void CanAddColumn()
        {
            const string tableName = "NewTable";

            var columnDefinition = new ColumnDefinition {Name = "NewColumn", Size = 5, Type = DbType.String};

            var expression = new CreateColumnExpression {Column = columnDefinition, TableName = tableName};

            string sql = _generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"public\".\"NewTable\" ADD \"NewColumn\" varchar(5) NOT NULL");
        }

        [Test]
        public void CanAddIdentityColumn()
        {
            const string tableName = "NewTable";

            var columnDefinition = new ColumnDefinition {Name = "id", IsIdentity = true, Type = DbType.Int32};

            var expression = new CreateColumnExpression {Column = columnDefinition, TableName = tableName};

            string sql = _generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"public\".\"NewTable\" ADD \"id\" serial NOT NULL");
        }

        [Test]
        public void CanAddIdentityColumnForInt64()
        {
            const string tableName = "NewTable";

            var columnDefinition = new ColumnDefinition {Name = "id", IsIdentity = true, Type = DbType.Int64};

            var expression = new CreateColumnExpression {Column = columnDefinition, TableName = tableName};

            string sql = _generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"public\".\"NewTable\" ADD \"id\" bigserial NOT NULL");
        }

        [Test]
        public void CanAddDecimalColumn()
        {
            const string tableName = "NewTable";

            var columnDefinition = new ColumnDefinition
                                       {
                                           Name = "NewColumn",
                                           Size = 19,
                                           Precision = 2,
                                           Type = DbType.Decimal
                                       };

            var expression = new CreateColumnExpression {Column = columnDefinition, TableName = tableName};

            string sql = _generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"public\".\"NewTable\" ADD \"NewColumn\" decimal(2,19) NOT NULL");
        }

        [Test]
        public void CanRenameTable()
        {
            var expression = new RenameTableExpression {OldName = "Table1", NewName = "Table2"};

            string sql = _generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"public\".\"Table1\" RENAME TO \"Table2\"");
        }

        [Test]
        public void CanRenameColumn()
        {
            var expression = new RenameColumnExpression
                                 {
                                     TableName = "Table1", OldName = "Column1", NewName = "Column2"
                                 };

            string sql = _generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"public\".\"Table1\" RENAME COLUMN \"Column1\" TO \"Column2\"");
        }

        [Test]
        public void CanDropIndex()
        {
            var expression = new DeleteIndexExpression
                                 {
                                     Index = {Name = "IX_TEST", TableName = "TEST_TABLE"}
                                 };

            string sql = _generator.Generate(expression);
            sql.ShouldBe("DROP INDEX \"public\".\"IX_TEST\"");
        }

        [Test]
        public void CanCreateForeignKey()
        {
            var expression = new CreateForeignKeyExpression
                                 {
                                     ForeignKey =
                                         {
                                             Name = "FK_Test",
                                             PrimaryTable = "TestPrimaryTable",
                                             ForeignTable = "TestForeignTable",
                                             PrimaryColumns = new[] {"Column1", "Column2"},
                                             ForeignColumns = new[] {"Column3", "Column4"}
                                         }
                                 };

            string sql = _generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"public\".\"TestForeignTable\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"Column3\",\"Column4\") REFERENCES \"public\".\"TestPrimaryTable\" (\"Column1\",\"Column2\")");
        }

        [Test]
        public void CanCreateForeignKeyToDifferentSchema()
        {
            var expression = new CreateForeignKeyExpression
                                 {
                                     ForeignKey =
                                         {
                                             Name = "FK_Test",
                                             PrimaryTable = "TestPrimaryTable",
                                             ForeignTable = "TestForeignTable",
                                             PrimaryColumns = new[] {"Column1", "Column2"},
                                             ForeignColumns = new[] {"Column3", "Column4"},
                                             PrimaryTableSchema = "wibble"
                                         }
                                 };

            string sql = _generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"public\".\"TestForeignTable\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"Column3\",\"Column4\") REFERENCES \"wibble\".\"TestPrimaryTable\" (\"Column1\",\"Column2\")");
        }

        [Test]
        public void CanCreateForeignKeyWithDeleteCascade()
        {
            var expression = new CreateForeignKeyExpression
                                 {
                                     ForeignKey =
                                         {
                                             Name = "FK_Test",
                                             PrimaryTable = "TestPrimaryTable",
                                             ForeignTable = "TestForeignTable",
                                             PrimaryColumns = new[] {"Column1", "Column2"},
                                             ForeignColumns = new[] {"Column3", "Column4"},
                                             OnDelete = Rule.Cascade
                                         }
                                 };

            string sql = _generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"public\".\"TestForeignTable\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"Column3\",\"Column4\") REFERENCES \"public\".\"TestPrimaryTable\" (\"Column1\",\"Column2\") ON DELETE CASCADE");
        }

        [Test]
        public void CanCreateForeignKeyWithUpdateSetNull()
        {
            var expression = new CreateForeignKeyExpression
                                 {
                                     ForeignKey =
                                         {
                                             Name = "FK_Test",
                                             PrimaryTable = "TestPrimaryTable",
                                             ForeignTable = "TestForeignTable",
                                             PrimaryColumns = new[] {"Column1", "Column2"},
                                             ForeignColumns = new[] {"Column3", "Column4"},
                                             OnUpdate = Rule.SetNull
                                         }
                                 };

            string sql = _generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"public\".\"TestForeignTable\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"Column3\",\"Column4\") REFERENCES \"public\".\"TestPrimaryTable\" (\"Column1\",\"Column2\") ON UPDATE SET NULL");
        }

        [Test]
        public void CanCreateForeignKeyWithUpdateAndDelete()
        {
            var expression = new CreateForeignKeyExpression
                                 {
                                     ForeignKey =
                                         {
                                             Name = "FK_Test",
                                             PrimaryTable = "TestPrimaryTable",
                                             ForeignTable = "TestForeignTable",
                                             PrimaryColumns = new[] {"Column1", "Column2"},
                                             ForeignColumns = new[] {"Column3", "Column4"},
                                             OnUpdate = Rule.SetNull,
                                             OnDelete = Rule.SetDefault
                                         }
                                 };

            string sql = _generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"public\".\"TestForeignTable\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"Column3\",\"Column4\") REFERENCES \"public\".\"TestPrimaryTable\" (\"Column1\",\"Column2\") ON DELETE SET DEFAULT ON UPDATE SET NULL");
        }

        [Test]
        public void CanDropForeignKey()
        {
            var expression = new DeleteForeignKeyExpression
                                 {
                                     ForeignKey = {Name = "FK_Test", ForeignTable = "TestPrimaryTable"}
                                 };

            string sql = _generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"public\".\"TestPrimaryTable\" DROP CONSTRAINT \"FK_Test\"");
        }

        [Test]
        public void CanInsertData()
        {
            var expression = new InsertDataExpression {TableName = "TestTable"};
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

            var sql = _generator.Generate(expression);

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

            var sql = _generator.Generate(expression);

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

            var sql = _generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"SOURCE\".\"TABLE\" SET SCHEMA \"DEST\"");
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
            var sql = _generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"Schema1\".\"Table1\" ALTER \"Col1\" TYPE text");
        }

        [Test]
        public void CanDeleteAllData()
        {
            var expression = new DeleteDataExpression
                                 {
                                     IsAllRows = true,
                                     TableName = "Table1"
                                 };

            var sql = _generator.Generate(expression);
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

            var sql = _generator.Generate(expression);
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

            var sql = _generator.Generate(expression);
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

            var sql = _generator.Generate(expression);
            sql.ShouldBe("DELETE FROM \"public\".\"Table1\" WHERE \"description\" IS NULL AND \"id\" = 10;");
        }

        private DeleteTableExpression GetDeleteTableExpression(string tableName)
        {
            return new DeleteTableExpression { TableName = tableName };
        }

        private CreateTableExpression GetCreateTableExpression(string tableName)
        {
            const string columnName1 = "ColumnName1";
            const string columnName2 = "ColumnName2";

            var column1 = new ColumnDefinition { Name = columnName1, Type = DbType.String, TableName = tableName };
            var column2 = new ColumnDefinition { Name = columnName2, Type = DbType.Int32, TableName = tableName };

            var expression = new CreateTableExpression { TableName = tableName };
            expression.Columns.Add(column1);
            expression.Columns.Add(column2);
            return expression;
        }
    }
}