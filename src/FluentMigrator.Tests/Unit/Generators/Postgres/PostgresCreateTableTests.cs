using System;
using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators.Postgres;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.Postgres
{
    [TestFixture]
    public class PostgresCreateTableTests : BaseTableCreateTests
    {
        private readonly PostgresGenerator _generator;

        public PostgresCreateTableTests()
        {
            _generator = new PostgresGenerator();
        }

        [Test]
        public override void CanCreateMultiColumnUniqueIndex()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueMultiColumnIndexExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE UNIQUE INDEX \"TestIndex\" ON \"public\".\"TestTable1\" (\"TestColumn1\" ASC,\"TestColumn2\" DESC)");
        }

        [Test]
        public override void CanCreateSchema()
        {
            var expression = new CreateSchemaExpression { SchemaName = "Schema1" };
            var sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE SCHEMA \"Schema1\"");
        }

        [Test]
        public override void CanCreateTable()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE \"public\".\"TestTable1\" (\"TestColumn1\" text NOT NULL, \"TestColumn2\" integer NOT NULL)");
        }

        public override void CanCreateTableWithCustomColumnType()
        {
            throw new NotImplementedException();
        }

        [Test]
        public override void CanCreateTableWithPrimaryKey()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithPrimaryKeyExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE \"public\".\"TestTable1\" (\"TestColumn1\" text NOT NULL, \"TestColumn2\" integer NOT NULL, PRIMARY KEY (\"TestColumn1\"))");
        }

        [Test]
        public override void CanCreateTableWithIdentity()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE \"public\".\"TestTable1\" (\"TestColumn1\" serial NOT NULL, \"TestColumn2\" integer NOT NULL)");
        }

        [Test]
        public override void CanCreateTableWithNullableField()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNullableColumn();
            var sql = _generator.Generate(expression);
            sql.ShouldBe(
                "CREATE TABLE \"public\".\"TestTable1\" (\"TestColumn1\" text, \"TestColumn2\" integer NOT NULL)");
        }

        [Test]
        public override void CanCreateTableNamedPrimaryKey()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedPrimaryKeyExpression();
            string sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE \"public\".\"TestTable1\" (\"TestColumn1\" text NOT NULL, \"TestColumn2\" integer NOT NULL, CONSTRAINT \"TestKey\" PRIMARY KEY (\"TestColumn1\"))");
        }

        [Test]
        public override void CanCreateTableWithDefaultValue()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE \"public\".\"TestTable1\" (\"TestColumn1\" text NOT NULL DEFAULT 'Default', \"TestColumn2\" integer NOT NULL DEFAULT 0)");
        }

        [Test]
        public override void CanCreateTableWithDefaultValueExplicitlySetToNull()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();
            expression.Columns[0].DefaultValue = null;
            var sql = _generator.Generate(expression);
            sql.ShouldBe(
                "CREATE TABLE \"public\".\"TestTable1\" (\"TestColumn1\" text NOT NULL DEFAULT NULL, \"TestColumn2\" integer NOT NULL DEFAULT 0)");
        }

        [Test]
        public override void CanCreateTableWithDefaultExpression()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE \"public\".\"TestTable1\" (\"TestColumn1\" varchar(50) NOT NULL DEFAULT TestFunction)");
        }

        [Test]
        public override void CanCreateTableWithDefaultGuid()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultGuid();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE \"public\".\"TestTable1\" (\"TestColumn1\" varchar(50) NOT NULL DEFAULT uuid_generate_v4())");
        }

        [Test]
        public override void CanCreateTableWithDefaultCurrentDate()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultCurrentDateTime();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE \"public\".\"TestTable1\" (\"TestColumn1\" varchar(50) NOT NULL DEFAULT now())");
        }

        [Test]
        public override void CanCreateTableWithMultiColumnPrimaryKey()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithMultiColumnPrimaryKeyExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE \"public\".\"TestTable1\" (\"TestColumn1\" text NOT NULL, \"TestColumn2\" integer NOT NULL, PRIMARY KEY (\"TestColumn1\",\"TestColumn2\"))");
        }

        [Test]
        public override void CanCreateTableNamedMultiColumnPrimaryKey()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithMultiColumNamedPrimaryKeyExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE \"public\".\"TestTable1\" (\"TestColumn1\" text NOT NULL, \"TestColumn2\" integer NOT NULL, CONSTRAINT \"TestKey\" PRIMARY KEY (\"TestColumn1\",\"TestColumn2\"))");
        }

        [Test]
        public override void CanCreateIndex()
        {
            var expression = GeneratorTestHelper.GetCreateIndexExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE INDEX \"TestIndex\" ON \"public\".\"TestTable1\" (\"TestColumn1\" ASC)");
        }

        [Test]
        public override void CanCreateMultiColumnIndex()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnCreateIndexExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE INDEX \"TestIndex\" ON \"public\".\"TestTable1\" (\"TestColumn1\" ASC,\"TestColumn2\" DESC)");
        }

        [Test]
        public override void CanCreateUniqueIndex()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueIndexExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE UNIQUE INDEX \"TestIndex\" ON \"public\".\"TestTable1\" (\"TestColumn1\" ASC)");
        }
    }
}