using System.Data;
using FluentMigrator.Runner.Generators.Jet;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.Jet
{
    [TestFixture]
    public class JetConstraintsTests
    {
        protected JetGenerator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new JetGenerator();
        }

        [Test]
        public void CanCreateNamedForeignKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedForeignKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ADD CONSTRAINT [FK_Test] FOREIGN KEY ([TestColumn1]) REFERENCES [TestTable2] ([TestColumn2])");
        }

        [Test]
        public void CanCreateNamedForeignKeyWithOnDeleteAndOnUpdateOptions()
        {
            var expression = GeneratorTestHelper.GetCreateNamedForeignKeyExpression();
            expression.ForeignKey.OnDelete = Rule.Cascade;
            expression.ForeignKey.OnUpdate = Rule.SetDefault;

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ADD CONSTRAINT [FK_Test] FOREIGN KEY ([TestColumn1]) REFERENCES [TestTable2] ([TestColumn2]) ON DELETE CASCADE ON UPDATE SET DEFAULT");
        }

        [TestCase(Rule.SetDefault, "SET DEFAULT"), TestCase(Rule.SetNull, "SET NULL"), TestCase(Rule.Cascade, "CASCADE")]
        public void CanCreateNamedForeignKeyWithOnDeleteOptions(Rule rule, string output)
        {
            var expression = GeneratorTestHelper.GetCreateNamedForeignKeyExpression();
            expression.ForeignKey.OnDelete = rule;

            var result = Generator.Generate(expression);
            result.ShouldBe(string.Format("ALTER TABLE [TestTable1] ADD CONSTRAINT [FK_Test] FOREIGN KEY ([TestColumn1]) REFERENCES [TestTable2] ([TestColumn2]) ON DELETE {0}", output));
        }

        [TestCase(Rule.SetDefault, "SET DEFAULT"), TestCase(Rule.SetNull, "SET NULL"), TestCase(Rule.Cascade, "CASCADE")]
        public void CanCreateNamedForeignKeyWithOnUpdateOptions(Rule rule, string output)
        {
            var expression = GeneratorTestHelper.GetCreateNamedForeignKeyExpression();
            expression.ForeignKey.OnUpdate = rule;

            var result = Generator.Generate(expression);
            result.ShouldBe(string.Format("ALTER TABLE [TestTable1] ADD CONSTRAINT [FK_Test] FOREIGN KEY ([TestColumn1]) REFERENCES [TestTable2] ([TestColumn2]) ON UPDATE {0}", output));
        }

        [Test]
        public void CanCreateNamedMultiColumnForeignKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnForeignKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ADD CONSTRAINT [FK_Test] FOREIGN KEY ([TestColumn1], [TestColumn3]) REFERENCES [TestTable2] ([TestColumn2], [TestColumn4])");
        }

        [Test]
        public void CanCreateNamedMultiColumnUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnUniqueConstraintExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ADD CONSTRAINT [TESTUNIQUECONSTRAINT] UNIQUE ([TestColumn1], [TestColumn2])");
        }

        [Test]
        public void CanDropForeignKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteForeignKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] DROP CONSTRAINT [FK_Test]");
        }
    }
}
