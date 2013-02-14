using System;
using FluentMigrator.Exceptions;
using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators.Jet;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.Jet
{
    public class JetDropTableTests : BaseTableDropTests
    {
        private JetGenerator _generator;

        [SetUp]
        public void SetUp()
        {
            _generator = new JetGenerator();
        }

        [Test]
        public override void CanDropColumn()
        {
            var expression = GeneratorTestHelper.GetDeleteColumnExpression();

            string sql = _generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE [TestTable1] DROP COLUMN [TestColumn1]");
        }

        [Test]
        public void CanDropMultipleColumns()
        {
            var expression = GeneratorTestHelper.GetDeleteColumnExpression(new[] { "TestColumn1", "TestColumn2" });

            string sql = _generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE [TestTable1] DROP COLUMN [TestColumn1];" + Environment.NewLine + "ALTER TABLE [TestTable1] DROP COLUMN [TestColumn2]");
        }

        [Test]
        public override void CanDropForeignKey()
        {
            var expression = GeneratorTestHelper.GetDeleteForeignKeyExpression();

            var sql = _generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE [TestTable1] DROP CONSTRAINT [FK_Test]");
        }

        [Test]
        public override void CanDropTable()
        {
            var expression = GeneratorTestHelper.GetDeleteTableExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("DROP TABLE [TestTable1]");
        }

        [Test]
        public override void CanDeleteIndex()
        {
            var expression = GeneratorTestHelper.GetDeleteIndexExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("DROP INDEX [TestIndex] ON [TestTable1]");
        }

        [Test]
        public override void CanDeleteSchema()
        {
            var expression = new DeleteSchemaExpression();
            var result = _generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }

        [Test]
        public void CanDeleteSchemaInStrictMode()
        {
            _generator.compatabilityMode = Runner.CompatabilityMode.STRICT;
            Assert.Throws<DatabaseOperationNotSupportedException>(() => _generator.Generate(new DeleteSchemaExpression()));
        }
    }
}