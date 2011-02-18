
namespace FluentMigrator.Tests.Unit.Generators.Jet
{
    using NUnit.Should;
    using NUnit.Framework;
    using FluentMigrator.Runner.Generators.Jet;
    using System.Data;
    using FluentMigrator.Runner.Generators;

    public class JetAlterTableTests : BaseTableAlterTests
    {
        protected JetGenerator SUT;

        [SetUp]
        public void SetUp()
        {
            SUT = new JetGenerator();
        }

        [Test]
        public override void CanAddColumn()
        {

            var expression = GeneratorTestHelper.GetCreateColumnExpression();

            var result = SUT.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ADD COLUMN [TestColumn1] VARCHAR(5) NOT NULL");
        }

        [Test]
        public override void CanAddDecimalColumn()
        {


            var expression = GeneratorTestHelper.GetCreateDecimalColumnExpression();

            var result = SUT.Generate(expression);

            result.ShouldBe("ALTER TABLE [TestTable1] ADD COLUMN [TestColumn1] DECIMAL(19,2) NOT NULL");
        }

        [Test]
        public override void CanCreateForeignKey()
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();

            var result = SUT.Generate(expression);
            result.ShouldBe(
                "ALTER TABLE [TestTable1] ADD CONSTRAINT FK_Test FOREIGN KEY ([TestColumn1]) REFERENCES [TestTable2] ([TestColumn2])");

        }

        [Test]
        public override void CanCreateMulitColumnForeignKey()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnForeignKeyExpression();

            var result = SUT.Generate(expression);
            result.ShouldBe(
                "ALTER TABLE [TestTable1] ADD CONSTRAINT FK_Test FOREIGN KEY ([TestColumn1],[TestColumn3]) REFERENCES [TestTable2] ([TestColumn2],[TestColumn4])");

        }

        [Test]
        public override void CanRenameColumn()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();
            Assert.Throws<DatabaseOperationNotSupportedExecption>(() => SUT.Generate(expression));
          
        }

        [Test]
        public override void CanRenameTable()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();
            Assert.Throws<DatabaseOperationNotSupportedExecption>(() => SUT.Generate(expression));
        }

        [Test]
        public override void CanAlterColumn()
        {
            var expression = GeneratorTestHelper.GetAlterTableExpression();

            var sql = SUT.Generate(expression);

            sql.ShouldBe("ALTER TABLE [TestTable1] ALTER COLUMN [TestColumn1] VARCHAR(20) NOT NULL");
        }

        [Test]
        public override void CanCreateAutoIncrementColumn()
        {
            throw new System.NotImplementedException();
        }
    }
}
