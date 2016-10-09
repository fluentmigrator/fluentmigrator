using FluentMigrator.Exceptions;
using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators.Oracle;
using Xunit;

namespace FluentMigrator.Tests.Unit.Generators.Oracle
{
    public class OracleGeneratorTests
    {
        protected OracleGenerator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new OracleGenerator();
        }

        [Fact]
        public void CanAlterColumnNoNullSettings()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpression();
            expression.Column.IsNullable = null;

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 MODIFY TestColumn1 NVARCHAR2(20)");
        }

        [Fact]
        public void CanAlterColumnNull()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpression();
            expression.Column.IsNullable = true;

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 MODIFY TestColumn1 NVARCHAR2(20) NULL");
        }

        [Fact]
        public void CanAlterColumnNotNull()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpression();
            expression.Column.IsNullable = false;

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 MODIFY TestColumn1 NVARCHAR2(20) NOT NULL");
        }

        [Fact]
        public void CanAlterSchemaInStrictMode()
        {
            Generator.compatabilityMode = Runner.CompatabilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(new CreateSchemaExpression()));
        }

        [Fact]
        public void CanCreateSchemaInStrictMode()
        {
            Generator.compatabilityMode = Runner.CompatabilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(new CreateSchemaExpression()));
        }

        [Fact]
        public void CanDropSchemaInStrictMode()
        {
            Generator.compatabilityMode = Runner.CompatabilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(new DeleteSchemaExpression()));
        }

        [Fact]
        public void CanCreateTableWithoutAnyDescriptions()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();

            var result = Generator.Generate(expression);

            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 NVARCHAR2(255) NOT NULL, TestColumn2 NUMBER(10,0) NOT NULL)");
        }

        [Fact]
        public void CanCreateTableWithDescriptionAndColumnDescription()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithTableDescriptionAndColumnDescriptions();

            var result = Generator.Generate(expression);

            result.ShouldBe("BEGIN EXECUTE IMMEDIATE 'CREATE TABLE TestTable1 (TestColumn1 NVARCHAR2(255), TestColumn2 NUMBER(10,0) NOT NULL)';EXECUTE IMMEDIATE 'COMMENT ON TABLE TestTable1 IS ''TestDescription''';EXECUTE IMMEDIATE 'COMMENT ON COLUMN TestTable1.TestColumn1 IS ''TestColumn1Description''';EXECUTE IMMEDIATE 'COMMENT ON COLUMN TestTable1.TestColumn2 IS ''TestColumn2Description'''; END;");
        }

        [Fact]
        public void CanAlterTableWithDescription()
        {
            var expression = GeneratorTestHelper.GetAlterTableWithDescriptionExpression();

            var result = Generator.Generate(expression);

            result.ShouldBe("COMMENT ON TABLE TestTable1 IS 'TestDescription'");
        }

        [Fact]
        public void CanAlterTableWithoutAnyDescripion()
        {
            var expression = GeneratorTestHelper.GetAlterTable();

            var result = Generator.Generate(expression);

            result.ShouldBe(string.Empty);
        }

        [Fact]
        public void CanCreateColumnWithDescription()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpressionWithDescription();

            var result = Generator.Generate(expression);

            result.ShouldBe("BEGIN EXECUTE IMMEDIATE 'ALTER TABLE TestTable1 ADD TestColumn1 NVARCHAR2(5) NOT NULL';EXECUTE IMMEDIATE 'COMMENT ON COLUMN TestTable1.TestColumn1 IS ''TestColumn1Description'''; END;");
        }

        [Fact]
        public void CanCreateColumnWithoutDescription()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();

            var result = Generator.Generate(expression);

            result.ShouldBe("ALTER TABLE TestTable1 ADD TestColumn1 NVARCHAR2(5) NOT NULL");
        }

        [Fact]
        public void CanAlterColumnWithDescription()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpressionWithDescription();

            var result = Generator.Generate(expression);

            result.ShouldBe("BEGIN EXECUTE IMMEDIATE 'ALTER TABLE TestTable1 MODIFY TestColumn1 NVARCHAR2(20) NOT NULL';EXECUTE IMMEDIATE 'COMMENT ON COLUMN TestTable1.TestColumn1 IS ''TestColumn1Description'''; END;");
        }

        [Fact]
        public void CanAlterColumnWithoutDescription()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpression();

            var result = Generator.Generate(expression);

            result.ShouldBe("ALTER TABLE TestTable1 MODIFY TestColumn1 NVARCHAR2(20) NOT NULL");
        }
    }
}
