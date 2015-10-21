using FluentMigrator.Runner.Generators.SqlAnywhere;
using NUnit.Framework;
using Shouldly;
using System.Text;

namespace FluentMigrator.Tests.Unit.Generators.SqlAnywhere
{
    [TestFixture]
    public class SqlAnywhere16ColumnTests : BaseColumnTests
    {
        protected SqlAnywhere16Generator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new SqlAnywhere16Generator();
        }

        [Test]
        [Category("SQLAnwyere"), Category("SQLAnwyere16"), Category("Generator"), Category("Column")]
        public override void CanAlterColumnWithCustomSchema()
        {
            //TODO: This will fail if there are any keys attached 
            var expression = GeneratorTestHelper.GetAlterColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ALTER [TestColumn1] NVARCHAR(20) NOT NULL");
        }

        [Test]
        [Category("SQLAnwyere"), Category("SQLAnwyere16"), Category("Generator"), Category("Column")]
        public override void CanAlterColumnWithDefaultSchema()
        {
            //TODO: This will fail if there are any keys attached 
            var expression = GeneratorTestHelper.GetAlterColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ALTER [TestColumn1] NVARCHAR(20) NOT NULL");
        }

        [Test]
        [Category("SQLAnwyere"), Category("SQLAnwyere16"), Category("Generator"), Category("Column")]
        public override void CanCreateAutoIncrementColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetAlterColumnAddAutoIncrementExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ALTER [TestColumn1] INTEGER NOT NULL DEFAULT AUTOINCREMENT");
        }

        [Test]
        [Category("SQLAnwyere"), Category("SQLAnwyere16"), Category("Generator"), Category("Column")]
        public override void CanCreateAutoIncrementColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetAlterColumnAddAutoIncrementExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ALTER [TestColumn1] INTEGER NOT NULL DEFAULT AUTOINCREMENT");
        }

        [Test]
        [Category("SQLAnwyere"), Category("SQLAnwyere16"), Category("Generator"), Category("Column")]
        public override void CanCreateColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD [TestColumn1] NVARCHAR(5) NOT NULL");
        }

        [Test]
        [Category("SQLAnwyere"), Category("SQLAnwyere16"), Category("Generator"), Category("Column")]
        public override void CanCreateColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD [TestColumn1] NVARCHAR(5) NOT NULL");
        }

        [Test]
        [Category("SQLAnwyere"), Category("SQLAnwyere16"), Category("Generator"), Category("Column")]
        public override void CanCreateDecimalColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateDecimalColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD [TestColumn1] DECIMAL(19,2) NOT NULL");
        }

        [Test]
        [Category("SQLAnwyere"), Category("SQLAnwyere16"), Category("Generator"), Category("Column")]
        public override void CanCreateDecimalColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateDecimalColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD [TestColumn1] DECIMAL(19,2) NOT NULL");
        }

        [Test]
        [Category("SQLAnwyere"), Category("SQLAnwyere16"), Category("Generator"), Category("Column")]
        public override void CanDropColumnWithCustomSchema()
        {
            //This does not work if it is a primary key
            var expression = GeneratorTestHelper.GetDeleteColumnExpression();
            expression.SchemaName = "TestSchema";

            var expected = "ALTER TABLE [TestSchema].[TestTable1] DROP [TestColumn1]\r\n";

            var result = Generator.Generate(expression);
            result.ShouldBe(expected);
        }

        [Test]
        [Category("SQLAnwyere"), Category("SQLAnwyere16"), Category("Generator"), Category("Column")]
        public override void CanDropColumnWithDefaultSchema()
        {
            //This does not work if it is a primary key
            var expression = GeneratorTestHelper.GetDeleteColumnExpression();

            var expected = "ALTER TABLE [dbo].[TestTable1] DROP [TestColumn1]\r\n";

            var result = Generator.Generate(expression);
            result.ShouldBe(expected);
        }

        [Test]
        [Category("SQLAnwyere"), Category("SQLAnwyere16"), Category("Generator"), Category("Column")]
        public override void CanDropMultipleColumnsWithCustomSchema()
        {
            //This does not work if it is a primary key
            var expression = GeneratorTestHelper.GetDeleteColumnExpression(new[] { "TestColumn1", "TestColumn2" });
            expression.SchemaName = "TestSchema";

            StringBuilder expected = new StringBuilder();
            expected.AppendLine("ALTER TABLE [TestSchema].[TestTable1] DROP [TestColumn1]");
            expected.AppendLine("ALTER TABLE [TestSchema].[TestTable1] DROP [TestColumn2]");

            var result = Generator.Generate(expression);
            result.ShouldBe(expected.ToString());
        }

        [Test]
        [Category("SQLAnwyere"), Category("SQLAnwyere16"), Category("Generator"), Category("Column")]
        public override void CanDropMultipleColumnsWithDefaultSchema()
        {
            //This does not work if it is a primary key
            var expression = GeneratorTestHelper.GetDeleteColumnExpression(new[] { "TestColumn1", "TestColumn2" });

            StringBuilder expected = new StringBuilder();
            expected.AppendLine("ALTER TABLE [dbo].[TestTable1] DROP [TestColumn1]");
            expected.AppendLine("ALTER TABLE [dbo].[TestTable1] DROP [TestColumn2]");

            var result = Generator.Generate(expression);
            result.ShouldBe(expected.ToString());
        }

        [Test]
        [Category("SQLAnwyere"), Category("SQLAnwyere16"), Category("Generator"), Category("Column")]
        public override void CanRenameColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] RENAME [TestColumn1] TO [TestColumn2]");
        }

        [Test]
        [Category("SQLAnwyere"), Category("SQLAnwyere16"), Category("Generator"), Category("Column")]
        public override void CanRenameColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] RENAME [TestColumn1] TO [TestColumn2]");
        }
    }
}