using System.Data;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer
{
    public class SqlServer2000CreateTableTests : BaseTableCreateTests
    {
        protected SqlServer2000Generator generator;

        [SetUp]
        public void Setup()
        {
            generator = new SqlServer2000Generator();
        }

        [Test] 
        public override void CanCreateTable()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE [TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL, [TestColumn2] INT NOT NULL)");
        }

        [Test]
        public override void CanCreateTableWithCustomColumnType()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].IsPrimaryKey = true;
            expression.Columns[1].Type = null;
            expression.Columns[1].CustomType = "[timestamp]";

            var sql = generator.Generate(expression);
            sql.ShouldBe(
                "CREATE TABLE [TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL, [TestColumn2] [timestamp] NOT NULL, PRIMARY KEY ([TestColumn1]))");
        }

        [Test]
        public void CanCreateTableWithGetDateDefault()
        {
            ColumnDefinition column = new ColumnDefinition
            {
                Name = "TestColumn1",
                Type = DbType.String,
                Size = 5,
                DefaultValue = (FunctionValue) "GetDate()"
            };
            var expression = new CreateTableExpression { TableName = "TestTable1" };
            expression.Columns.Add(column);
            var sql = generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE [TestTable1] ([TestColumn1] NVARCHAR(5) NOT NULL DEFAULT GetDate())");
        }

        [Test]
        public override void CanCreateTableWithPrimaryKey()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithPrimaryKeyExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe(
                "CREATE TABLE [TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL, [TestColumn2] INT NOT NULL, PRIMARY KEY ([TestColumn1]))");
        }

        [Test]
        public override void CanCreateTableNamedPrimaryKey()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedPrimaryKeyExpression();
            string sql = generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE [TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL, [TestColumn2] INT NOT NULL, CONSTRAINT [TestKey] PRIMARY KEY ([TestColumn1]))");
        }

        [Test]
        public override void CanCreateTableNamedMultiColumnPrimaryKey()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithMultiColumNamedPrimaryKeyExpression();
            string sql = generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE [TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL, [TestColumn2] INT NOT NULL, CONSTRAINT [TestKey] PRIMARY KEY ([TestColumn1], [TestColumn2]))");
        }

        [Test]
        public override void CanCreateTableWithIdentity()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe(
                "CREATE TABLE [TestTable1] ([TestColumn1] INT NOT NULL IDENTITY(1,1), [TestColumn2] INT NOT NULL)");
        }

        [Test]
        public override void CanCreateTableWithNullableField()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].IsNullable = true;
            var sql = generator.Generate(expression);
            sql.ShouldBe(
                "CREATE TABLE [TestTable1] ([TestColumn1] NVARCHAR(255), [TestColumn2] INT NOT NULL)");
        }

        [Test]
        public override void CanCreateTableWithDefaultValue()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();
            var sql = generator.Generate(expression);
            sql.ShouldBe(
                "CREATE TABLE [TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL CONSTRAINT DF_TestTable1_TestColumn1 DEFAULT 'Default', [TestColumn2] INT NOT NULL CONSTRAINT DF_TestTable1_TestColumn2 DEFAULT 0)");
        }

        [Test]
        public override void CanCreateTableWithDefaultValueExplicitlySetToNull()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].DefaultValue = null;
            expression.Columns[0].TableName = expression.TableName;
            var sql = generator.Generate(expression);
            sql.ShouldBe(
                "CREATE TABLE [TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL CONSTRAINT DF_TestTable1_TestColumn1 DEFAULT NULL, [TestColumn2] INT NOT NULL)");
        }

        [Test]
        public override void CanCreateIndex()
        {
            var expression = GeneratorTestHelper.GetCreateIndexExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe("CREATE INDEX [TestIndex] ON [TestTable1] ([TestColumn1] ASC)");
        }

        [Test]
        public override void CanCreateMultiColumnIndex()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnCreateIndexExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe("CREATE INDEX [TestIndex] ON [TestTable1] ([TestColumn1] ASC, [TestColumn2] DESC)");
        }

        [Test]
        public override void CanCreateUniqueIndex()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueIndexExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe("CREATE UNIQUE INDEX [TestIndex] ON [TestTable1] ([TestColumn1] ASC)");
        }

        [Test]
        public override void CanCreateMultiColumnUniqueIndex()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueMultiColumnIndexExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe("CREATE UNIQUE INDEX [TestIndex] ON [TestTable1] ([TestColumn1] ASC, [TestColumn2] DESC)");
        }

        [Test]
        public void CanCreateClusteredIndex()
        {
            var expression = GeneratorTestHelper.GetCreateIndexExpression();
            expression.Index.IsClustered = true;
            var sql = generator.Generate(expression);
            sql.ShouldBe("CREATE CLUSTERED INDEX [TestIndex] ON [TestTable1] ([TestColumn1] ASC)");
        }

        [Test]
        public void CanCreateClusteredMultiColumnIndex()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnCreateIndexExpression();
            expression.Index.IsClustered = true;
            var sql = generator.Generate(expression);
            sql.ShouldBe("CREATE CLUSTERED INDEX [TestIndex] ON [TestTable1] ([TestColumn1] ASC, [TestColumn2] DESC)");
        }

        [Test]
        public void CanCreateClusteredUniqueIndex()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueIndexExpression();
            expression.Index.IsClustered = true;
            var sql = generator.Generate(expression);
            sql.ShouldBe("CREATE UNIQUE CLUSTERED INDEX [TestIndex] ON [TestTable1] ([TestColumn1] ASC)");
        }

        [Test]
        public void CanCreatClusteredUniqueMultiColumnIndex()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueMultiColumnIndexExpression();
            expression.Index.IsClustered = true;
            var sql = generator.Generate(expression);
            sql.ShouldBe("CREATE UNIQUE CLUSTERED INDEX [TestIndex] ON [TestTable1] ([TestColumn1] ASC, [TestColumn2] DESC)");
        }

        [Test]
        public override void CanCreateTableWithMultiColumnPrimaryKey()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithMultiColumnPrimaryKeyExpression();
            var result = generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL, [TestColumn2] INT NOT NULL, PRIMARY KEY ([TestColumn1], [TestColumn2]))");
        }

        [Test]
        public override void CanCreateSchema()
        {
            var expression = new CreateSchemaExpression() { SchemaName = "TestSchema" };
            var result = generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }

        [Test]
        public void CanCreateSchemaInStrictMode()
        {
            generator.compatabilityMode = Runner.CompatabilityMode.STRICT;
            Assert.Throws<DatabaseOperationNotSupportedException>(() => generator.Generate(new CreateSchemaExpression()));
        }
    }
}