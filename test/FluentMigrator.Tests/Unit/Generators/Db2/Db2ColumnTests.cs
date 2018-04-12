using System;
using System.Linq;

using FluentMigrator.Runner.Generators.DB2;
using FluentMigrator.Runner.Generators.DB2.iSeries;

using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.Db2
{
    [TestFixture]
    public class Db2ColumnTests : BaseColumnTests
    {
        public Db2Generator Generator
        {
            get; set;
        }

        [Test]
        public override void CanAlterColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpression();
            expression.Column.IsNullable = null;
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestSchema.TestTable1 ALTER COLUMN TestColumn1 SET DATA TYPE VARGRAPHIC(20) CCSID 1200 NOT NULL");
        }

        [Test]
        public override void CanAlterColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpression();
            expression.Column.IsNullable = null;

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 ALTER COLUMN TestColumn1 SET DATA TYPE VARGRAPHIC(20) CCSID 1200 NOT NULL");
        }

        [Test]
        public override void CanCreateAutoIncrementColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetAlterColumnAddAutoIncrementExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }

        [Test]
        public override void CanCreateAutoIncrementColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetAlterColumnAddAutoIncrementExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }

        [Test]
        public override void CanCreateColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestSchema.TestTable1 ADD COLUMN TestColumn1 VARGRAPHIC(5) CCSID 1200 NOT NULL DEFAULT");
        }

        [Test]
        public override void CanCreateColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 ADD COLUMN TestColumn1 VARGRAPHIC(5) CCSID 1200 NOT NULL DEFAULT");
        }

        [Test]
        public override void CanCreateColumnWithSystemMethodAndCustomSchema()
        {
            var expressions = GeneratorTestHelper.GetCreateColumnWithSystemMethodExpression("TestSchema");
            var result = string.Join(Environment.NewLine, expressions.Select(x => (string)Generator.Generate((dynamic)x)));
            result.ShouldBe(
                @"ALTER TABLE TestSchema.TestTable1 ADD COLUMN TestColumn1 VARGRAPHIC(5) CCSID 1200 DEFAULT" + Environment.NewLine +
                @"UPDATE TestSchema.TestTable1 SET TestColumn1 = CURRENT_TIMESTAMP");
        }

        [Test]
        public override void CanCreateColumnWithSystemMethodAndDefaultSchema()
        {
            var expressions = GeneratorTestHelper.GetCreateColumnWithSystemMethodExpression();
            var result = string.Join(Environment.NewLine, expressions.Select(x => (string)Generator.Generate((dynamic)x)));
            result.ShouldBe(
                @"ALTER TABLE TestTable1 ADD COLUMN TestColumn1 VARGRAPHIC(5) CCSID 1200 DEFAULT" + Environment.NewLine +
                @"UPDATE TestTable1 SET TestColumn1 = CURRENT_TIMESTAMP");
        }

        [Test]
        public override void CanCreateDecimalColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateDecimalColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestSchema.TestTable1 ADD COLUMN TestColumn1 DECIMAL(19,2) NOT NULL DEFAULT");
        }

        [Test]
        public override void CanCreateDecimalColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateDecimalColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 ADD COLUMN TestColumn1 DECIMAL(19,2) NOT NULL DEFAULT");
        }

        [Test]
        public override void CanDropColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestSchema.TestTable1 DROP COLUMN TestColumn1");
        }

        [Test]
        public override void CanDropColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 DROP COLUMN TestColumn1");
        }

        [Test]
        public override void CanDropMultipleColumnsWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteColumnExpression(new[] { "TestColumn1", "TestColumn2" });
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestSchema.TestTable1 DROP COLUMN TestColumn1 DROP COLUMN TestColumn2");
        }

        [Test]
        public override void CanDropMultipleColumnsWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteColumnExpression(new[] { "TestColumn1", "TestColumn2" });

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 DROP COLUMN TestColumn1 DROP COLUMN TestColumn2");
        }

        [Test]
        public override void CanRenameColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }

        [Test]
        public override void CanRenameColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }

        [SetUp]
        public void SetUp()
        {
            Generator = new Db2Generator(new Db2ISeriesQuoter());
        }
    }
}
