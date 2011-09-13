using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators
{
    public class SqlServerCeGeneratorTests : GeneratorTestBase
    {
        private SqlServerCeGenerator _generator;

        [SetUp]
        public void SetUp()
        {
            _generator = new SqlServerCeGenerator();
        }

        [Test]
        public void CannotCreateASchema()
        {
            Assert.Throws<DatabaseOperationNotSupportedException>(() => _generator.Generate(new CreateSchemaExpression()));
        }

        [Test]
        public void CannotAlterASchema()
        {
            Assert.Throws<DatabaseOperationNotSupportedException>(() => _generator.Generate(new AlterSchemaExpression()));
        }

        [Test]
        public void CannotDeleteASchema()
        {
            Assert.Throws<DatabaseOperationNotSupportedException>(() => _generator.Generate(new DeleteSchemaExpression()));
        }

        [Test]
        public void CreatesTheCorrectSyntaxToDropAnIndex()
        {
            var expression = GeneratorTestHelper.GetDeleteIndexExpression();
            var sql = _generator.Generate(expression);

            sql.ShouldBe("DROP INDEX [TestTable1].[TestIndex]");
        }
    }
}