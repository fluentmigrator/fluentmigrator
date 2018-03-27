using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer2005
{
    [TestFixture]
    public class SqlServer2005ColumnTests : BaseColumnTests
    {
        protected SqlServer2005Generator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new SqlServer2005Generator();
        }

        [Test]
        public override void CanAlterColumnWithCustomSchema()
        {
            //TODO: This will fail if there are any keys attached 
            var expression = GeneratorTestHelper.GetAlterColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ALTER COLUMN [TestColumn1] NVARCHAR(20) NOT NULL");
        }

        [Test]
        public override void CanAlterColumnWithDefaultSchema()
        {
            //TODO: This will fail if there are any keys attached 
            var expression = GeneratorTestHelper.GetAlterColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ALTER COLUMN [TestColumn1] NVARCHAR(20) NOT NULL");
        }

        [Test]
        public override void CanCreateAutoIncrementColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetAlterColumnAddAutoIncrementExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ALTER COLUMN [TestColumn1] INT NOT NULL IDENTITY(1,1)");
        }

        [Test]
        public override void CanCreateAutoIncrementColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetAlterColumnAddAutoIncrementExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ALTER COLUMN [TestColumn1] INT NOT NULL IDENTITY(1,1)");
        }

        [Test]
        public override void CanCreateColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD [TestColumn1] NVARCHAR(5) NOT NULL");
        }

        [Test]
        public override void CanCreateColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD [TestColumn1] NVARCHAR(5) NOT NULL");
        }

        [Test]
        public override void CanCreateDecimalColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateDecimalColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD [TestColumn1] DECIMAL(19,2) NOT NULL");
        }

        [Test]
        public override void CanCreateDecimalColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateDecimalColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD [TestColumn1] DECIMAL(19,2) NOT NULL");
        }

        [Test]
        public override void CanDropColumnWithCustomSchema()
        {
            //This does not work if it is a primary key
            var expression = GeneratorTestHelper.GetDeleteColumnExpression();
            expression.SchemaName = "TestSchema";

            var expected = "DECLARE @default sysname, @sql nvarchar(max);" + System.Environment.NewLine + System.Environment.NewLine +
                         "-- get name of default constraint" + System.Environment.NewLine +
                         "SELECT @default = name" + System.Environment.NewLine +
                         "FROM sys.default_constraints" + System.Environment.NewLine +
                         "WHERE parent_object_id = object_id('[TestSchema].[TestTable1]')" + System.Environment.NewLine +
                         "AND type = 'D'" + System.Environment.NewLine +
                         "AND parent_column_id = (" + System.Environment.NewLine +
                         "SELECT column_id" + System.Environment.NewLine +
                         "FROM sys.columns" + System.Environment.NewLine +
                         "WHERE object_id = object_id('[TestSchema].[TestTable1]')" + System.Environment.NewLine +
                         "AND name = 'TestColumn1'" + System.Environment.NewLine +
                         ");" + System.Environment.NewLine + System.Environment.NewLine +
                         "-- create alter table command to drop constraint as string and run it" + System.Environment.NewLine +
                         "SET @sql = N'ALTER TABLE [TestSchema].[TestTable1] DROP CONSTRAINT ' + @default;" + System.Environment.NewLine +
                         "EXEC sp_executesql @sql;" + System.Environment.NewLine + System.Environment.NewLine +
                         "-- now we can finally drop column" + System.Environment.NewLine +
                         "ALTER TABLE [TestSchema].[TestTable1] DROP COLUMN [TestColumn1];" + System.Environment.NewLine;

            var result = Generator.Generate(expression);
            result.ShouldBe(expected);
        }

        [Test]
        public override void CanDropColumnWithDefaultSchema()
        {
            //This does not work if it is a primary key
            var expression = GeneratorTestHelper.GetDeleteColumnExpression();

            var expected = "DECLARE @default sysname, @sql nvarchar(max);" + System.Environment.NewLine + System.Environment.NewLine +
                        "-- get name of default constraint" + System.Environment.NewLine +
                        "SELECT @default = name" + System.Environment.NewLine +
                        "FROM sys.default_constraints" + System.Environment.NewLine +
                        "WHERE parent_object_id = object_id('[dbo].[TestTable1]')" + System.Environment.NewLine +
                        "AND type = 'D'" + System.Environment.NewLine +
                        "AND parent_column_id = (" + System.Environment.NewLine +
                        "SELECT column_id" + System.Environment.NewLine +
                        "FROM sys.columns" + System.Environment.NewLine +
                        "WHERE object_id = object_id('[dbo].[TestTable1]')" + System.Environment.NewLine +
                        "AND name = 'TestColumn1'" + System.Environment.NewLine +
                        ");" + System.Environment.NewLine + System.Environment.NewLine +
                        "-- create alter table command to drop constraint as string and run it" + System.Environment.NewLine +
                        "SET @sql = N'ALTER TABLE [dbo].[TestTable1] DROP CONSTRAINT ' + @default;" + System.Environment.NewLine +
                        "EXEC sp_executesql @sql;" + System.Environment.NewLine + System.Environment.NewLine +
                        "-- now we can finally drop column" + System.Environment.NewLine +
                        "ALTER TABLE [dbo].[TestTable1] DROP COLUMN [TestColumn1];" + System.Environment.NewLine;

            var result = Generator.Generate(expression);
            result.ShouldBe(expected);
        }

        [Test]
        public override void CanDropMultipleColumnsWithCustomSchema()
        {
            //This does not work if it is a primary key
            var expression = GeneratorTestHelper.GetDeleteColumnExpression(new [] { "TestColumn1", "TestColumn2" });
            expression.SchemaName = "TestSchema";

            var expected = "DECLARE @default sysname, @sql nvarchar(max);" + System.Environment.NewLine + System.Environment.NewLine +
                         "-- get name of default constraint" + System.Environment.NewLine +
                         "SELECT @default = name" + System.Environment.NewLine +
                         "FROM sys.default_constraints" + System.Environment.NewLine +
                         "WHERE parent_object_id = object_id('[TestSchema].[TestTable1]')" + System.Environment.NewLine +
                         "AND type = 'D'" + System.Environment.NewLine +
                         "AND parent_column_id = (" + System.Environment.NewLine +
                         "SELECT column_id" + System.Environment.NewLine +
                         "FROM sys.columns" + System.Environment.NewLine +
                         "WHERE object_id = object_id('[TestSchema].[TestTable1]')" + System.Environment.NewLine +
                         "AND name = 'TestColumn1'" + System.Environment.NewLine +
                         ");" + System.Environment.NewLine + System.Environment.NewLine +
                         "-- create alter table command to drop constraint as string and run it" + System.Environment.NewLine +
                         "SET @sql = N'ALTER TABLE [TestSchema].[TestTable1] DROP CONSTRAINT ' + @default;" + System.Environment.NewLine +
                         "EXEC sp_executesql @sql;" + System.Environment.NewLine + System.Environment.NewLine +
                         "-- now we can finally drop column" + System.Environment.NewLine +
                         "ALTER TABLE [TestSchema].[TestTable1] DROP COLUMN [TestColumn1];" + System.Environment.NewLine +
                         "GO" + System.Environment.NewLine +
                         "DECLARE @default sysname, @sql nvarchar(max);" + System.Environment.NewLine + System.Environment.NewLine +
                         "-- get name of default constraint" + System.Environment.NewLine +
                         "SELECT @default = name" + System.Environment.NewLine +
                         "FROM sys.default_constraints" + System.Environment.NewLine +
                         "WHERE parent_object_id = object_id('[TestSchema].[TestTable1]')" + System.Environment.NewLine +
                         "AND type = 'D'" + System.Environment.NewLine +
                         "AND parent_column_id = (" + System.Environment.NewLine +
                         "SELECT column_id" + System.Environment.NewLine +
                         "FROM sys.columns" + System.Environment.NewLine +
                         "WHERE object_id = object_id('[TestSchema].[TestTable1]')" + System.Environment.NewLine +
                         "AND name = 'TestColumn2'" + System.Environment.NewLine +
                         ");" + System.Environment.NewLine + System.Environment.NewLine +
                         "-- create alter table command to drop constraint as string and run it" + System.Environment.NewLine +
                         "SET @sql = N'ALTER TABLE [TestSchema].[TestTable1] DROP CONSTRAINT ' + @default;" + System.Environment.NewLine +
                         "EXEC sp_executesql @sql;" + System.Environment.NewLine + System.Environment.NewLine +
                         "-- now we can finally drop column" + System.Environment.NewLine +
                         "ALTER TABLE [TestSchema].[TestTable1] DROP COLUMN [TestColumn2];" + System.Environment.NewLine;

            var result = Generator.Generate(expression);
            result.ShouldBe(expected);
        }

        [Test]
        public override void CanDropMultipleColumnsWithDefaultSchema()
        {
            //This does not work if it is a primary key
            var expression = GeneratorTestHelper.GetDeleteColumnExpression(new [] { "TestColumn1", "TestColumn2" });

            var expected = "DECLARE @default sysname, @sql nvarchar(max);" + System.Environment.NewLine + System.Environment.NewLine +
                        "-- get name of default constraint" + System.Environment.NewLine +
                        "SELECT @default = name" + System.Environment.NewLine +
                        "FROM sys.default_constraints" + System.Environment.NewLine +
                        "WHERE parent_object_id = object_id('[dbo].[TestTable1]')" + System.Environment.NewLine + "" +
                        "AND type = 'D'" + System.Environment.NewLine +
                        "AND parent_column_id = (" + System.Environment.NewLine +
                        "SELECT column_id" + System.Environment.NewLine +
                        "FROM sys.columns" + System.Environment.NewLine +
                        "WHERE object_id = object_id('[dbo].[TestTable1]')" + System.Environment.NewLine +
                        "AND name = 'TestColumn1'" + System.Environment.NewLine +
                        ");" + System.Environment.NewLine + System.Environment.NewLine +
                        "-- create alter table command to drop constraint as string and run it" + System.Environment.NewLine +
                        "SET @sql = N'ALTER TABLE [dbo].[TestTable1] DROP CONSTRAINT ' + @default;" + System.Environment.NewLine +
                        "EXEC sp_executesql @sql;" + System.Environment.NewLine + System.Environment.NewLine +
                        "-- now we can finally drop column" + System.Environment.NewLine +
                        "ALTER TABLE [dbo].[TestTable1] DROP COLUMN [TestColumn1];" + System.Environment.NewLine +
                        "GO" + System.Environment.NewLine +
                        "DECLARE @default sysname, @sql nvarchar(max);" + System.Environment.NewLine + System.Environment.NewLine +
                        "-- get name of default constraint" + System.Environment.NewLine +
                        "SELECT @default = name" + System.Environment.NewLine +
                        "FROM sys.default_constraints" + System.Environment.NewLine +
                        "WHERE parent_object_id = object_id('[dbo].[TestTable1]')" + System.Environment.NewLine + "" +
                        "AND type = 'D'" + System.Environment.NewLine +
                        "AND parent_column_id = (" + System.Environment.NewLine +
                        "SELECT column_id" + System.Environment.NewLine +
                        "FROM sys.columns" + System.Environment.NewLine +
                        "WHERE object_id = object_id('[dbo].[TestTable1]')" + System.Environment.NewLine +
                        "AND name = 'TestColumn2'" + System.Environment.NewLine +
                        ");" + System.Environment.NewLine + System.Environment.NewLine +
                        "-- create alter table command to drop constraint as string and run it" + System.Environment.NewLine +
                        "SET @sql = N'ALTER TABLE [dbo].[TestTable1] DROP CONSTRAINT ' + @default;" + System.Environment.NewLine +
                        "EXEC sp_executesql @sql;" + System.Environment.NewLine + System.Environment.NewLine +
                        "-- now we can finally drop column" + System.Environment.NewLine +
                        "ALTER TABLE [dbo].[TestTable1] DROP COLUMN [TestColumn2];" + System.Environment.NewLine;

            var result = Generator.Generate(expression);
            result.ShouldBe(expected);
        }

        [Test]
        public override void CanRenameColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("sp_rename '[TestSchema].[TestTable1].[TestColumn1]', 'TestColumn2'");
        }

        [Test]
        public override void CanRenameColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("sp_rename '[dbo].[TestTable1].[TestColumn1]', 'TestColumn2'");
        }
    }
}
