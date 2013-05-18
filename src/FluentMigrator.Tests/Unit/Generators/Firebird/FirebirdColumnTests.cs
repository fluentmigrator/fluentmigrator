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
    public class FirebirdColumnTests
    {
        protected FirebirdGenerator generator;

        [SetUp]
        public void Setup()
        {
            generator = new FirebirdGenerator(FirebirdOptions.StandardBehaviour());
        }

        [Test]
        public void CanAlterColumnWithDefaultSchema()
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
        public void CanCreateColumnWithDefaultSchema()
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
        public void CanCreateDecimalColumnWithDefaultSchema()
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
        public void CanDropColumnWithDefaultSchema()
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
        public void CanDropMultipleColumnsWithDefaultSchema()
        {
            var expression = new DeleteColumnExpression();
            expression.TableName = "NewTable";
            expression.ColumnNames.Add("NewColumn");
            expression.ColumnNames.Add("OtherColumn");

            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"NewTable\" DROP \"NewColumn\";" + Environment.NewLine +
                "ALTER TABLE \"NewTable\" DROP \"OtherColumn\"");
        }

        [Test]
        public void CanRenameColumnWithDefaultSchema()
        {
            var expression = new RenameColumnExpression();
            expression.TableName = "Table1";
            expression.OldName = "Column1";
            expression.NewName = "Column2";

            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"Table1\" ALTER COLUMN \"Column1\" TO \"Column2\"");
        }
    }
}
