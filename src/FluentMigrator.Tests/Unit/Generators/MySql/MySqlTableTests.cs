using FluentMigrator.Runner.Generators.MySql;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.MySql
{
    [TestFixture]
    public class MySqlTableTests
    {
        protected MySqlGenerator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new MySqlGenerator();
        }

        [Test]
        public void CanCreateTableWithCustomColumnTypeWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].IsPrimaryKey = true;
            expression.Columns[1].Type = null;
            expression.Columns[1].CustomType = "[timestamp]";
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE `TestTable1` (`TestColumn1` VARCHAR(255) NOT NULL, `TestColumn2` [timestamp] NOT NULL, PRIMARY KEY (`TestColumn1`)) ENGINE = INNODB");
        }

        [Test]
        public void CanCreateTableWithCustomColumnTypeWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].IsPrimaryKey = true;
            expression.Columns[1].Type = null;
            expression.Columns[1].CustomType = "[timestamp]";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE `TestTable1` (`TestColumn1` VARCHAR(255) NOT NULL, `TestColumn2` [timestamp] NOT NULL, PRIMARY KEY (`TestColumn1`)) ENGINE = INNODB");
        }

        [Test]
        public void CanCreateTableWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE `TestTable1` (`TestColumn1` VARCHAR(255) NOT NULL, `TestColumn2` INTEGER NOT NULL) ENGINE = INNODB");
        }

        [Test]
        public void CanCreateTableWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE `TestTable1` (`TestColumn1` VARCHAR(255) NOT NULL, `TestColumn2` INTEGER NOT NULL) ENGINE = INNODB");
        }

        [Test]
        public void CanCreateTableWithDefaultValueExplicitlySetToNullWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();
            expression.Columns[0].DefaultValue = null;
            expression.Columns[0].TableName = expression.TableName;
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE `TestTable1` (`TestColumn1` VARCHAR(255) NOT NULL DEFAULT NULL, `TestColumn2` INTEGER NOT NULL DEFAULT 0) ENGINE = INNODB");
        }

        [Test]
        public void CanCreateTableWithDefaultValueExplicitlySetToNullWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();
            expression.Columns[0].DefaultValue = null;
            expression.Columns[0].TableName = expression.TableName;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE `TestTable1` (`TestColumn1` VARCHAR(255) NOT NULL DEFAULT NULL, `TestColumn2` INTEGER NOT NULL DEFAULT 0) ENGINE = INNODB");
        }

        [Test]
        public void CanCreateTableWithDefaultValueWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE `TestTable1` (`TestColumn1` VARCHAR(255) NOT NULL DEFAULT 'Default', `TestColumn2` INTEGER NOT NULL DEFAULT 0) ENGINE = INNODB");
        }

        [Test]
        public void CanCreateTableWithDefaultValueWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE `TestTable1` (`TestColumn1` VARCHAR(255) NOT NULL DEFAULT 'Default', `TestColumn2` INTEGER NOT NULL DEFAULT 0) ENGINE = INNODB");
        }

        [Test]
        public void CanCreateTableWithIdentityWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE `TestTable1` (`TestColumn1` INTEGER NOT NULL AUTO_INCREMENT, `TestColumn2` INTEGER NOT NULL) ENGINE = INNODB");
        }

        [Test]
        public void CanCreateTableWithIdentityWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE `TestTable1` (`TestColumn1` INTEGER NOT NULL AUTO_INCREMENT, `TestColumn2` INTEGER NOT NULL) ENGINE = INNODB");
        }

        [Test]
        public void CanCreateTableWithMultiColumnPrimaryKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithMultiColumnPrimaryKeyExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE `TestTable1` (`TestColumn1` VARCHAR(255) NOT NULL, `TestColumn2` INTEGER NOT NULL, PRIMARY KEY (`TestColumn1`, `TestColumn2`)) ENGINE = INNODB");
        }

        [Test]
        public void CanCreateTableWithMultiColumnPrimaryKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithMultiColumnPrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE `TestTable1` (`TestColumn1` VARCHAR(255) NOT NULL, `TestColumn2` INTEGER NOT NULL, PRIMARY KEY (`TestColumn1`, `TestColumn2`)) ENGINE = INNODB");
        }

        [Test]
        public void CanCreateTableWithNamedMultiColumnPrimaryKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedMultiColumnPrimaryKeyExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE `TestTable1` (`TestColumn1` VARCHAR(255) NOT NULL, `TestColumn2` INTEGER NOT NULL, CONSTRAINT `TestKey` PRIMARY KEY (`TestColumn1`, `TestColumn2`)) ENGINE = INNODB");
        }

        [Test]
        public void CanCreateTableWithNamedMultiColumnPrimaryKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedMultiColumnPrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE `TestTable1` (`TestColumn1` VARCHAR(255) NOT NULL, `TestColumn2` INTEGER NOT NULL, CONSTRAINT `TestKey` PRIMARY KEY (`TestColumn1`, `TestColumn2`)) ENGINE = INNODB");
        }

        [Test]
        public void CanCreateTableWithNamedPrimaryKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedPrimaryKeyExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE `TestTable1` (`TestColumn1` VARCHAR(255) NOT NULL, `TestColumn2` INTEGER NOT NULL, CONSTRAINT `TestKey` PRIMARY KEY (`TestColumn1`)) ENGINE = INNODB");
        }

        [Test]
        public void CanCreateTableWithNamedPrimaryKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedPrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE `TestTable1` (`TestColumn1` VARCHAR(255) NOT NULL, `TestColumn2` INTEGER NOT NULL, CONSTRAINT `TestKey` PRIMARY KEY (`TestColumn1`)) ENGINE = INNODB");
        }

        [Test]
        public void CanCreateTableWithNullableFieldWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].IsNullable = true;
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE `TestTable1` (`TestColumn1` VARCHAR(255), `TestColumn2` INTEGER NOT NULL) ENGINE = INNODB");
        }

        [Test]
        public void CanCreateTableWithNullableFieldWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].IsNullable = true;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE `TestTable1` (`TestColumn1` VARCHAR(255), `TestColumn2` INTEGER NOT NULL) ENGINE = INNODB");
        }

        [Test]
        public void CanCreateTableWithPrimaryKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithPrimaryKeyExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE `TestTable1` (`TestColumn1` VARCHAR(255) NOT NULL, `TestColumn2` INTEGER NOT NULL, PRIMARY KEY (`TestColumn1`)) ENGINE = INNODB");
        }

        [Test]
        public void CanCreateTableWithPrimaryKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithPrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE `TestTable1` (`TestColumn1` VARCHAR(255) NOT NULL, `TestColumn2` INTEGER NOT NULL, PRIMARY KEY (`TestColumn1`)) ENGINE = INNODB");
        }

        [Test]
        public void CanDropTableWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteTableExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("DROP TABLE `TestTable1`");
        }

        [Test]
        public void CanDropTableWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteTableExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("DROP TABLE `TestTable1`");
        }

        [Test]
        public void CanRenameTableWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetRenameTableExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("RENAME TABLE `TestTable1` TO `TestTable2`");
        }

        [Test]
        public void CanRenameTableWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetRenameTableExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("RENAME TABLE `TestTable1` TO `TestTable2`");
        }
    }
}
