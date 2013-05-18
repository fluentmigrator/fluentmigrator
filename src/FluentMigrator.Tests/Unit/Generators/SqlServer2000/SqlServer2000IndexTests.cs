using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer2000
{
    [TestFixture]
    public class SqlServer2000IndexTests
    {
        protected SqlServer2000Generator _generator;

        [SetUp]
        public void Setup()
        {
            _generator = new SqlServer2000Generator();
        }

        [Test]
        public void CanCreateIndex()
        {
            var expression = GeneratorTestHelper.GetCreateIndexExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE INDEX [TestIndex] ON [TestTable1] ([TestColumn1] ASC)");
        }

        [Test]
        public void CanCreateMultiColumnIndex()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnCreateIndexExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE INDEX [TestIndex] ON [TestTable1] ([TestColumn1] ASC, [TestColumn2] DESC)");
        }

        [Test]
        public void CanCreateMultiColumnUniqueIndex()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueMultiColumnIndexExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE UNIQUE INDEX [TestIndex] ON [TestTable1] ([TestColumn1] ASC, [TestColumn2] DESC)");
        }

        [Test]
        public void CanCreateUniqueIndex()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueIndexExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE UNIQUE INDEX [TestIndex] ON [TestTable1] ([TestColumn1] ASC)");
        }

        [Test]
        public void CanDeleteIndex()
        {
            var expression = GeneratorTestHelper.GetDeleteIndexExpression();

            var sql = _generator.Generate(expression);
            sql.ShouldBe("DROP INDEX [TestTable1].[TestIndex]");
        }
    }
}
