using System.Linq;
using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.Jet;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.Jet
{
    public class JetCreateTableTests : BaseTableCreateTests
    {
        private JetGenerator _generator;

        [SetUp]
        public void SetUp()
        {
            _generator = new JetGenerator();
        }

        [Test]
        public override void CanCreateTable()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE [TestTable1] ([TestColumn1] VARCHAR(255) NOT NULL, [TestColumn2] INTEGER NOT NULL)");
        }

        [Test]
        public override void CanCreateTableWithCustomColumnType()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[1].Type = null;
            expression.Columns[1].CustomType = "[timestamp]";
            var sql = _generator.Generate(expression);
            sql.ShouldBe(
                "CREATE TABLE [TestTable1] ([TestColumn1] VARCHAR(255) NOT NULL, [TestColumn2] [timestamp] NOT NULL)");
        }

        [Test]
        public override void CanCreateTableWithPrimaryKey()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithPrimaryKeyExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe(
                "CREATE TABLE [TestTable1] ([TestColumn1] VARCHAR(255) NOT NULL, [TestColumn2] INTEGER NOT NULL, PRIMARY KEY ([TestColumn1]))");
        }

        [Test]
        public override void CanCreateTableWithMultiColumnPrimaryKey()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithMultiColumnPrimaryKeyExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe(
                "CREATE TABLE [TestTable1] ([TestColumn1] VARCHAR(255) NOT NULL, [TestColumn2] INTEGER NOT NULL, PRIMARY KEY ([TestColumn1], [TestColumn2]))");
        }

        [Test]
        public override void CanCreateTableNamedPrimaryKey()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedPrimaryKeyExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe(
                "CREATE TABLE [TestTable1] ([TestColumn1] VARCHAR(255) NOT NULL, [TestColumn2] INTEGER NOT NULL, CONSTRAINT [TestKey] PRIMARY KEY ([TestColumn1]))");
        }

        [Test]
        public override void CanCreateTableNamedMultiColumnPrimaryKey()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithMultiColumNamedPrimaryKeyExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe(
                "CREATE TABLE [TestTable1] ([TestColumn1] VARCHAR(255) NOT NULL, [TestColumn2] INTEGER NOT NULL, CONSTRAINT [TestKey] PRIMARY KEY ([TestColumn1], [TestColumn2]))");
        }

        [Test]
        public override void CanCreateTableWithIdentity()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe(
                "CREATE TABLE [TestTable1] ([TestColumn1] COUNTER NOT NULL, [TestColumn2] INTEGER NOT NULL)");
        }

        [Test]
        public override void CanCreateTableWithNullableField()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNullableColumn();
            var sql = _generator.Generate(expression);
            sql.ShouldBe(
                "CREATE TABLE [TestTable1] ([TestColumn1] VARCHAR(255), [TestColumn2] INTEGER NOT NULL)");
        }

        [Test]
        public override void CanCreateTableWithDefaultValue()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();
            var sql = _generator.Generate(expression);
            sql.ShouldBe(
                "CREATE TABLE [TestTable1] ([TestColumn1] VARCHAR(255) NOT NULL DEFAULT 'Default', [TestColumn2] INTEGER NOT NULL DEFAULT 0)");
        }

        [Test]
        public override void CanCreateTableWithDefaultValueExplicitlySetToNull()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();
            expression.Columns.First().DefaultValue = null;
            var sql = _generator.Generate(expression);
            sql.ShouldBe(
                "CREATE TABLE [TestTable1] ([TestColumn1] VARCHAR(255) NOT NULL DEFAULT NULL, [TestColumn2] INTEGER NOT NULL DEFAULT 0)");
        }

        [Test]
        public override void CanCreateTableWithDefaultExpression()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultExpression();
            Assert.Throws<DatabaseOperationNotSupportedException>(() => _generator.Generate(expression));
        }

        [Test]
        public override void CanCreateTableWithDefaultGuid()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultGuid();
            Assert.Throws<DatabaseOperationNotSupportedException>(() => _generator.Generate(expression));
        }

        [Test]
        public override void CanCreateTableWithDefaultCurrentDate()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultCurrentDateTime();
            Assert.Throws<DatabaseOperationNotSupportedException>(() => _generator.Generate(expression));
        }

        [Test]
        public override void CanCreateIndex()
        {
            var expression = GeneratorTestHelper.GetCreateIndexExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE INDEX [TestIndex] ON [TestTable1] ([TestColumn1] ASC)");
        }

        [Test]
        public override void CanCreateMultiColumnIndex()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnCreateIndexExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE INDEX [TestIndex] ON [TestTable1] ([TestColumn1] ASC, [TestColumn2] DESC)");
        }

        [Test]
        public override void CanCreateUniqueIndex()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueIndexExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE UNIQUE INDEX [TestIndex] ON [TestTable1] ([TestColumn1] ASC)");
        }

        [Test]
        public override void CanCreateMultiColumnUniqueIndex()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueMultiColumnIndexExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE UNIQUE INDEX [TestIndex] ON [TestTable1] ([TestColumn1] ASC, [TestColumn2] DESC)");
        }

        [Test]
        public override void CanCreateSchema()
        {
            var expression = new CreateSchemaExpression { SchemaName = "TestSchema" };
            var result = _generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }

        [Test]
        public void CanCreateSchemaInStrictMode()
        {
            _generator.compatabilityMode = Runner.CompatabilityMode.STRICT;
            var expression = new CreateSchemaExpression();
            Assert.Throws<DatabaseOperationNotSupportedException>(() => _generator.Generate(expression));
        }
    }
}