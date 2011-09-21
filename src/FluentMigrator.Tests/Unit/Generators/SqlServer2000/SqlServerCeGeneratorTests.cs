using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer
{
    public class SqlServerCeGeneratorTests
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
            var expression = new CreateSchemaExpression();
            Assert.Throws<DatabaseOperationNotSupportedException>(() => _generator.Generate(expression));
        }

        [Test]
        public void CannotAlterASchema()
        {
            var expression = new AlterSchemaExpression();
            Assert.Throws<DatabaseOperationNotSupportedException>(() => _generator.Generate(expression));
        }

        [Test]
        public void CannotDeleteASchema()
        {
            var expression = new DeleteSchemaExpression();
            Assert.Throws<DatabaseOperationNotSupportedException>(() => _generator.Generate(expression));
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