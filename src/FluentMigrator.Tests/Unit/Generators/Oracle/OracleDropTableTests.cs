using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using FluentMigrator.Runner.Generators.Oracle;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.Oracle
{
    public class OracleDropTableTests : BaseTableDropTests
    {
        private OracleGenerator generator;

        [SetUp]
        public void Setup()
        {
            generator = new OracleGenerator();
        }

        [Test]
        public override void CanDropColumn()
        {
            var expression = GeneratorTestHelper.GetDeleteColumnExpression();
            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE TestTable1 DROP COLUMN TestColumn1");
        }

        [Test]
        public override void CanDropForeignKey()
        {
            var expression = GeneratorTestHelper.GetDeleteForeignKeyExpression();
            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE TestTable1 DROP CONSTRAINT FK_Test");
        }

        [Test]
        public override void CanDropTable()
        {
            var expression = GeneratorTestHelper.GetDeleteTableExpression();
            string sql = generator.Generate(expression);
            sql.ShouldBe("DROP TABLE TestTable1");
        }

        [Test]
        public override void CanDeleteIndex()
        {
            var expression = GeneratorTestHelper.GetDeleteIndexExpression();
            string sql = generator.Generate(expression);
            sql.ShouldBe("DROP INDEX IX_TEST");
        }
    }
}
