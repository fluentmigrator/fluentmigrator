using System.Linq;
using FluentMigrator.Runner.Generators.Postgres;
using Xunit;

namespace FluentMigrator.Tests.Unit.Generators.Postgres
{
    public class PostgresDescriptionGeneratorTests : BaseDescriptionGeneratorTests
    {
        public PostgresDescriptionGeneratorTests()
        {
            descriptionGenerator = new PostgresDescriptionGenerator();
        }

        [Fact]
        public override void GenerateDescriptionStatementsForCreateTableReturnTableDescriptionStatement()
        {
            var createTableExpression = GeneratorTestHelper.GetCreateTableWithTableDescription();
            var statements = descriptionGenerator.GenerateDescriptionStatements(createTableExpression);

            var result = statements.First();
            result.ShouldBe( "COMMENT ON TABLE \"TestTable1\" IS 'TestDescription';" );
        }

        [Fact]
        public override void GenerateDescriptionStatementsForCreateTableReturnTableDescriptionAndColumnDescriptionsStatements()
        {
            var createTableExpression = GeneratorTestHelper.GetCreateTableWithTableDescriptionAndColumnDescriptions();
            var statements = descriptionGenerator.GenerateDescriptionStatements(createTableExpression).ToArray();

            var result = string.Join("", statements);
            result.ShouldBe( "COMMENT ON TABLE \"TestTable1\" IS 'TestDescription';COMMENT ON COLUMN \"TestTable1\".\"TestColumn1\" IS 'TestColumn1Description';COMMENT ON COLUMN \"TestTable1\".\"TestColumn2\" IS 'TestColumn2Description';" );
        }

        [Fact]
        public override void GenerateDescriptionStatementForAlterTableReturnTableDescriptionStatement()
        {
            var alterTableExpression = GeneratorTestHelper.GetAlterTableWithDescriptionExpression();
            var statement = descriptionGenerator.GenerateDescriptionStatement(alterTableExpression);

            statement.ShouldBe( "COMMENT ON TABLE \"TestTable1\" IS 'TestDescription';" );
        }

        [Fact]
        public override void GenerateDescriptionStatementForCreateColumnReturnColumnDescriptionStatement()
        {
            var createColumnExpression = GeneratorTestHelper.GetCreateColumnExpressionWithDescription();
            var statement = descriptionGenerator.GenerateDescriptionStatement(createColumnExpression);

            statement.ShouldBe( "COMMENT ON COLUMN \"TestTable1\".\"TestColumn1\" IS 'TestColumn1Description';" );
        }

        [Fact]
        public override void GenerateDescriptionStatementForAlterColumnReturnColumnDescriptionStatement()
        {
            var alterColumnExpression = GeneratorTestHelper.GetAlterColumnExpressionWithDescription();
            var statement = descriptionGenerator.GenerateDescriptionStatement(alterColumnExpression);

            statement.ShouldBe( "COMMENT ON COLUMN \"TestTable1\".\"TestColumn1\" IS 'TestColumn1Description';" );
        }
    }
}

