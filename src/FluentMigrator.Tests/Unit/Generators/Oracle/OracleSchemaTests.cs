using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators.Oracle;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.Oracle
{
    [TestFixture]
    public class OracleSchemaTests
    {
        protected OracleGenerator _generator;
        protected OracleGenerator generator;

        [SetUp]
        public void Setup()
        {
            _generator = new OracleGenerator();
            generator = new OracleGenerator();
        }

        [Test]
        public void CanAlterSchema()
        {
            var expression = new AlterSchemaExpression();
            var result = generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }

        [Test]
        public void CanCreateSchema()
        {
            var expression = new CreateSchemaExpression() { SchemaName = "TestSchema" };
            var result = _generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }

        [Test]
        public void CanDeleteSchema()
        {
            var expression = new DeleteSchemaExpression();
            var result = _generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }
    }
}