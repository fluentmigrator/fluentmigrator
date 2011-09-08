using System;
using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.SQLite;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.SQLite
{
    public class SQLiteDropTableTests : BaseTableDropTests
    {
        protected SqliteGenerator generator;

        [SetUp]
        public void Setup()
        {
            generator = new SqliteGenerator();
        }

        [Test]
        public override void CanDropColumn()
        {
            var expression = GeneratorTestHelper.GetDeleteColumnExpression();
            string sql = generator.Generate(expression);
            sql.ShouldBe(String.Empty); //because sqlite doesnt support removing columns
        }

        [Test]
        public override void CanDropForeignKey()
        {
            var expression = GeneratorTestHelper.GetDeleteForeignKeyExpression();
            var result = generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }

        [Test]
        public void CanDropForeignKeyInStrictMode()
        {
            var expression = GeneratorTestHelper.GetDeleteForeignKeyExpression();
            generator.compatabilityMode = Runner.CompatabilityMode.STRICT;
            Assert.Throws<DatabaseOperationNotSupportedException>(() => generator.Generate(expression));
        }

        [Test]
        public override void CanDropTable()
        {
            var expression = GeneratorTestHelper.GetDeleteTableExpression();
            string sql = generator.Generate(expression);
            sql.ShouldBe("DROP TABLE 'TestTable1'");
        }

        [Test]
        public override void CanDeleteIndex()
        {
            var expression = GeneratorTestHelper.GetDeleteIndexExpression();
            string sql = generator.Generate(expression);
            sql.ShouldBe("DROP INDEX 'TestIndex'");
        }

        [Test]
        public override void CanDeleteSchema()
        {
            var expression = new DeleteSchemaExpression();
            var result = generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }

        [Test]
        public void CanDeleteSchemaInStrictMode()
        {
            generator.compatabilityMode = Runner.CompatabilityMode.STRICT;
            Assert.Throws<DatabaseOperationNotSupportedException>(() => generator.Generate(new DeleteSchemaExpression()));
        }
    }
}