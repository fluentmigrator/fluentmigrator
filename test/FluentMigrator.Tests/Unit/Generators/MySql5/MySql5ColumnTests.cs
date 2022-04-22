using System;

using FluentMigrator.Runner.Generators.MySql;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.MySql5
{
    [TestFixture]
    public class MySql5ColumnTest
    {
        protected MySql4Generator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new MySql5Generator();
        }

        [Test]
        public void CanAlterColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` MODIFY COLUMN `TestColumn1` NVARCHAR(20) NOT NULL");
        }

        [Test]
        public void CanAlterColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` MODIFY COLUMN `TestColumn1` NVARCHAR(20) NOT NULL");
        }

        [Test]
        public void CanCreateColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` ADD COLUMN `TestColumn1` NVARCHAR(5) NOT NULL");
        }

        [Test]
        public void CanCreateColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` ADD COLUMN `TestColumn1` NVARCHAR(5) NOT NULL");
        }

        [Test]
        public void CanAlterColumnWithDescription()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpressionWithDescription();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` MODIFY COLUMN `TestColumn1` NVARCHAR(20) NOT NULL COMMENT 'Description:TestColumn1Description'");
        }

        [Test]
        public void CanAlterColumnWithDescriptionWithAdditionalDescriptions()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpressionWithDescriptionWithAdditionalDescriptions();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` MODIFY COLUMN `TestColumn1` NVARCHAR(20) NOT NULL COMMENT 'Description:TestColumn1Description" + Environment.NewLine +
                            "AdditionalColumnDescriptionKey1:AdditionalColumnDescriptionValue1'");
        }

        [Test]
        public void CanCreateColumnWithDescription()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpressionWithDescription();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` ADD COLUMN `TestColumn1` NVARCHAR(5) NOT NULL COMMENT 'Description:TestColumn1Description'");
        }

        [Test]
        public void CanCreateColumnWithDescriptionWithAdditionalDescriptions()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpressionWithDescriptionWithAdditionalDescriptions();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` ADD COLUMN `TestColumn1` NVARCHAR(5) NOT NULL COMMENT 'Description:TestColumn1Description" + Environment.NewLine +
                            "AdditionalColumnDescriptionKey1:AdditionalColumnDescriptionValue1'");
        }
    }
}
