﻿using FluentMigrator.Runner.Generators.Firebird;
using FluentMigrator.Runner.Processors.Firebird;
using Xunit;

namespace FluentMigrator.Tests.Unit.Generators.Firebird
{
    public class FirebirdTableTests : BaseTableTests
    {
        protected FirebirdGenerator Generator;

        public FirebirdTableTests()
        {
            Generator = new FirebirdGenerator(FirebirdOptions.StandardBehaviour());
        }

        [Fact]
        public override void CanCreateTableWithCustomColumnTypeWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].IsPrimaryKey = true;
            expression.Columns[1].Type = null;
            expression.Columns[1].CustomType = "[timestamp]";
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 VARCHAR(255) NOT NULL, TestColumn2 [timestamp] NOT NULL, PRIMARY KEY (TestColumn1))");
        }

        [Fact]
        public override void CanCreateTableWithCustomColumnTypeWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].IsPrimaryKey = true;
            expression.Columns[1].Type = null;
            expression.Columns[1].CustomType = "[timestamp]";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 VARCHAR(255) NOT NULL, TestColumn2 [timestamp] NOT NULL, PRIMARY KEY (TestColumn1))");
        }

        [Fact]
        public override void CanCreateTableWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 VARCHAR(255) NOT NULL, TestColumn2 INTEGER NOT NULL)");
        }

        [Fact]
        public override void CanCreateTableWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 VARCHAR(255) NOT NULL, TestColumn2 INTEGER NOT NULL)");
        }

        [Fact]
        public override void CanCreateTableWithDefaultValueExplicitlySetToNullWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();
            expression.Columns[0].DefaultValue = null;
            expression.Columns[0].TableName = expression.TableName;
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 VARCHAR(255) DEFAULT NULL NOT NULL, TestColumn2 INTEGER DEFAULT 0 NOT NULL)");
        }

        [Fact]
        public override void CanCreateTableWithDefaultValueExplicitlySetToNullWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();
            expression.Columns[0].DefaultValue = null;
            expression.Columns[0].TableName = expression.TableName;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 VARCHAR(255) DEFAULT NULL NOT NULL, TestColumn2 INTEGER DEFAULT 0 NOT NULL)");

        }

        [Fact]
        public override void CanCreateTableWithDefaultValueWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 VARCHAR(255) DEFAULT 'Default' NOT NULL, TestColumn2 INTEGER DEFAULT 0 NOT NULL)");
        }

        [Fact]
        public override void CanCreateTableWithDefaultValueWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 VARCHAR(255) DEFAULT 'Default' NOT NULL, TestColumn2 INTEGER DEFAULT 0 NOT NULL)");
        }

        [Fact]
        public override void CanCreateTableWithIdentityWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 INTEGER NOT NULL, TestColumn2 INTEGER NOT NULL)");
        }

        [Fact]
        public override void CanCreateTableWithIdentityWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 INTEGER NOT NULL, TestColumn2 INTEGER NOT NULL)");
        }

        [Fact]
        public override void CanCreateTableWithMultiColumnPrimaryKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithMultiColumnPrimaryKeyExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 VARCHAR(255) NOT NULL, TestColumn2 INTEGER NOT NULL, PRIMARY KEY (TestColumn1, TestColumn2))");
        }

        [Fact]
        public override void CanCreateTableWithMultiColumnPrimaryKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithMultiColumnPrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 VARCHAR(255) NOT NULL, TestColumn2 INTEGER NOT NULL, PRIMARY KEY (TestColumn1, TestColumn2))");
        }

        [Fact]
        public override void CanCreateTableWithNamedMultiColumnPrimaryKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedMultiColumnPrimaryKeyExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 VARCHAR(255) NOT NULL, TestColumn2 INTEGER NOT NULL, CONSTRAINT TestKey PRIMARY KEY (TestColumn1, TestColumn2))");
        }

        [Fact]
        public override void CanCreateTableWithNamedMultiColumnPrimaryKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedMultiColumnPrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 VARCHAR(255) NOT NULL, TestColumn2 INTEGER NOT NULL, CONSTRAINT TestKey PRIMARY KEY (TestColumn1, TestColumn2))");
        }

        [Fact]
        public override void CanCreateTableWithNamedPrimaryKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedPrimaryKeyExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 VARCHAR(255) NOT NULL, TestColumn2 INTEGER NOT NULL, CONSTRAINT TestKey PRIMARY KEY (TestColumn1))");
        }

        [Fact]
        public override void CanCreateTableWithNamedPrimaryKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedPrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 VARCHAR(255) NOT NULL, TestColumn2 INTEGER NOT NULL, CONSTRAINT TestKey PRIMARY KEY (TestColumn1))");
        }

        [Fact]
        public override void CanCreateTableWithNullableFieldWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNullableColumn();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 VARCHAR(255), TestColumn2 INTEGER NOT NULL)");
        }

        [Fact]
        public override void CanCreateTableWithNullableFieldWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNullableColumn();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 VARCHAR(255), TestColumn2 INTEGER NOT NULL)");
        }

        [Fact]
        public override void CanCreateTableWithPrimaryKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithPrimaryKeyExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 VARCHAR(255) NOT NULL, TestColumn2 INTEGER NOT NULL, PRIMARY KEY (TestColumn1))");
        }

        [Fact]
        public override void CanCreateTableWithPrimaryKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithPrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 VARCHAR(255) NOT NULL, TestColumn2 INTEGER NOT NULL, PRIMARY KEY (TestColumn1))");
        }

        [Fact]
        public override void CanDropTableWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteTableExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("DROP TABLE TestTable1");
        }

        [Fact]
        public override void CanDropTableWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteTableExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("DROP TABLE TestTable1");
        }

        [Fact]
        public override void CanRenameTableWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetRenameTableExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }

        [Fact]
        public override void CanRenameTableWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetRenameTableExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }
    }
}
