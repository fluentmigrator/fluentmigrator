using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Should;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Expressions;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer
{

    public class SqlServer2000DropTableTests : BaseTableDropTests
    {
        protected SqlServer2000Generator generator;

        [SetUp]
        public void Setup()
        {
            generator = new SqlServer2000Generator();


        }

        [Test]
        public override void CanDropColumn()
        {
            //This does not work if it is a primary key
            var expression = GeneratorTestHelper.GetDeleteColumnExpression();
            var sql = generator.Generate(expression);

            var expectedSql = "\n\t\t\tDECLARE @default sysname, @sql nvarchar(max);\n\n\t\t\t-- get name of default constraint\n\t\t\tSELECT @default = name\n\t\t\tFROM sys.default_constraints \n\t\t\tWHERE parent_object_id = object_id('[TestTable1]')\n\t\t\tAND type = 'D'\n\t\t\tAND parent_column_id = (\n\t\t\t\tSELECT column_id \n\t\t\t\tFROM sys.columns \n\t\t\t\tWHERE object_id = object_id('[TestTable1]')\n\t\t\t\tAND name = '[TestColumn1]'\n\t\t\t);\n\n\t\t\t-- create alter table command as string and run it\n\t\t\tSET @sql = N'ALTER TABLE [TestTable1] DROP CONSTRAINT ' + @default;\n\t\t\tEXEC sp_executesql @sql;\n\n\t\t\t-- now we can finally drop column\n\t\t\tALTER TABLE [TestTable1] DROP COLUMN [TestColumn1];";

            sql.ShouldBe(expectedSql);
        }

        [Test]
        public override void CanDropForeignKey()
        {
            var expression = GeneratorTestHelper.GetDeleteForeignKeyExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE [TestTable1] DROP CONSTRAINT [FK_Test]");
        }

        [Test]
        public override void CanDropTable()
        {
            var expression = GeneratorTestHelper.GetDeleteTableExpression();

            var sql = generator.Generate(expression);
            sql.ShouldBe("DROP TABLE [TestTable1]");
        }

        [Test]
        public override void CanDeleteIndex()
        {
            var expression = GeneratorTestHelper.GetDeleteIndexExpression();

            var sql = generator.Generate(expression);
            sql.ShouldBe("DROP INDEX [TestTable1].[TestIndex]");
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
            Assert.Throws<DatabaseOperationNotSupportedExecption>(() => generator.Generate(new DeleteSchemaExpression()));
        }
    }
}
