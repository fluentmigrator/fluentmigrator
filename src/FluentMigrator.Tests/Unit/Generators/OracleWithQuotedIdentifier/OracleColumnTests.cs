using FluentMigrator.Exceptions;
using FluentMigrator.Runner.Generators.Oracle;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.OracleWithQuotedIdentifier
{
    [TestFixture]
    public class OracleColumnTests
    {
        protected OracleGenerator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new OracleGenerator(useQuotedIdentifiers: true);
        }

        [Test]
        public void CanAlterColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" MODIFY \"TestColumn1\" NVARCHAR2(20) NOT NULL");
        }

        [Test]
        public void CanAlterColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" MODIFY \"TestColumn1\" NVARCHAR2(20) NOT NULL");
        }

        [Test]
        public void CanCreateAutoIncrementColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetAlterColumnAddAutoIncrementExpression();
            expression.SchemaName = "TestSchema";

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(expression));
        }

        [Test]
        public void CanCreateAutoIncrementColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetAlterColumnAddAutoIncrementExpression();

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(expression));
        }

        [Test]
        public void CanCreateColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" ADD \"TestColumn1\" NVARCHAR2(5) NOT NULL");
        }

        [Test]
        public void CanCreateColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" ADD \"TestColumn1\" NVARCHAR2(5) NOT NULL");
        }

        [Test]
        public void CanCreateDecimalColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateDecimalColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" ADD \"TestColumn1\" NUMBER(19,2) NOT NULL");
        }

        [Test]
        public void CanCreateDecimalColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateDecimalColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" ADD \"TestColumn1\" NUMBER(19,2) NOT NULL");
        }

        [Test]
        public void CanDropColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" DROP COLUMN \"TestColumn1\"");
        }

        [Test]
        public void CanDropColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" DROP COLUMN \"TestColumn1\"");
        }

        [Test]
        public void CanDropMultipleColumnsWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteColumnExpression(new[] { "\"TestColumn1\"", "\"TestColumn2\"" });
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" DROP COLUMN \"TestColumn1\";" + System.Environment.NewLine + "ALTER TABLE \"TestTable1\" DROP COLUMN \"TestColumn2\"");
        }

        [Test]
        public void CanDropMultipleColumnsWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteColumnExpression(new[] { "\"TestColumn1\"", "\"TestColumn2\"" });

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" DROP COLUMN \"TestColumn1\";" + System.Environment.NewLine + "ALTER TABLE \"TestTable1\" DROP COLUMN \"TestColumn2\"");
        }

        [Test]
        public void CanRenameColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" RENAME COLUMN \"TestColumn1\" TO \"TestColumn2\"");
        }

        [Test]
        public void CanRenameColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" RENAME COLUMN \"TestColumn1\" TO \"TestColumn2\"");
        }
    }
}