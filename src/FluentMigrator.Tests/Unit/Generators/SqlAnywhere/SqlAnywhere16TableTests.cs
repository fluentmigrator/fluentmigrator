using FluentMigrator.Runner.Generators.SqlAnywhere;
using NUnit.Framework;
using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.SqlAnywhere
{
    [TestFixture]
    public class SqlAnywhere16TableTests : BaseTableTests
    {
        protected SqlAnywhere16Generator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new SqlAnywhere16Generator();
        }

        [Test]
        [Category("SQLAnywhere"), Category("SQLAnywhere16"), Category("Generator"), Category("Table")]
        public override void CanCreateTableWithCustomColumnTypeWithCustomSchema()
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
        [Category("SQLAnywhere"), Category("SQLAnywhere16"), Category("Generator"), Category("Table")]
        public override void CanCreateTableWithCustomColumnTypeWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].IsPrimaryKey = true;
            expression.Columns[1].Type = null;
            expression.Columns[1].CustomType = "[timestamp]";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [dbo].[TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL, [TestColumn2] [timestamp] NOT NULL, PRIMARY KEY ([TestColumn1]))");
        }

        [Test]
        [Category("SQLAnywhere"), Category("SQLAnywhere16"), Category("Generator"), Category("Table")]
        public override void CanCreateTableWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [TestSchema].[TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL, [TestColumn2] INTEGER NOT NULL)");
        }

        [Test]
        [Category("SQLAnywhere"), Category("SQLAnywhere16"), Category("Generator"), Category("Table")]
        public override void CanCreateTableWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [dbo].[TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL, [TestColumn2] INTEGER NOT NULL)");
        }

        [Test]
        [Category("SQLAnywhere"), Category("SQLAnywhere16"), Category("Generator"), Category("Table")]
        public override void CanCreateTableWithDefaultValueExplicitlySetToNullWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.SchemaName = "TestSchema";
            expression.Columns[0].DefaultValue = null;
            expression.Columns[0].TableName = expression.TableName;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [TestSchema].[TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL DEFAULT (null), [TestColumn2] INTEGER NOT NULL)");
        }

        [Test]
        [Category("SQLAnywhere"), Category("SQLAnywhere16"), Category("Generator"), Category("Table")]
        public override void CanCreateTableWithDefaultValueExplicitlySetToNullWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].DefaultValue = null;
            expression.Columns[0].TableName = expression.TableName;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [dbo].[TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL DEFAULT (null), [TestColumn2] INTEGER NOT NULL)");
        }

        [Test]
        [Category("SQLAnywhere"), Category("SQLAnywhere16"), Category("Generator"), Category("Table")]
        public override void CanCreateTableWithDefaultValueWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [TestSchema].[TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL DEFAULT 'Default', [TestColumn2] INTEGER NOT NULL DEFAULT 0)");
        }

        [Test]
        [Category("SQLAnywhere"), Category("SQLAnywhere16"), Category("Generator"), Category("Table")]
        public override void CanCreateTableWithDefaultValueWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [dbo].[TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL DEFAULT 'Default', [TestColumn2] INTEGER NOT NULL DEFAULT 0)");
        }

        [Test]
        [Category("SQLAnywhere"), Category("SQLAnywhere16"), Category("Generator"), Category("Table")]
        public override void CanCreateTableWithIdentityWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [TestSchema].[TestTable1] ([TestColumn1] INTEGER NOT NULL DEFAULT AUTOINCREMENT, [TestColumn2] INTEGER NOT NULL)");
        }

        [Test]
        [Category("SQLAnywhere"), Category("SQLAnywhere16"), Category("Generator"), Category("Table")]
        public override void CanCreateTableWithIdentityWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [dbo].[TestTable1] ([TestColumn1] INTEGER NOT NULL DEFAULT AUTOINCREMENT, [TestColumn2] INTEGER NOT NULL)");
        }

        [Test]
        [Category("SQLAnywhere"), Category("SQLAnywhere16"), Category("Generator"), Category("Table")]
        public override void CanCreateTableWithMultiColumnPrimaryKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithMultiColumnPrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [dbo].[TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL, [TestColumn2] INTEGER NOT NULL, PRIMARY KEY ([TestColumn1], [TestColumn2]))");
        }

        [Test]
        [Category("SQLAnywhere"), Category("SQLAnywhere16"), Category("Generator"), Category("Table")]
        public override void CanCreateTableWithMultiColumnPrimaryKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithMultiColumnPrimaryKeyExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [TestSchema].[TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL, [TestColumn2] INTEGER NOT NULL, PRIMARY KEY ([TestColumn1], [TestColumn2]))");
        }

        [Test]
        [Category("SQLAnywhere"), Category("SQLAnywhere16"), Category("Generator"), Category("Table")]
        public override void CanCreateTableWithNamedMultiColumnPrimaryKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedMultiColumnPrimaryKeyExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [TestSchema].[TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL, [TestColumn2] INTEGER NOT NULL, CONSTRAINT [TestKey] PRIMARY KEY ([TestColumn1], [TestColumn2]))");
        }

        [Test]
        [Category("SQLAnywhere"), Category("SQLAnywhere16"), Category("Generator"), Category("Table")]
        public override void CanCreateTableWithNamedMultiColumnPrimaryKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedMultiColumnPrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [dbo].[TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL, [TestColumn2] INTEGER NOT NULL, CONSTRAINT [TestKey] PRIMARY KEY ([TestColumn1], [TestColumn2]))");
        }

        [Test]
        [Category("SQLAnywhere"), Category("SQLAnywhere16"), Category("Generator"), Category("Table")]
        public override void CanCreateTableWithNamedPrimaryKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedPrimaryKeyExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [TestSchema].[TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL, [TestColumn2] INTEGER NOT NULL, CONSTRAINT [TestKey] PRIMARY KEY ([TestColumn1]))");
        }

        [Test]
        [Category("SQLAnywhere"), Category("SQLAnywhere16"), Category("Generator"), Category("Table")]
        public override void CanCreateTableWithNamedPrimaryKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedPrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [dbo].[TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL, [TestColumn2] INTEGER NOT NULL, CONSTRAINT [TestKey] PRIMARY KEY ([TestColumn1]))");
        }

        [Test]
        [Category("SQLAnywhere"), Category("SQLAnywhere16"), Category("Generator"), Category("Table")]
        public override void CanCreateTableWithNullableFieldWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.SchemaName = "TestSchema";
            expression.Columns[0].IsNullable = true;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [TestSchema].[TestTable1] ([TestColumn1] NVARCHAR(255) NULL, [TestColumn2] INTEGER NOT NULL)");
        }

        [Test]
        [Category("SQLAnywhere"), Category("SQLAnywhere16"), Category("Generator"), Category("Table")]
        public override void CanCreateTableWithNullableFieldWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].IsNullable = true;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [dbo].[TestTable1] ([TestColumn1] NVARCHAR(255) NULL, [TestColumn2] INTEGER NOT NULL)");
        }

        [Test]
        [Category("SQLAnywhere"), Category("SQLAnywhere16"), Category("Generator"), Category("Table")]
        public override void CanCreateTableWithPrimaryKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithPrimaryKeyExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [TestSchema].[TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL, [TestColumn2] INTEGER NOT NULL, PRIMARY KEY ([TestColumn1]))");
        }

        [Test]
        [Category("SQLAnywhere"), Category("SQLAnywhere16"), Category("Generator"), Category("Table")]
        public override void CanCreateTableWithPrimaryKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithPrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [dbo].[TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL, [TestColumn2] INTEGER NOT NULL, PRIMARY KEY ([TestColumn1]))");
        }

        [Test]
        [Category("SQLAnywhere"), Category("SQLAnywhere16"), Category("Generator"), Category("Table")]
        public override void CanDropTableWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteTableExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("DROP TABLE [TestSchema].[TestTable1]");
        }

        [Test]
        [Category("SQLAnywhere"), Category("SQLAnywhere16"), Category("Generator"), Category("Table")]
        public override void CanDropTableWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteTableExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("DROP TABLE [dbo].[TestTable1]");
        }

        [Test]
        [Category("SQLAnywhere"), Category("SQLAnywhere16"), Category("Generator"), Category("Table")]
        public override void CanRenameTableWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetRenameTableExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] RENAME [TestTable2]");
        }

        [Test]
        [Category("SQLAnywhere"), Category("SQLAnywhere16"), Category("Generator"), Category("Table")]
        public override void CanRenameTableWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetRenameTableExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] RENAME [TestTable2]");
        }
    }
}