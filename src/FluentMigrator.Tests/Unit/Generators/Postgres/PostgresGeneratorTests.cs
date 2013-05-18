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
        protected PostgresGenerator generator;

        [SetUp]
        public void Setup()
        {
            generator = new PostgresGenerator();
        }

        [Test]
        public void CanCreateAutoIncrementColumnForInt64()
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
        public void CanCreateTableWithBinaryColumnWithSize()
        {
            string tableName = "NewTable";
            string columnName = "ColumnName1";

            var column1 = new ColumnDefinition { Name = columnName, Type = DbType.Binary, TableName = tableName, Size = 10000 };

            var expression = new CreateTableExpression { TableName = tableName };
            expression.Columns.Add(column1);
            string sql = generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE \"public\".\"NewTable\" (\"ColumnName1\" bytea NOT NULL)"); // PostgreSQL does not actually use the configured size
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
        public void CanUseSystemMethodCurrentUserAsADefaultValueForAColumn()
        {
            const string tableName = "NewTable";

            var columnDefinition = new ColumnDefinition { Name = "NewColumn", Size = 15, Type = DbType.String, DefaultValue = SystemMethods.CurrentUser };

            var expression = new CreateColumnExpression {Column = columnDefinition, TableName = tableName};

            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"public\".\"NewTable\" ADD \"NewColumn\" varchar(15) NOT NULL DEFAULT current_user");
        }

        [Test]
        public void CanUseSystemMethodCurrentUTCDateTimeAsADefaultValueForAColumn()
        {
            const string tableName = "NewTable";

            var columnDefinition = new ColumnDefinition { Name = "NewColumn", Size = 5, Type = DbType.String, DefaultValue = SystemMethods.CurrentUTCDateTime };

            var expression = new CreateColumnExpression { Column = columnDefinition, TableName = tableName };

            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"public\".\"NewTable\" ADD \"NewColumn\" varchar(5) NOT NULL DEFAULT (now() at time zone 'UTC')");
        }

        [Test]
        public void ExplicitUnicodeStringIgnoredForNonSqlServer()
        {
            var expression = new InsertDataExpression();
            expression.TableName = "TestTable";
            expression.Rows.Add(new InsertionDataDefinition
                                    {
                                        new KeyValuePair<string, object>("NormalString", "Just'in"),
                                        new KeyValuePair<string, object>("UnicodeString", new ExplicitUnicodeString("codethinked'.com"))
                                    });

            var sql = generator.Generate(expression);

            var expected = "INSERT INTO \"public\".\"TestTable\" (\"NormalString\",\"UnicodeString\") VALUES ('Just''in','codethinked''.com');";

            sql.ShouldBe(expected);
        }

        [Test]
        public void CanAlterColumnAndSetAsNullable()
        {
            var expression = new AlterColumnExpression
            {
                Column = new ColumnDefinition { Type = DbType.String, Name = "Col1", IsNullable = true},
                SchemaName = "Schema1",
                TableName = "Table1"
            };
            var sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"Schema1\".\"Table1\" ALTER \"Col1\" TYPE text, ALTER \"Col1\" DROP NOT NULL");
        }

        [Test]
        public void CanAlterColumnAndSetAsNotNullable()
        {
            var expression = new AlterColumnExpression
            {
                Column = new ColumnDefinition { Type = DbType.String, Name = "Col1", IsNullable = false },
                SchemaName = "Schema1",
                TableName = "Table1"
            };
            var sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"Schema1\".\"Table1\" ALTER \"Col1\" TYPE text, ALTER \"Col1\" SET NOT NULL");
        }

        [Test]
        public void CanAlterDefaultConstraintToNewGuid()
        {
            var expression = new AlterDefaultConstraintExpression
            {
                SchemaName = "Schema1",
                TableName = "Table1",
                ColumnName = "Col1",
                DefaultValue = SystemMethods.NewGuid
            };

            var sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"Schema1\".\"Table1\" ALTER \"Col1\" DROP DEFAULT, ALTER \"Col1\" SET DEFAULT uuid_generate_v4()");
        }

        [Test]
        public void CanDeleteDefaultConstraint()
        {
            var expression = new DeleteDefaultConstraintExpression
                                 {
                                     ColumnName = "Col1",
                                     SchemaName = "Schema1",
                                     TableName = "Table1"
                                 };

            var sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"Schema1\".\"Table1\" ALTER \"Col1\" DROP DEFAULT");
        }

        [Test]
        public void CanAlterDefaultConstraintToCurrentUser()
        {
            var expression = new AlterDefaultConstraintExpression
                                 {
                                     SchemaName = "Schema1",
                                     TableName = "Table1",
                                     ColumnName = "Col1",
                                     DefaultValue = SystemMethods.CurrentUser
                                 };

            var sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"Schema1\".\"Table1\" ALTER \"Col1\" DROP DEFAULT, ALTER \"Col1\" SET DEFAULT current_user");
        }

        [Test]
        public void CanAlterDefaultConstraintToCurrentDate()
        {
            var expression = new AlterDefaultConstraintExpression
            {
                SchemaName = "Schema1",
                TableName = "Table1",
                ColumnName = "Col1",
                DefaultValue = SystemMethods.CurrentDateTime
            };

            var sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"Schema1\".\"Table1\" ALTER \"Col1\" DROP DEFAULT, ALTER \"Col1\" SET DEFAULT now()");
        }

        [Test]
        public void CanAlterDefaultConstraintToCurrentUtcDateTime()
        {
            var expression = new AlterDefaultConstraintExpression
            {
                SchemaName = "Schema1",
                TableName = "Table1",
                ColumnName = "Col1",
                DefaultValue = SystemMethods.CurrentUTCDateTime
            };

            var sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"Schema1\".\"Table1\" ALTER \"Col1\" DROP DEFAULT, ALTER \"Col1\" SET DEFAULT (now() at time zone 'UTC')");
        }

        [Test]
        public void CanAlterColumnAndOnlySetTypeIfIsNullableNotSet()
        {
            var expression = new AlterColumnExpression
            {
                Column = new ColumnDefinition { Type = DbType.String, Name = "Col1", IsNullable = null },
                SchemaName = "Schema1",
                TableName = "Table1"
            };
            var sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"Schema1\".\"Table1\" ALTER \"Col1\" TYPE text");
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