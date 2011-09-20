using System.Linq;
using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.SQLite;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.SQLite
{
    public class SQLiteCreateTableTests : BaseTableCreateTests
    {
        private SqliteGenerator _generator;

        [SetUp]
        public void Setup()
        {
            _generator = new SqliteGenerator();
        }

        [Test]
        public override void CanCreateTable()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE 'TestTable1' ('TestColumn1' TEXT NOT NULL, 'TestColumn2' INTEGER NOT NULL)");
        }

        [Test]
        public override void CanCreateTableWithCustomColumnType()
        {
            //Not sure what custom column types there are in sqlite so not testing
        }

        [Test]
        public override void CanCreateTableWithPrimaryKey()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithPrimaryKeyExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE 'TestTable1' ('TestColumn1' TEXT NOT NULL, 'TestColumn2' INTEGER NOT NULL, PRIMARY KEY ('TestColumn1'))");
        }

        [Test]
        public override void CanCreateTableNamedPrimaryKey()
        {
            //Should work. I think from the docs
            var expression = GeneratorTestHelper.GetCreateTableWithNamedPrimaryKeyExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE 'TestTable1' ('TestColumn1' TEXT NOT NULL, 'TestColumn2' INTEGER NOT NULL, CONSTRAINT 'TestKey' PRIMARY KEY ('TestColumn1'))");
        }

        [Test]
        public override void CanCreateTableNamedMultiColumnPrimaryKey()
        {
            //Should work. I think from the docs
            var expression = GeneratorTestHelper.GetCreateTableWithMultiColumNamedPrimaryKeyExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE 'TestTable1' ('TestColumn1' TEXT NOT NULL, 'TestColumn2' INTEGER NOT NULL, CONSTRAINT 'TestKey' PRIMARY KEY ('TestColumn1', 'TestColumn2'))");
        }

        [Test]
        public override void CanCreateTableWithIdentity()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression();
            //Have to force it to primary key for SQLite
            expression.Columns.First().IsPrimaryKey = true;

            var sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE 'TestTable1' ('TestColumn1' INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 'TestColumn2' INTEGER NOT NULL)");
        }

        [Test]
        public override void CanCreateTableWithNullableField()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNullableColumn();
            var sql = _generator.Generate(expression);
            sql.ShouldBe(
                "CREATE TABLE 'TestTable1' ('TestColumn1' TEXT, 'TestColumn2' INTEGER NOT NULL)");
        }

        [Test]
        public override void CanCreateTableWithDefaultValue()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();
            var sql = _generator.Generate(expression);
            sql.ShouldBe(
                "CREATE TABLE 'TestTable1' ('TestColumn1' TEXT NOT NULL DEFAULT 'Default', 'TestColumn2' INTEGER NOT NULL DEFAULT 0)");
        }

        [Test]
        public override void CanCreateTableWithDefaultValueExplicitlySetToNull()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();
            expression.Columns.First().DefaultValue = null;
            var sql = _generator.Generate(expression);
            sql.ShouldBe(
                "CREATE TABLE 'TestTable1' ('TestColumn1' TEXT NOT NULL DEFAULT NULL, 'TestColumn2' INTEGER NOT NULL DEFAULT 0)");
        }

        [Test]
        public override void CanCreateTableWithDefaultExpression()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe(
                "CREATE TABLE 'TestTable1' ('TestColumn1' TEXT NOT NULL DEFAULT (TestFunction))");
        }

        [Test]
        public override void CanCreateTableWithDefaultGuid()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultGuid();
            Assert.Throws<DatabaseOperationNotSupportedException>(() => _generator.Generate(expression));
        }

        [Test]
        public override void CanCreateTableWithDefaultCurrentDate()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultCurrentDateTime();
            var sql = _generator.Generate(expression);
            sql.ShouldBe(
                "CREATE TABLE 'TestTable1' ('TestColumn1' TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP)");
        }

        [Test]
        public override void CanCreateIndex()
        {
            var expression = GeneratorTestHelper.GetCreateIndexExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE INDEX 'TestIndex' ON 'TestTable1' ('TestColumn1' ASC)");
        }

        [Test]
        public override void CanCreateMultiColumnIndex()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnCreateIndexExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE INDEX 'TestIndex' ON 'TestTable1' ('TestColumn1' ASC, 'TestColumn2' DESC)");
        }

        [Test]
        public override void CanCreateTableWithMultiColumnPrimaryKey()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithMultiColumnPrimaryKeyExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE 'TestTable1' ('TestColumn1' TEXT NOT NULL, 'TestColumn2' INTEGER NOT NULL, PRIMARY KEY ('TestColumn1', 'TestColumn2'))");
        }

        [Test]
        public override void CanCreateUniqueIndex()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueIndexExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE UNIQUE INDEX 'TestIndex' ON 'TestTable1' ('TestColumn1' ASC)");
        }

        [Test]
        public override void CanCreateMultiColumnUniqueIndex()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueMultiColumnIndexExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE UNIQUE INDEX 'TestIndex' ON 'TestTable1' ('TestColumn1' ASC, 'TestColumn2' DESC)");
        }

        [Test]
        public override void CanCreateSchema()
        {
            var expression = new CreateSchemaExpression { SchemaName = "TestSchema" };
            var sql = _generator.Generate(expression);
            sql.ShouldBe(string.Empty);
        }

        [Test]
        public void CanCreateSchemaInStrictMode()
        {
            _generator.compatabilityMode = Runner.CompatabilityMode.STRICT;
            Assert.Throws<DatabaseOperationNotSupportedException>(() => _generator.Generate(new CreateSchemaExpression()));
        }
    }
}