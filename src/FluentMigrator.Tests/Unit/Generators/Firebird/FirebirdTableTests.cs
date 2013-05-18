using System;
using System.Data;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Processors.Firebird;
using FluentMigrator.Runner.Generators.Firebird;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.Firebird
{
    [TestFixture]
    public class FirebirdTableTests
    {
        protected FirebirdGenerator generator;

        [SetUp]
        public void Setup()
        {
            generator = new FirebirdGenerator(FirebirdOptions.StandardBehaviour());
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
        public void CanCreateTable()
        {
            string tableName = "NewTable";
            CreateTableExpression expression = GetCreateTableExpression(tableName);
            string sql = generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE \"NewTable\" (\"ColumnName1\" BLOB SUB_TYPE TEXT NOT NULL, \"ColumnName2\" INTEGER NOT NULL)");
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
        public void CanCreateTableWithDefaultValue()
        {
            string tableName = "NewTable";
            CreateTableExpression expression = GetCreateTableExpression(tableName);
            expression.Columns[0].DefaultValue = "abc";
            string sql = generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE \"NewTable\" (\"ColumnName1\" BLOB SUB_TYPE TEXT DEFAULT 'abc' NOT NULL, \"ColumnName2\" INTEGER NOT NULL)");
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
        public void CanCreateTableWithPrimaryKey()
        {
            string tableName = "NewTable";
            CreateTableExpression expression = GetCreateTableExpression(tableName);
            expression.Columns[0].IsPrimaryKey = true;
            string sql = generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE \"NewTable\" (\"ColumnName1\" BLOB SUB_TYPE TEXT NOT NULL, \"ColumnName2\" INTEGER NOT NULL, PRIMARY KEY (\"ColumnName1\"))");
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
        public void CanDropTable()
        {
            string tableName = "NewTable";
            DeleteTableExpression expression = GetDeleteTableExpression(tableName);
            string sql = generator.Generate(expression);
            sql.ShouldBe("DROP TABLE \"NewTable\"");
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

        private DeleteTableExpression GetDeleteTableExpression(string tableName)
        {
            return new DeleteTableExpression { TableName = tableName };
        }
    }
}
