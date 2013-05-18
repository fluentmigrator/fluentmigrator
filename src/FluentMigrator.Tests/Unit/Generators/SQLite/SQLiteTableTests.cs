using FluentMigrator.Runner.Generators.SQLite;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.SQLite
{
    [TestFixture]
    public class SQLiteTableTests
    {
        protected SqliteGenerator _generator;

        [SetUp]
        public void Setup()
        {
            _generator = new SqliteGenerator();
        }

        [Test]
        public void CanCreateTableWithCustomColumnType()
        {
            //Not sure what custom column types there are in sqlite so not testing
        }

        [Test]
        public void CanCreateTable()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            string sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE \"TestTable1\" (\"TestColumn1\" TEXT NOT NULL, \"TestColumn2\" INTEGER NOT NULL)");
        }

        [Test]
        public void CanCreateTableWithDefaultValueExplicitlySetToNull()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();
            expression.Columns[0].DefaultValue = null;

            var result = _generator.Generate(expression);

            result.ShouldBe(
                "CREATE TABLE \"TestTable1\" (\"TestColumn1\" TEXT NOT NULL DEFAULT NULL, \"TestColumn2\" INTEGER NOT NULL DEFAULT 0)");
        }

        [Test]
        public void CanCreateTableWithDefaultValue()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();

            var result = _generator.Generate(expression);

            result.ShouldBe(
                "CREATE TABLE \"TestTable1\" (\"TestColumn1\" TEXT NOT NULL DEFAULT 'Default', \"TestColumn2\" INTEGER NOT NULL DEFAULT 0)");
        }

        [Test]
        public void CanCreateTableWithIdentity()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression();
            //Have to force it to primary key for SQLite
            expression.Columns[0].IsPrimaryKey = true;

            var sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE \"TestTable1\" (\"TestColumn1\" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, \"TestColumn2\" INTEGER NOT NULL)");
        }

        [Test]
        public void CanCreateTableWithMultiColumnPrimaryKey()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithMultiColumnPrimaryKeyExpression();
            var result = _generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"TestTable1\" (\"TestColumn1\" TEXT NOT NULL, \"TestColumn2\" INTEGER NOT NULL, PRIMARY KEY (\"TestColumn1\", \"TestColumn2\"))");
        }

        [Test]
        public void CanCreateTableNamedMultiColumnPrimaryKey()
        {
            //Should work. I think from the docs
            var expression = GeneratorTestHelper.GetCreateTableWithMultiColumNamedPrimaryKeyExpression();
            string sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE \"TestTable1\" (\"TestColumn1\" TEXT NOT NULL, \"TestColumn2\" INTEGER NOT NULL, CONSTRAINT \"TestKey\" PRIMARY KEY (\"TestColumn1\", \"TestColumn2\"))");
        }

        [Test]
        public void CanCreateTableNamedPrimaryKey()
        {
            //Should work. I think from the docs
            var expression = GeneratorTestHelper.GetCreateTableWithNamedPrimaryKeyExpression();
            string sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE \"TestTable1\" (\"TestColumn1\" TEXT NOT NULL, \"TestColumn2\" INTEGER NOT NULL, CONSTRAINT \"TestKey\" PRIMARY KEY (\"TestColumn1\"))");
        }

        [Test]
        public void CanCreateTableWithNullableField()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNullableColumn();

            var result = _generator.Generate(expression);

            result.ShouldBe(
                "CREATE TABLE \"TestTable1\" (\"TestColumn1\" TEXT, \"TestColumn2\" INTEGER NOT NULL)");
        }

        [Test]
        public void CanCreateTableWithPrimaryKey()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithPrimaryKeyExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE \"TestTable1\" (\"TestColumn1\" TEXT NOT NULL, \"TestColumn2\" INTEGER NOT NULL, PRIMARY KEY (\"TestColumn1\"))");
        }

        [Test]
        public void CanDropTable()
        {
            var expression = GeneratorTestHelper.GetDeleteTableExpression();
            string sql = _generator.Generate(expression);
            sql.ShouldBe("DROP TABLE \"TestTable1\"");
        }

        [Test]
        public void CanRenameTable()
        {
            var expression = GeneratorTestHelper.GetRenameTableExpression();
            string sql = _generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"TestTable1\" RENAME TO \"TestTable2\"");
        }
    }
}