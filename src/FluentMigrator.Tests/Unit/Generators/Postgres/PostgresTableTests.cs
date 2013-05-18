using System.Data;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Postgres;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.Postgres
{
    [TestFixture]
    public class PostgresTableTests
    {
        protected PostgresGenerator generator;

        [SetUp]
        public void Setup()
        {
            generator = new PostgresGenerator();
        }

        [Test]
        public void CanCreateTableWithCustomSchema()
        {
            string tableName = "NewTable";
            CreateTableExpression expression = GetCreateTableExpression(tableName);
            expression.SchemaName = "wibble";
            string sql = generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE \"wibble\".\"NewTable\" (\"ColumnName1\" text NOT NULL, \"ColumnName2\" integer NOT NULL)");
        }

        [Test]
        public void CanCreateTableWithDefaultSchema()
        {
            string tableName = "NewTable";
            CreateTableExpression expression = GetCreateTableExpression(tableName);
            string sql = generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE \"public\".\"NewTable\" (\"ColumnName1\" text NOT NULL, \"ColumnName2\" integer NOT NULL)");
        }

        [Test]
        public void CanCreateTableWithDefaultValueExplicitlySetToNullWithDefaultSchema()
        {
            string tableName = "NewTable";
            var expression = GetCreateTableExpression(tableName);
            expression.Columns[0].DefaultValue = null;
            var sql = generator.Generate(expression);
            sql.ShouldBe(
                "CREATE TABLE \"public\".\"NewTable\" (\"ColumnName1\" text NOT NULL DEFAULT NULL, \"ColumnName2\" integer NOT NULL)");

        }

        [Test]
        public void CanCreateTableWithDefaultValueWithDefaultSchema()
        {
            string tableName = "NewTable";
            CreateTableExpression expression = GetCreateTableExpression(tableName);
            expression.Columns[0].DefaultValue = "abc";
            string sql = generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE \"public\".\"NewTable\" (\"ColumnName1\" text NOT NULL DEFAULT 'abc', \"ColumnName2\" integer NOT NULL)");
        }

        [Test]
        public void CanCreateTableWithMultiColumnPrimaryKeyWithDefaultSchema()
        {
            string tableName = "NewTable";
            CreateTableExpression expression = GetCreateTableExpression(tableName);
            expression.Columns[0].IsPrimaryKey = true;
            expression.Columns[1].IsPrimaryKey = true;
            string sql = generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE \"public\".\"NewTable\" (\"ColumnName1\" text NOT NULL, \"ColumnName2\" integer NOT NULL, PRIMARY KEY (\"ColumnName1\",\"ColumnName2\"))");
        }

        [Test]
        public void CanCreateTableWithNamedMultiColumnPrimaryKeyWithDefaultSchema()
        {
            string tableName = "NewTable";
            CreateTableExpression expression = GetCreateTableExpression(tableName);
            expression.Columns[0].IsPrimaryKey = true;
            expression.Columns[0].PrimaryKeyName = "wibble";
            expression.Columns[1].IsPrimaryKey = true;
            string sql = generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE \"public\".\"NewTable\" (\"ColumnName1\" text NOT NULL, \"ColumnName2\" integer NOT NULL, CONSTRAINT \"wibble\" PRIMARY KEY (\"ColumnName1\",\"ColumnName2\"))");
        }

        [Test]
        public void CanCreateTableWithNamedPrimaryKeyWithDefaultSchema()
        {
            string tableName = "NewTable";
            CreateTableExpression expression = GetCreateTableExpression(tableName);
            expression.Columns[0].IsPrimaryKey = true;
            expression.Columns[0].PrimaryKeyName = "PK_NewTable";
            string sql = generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE \"public\".\"NewTable\" (\"ColumnName1\" text NOT NULL, \"ColumnName2\" integer NOT NULL, CONSTRAINT \"PK_NewTable\" PRIMARY KEY (\"ColumnName1\"))");
        }

        [Test]
        public void CanCreateTableWithPrimaryKeyWithDefaultSchema()
        {
            string tableName = "NewTable";
            CreateTableExpression expression = GetCreateTableExpression(tableName);
            expression.Columns[0].IsPrimaryKey = true;
            string sql = generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE \"public\".\"NewTable\" (\"ColumnName1\" text NOT NULL, \"ColumnName2\" integer NOT NULL, PRIMARY KEY (\"ColumnName1\"))");
        }

        [Test]
        public void CanDropTableWithCustomSchema()
        {
            string tableName = "NewTable";
            DeleteTableExpression expression = GetDeleteTableExpression(tableName);
            expression.SchemaName = "wibble";
            string sql = generator.Generate(expression);
            sql.ShouldBe("DROP TABLE \"wibble\".\"NewTable\"");
        }

        [Test]
        public void CanDropTableWithDefaultSchema()
        {
            string tableName = "NewTable";
            DeleteTableExpression expression = GetDeleteTableExpression(tableName);
            string sql = generator.Generate(expression);
            sql.ShouldBe("DROP TABLE \"public\".\"NewTable\"");
        }

        [Test]
        public void CanRenameTableWithDefaultSchema()
        {
            var expression = new RenameTableExpression();
            expression.OldName = "Table1";
            expression.NewName = "Table2";

            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"public\".\"Table1\" RENAME TO \"Table2\"");
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