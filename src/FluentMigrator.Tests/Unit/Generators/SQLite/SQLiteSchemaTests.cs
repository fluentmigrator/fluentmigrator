using FluentMigrator.Runner.Generators.SQLite;
using Xunit;

namespace FluentMigrator.Tests.Unit.Generators.SQLite
{
    public class SQLiteSchemaTests : BaseSchemaTests
    {
        protected SQLiteGenerator Generator;

        public SQLiteSchemaTests()
        {
            Generator = new SQLiteGenerator();
        }

        [Fact]
        public override void CanAlterSchema()
        {
            var expression = GeneratorTestHelper.GetAlterSchemaExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }

        [Fact]
        public override void CanCreateSchema()
        {
            var expression = GeneratorTestHelper.GetCreateSchemaExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }

        [Fact]
        public override void CanDropSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteSchemaExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }
    }
}
