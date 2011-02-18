using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using FluentMigrator.Runner.Generators.MySql;
using NUnit.Should;
using FluentMigrator.Runner.Generators;

namespace FluentMigrator.Tests.Unit.Generators.MySql
{
    public class MySqlAlterTableTestscs : BaseTableAlterTests
    {
        protected MySqlGenerator generator;

        [SetUp]
        public void Setup()
        {
            generator = new MySqlGenerator();
        }

        [Test]
        public override void CanAddColumn()
        {

            var expression = GeneratorTestHelper.GetCreateColumnExpression();
        

            var sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE `TestTable1` ADD `TestColumn1` VARCHAR(5) NOT NULL");
        }

        [Test]
        public override void CanAddDecimalColumn()
        {

            var expression = GeneratorTestHelper.GetCreateDecimalColumnExpression();
    

            var sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE `NewTable` ADD `TestColumn1` DECIMAL(19,2) NOT NULL");
        }

        [Test]
        public override void CanRenameColumn()
        {
                // MySql does not appear to have a way to change column without re-specifying the existing column definition
            var expression = GeneratorTestHelper.GetRenameColumnExpression();
            	Assert.Throws<DatabaseOperationNotSupportedExecption>(() =>generator.Generate(expression));
        }

        [Test]
        public override void CanRenameTable()
        {
            var expression = GeneratorTestHelper.GetRenameTableExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe("RENAME TABLE `TestTable1` TO `TestTable2`");
        }

        [Test]
        public override void CanAlterColumn()
        {
            throw new NotImplementedException();
        }

        [Test]
        public override void CanCreateForeignKey()
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
                    var sql = generator.Generate(expression);
            sql.ShouldBe(
                "ALTER TABLE `TestTable1` ADD CONSTRAINT `FK_Test` FOREIGN KEY (`TestColumn1,`TestColumn3`) REFERENCES `TestTable2` (`TestColumn2`,`TestColumn4`)");

        }

        [Test]
        public override void CanCreateMulitColumnForeignKey()
        {
            throw new NotImplementedException();
        }

        [Test]
        public override void CanCreateAutoIncrementColumn()
        {
            throw new NotImplementedException();
        }
    }
}
