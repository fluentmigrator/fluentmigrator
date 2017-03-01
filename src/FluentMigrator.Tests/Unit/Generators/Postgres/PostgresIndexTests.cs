using FluentMigrator.Runner.Generators.Postgres;
using Xunit;

namespace FluentMigrator.Tests.Unit.Generators.Postgres
{
    public class PostgresIndexTests : BaseIndexTests
    {
        protected PostgresGenerator Generator;

        public PostgresIndexTests()
        {
            Generator = new PostgresGenerator();
        }

        [Fact]
        public override void CanCreateIndexWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateIndexExpression();
            expression.Index.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE INDEX \"TestIndex\" ON \"TestSchema\".\"TestTable1\" (\"TestColumn1\" ASC);");
        }

        [Fact]
        public override void CanCreateIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateIndexExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE INDEX \"TestIndex\" ON \"public\".\"TestTable1\" (\"TestColumn1\" ASC);");
        }

        [Fact]
        public override void CanCreateMultiColumnIndexWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnCreateIndexExpression();
            expression.Index.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE INDEX \"TestIndex\" ON \"TestSchema\".\"TestTable1\" (\"TestColumn1\" ASC,\"TestColumn2\" DESC);");
        }

        [Fact]
        public override void CanCreateMultiColumnIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnCreateIndexExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE INDEX \"TestIndex\" ON \"public\".\"TestTable1\" (\"TestColumn1\" ASC,\"TestColumn2\" DESC);");
        }

        [Fact]
        public override void CanCreateMultiColumnUniqueIndexWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueMultiColumnIndexExpression();
            expression.Index.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX \"TestIndex\" ON \"TestSchema\".\"TestTable1\" (\"TestColumn1\" ASC,\"TestColumn2\" DESC);");
        }

        [Fact]
        public override void CanCreateMultiColumnUniqueIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueMultiColumnIndexExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX \"TestIndex\" ON \"public\".\"TestTable1\" (\"TestColumn1\" ASC,\"TestColumn2\" DESC);");
        }

        [Fact]
        public override void CanCreateUniqueIndexWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueIndexExpression();
            expression.Index.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX \"TestIndex\" ON \"TestSchema\".\"TestTable1\" (\"TestColumn1\" ASC);");
        }

        [Fact]
        public override void CanCreateUniqueIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueIndexExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX \"TestIndex\" ON \"public\".\"TestTable1\" (\"TestColumn1\" ASC);");
        }

        [Fact]
        public override void CanDropIndexWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteIndexExpression();
            expression.Index.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("DROP INDEX \"TestSchema\".\"TestIndex\";");
        }

        [Fact]
        public override void CanDropIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteIndexExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("DROP INDEX \"public\".\"TestIndex\";");
        }
    }
}
