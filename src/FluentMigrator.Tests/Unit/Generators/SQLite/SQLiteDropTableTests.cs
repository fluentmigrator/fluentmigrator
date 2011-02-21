using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using FluentMigrator.Runner.Generators.SQLite;
using FluentMigrator.Runner.Generators;
using NUnit.Should;
using FluentMigrator.Expressions;

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
            sql.ShouldBe("ALTER TABLE 'TestTable1' DROP COLUMN 'TestColumn1'");

        }

        [Test]
        public override void CanDropForeignKey()
        {
            var expression = GeneratorTestHelper.GetDeleteForeignKeyExpression();
            Assert.Throws<DatabaseOperationNotSupportedExecption>(() => generator.Generate(expression));
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

        public override void CanDeleteSchema()
        {
            Assert.Throws<DatabaseOperationNotSupportedExecption>(() => generator.Generate(new DeleteSchemaExpression()));
        }
    }
}
