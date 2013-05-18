using System;
using System.Data;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Postgres;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.Postgres
{
    [TestFixture]
    public class PostgresColumnTests
    {
        protected PostgresGenerator generator;

        [SetUp]
        public void Setup()
        {
            generator = new PostgresGenerator();
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
            sql.ShouldBe("ALTER TABLE \"Schema1\".\"Table1\" ALTER \"Col1\" TYPE text");
        }

        [Test]
        public void CanAddIdentityColumn()
        {
            string tableName = "NewTable";

            var columnDefinition = new ColumnDefinition();
            columnDefinition.Name = "id";
            columnDefinition.IsIdentity = true;
            columnDefinition.Type = DbType.Int32;

            var expression = new CreateColumnExpression();
            expression.Column = columnDefinition;
            expression.TableName = tableName;

            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"public\".\"NewTable\" ADD \"id\" serial NOT NULL");
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
        public void CanAddDecimalColumn()
        {
            const string tableName = "NewTable";

            var columnDefinition = new ColumnDefinition();
            columnDefinition.Name = "NewColumn";
            columnDefinition.Size = 19;
            columnDefinition.Precision = 2;
            columnDefinition.Type = DbType.Decimal;

            var expression = new CreateColumnExpression { Column = columnDefinition, TableName = tableName };

            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"public\".\"NewTable\" ADD \"NewColumn\" decimal(19,2) NOT NULL");
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
            sql.ShouldBe("ALTER TABLE \"public\".\"NewTable\" DROP COLUMN \"NewColumn\"");
        }

        [Test]
        public void CanDropMultipleColumns()
        {
            var expression = new DeleteColumnExpression();
            expression.TableName = "NewTable";
            expression.ColumnNames.Add("NewColumn");
            expression.ColumnNames.Add("OtherColumn");

            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"public\".\"NewTable\" DROP COLUMN \"NewColumn\";" + Environment.NewLine +
                "ALTER TABLE \"public\".\"NewTable\" DROP COLUMN \"OtherColumn\"");
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
    }
}