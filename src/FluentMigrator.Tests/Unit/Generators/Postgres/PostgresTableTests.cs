﻿using FluentMigrator.Runner.Generators.Postgres;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.Postgres
{
    [TestFixture]
    public class PostgresTableTests : BaseTableTests
    {
        protected PostgresGenerator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new PostgresGenerator();
        }

        [Test]
        public override void CanCreateTableWithCustomColumnTypeWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].IsPrimaryKey = true;
            expression.Columns[1].Type = null;
            expression.Columns[1].CustomType = "json";
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"TestSchema\".\"TestTable1\" (\"TestColumn1\" text NOT NULL, \"TestColumn2\" json NOT NULL, PRIMARY KEY (\"TestColumn1\"));");
        }

        [Test]
        public override void CanCreateTableWithCustomColumnTypeWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].IsPrimaryKey = true;
            expression.Columns[1].Type = null;
            expression.Columns[1].CustomType = "json";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"public\".\"TestTable1\" (\"TestColumn1\" text NOT NULL, \"TestColumn2\" json NOT NULL, PRIMARY KEY (\"TestColumn1\"));");
        }

        [Test]
        public override void CanCreateTableWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"TestSchema\".\"TestTable1\" (\"TestColumn1\" text NOT NULL, \"TestColumn2\" integer NOT NULL);");
        }

        [Test]
        public override void CanCreateTableWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"public\".\"TestTable1\" (\"TestColumn1\" text NOT NULL, \"TestColumn2\" integer NOT NULL);");
        }

        [Test]
        public override void CanCreateTableWithDefaultValueExplicitlySetToNullWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();
            expression.Columns[0].DefaultValue = null;
            expression.Columns[0].TableName = expression.TableName;
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"TestSchema\".\"TestTable1\" (\"TestColumn1\" text NOT NULL DEFAULT NULL, \"TestColumn2\" integer NOT NULL DEFAULT 0);");
        }

        [Test]
        public override void CanCreateTableWithDefaultValueExplicitlySetToNullWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();
            expression.Columns[0].DefaultValue = null;
            expression.Columns[0].TableName = expression.TableName;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"public\".\"TestTable1\" (\"TestColumn1\" text NOT NULL DEFAULT NULL, \"TestColumn2\" integer NOT NULL DEFAULT 0);");

        }

        [Test]
        public override void CanCreateTableWithDefaultValueWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"TestSchema\".\"TestTable1\" (\"TestColumn1\" text NOT NULL DEFAULT 'Default', \"TestColumn2\" integer NOT NULL DEFAULT 0);");
        }

        [Test]
        public override void CanCreateTableWithIdentityWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"TestSchema\".\"TestTable1\" (\"TestColumn1\" serial NOT NULL, \"TestColumn2\" integer NOT NULL);");
        }

        [Test]
        public override void CanCreateTableWithIdentityWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"public\".\"TestTable1\" (\"TestColumn1\" serial NOT NULL, \"TestColumn2\" integer NOT NULL);");
        }

        [Test]
        public override void CanCreateTableWithDefaultValueWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"public\".\"TestTable1\" (\"TestColumn1\" text NOT NULL DEFAULT 'Default', \"TestColumn2\" integer NOT NULL DEFAULT 0);");
        }

        [Test]
        public override void CanCreateTableWithMultiColumnPrimaryKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithMultiColumnPrimaryKeyExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"TestSchema\".\"TestTable1\" (\"TestColumn1\" text NOT NULL, \"TestColumn2\" integer NOT NULL, PRIMARY KEY (\"TestColumn1\",\"TestColumn2\"));");
        }

        [Test]
        public override void CanCreateTableWithMultiColumnPrimaryKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithMultiColumnPrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"public\".\"TestTable1\" (\"TestColumn1\" text NOT NULL, \"TestColumn2\" integer NOT NULL, PRIMARY KEY (\"TestColumn1\",\"TestColumn2\"));");
        }

        [Test]
        public override void CanCreateTableWithNamedMultiColumnPrimaryKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedMultiColumnPrimaryKeyExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"TestSchema\".\"TestTable1\" (\"TestColumn1\" text NOT NULL, \"TestColumn2\" integer NOT NULL, CONSTRAINT \"TestKey\" PRIMARY KEY (\"TestColumn1\",\"TestColumn2\"));");
        }

        [Test]
        public override void CanCreateTableWithNamedMultiColumnPrimaryKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedMultiColumnPrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"public\".\"TestTable1\" (\"TestColumn1\" text NOT NULL, \"TestColumn2\" integer NOT NULL, CONSTRAINT \"TestKey\" PRIMARY KEY (\"TestColumn1\",\"TestColumn2\"));");
        }

        [Test]
        public override void CanCreateTableWithNamedPrimaryKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedPrimaryKeyExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"TestSchema\".\"TestTable1\" (\"TestColumn1\" text NOT NULL, \"TestColumn2\" integer NOT NULL, CONSTRAINT \"TestKey\" PRIMARY KEY (\"TestColumn1\"));");
        }

        [Test]
        public override void CanCreateTableWithNamedPrimaryKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedPrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"public\".\"TestTable1\" (\"TestColumn1\" text NOT NULL, \"TestColumn2\" integer NOT NULL, CONSTRAINT \"TestKey\" PRIMARY KEY (\"TestColumn1\"));");
        }

        [Test]
        public override void CanCreateTableWithNullableFieldWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNullableColumn();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"TestSchema\".\"TestTable1\" (\"TestColumn1\" text, \"TestColumn2\" integer NOT NULL);");
        }

        [Test]
        public override void CanCreateTableWithNullableFieldWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNullableColumn();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"public\".\"TestTable1\" (\"TestColumn1\" text, \"TestColumn2\" integer NOT NULL);");
        }

        [Test]
        public override void CanCreateTableWithPrimaryKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithPrimaryKeyExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"TestSchema\".\"TestTable1\" (\"TestColumn1\" text NOT NULL, \"TestColumn2\" integer NOT NULL, PRIMARY KEY (\"TestColumn1\"));");
        }

        [Test]
        public override void CanCreateTableWithPrimaryKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithPrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"public\".\"TestTable1\" (\"TestColumn1\" text NOT NULL, \"TestColumn2\" integer NOT NULL, PRIMARY KEY (\"TestColumn1\"));");
        }

        [Test]
        public override void CanDropTableWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteTableExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("DROP TABLE \"TestSchema\".\"TestTable1\";");
        }

        [Test]
        public override void CanDropTableWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteTableExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("DROP TABLE \"public\".\"TestTable1\";");
        }

        [Test]
        public override void CanRenameTableWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetRenameTableExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" RENAME TO \"TestTable2\";");
        }

        [Test]
        public override void CanRenameTableWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetRenameTableExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"public\".\"TestTable1\" RENAME TO \"TestTable2\";");
        }
    }
}