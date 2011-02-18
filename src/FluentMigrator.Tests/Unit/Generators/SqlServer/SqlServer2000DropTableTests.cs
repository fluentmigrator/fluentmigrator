using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Should;

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
            var expression = GeneratorTestHelper.GetDeleteColumnExpression();
            var sql = generator.Generate(expression);

            var expectedSql =
                @"
			DECLARE @default sysname, @sql nvarchar(max);

			-- get name of default constraint
			SELECT @default = name 
			FROM sys.default_constraints 
			WHERE parent_object_id = object_id('NewTable')
			AND type = 'D'
			AND parent_column_id = (
				SELECT column_id 
				FROM sys.columns 
				WHERE object_id = object_id('NewTable')
				AND name = 'NewColumn'
			);

			-- create alter table command as string and run it
			SET @sql = N'ALTER TABLE [NewTable] DROP CONSTRAINT ' + @default;
			EXEC sp_executesql @sql;

			-- now we can finally drop column
			ALTER TABLE [NewTable] DROP COLUMN [NewColumn];";

            sql.ShouldBe(expectedSql);
        }

        [Test]
        public override void CanDropForeignKey()
        {
            var expression = GeneratorTestHelper.GetDeleteForeignKeyExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE [TestPrimaryTable] DROP CONSTRAINT FK_Test");
        }

        [Test]
        public override void CanDropTable()
        {
            throw new NotImplementedException();
        }

        [Test]
        public override void CanDeleteIndex()
        {
            var expression = GeneratorTestHelper.GetDeleteIndexExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe("DROP INDEX [IX_TEST] ON [TEST_TABLE]");
        }
    }
}
