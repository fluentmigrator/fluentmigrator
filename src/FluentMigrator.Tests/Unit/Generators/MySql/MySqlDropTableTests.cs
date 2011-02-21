using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using FluentMigrator.Runner.Generators.MySql;
using NUnit.Should;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Expressions;

namespace FluentMigrator.Tests.Unit.Generators.MySql
{
    public class MySqlDropTableTests : BaseTableDropTests
    {
        protected MySqlGenerator generator;

        [SetUp]
        public void Setup()
        {
            generator = new MySqlGenerator();
        }

        [Test]
        public override void CanDropColumn()
        {

            var expression = GeneratorTestHelper.GetDeleteColumnExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE `TestTable1` DROP COLUMN `TestColumn1`");
        }

        [Test]
        public override void CanDropForeignKey()
        {
            var expression = GeneratorTestHelper.GetDeleteForeignKeyExpression();
               var sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE `TestTable1` DROP FOREIGN KEY `FK_Test`");
        }

        [Test]
        public override void CanDropTable()
        {
            var expression = GeneratorTestHelper.GetDeleteTableExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe("DROP TABLE `TestTable1`");
        }

        [Test]
        public override void CanDeleteIndex()
        {
            var expression = GeneratorTestHelper.GetDeleteIndexExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe("DROP INDEX `TestIndex`");
        }

        public override void CanDeleteSchema()
        {
            Assert.Throws<DatabaseOperationNotSupportedExecption>(() => generator.Generate(new DeleteSchemaExpression()));
        }
    }
}
