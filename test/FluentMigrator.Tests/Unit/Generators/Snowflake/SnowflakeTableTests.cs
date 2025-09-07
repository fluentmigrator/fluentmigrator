using System.Collections.Generic;
using System.Data;
using System.Linq;

using FluentMigrator.Builders.Create;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators.Snowflake;
using FluentMigrator.Runner.Processors.Snowflake;

using Moq;

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Generators.Snowflake
{
    [TestFixture(true)]
    [TestFixture(false)]
    [Category("Generator")]
    [Category("Snowflake")]
    public class SnowflakeTableTests : BaseTableTests
    {
        protected SnowflakeGenerator Generator;
        private readonly bool _quotingEnabled;
        private const string TestSchema = "TestSchema";

        public SnowflakeTableTests(bool quotingEnabled)
        {
            _quotingEnabled = quotingEnabled;
        }

        [SetUp]
        public void Setup()
        {
            var sfOptions = _quotingEnabled ? SnowflakeOptions.QuotingEnabled() : SnowflakeOptions.QuotingDisabled();
            Generator = new SnowflakeGenerator(sfOptions);
        }

        [Test]
        public override void CanCreateTableWithCustomColumnTypeWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.SchemaName = TestSchema;
            expression.Columns[0].IsPrimaryKey = true;
            expression.Columns[1].Type = null;
            expression.Columns[1].CustomType = "timestamp";

            var result = Generator.Generate(expression);
            result.ShouldBe($@"CREATE TABLE ""{TestSchema}"".""TestTable1"" (""TestColumn1"" VARCHAR NOT NULL, ""TestColumn2"" timestamp NOT NULL, PRIMARY KEY (""TestColumn1""));", _quotingEnabled);
        }

        [Test]
        public override void CanCreateTableWithCustomColumnTypeWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].IsPrimaryKey = true;
            expression.Columns[1].Type = null;
            expression.Columns[1].CustomType = "timestamp";

            var result = Generator.Generate(expression);
            result.ShouldBe(@"CREATE TABLE ""PUBLIC"".""TestTable1"" (""TestColumn1"" VARCHAR NOT NULL, ""TestColumn2"" timestamp NOT NULL, PRIMARY KEY (""TestColumn1""));", _quotingEnabled);
        }

        [Test]
        public override void CanCreateTableWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.SchemaName = TestSchema;

            var result = Generator.Generate(expression);
            result.ShouldBe($@"CREATE TABLE ""{TestSchema}"".""TestTable1"" (""TestColumn1"" VARCHAR NOT NULL, ""TestColumn2"" NUMBER NOT NULL);", _quotingEnabled);
        }

        [Test]
        public override void CanCreateTableWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            var result = Generator.Generate(expression);
            result.ShouldBe(@"CREATE TABLE ""PUBLIC"".""TestTable1"" (""TestColumn1"" VARCHAR NOT NULL, ""TestColumn2"" NUMBER NOT NULL);", _quotingEnabled);
        }

        [Test]
        public override void CanCreateTableWithDefaultValueExplicitlySetToNullWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.SchemaName = TestSchema;
            expression.Columns[0].DefaultValue = null;
            expression.Columns[0].TableName = expression.TableName;

            var result = Generator.Generate(expression);
            result.ShouldBe($@"CREATE TABLE ""{TestSchema}"".""TestTable1"" (""TestColumn1"" VARCHAR NOT NULL DEFAULT NULL, ""TestColumn2"" NUMBER NOT NULL);", _quotingEnabled);
        }

        [Test]
        public override void CanCreateTableWithDefaultValueExplicitlySetToNullWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].DefaultValue = null;
            expression.Columns[0].TableName = expression.TableName;

            var result = Generator.Generate(expression);
            result.ShouldBe($@"CREATE TABLE ""PUBLIC"".""TestTable1"" (""TestColumn1"" VARCHAR NOT NULL DEFAULT NULL, ""TestColumn2"" NUMBER NOT NULL);", _quotingEnabled);
        }

        [Test]
        public override void CanCreateTableWithDefaultValueWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();
            expression.SchemaName = TestSchema;

            var result = Generator.Generate(expression);
            result.ShouldBe($@"CREATE TABLE ""{TestSchema}"".""TestTable1"" (""TestColumn1"" VARCHAR NOT NULL DEFAULT 'Default', ""TestColumn2"" NUMBER NOT NULL DEFAULT 0);", _quotingEnabled);
        }

        [Test]
        public override void CanCreateTableWithDefaultValueWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();
            var result = Generator.Generate(expression);
            result.ShouldBe(@"CREATE TABLE ""PUBLIC"".""TestTable1"" (""TestColumn1"" VARCHAR NOT NULL DEFAULT 'Default', ""TestColumn2"" NUMBER NOT NULL DEFAULT 0);", _quotingEnabled);
        }

        [Test]
        public override void CanCreateTableWithIdentityWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression();
            expression.SchemaName = TestSchema;

            var result = Generator.Generate(expression);
            result.ShouldBe($@"CREATE TABLE ""{TestSchema}"".""TestTable1"" (""TestColumn1"" NUMBER NOT NULL IDENTITY(1,1), ""TestColumn2"" NUMBER NOT NULL);", _quotingEnabled);
        }

        [Test]
        public override void CanCreateTableWithIdentityWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression();
            var result = Generator.Generate(expression);
            result.ShouldBe(@"CREATE TABLE ""PUBLIC"".""TestTable1"" (""TestColumn1"" NUMBER NOT NULL IDENTITY(1,1), ""TestColumn2"" NUMBER NOT NULL);", _quotingEnabled);
        }

        [Test]
        public override void CanCreateTableWithMultiColumnPrimaryKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithMultiColumnPrimaryKeyExpression();
            expression.SchemaName = TestSchema;

            var result = Generator.Generate(expression);
            result.ShouldBe($@"CREATE TABLE ""{TestSchema}"".""TestTable1"" (""TestColumn1"" VARCHAR NOT NULL, ""TestColumn2"" NUMBER NOT NULL, PRIMARY KEY (""TestColumn1"", ""TestColumn2""));", _quotingEnabled);
        }

        [Test]
        public override void CanCreateTableWithMultiColumnPrimaryKeyWithDefaultSchema([Values] CompatibilityMode compatibilityMode)
        {
            var expression = GeneratorTestHelper.GetCreateTableWithMultiColumnPrimaryKeyExpression();

            Generator.CompatibilityMode = compatibilityMode;
            var result = Generator.Generate(expression);
            result.ShouldBe(@"CREATE TABLE ""PUBLIC"".""TestTable1"" (""TestColumn1"" VARCHAR NOT NULL, ""TestColumn2"" NUMBER NOT NULL, PRIMARY KEY (""TestColumn1"", ""TestColumn2""));", _quotingEnabled);
        }

        [Test]
        public override void CanCreateTableWithNamedMultiColumnPrimaryKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedMultiColumnPrimaryKeyExpression();
            expression.SchemaName = TestSchema;

            var result = Generator.Generate(expression);
            result.ShouldBe($@"CREATE TABLE ""{TestSchema}"".""TestTable1"" (""TestColumn1"" VARCHAR NOT NULL, ""TestColumn2"" NUMBER NOT NULL, CONSTRAINT ""TestKey"" PRIMARY KEY (""TestColumn1"", ""TestColumn2""));", _quotingEnabled);
        }

        [Test]
        public override void CanCreateTableWithNamedMultiColumnPrimaryKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedMultiColumnPrimaryKeyExpression();
            var result = Generator.Generate(expression);
            result.ShouldBe(@"CREATE TABLE ""PUBLIC"".""TestTable1"" (""TestColumn1"" VARCHAR NOT NULL, ""TestColumn2"" NUMBER NOT NULL, CONSTRAINT ""TestKey"" PRIMARY KEY (""TestColumn1"", ""TestColumn2""));", _quotingEnabled);
        }

        [Test]
        public override void CanCreateTableWithNamedPrimaryKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedPrimaryKeyExpression();
            expression.SchemaName = TestSchema;

            var result = Generator.Generate(expression);
            result.ShouldBe($@"CREATE TABLE ""{TestSchema}"".""TestTable1"" (""TestColumn1"" VARCHAR NOT NULL, ""TestColumn2"" NUMBER NOT NULL, CONSTRAINT ""TestKey"" PRIMARY KEY (""TestColumn1""));", _quotingEnabled);
        }

        [Test]
        public override void CanCreateTableWithNamedPrimaryKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedPrimaryKeyExpression();
            var result = Generator.Generate(expression);
            result.ShouldBe(@"CREATE TABLE ""PUBLIC"".""TestTable1"" (""TestColumn1"" VARCHAR NOT NULL, ""TestColumn2"" NUMBER NOT NULL, CONSTRAINT ""TestKey"" PRIMARY KEY (""TestColumn1""));", _quotingEnabled);
        }

        [Test]
        public override void CanCreateTableWithNullableFieldWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.SchemaName = TestSchema;
            expression.Columns[0].IsNullable = true;

            var result = Generator.Generate(expression);
            result.ShouldBe($@"CREATE TABLE ""{TestSchema}"".""TestTable1"" (""TestColumn1"" VARCHAR, ""TestColumn2"" NUMBER NOT NULL);", _quotingEnabled);
        }

        [Test]
        public override void CanCreateTableWithNullableFieldWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].IsNullable = true;
            var result = Generator.Generate(expression);
            result.ShouldBe(@"CREATE TABLE ""PUBLIC"".""TestTable1"" (""TestColumn1"" VARCHAR, ""TestColumn2"" NUMBER NOT NULL);", _quotingEnabled);
        }

        [Test]
        public override void CanCreateTableWithPrimaryKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithPrimaryKeyExpression();
            expression.SchemaName = TestSchema;

            var result = Generator.Generate(expression);
            result.ShouldBe($@"CREATE TABLE ""{TestSchema}"".""TestTable1"" (""TestColumn1"" VARCHAR NOT NULL, ""TestColumn2"" NUMBER NOT NULL, PRIMARY KEY (""TestColumn1""));", _quotingEnabled);
        }

        [Test]
        public override void CanCreateTableWithPrimaryKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithPrimaryKeyExpression();
            var result = Generator.Generate(expression);
            result.ShouldBe($@"CREATE TABLE ""PUBLIC"".""TestTable1"" (""TestColumn1"" VARCHAR NOT NULL, ""TestColumn2"" NUMBER NOT NULL, PRIMARY KEY (""TestColumn1""));", _quotingEnabled);
        }

        [Test]
        public void CanCreateTableWithForeignKeyColumnWithDefaultSchema()
        {
            var expressions = new List<IMigrationExpression>();
            var migrationContexMock = new Mock<IMigrationContext>();
            migrationContexMock.SetupGet(mc => mc.Expressions).Returns(expressions);
            var migrationContext = migrationContexMock.Object;
            new CreateExpressionRoot(migrationContext)
                .Table("FooTable")
                .WithColumn("FooColumn").AsInt32().ForeignKey("BarTable", "BarColumn");
            var createTableExpression = migrationContext.Expressions.OfType<CreateTableExpression>().First();
            var createForeignKeyExpression = migrationContext.Expressions.OfType<CreateForeignKeyExpression>().First();

            // ReSharper disable once UnusedVariable
            var processed = createForeignKeyExpression.Apply(ConventionSets.NoSchemaName);

            var createTableResult = Generator.Generate(createTableExpression);
            var createForeignKeyResult = Generator.Generate(createForeignKeyExpression);
            createTableResult.ShouldBe(@"CREATE TABLE ""PUBLIC"".""FooTable"" (""FooColumn"" NUMBER NOT NULL);", _quotingEnabled);
            createForeignKeyResult.ShouldBe(@"ALTER TABLE ""PUBLIC"".""FooTable"" ADD CONSTRAINT ""FK_FooTable_FooColumn_BarTable_BarColumn"" FOREIGN KEY (""FooColumn"") REFERENCES ""PUBLIC"".""BarTable"" (""BarColumn"");", _quotingEnabled);
        }

        [Test]
        public void CanCreateTableWithForeignKeyColumnWithCustomSchema()
        {
            var expressions = new List<IMigrationExpression>();
            var migrationContexMock = new Mock<IMigrationContext>();
            migrationContexMock.SetupGet(mc => mc.Expressions).Returns(expressions);
            var migrationContext = migrationContexMock.Object;
            new CreateExpressionRoot(migrationContext)
                .Table("FooTable").InSchema("FooSchema")
                .WithColumn("FooColumn").AsInt32().ForeignKey("fk_bar_foo", "BarSchema", "BarTable", "BarColumn");
            var createTableExpression = migrationContext.Expressions.OfType<CreateTableExpression>().First();
            var createForeignKeyExpression = migrationContext.Expressions.OfType<CreateForeignKeyExpression>().First();

            var createTableResult = Generator.Generate(createTableExpression);
            var createForeignKeyResult = Generator.Generate(createForeignKeyExpression);
            createTableResult.ShouldBe(@"CREATE TABLE ""FooSchema"".""FooTable"" (""FooColumn"" NUMBER NOT NULL);", _quotingEnabled);
            createForeignKeyResult.ShouldBe(@"ALTER TABLE ""FooSchema"".""FooTable"" ADD CONSTRAINT ""fk_bar_foo"" FOREIGN KEY (""FooColumn"") REFERENCES ""BarSchema"".""BarTable"" (""BarColumn"");", _quotingEnabled);
        }

        [Test]
        public override void CanDropTableWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteTableExpression();
            expression.SchemaName = TestSchema;

            var result = Generator.Generate(expression);
            result.ShouldBe($@"DROP TABLE ""{TestSchema}"".""TestTable1"";", _quotingEnabled);
        }

        [Test]
        public override void CanDropTableWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteTableExpression();
            var result = Generator.Generate(expression);
            result.ShouldBe(@"DROP TABLE ""PUBLIC"".""TestTable1"";", _quotingEnabled);
        }

        /// <inheritdoc />
        public override void CanDropTableIfExistsWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteTableIfExistsExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("DROP TABLE IF EXISTS \"public\".\"TestTable1\";", _quotingEnabled);
        }

        [Test]
        public override void CanRenameTableWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetRenameTableExpression();
            expression.SchemaName = TestSchema;

            var result = Generator.Generate(expression);
            result.ShouldBe($@"ALTER TABLE ""{TestSchema}"".""TestTable1"" RENAME TO ""{TestSchema}"".""TestTable2"";", _quotingEnabled);
        }

        [Test]
        public override void CanRenameTableWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetRenameTableExpression();
            var result = Generator.Generate(expression);
            result.ShouldBe(@"ALTER TABLE ""PUBLIC"".""TestTable1"" RENAME TO ""PUBLIC"".""TestTable2"";", _quotingEnabled);
        }

        [Test]
        public override void CanCreateTableWithFluentMultiColumnForeignKey()
        {
            // Test the new fluent API for multi-column foreign keys
            // Snowflake doesn't support inline foreign keys in CREATE TABLE, so the foreign key definition is ignored
            var expression = new CreateTableExpression { TableName = "Area", SchemaName = "PUBLIC" };
            expression.Columns.Add(new ColumnDefinition { Name = "ArticleId", Type = DbType.String });
            expression.Columns.Add(new ColumnDefinition { Name = "AreaGroupIndex", Type = DbType.Int32 });
            expression.Columns.Add(new ColumnDefinition { Name = "Index", Type = DbType.Int32, 
                IsForeignKey = true,
                ForeignKey = new ForeignKeyDefinition
                {
                    Name = "FK_Area_AreaGroup",
                    PrimaryTable = "AreaGroup",
                    ForeignTable = "Area",
                    PrimaryColumns = ["ArticleId", "Index"],
                    ForeignColumns = ["ArticleId", "AreaGroupIndex"]
                }
            });

            var result = Generator.Generate(expression);
            result.ShouldContain("CREATE TABLE");
            result.ShouldContain("Area");
            result.ShouldContain("ArticleId");
            result.ShouldContain("AreaGroupIndex");
            result.ShouldContain("Index");
            // Foreign key constraint should not be present in CREATE TABLE for Snowflake
            result.ShouldNotContain("FOREIGN KEY");
        }
    }
}
