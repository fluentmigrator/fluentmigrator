using FluentMigrator.Runner.Generators.Postgres;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.Postgres
{
    [TestFixture]
    public class PostgresSchemaTests
    {
        protected PostgresGenerator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new PostgresGenerator();
        }

        [Test]
        public void CanAlterSchema()
        {
            var expression = GeneratorTestHelper.GetAlterSchemaExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema1\".\"TestTable\" SET SCHEMA \"TestSchema2\"");
        }

        [Test]
        public void CanCreateSchema()
        {
            var expression = GeneratorTestHelper.GetCreateSchemaExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE SCHEMA \"TestSchema\"");
        }

        [Test]
        public void CanDropSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteSchemaExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("DROP SCHEMA \"TestSchema\"");
        }
    }
}