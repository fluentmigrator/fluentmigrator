using System;
using System.Linq;
using FluentMigrator.Runner.Generators.Postgres;
using FluentMigrator.Runner.Processors.Postgres;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.Postgres
{
    [TestFixture]
    public class PostgresDescriptionGeneratorTests : BaseDescriptionGeneratorTests
    {
        [SetUp]
        public void Setup()
        {
            var quoter = new PostgresQuoter(new PostgresOptions());
            DescriptionGenerator = new PostgresDescriptionGenerator(quoter);
        }

        [Test]
        public override void GenerateDescriptionStatementsForCreateTableReturnTableDescriptionStatement()
        {
            var createTableExpression = GeneratorTestHelper.GetCreateTableWithTableDescription();
            var statements = DescriptionGenerator.GenerateDescriptionStatements(createTableExpression);

            var result = statements.First();
            result.ShouldBe("COMMENT ON TABLE \"public\".\"TestTable1\" IS 'TestDescription';");
        }

        [Test]
        public override void GenerateDescriptionStatementsForCreateTableReturnTableDescriptionAndColumnDescriptionsStatements()
        {
            var createTableExpression = GeneratorTestHelper.GetCreateTableWithTableDescriptionAndColumnDescriptions();
            var statements = DescriptionGenerator.GenerateDescriptionStatements(createTableExpression).ToArray();

            var result = string.Join("", statements);
            result.ShouldBe("COMMENT ON TABLE \"public\".\"TestTable1\" IS 'TestDescription';COMMENT ON COLUMN \"public\".\"TestTable1\".\"TestColumn1\" IS 'Description:TestColumn1Description';COMMENT ON COLUMN \"public\".\"TestTable1\".\"TestColumn2\" IS 'Description:TestColumn2Description';");
        }

        [Test]
        public override void GenerateDescriptionStatementsForCreateTableReturnTableDescriptionAndColumnDescriptionsWithAdditionalDescriptionsStatements()
        {
            var createTableExpression = GeneratorTestHelper.GetCreateTableWithTableDescriptionAndColumnDescriptionsAndAdditionalDescriptions();
            var statements = DescriptionGenerator.GenerateDescriptionStatements(createTableExpression).ToArray();

            var result = string.Join("", statements);
            result.ShouldBe("COMMENT ON TABLE \"public\".\"TestTable1\" IS 'TestDescription';COMMENT ON COLUMN \"public\".\"TestTable1\".\"TestColumn1\" IS 'Description:TestColumn1Description"+Environment.NewLine+
                "AdditionalColumnDescriptionKey1:AdditionalColumnDescriptionValue1';COMMENT ON COLUMN \"public\".\"TestTable1\".\"TestColumn2\" IS 'Description:TestColumn2Description" + Environment.NewLine +
                "AdditionalColumnDescriptionKey2:AdditionalColumnDescriptionValue2';");
        }

        [Test]
        public override void GenerateDescriptionStatementForAlterTableReturnTableDescriptionStatement()
        {
            var alterTableExpression = GeneratorTestHelper.GetAlterTableWithDescriptionExpression();
            var statement = DescriptionGenerator.GenerateDescriptionStatement(alterTableExpression);

            statement.ShouldBe("COMMENT ON TABLE \"public\".\"TestTable1\" IS 'TestDescription';");
        }

        [Test]
        public override void GenerateDescriptionStatementForCreateColumnReturnColumnDescriptionStatement()
        {
            var createColumnExpression = GeneratorTestHelper.GetCreateColumnExpressionWithDescription();
            var statement = DescriptionGenerator.GenerateDescriptionStatement(createColumnExpression);

            statement.ShouldBe("COMMENT ON COLUMN \"public\".\"TestTable1\".\"TestColumn1\" IS 'Description:TestColumn1Description';");
        }

        [Test]
        public override void GenerateDescriptionStatementForCreateColumnReturnColumnDescriptionStatementWithAdditionalDescriptions()
        {
            var createColumnExpression = GeneratorTestHelper.GetCreateColumnExpressionWithDescriptionWithAdditionalDescriptions();
            var statement = DescriptionGenerator.GenerateDescriptionStatement(createColumnExpression);

            statement.ShouldBe("COMMENT ON COLUMN \"public\".\"TestTable1\".\"TestColumn1\" IS 'Description:TestColumn1Description"+Environment.NewLine +
                "AdditionalColumnDescriptionKey1:AdditionalColumnDescriptionValue1';");
        }

        [Test]
        public override void GenerateDescriptionStatementForAlterColumnReturnColumnDescriptionStatement()
        {
            var alterColumnExpression = GeneratorTestHelper.GetAlterColumnExpressionWithDescription();
            var statement = DescriptionGenerator.GenerateDescriptionStatement(alterColumnExpression);

            statement.ShouldBe("COMMENT ON COLUMN \"public\".\"TestTable1\".\"TestColumn1\" IS 'Description:TestColumn1Description';");
        }

        [Test]
        public override void GenerateDescriptionStatementForAlterColumnReturnColumnDescriptionStatementWithAdditionalDescriptions()
        {
            var alterColumnExpression = GeneratorTestHelper.GetAlterColumnExpressionWithDescriptionWithAdditionalDescriptions();
            var statement = DescriptionGenerator.GenerateDescriptionStatement(alterColumnExpression);

            statement.ShouldBe("COMMENT ON COLUMN \"public\".\"TestTable1\".\"TestColumn1\" IS 'Description:TestColumn1Description"+Environment.NewLine+
                "AdditionalColumnDescriptionKey1:AdditionalColumnDescriptionValue1';");
        }
    }
}
