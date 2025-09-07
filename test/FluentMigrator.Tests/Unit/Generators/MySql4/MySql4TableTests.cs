using System;
using System.Data;

using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators.MySql;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.MySql4
{
    [TestFixture]
    [Category("Table")]
    [Category("MySql4")]
    public class MySql4TableTests : BaseTableTests
    {
        protected MySql4Generator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new MySql4Generator();
        }

        [Test]
        public override void CanCreateTableWithCustomColumnTypeWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].IsPrimaryKey = true;
            expression.Columns[1].Type = null;
            expression.Columns[1].CustomType = "[timestamp]";
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE `TestTable1` (`TestColumn1` VARCHAR(255) NOT NULL, `TestColumn2` [timestamp] NOT NULL, PRIMARY KEY (`TestColumn1`)) ENGINE = INNODB;");
        }

        [Test]
        public override void CanCreateTableWithCustomColumnTypeWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].IsPrimaryKey = true;
            expression.Columns[1].Type = null;
            expression.Columns[1].CustomType = "[timestamp]";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE `TestTable1` (`TestColumn1` VARCHAR(255) NOT NULL, `TestColumn2` [timestamp] NOT NULL, PRIMARY KEY (`TestColumn1`)) ENGINE = INNODB;");
        }

        [Test]
        public override void CanCreateTableWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE `TestTable1` (`TestColumn1` VARCHAR(255) NOT NULL, `TestColumn2` INTEGER NOT NULL) ENGINE = INNODB;");
        }

        [Test]
        public override void CanCreateTableWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE `TestTable1` (`TestColumn1` VARCHAR(255) NOT NULL, `TestColumn2` INTEGER NOT NULL) ENGINE = INNODB;");
        }

        [Test]
        public override void CanCreateTableWithDefaultValueExplicitlySetToNullWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();
            expression.Columns[0].DefaultValue = null;
            expression.Columns[0].TableName = expression.TableName;
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE `TestTable1` (`TestColumn1` VARCHAR(255) NOT NULL DEFAULT NULL, `TestColumn2` INTEGER NOT NULL DEFAULT 0) ENGINE = INNODB;");
        }

        [Test]
        public override void CanCreateTableWithDefaultValueExplicitlySetToNullWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();
            expression.Columns[0].DefaultValue = null;
            expression.Columns[0].TableName = expression.TableName;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE `TestTable1` (`TestColumn1` VARCHAR(255) NOT NULL DEFAULT NULL, `TestColumn2` INTEGER NOT NULL DEFAULT 0) ENGINE = INNODB;");
        }

        [Test]
        public override void CanCreateTableWithDefaultValueWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE `TestTable1` (`TestColumn1` VARCHAR(255) NOT NULL DEFAULT 'Default', `TestColumn2` INTEGER NOT NULL DEFAULT 0) ENGINE = INNODB;");
        }

        [Test]
        public override void CanCreateTableWithDefaultValueWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE `TestTable1` (`TestColumn1` VARCHAR(255) NOT NULL DEFAULT 'Default', `TestColumn2` INTEGER NOT NULL DEFAULT 0) ENGINE = INNODB;");
        }

        [Test]
        public override void CanCreateTableWithIdentityWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE `TestTable1` (`TestColumn1` INTEGER NOT NULL AUTO_INCREMENT, `TestColumn2` INTEGER NOT NULL) ENGINE = INNODB;");
        }

        [Test]
        public override void CanCreateTableWithIdentityWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE `TestTable1` (`TestColumn1` INTEGER NOT NULL AUTO_INCREMENT, `TestColumn2` INTEGER NOT NULL) ENGINE = INNODB;");
        }

        [Test]
        public override void CanCreateTableWithMultiColumnPrimaryKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithMultiColumnPrimaryKeyExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE `TestTable1` (`TestColumn1` VARCHAR(255) NOT NULL, `TestColumn2` INTEGER NOT NULL, PRIMARY KEY (`TestColumn1`, `TestColumn2`)) ENGINE = INNODB;");
        }

        [Test]
        public override void CanCreateTableWithMultiColumnPrimaryKeyWithDefaultSchema([Values] CompatibilityMode compatibilityMode)
        {
            var expression = GeneratorTestHelper.GetCreateTableWithMultiColumnPrimaryKeyExpression();

            Generator.CompatibilityMode = compatibilityMode;
            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE `TestTable1` (`TestColumn1` VARCHAR(255) NOT NULL, `TestColumn2` INTEGER NOT NULL, PRIMARY KEY (`TestColumn1`, `TestColumn2`)) ENGINE = INNODB;");
        }

        [Test]
        public override void CanCreateTableWithNamedMultiColumnPrimaryKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedMultiColumnPrimaryKeyExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE `TestTable1` (`TestColumn1` VARCHAR(255) NOT NULL, `TestColumn2` INTEGER NOT NULL, CONSTRAINT `TestKey` PRIMARY KEY (`TestColumn1`, `TestColumn2`)) ENGINE = INNODB;");
        }

        [Test]
        public override void CanCreateTableWithNamedMultiColumnPrimaryKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedMultiColumnPrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE `TestTable1` (`TestColumn1` VARCHAR(255) NOT NULL, `TestColumn2` INTEGER NOT NULL, CONSTRAINT `TestKey` PRIMARY KEY (`TestColumn1`, `TestColumn2`)) ENGINE = INNODB;");
        }

        [Test]
        public override void CanCreateTableWithNamedPrimaryKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedPrimaryKeyExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE `TestTable1` (`TestColumn1` VARCHAR(255) NOT NULL, `TestColumn2` INTEGER NOT NULL, CONSTRAINT `TestKey` PRIMARY KEY (`TestColumn1`)) ENGINE = INNODB;");
        }

        [Test]
        public override void CanCreateTableWithNamedPrimaryKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedPrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE `TestTable1` (`TestColumn1` VARCHAR(255) NOT NULL, `TestColumn2` INTEGER NOT NULL, CONSTRAINT `TestKey` PRIMARY KEY (`TestColumn1`)) ENGINE = INNODB;");
        }

        [Test]
        public override void CanCreateTableWithNullableFieldWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].IsNullable = true;
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE `TestTable1` (`TestColumn1` VARCHAR(255), `TestColumn2` INTEGER NOT NULL) ENGINE = INNODB;");
        }

        [Test]
        public override void CanCreateTableWithNullableFieldWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].IsNullable = true;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE `TestTable1` (`TestColumn1` VARCHAR(255), `TestColumn2` INTEGER NOT NULL) ENGINE = INNODB;");
        }

        [Test]
        public override void CanCreateTableWithPrimaryKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithPrimaryKeyExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE `TestTable1` (`TestColumn1` VARCHAR(255) NOT NULL, `TestColumn2` INTEGER NOT NULL, PRIMARY KEY (`TestColumn1`)) ENGINE = INNODB;");
        }

        [Test]
        public override void CanCreateTableWithPrimaryKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithPrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE `TestTable1` (`TestColumn1` VARCHAR(255) NOT NULL, `TestColumn2` INTEGER NOT NULL, PRIMARY KEY (`TestColumn1`)) ENGINE = INNODB;");
        }

        [Test]
        public override void CanDropTableWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteTableExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("DROP TABLE `TestTable1`;");
        }

        [Test]
        public override void CanDropTableWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteTableExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("DROP TABLE `TestTable1`;");
        }

        [Test]
        public override void CanDropTableIfExistsWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteTableIfExistsExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("DROP TABLE IF EXISTS `TestTable1`;");
        }

        [Test]
        public override void CanRenameTableWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetRenameTableExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("RENAME TABLE `TestTable1` TO `TestTable2`;");
        }

        [Test]
        public override void CanRenameTableWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetRenameTableExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("RENAME TABLE `TestTable1` TO `TestTable2`;");
        }

        [Test]
        public void CantCreateTableWithDescription()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithTableDescription();
            Assert.Throws<ArgumentException>(() => Generator.Generate(expression));
        }

        [Test]
        public void CanCreateTableWithDescriptionAndColumnDescriptions()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithTableDescriptionAndColumnDescriptions();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE `TestTable1` (`TestColumn1` VARCHAR(255) COMMENT 'Description:TestColumn1Description', `TestColumn2` INTEGER NOT NULL COMMENT 'Description:TestColumn2Description') COMMENT 'TestDescription' ENGINE = INNODB;");
        }

        [Test]
        public void CanCreateTableWithDescriptionAndColumnDescriptionsWithAdditionalDescriptions()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithTableDescriptionAndColumnDescriptionsAndAdditionalDescriptions();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE `TestTable1` (`TestColumn1` VARCHAR(255) COMMENT 'Description:TestColumn1Description" + Environment.NewLine +
                            "AdditionalColumnDescriptionKey1:AdditionalColumnDescriptionValue1', `TestColumn2` INTEGER NOT NULL COMMENT 'Description:TestColumn2Description" + Environment.NewLine +
                            "AdditionalColumnDescriptionKey2:AdditionalColumnDescriptionValue2') COMMENT 'TestDescription' ENGINE = INNODB;");
        }

        [Test]
        public void CanAlterTableWithDescription()
        {
            var expression = GeneratorTestHelper.GetAlterTableWithDescriptionExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` COMMENT 'TestDescription';");
        }

        [Test]
        public override void CanCreateTableWithFluentMultiColumnForeignKey()
        {
            // Test the new fluent API for multi-column foreign keys
            // MySQL doesn't support inline foreign keys in CREATE TABLE, so the foreign key definition is ignored
            var expression = new CreateTableExpression { TableName = "Area" };
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
            result.ShouldBe("CREATE TABLE `Area` (`ArticleId` VARCHAR(255) NOT NULL, `AreaGroupIndex` INTEGER NOT NULL, `Index` INTEGER NOT NULL) ENGINE = INNODB;");
        }
    }
}
