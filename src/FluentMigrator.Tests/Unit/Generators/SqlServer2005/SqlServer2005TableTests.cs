using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer2005
{
    [TestFixture]
    public class SqlServer2005TableTests
    {
        protected SqlServer2005Generator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new SqlServer2005Generator();
        }

        [Test]
        public void CanCreateTableWithCustomColumnTypeWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.SchemaName = "TestSchema";
            expression.Columns[0].IsPrimaryKey = true;
            expression.Columns[1].Type = null;
            expression.Columns[1].CustomType = "[timestamp]";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [TestSchema].[TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL, [TestColumn2] [timestamp] NOT NULL, PRIMARY KEY ([TestColumn1]))");
        }

        [Test]
        public void CanCreateTableWithCustomColumnTypeWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].IsPrimaryKey = true;
            expression.Columns[1].Type = null;
            expression.Columns[1].CustomType = "[timestamp]";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [dbo].[TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL, [TestColumn2] [timestamp] NOT NULL, PRIMARY KEY ([TestColumn1]))");
        }

        [Test]
        public void CanCreateTableWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [TestSchema].[TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL, [TestColumn2] INT NOT NULL)");
        }

        [Test]
        public void CanCreateTableWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [dbo].[TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL, [TestColumn2] INT NOT NULL)");
        }

        [Test]
        public void CanCreateTableWithDefaultValueExplicitlySetToNullWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.SchemaName = "TestSchema";
            expression.Columns[0].DefaultValue = null;
            expression.Columns[0].TableName = expression.TableName;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [TestSchema].[TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL CONSTRAINT [DF_TestTable1_TestColumn1] DEFAULT NULL, [TestColumn2] INT NOT NULL)");
        }

        [Test]
        public void CanCreateTableWithDefaultValueExplicitlySetToNullWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].DefaultValue = null;
            expression.Columns[0].TableName = expression.TableName;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [dbo].[TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL CONSTRAINT [DF_TestTable1_TestColumn1] DEFAULT NULL, [TestColumn2] INT NOT NULL)");
        }

        [Test]
        public void CanCreateTableWithDefaultValueWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [TestSchema].[TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL CONSTRAINT [DF_TestTable1_TestColumn1] DEFAULT 'Default', [TestColumn2] INT NOT NULL CONSTRAINT [DF_TestTable1_TestColumn2] DEFAULT 0)");
        }

        [Test]
        public void CanCreateTableWithDefaultValueWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [dbo].[TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL CONSTRAINT [DF_TestTable1_TestColumn1] DEFAULT 'Default', [TestColumn2] INT NOT NULL CONSTRAINT [DF_TestTable1_TestColumn2] DEFAULT 0)");
        }

        [Test]
        public void CanCreateTableWithIdentityWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [TestSchema].[TestTable1] ([TestColumn1] INT NOT NULL IDENTITY(1,1), [TestColumn2] INT NOT NULL)");
        }

        [Test]
        public void CanCreateTableWithIdentityWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [dbo].[TestTable1] ([TestColumn1] INT NOT NULL IDENTITY(1,1), [TestColumn2] INT NOT NULL)");
        }

        [Test]
        public void CanCreateTableWithMultiColumnPrimaryKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithMultiColumnPrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [dbo].[TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL, [TestColumn2] INT NOT NULL, PRIMARY KEY ([TestColumn1], [TestColumn2]))");
        }

        [Test]
        public void CanCreateTableWithMultiColumnPrimaryKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithMultiColumnPrimaryKeyExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [TestSchema].[TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL, [TestColumn2] INT NOT NULL, PRIMARY KEY ([TestColumn1], [TestColumn2]))");
        }

        [Test]
        public void CanCreateTableWithNamedMultiColumnPrimaryKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedMultiColumnPrimaryKeyExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [TestSchema].[TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL, [TestColumn2] INT NOT NULL, CONSTRAINT [TestKey] PRIMARY KEY ([TestColumn1], [TestColumn2]))");
        }

        [Test]
        public void CanCreateTableWithNamedMultiColumnPrimaryKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedMultiColumnPrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [dbo].[TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL, [TestColumn2] INT NOT NULL, CONSTRAINT [TestKey] PRIMARY KEY ([TestColumn1], [TestColumn2]))");
        }

        [Test]
        public void CanCreateTableWithNamedPrimaryKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedPrimaryKeyExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [TestSchema].[TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL, [TestColumn2] INT NOT NULL, CONSTRAINT [TestKey] PRIMARY KEY ([TestColumn1]))");
        }

        [Test]
        public void CanCreateTableWithNamedPrimaryKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedPrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [dbo].[TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL, [TestColumn2] INT NOT NULL, CONSTRAINT [TestKey] PRIMARY KEY ([TestColumn1]))");
        }

        [Test]
        public void CanCreateTableWithNullableFieldWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.SchemaName = "TestSchema";
            expression.Columns[0].IsNullable = true;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [TestSchema].[TestTable1] ([TestColumn1] NVARCHAR(255), [TestColumn2] INT NOT NULL)");
        }

        [Test]
        public void CanCreateTableWithNullableFieldWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].IsNullable = true;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [dbo].[TestTable1] ([TestColumn1] NVARCHAR(255), [TestColumn2] INT NOT NULL)");
        }

        [Test]
        public void CanCreateTableWithPrimaryKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithPrimaryKeyExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [TestSchema].[TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL, [TestColumn2] INT NOT NULL, PRIMARY KEY ([TestColumn1]))");
        }

        [Test]
        public void CanCreateTableWithPrimaryKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithPrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [dbo].[TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL, [TestColumn2] INT NOT NULL, PRIMARY KEY ([TestColumn1]))");
        }

        [Test]
        public void CanDropTableWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteTableExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("DROP TABLE [TestSchema].[TestTable1]");
        }

        [Test]
        public void CanDropTableWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteTableExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("DROP TABLE [dbo].[TestTable1]");
        }

        [Test]
        public void CanRenameTableWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetRenameTableExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("sp_rename '[TestSchema].[TestTable1]', 'TestTable2'");
        }

        [Test]
        public void CanRenameTableWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetRenameTableExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("sp_rename '[dbo].[TestTable1]', 'TestTable2'");
        }
    }
}
