using FluentMigrator.Runner.Generators.SQLite;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.SQLite
{
    [TestFixture]
    public class SQLiteIndexTests
    {
        protected SqliteGenerator _generator;

        [SetUp]
        public void Setup()
        {
            _generator = new SqliteGenerator();
        }

        [Test]
        public void CanCreateIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateIndexExpression();
            string sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE INDEX \"TestIndex\" ON \"TestTable1\" (\"TestColumn1\" ASC)");
        }

        [Test]
        public void CanCreateMultiColumnIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnCreateIndexExpression();
            string sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE INDEX \"TestIndex\" ON \"TestTable1\" (\"TestColumn1\" ASC, \"TestColumn2\" DESC)");
        }

        [Test]
        public void CanCreateMultiColumnUniqueIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueMultiColumnIndexExpression();

            var sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE UNIQUE INDEX \"TestIndex\" ON \"TestTable1\" (\"TestColumn1\" ASC, \"TestColumn2\" DESC)");
        }

        [Test]
        public void CanCreateUniqueIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueIndexExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE UNIQUE INDEX \"TestIndex\" ON \"TestTable1\" (\"TestColumn1\" ASC)");
        }

        [Test]
        public void CanDropIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteIndexExpression();
            string sql = _generator.Generate(expression);
            sql.ShouldBe("DROP INDEX \"TestIndex\"");
        }
    }
}