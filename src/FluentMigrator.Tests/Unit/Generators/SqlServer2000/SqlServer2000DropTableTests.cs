using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer
{
    public class SqlServer2000DropTableTests : BaseTableDropTests
    {
        private SqlServer2000Generator _generator;

        [SetUp]
        public void Setup()
        {
            _generator = new SqlServer2000Generator();
        }

        [Test]
        public override void CanDropColumn()
        {
            //This does not work if it is a primary key
            var expression = GeneratorTestHelper.GetDeleteColumnExpression();
            var sql = _generator.Generate(expression);

            var expectedSql =
                @"
            DECLARE @default sysname, @sql nvarchar(max);

            -- get name of default constraint
            SELECT @default = name
            FROM sys.default_constraints 
            WHERE parent_object_id = object_id('[TestTable1]')
            AND type = 'D'
            AND parent_column_id = (
                SELECT column_id 
                FROM sys.columns 
                WHERE object_id = object_id('[TestTable1]')
                AND name = '[TestColumn1]'
            );

            -- create alter table command as string and run it
            SET @sql = N'ALTER TABLE [TestTable1] DROP CONSTRAINT ' + @default;
            EXEC sp_executesql @sql;

            -- now we can finally drop column
            ALTER TABLE [TestTable1] DROP COLUMN [TestColumn1];";

            sql.ShouldBe(expectedSql);
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
            sql.ShouldBe("DROP INDEX [TestTable1].[TestIndex]");
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