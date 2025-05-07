#region License
//
// Copyright (c) 2018, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;
using System.Data;
using System.Linq;

using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.MySql;
using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.MySql4
{
    [TestFixture]
    public class MySql4ColumnTests : BaseColumnTests
    {
        protected MySql4Generator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new MySql4Generator();
        }

        [Test]
        public override void CanCreateNullableColumnWithCustomDomainTypeAndCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpressionWithNullableCustomType();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` ADD COLUMN `TestColumn1` MyDomainType;");
        }

        [Test]
        public override void CanCreateNullableColumnWithCustomDomainTypeAndDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpressionWithNullableCustomType();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` ADD COLUMN `TestColumn1` MyDomainType;");
        }

        [Test]
        public override void CanAlterColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` MODIFY COLUMN `TestColumn1` VARCHAR(20) NOT NULL;");
        }

        [Test]
        public override void CanAlterColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` MODIFY COLUMN `TestColumn1` VARCHAR(20) NOT NULL;");
        }

        [Test]
        public override void CanCreateAutoIncrementColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetAlterColumnAddAutoIncrementExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` MODIFY COLUMN `TestColumn1` INTEGER NOT NULL AUTO_INCREMENT;");
        }

        [Test]
        public override void CanCreateAutoIncrementColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetAlterColumnAddAutoIncrementExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` MODIFY COLUMN `TestColumn1` INTEGER NOT NULL AUTO_INCREMENT;");
        }

        [Test]
        public override void CanCreateColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` ADD COLUMN `TestColumn1` VARCHAR(5) NOT NULL;");
        }

        [Test]
        public override void CanCreateColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` ADD COLUMN `TestColumn1` VARCHAR(5) NOT NULL;");
        }

        [Test]
        public override void CanCreateColumnWithSystemMethodAndCustomSchema()
        {
            var expressions = GeneratorTestHelper.GetCreateColumnWithSystemMethodExpression("TestSchema");
            var result = expressions.Select(x => (string)Generator.Generate((dynamic)x));
            result.ShouldBe([
                @"ALTER TABLE `TestTable1` ADD COLUMN `TestColumn1` DATETIME;",
                @"UPDATE `TestTable1` SET `TestColumn1` = CURRENT_TIMESTAMP WHERE 1 = 1;"
            ]);
        }

        [Test]
        public override void CanCreateColumnWithSystemMethodAndDefaultSchema()
        {
            var expressions = GeneratorTestHelper.GetCreateColumnWithSystemMethodExpression();
            var result = expressions.Select(x => (string)Generator.Generate((dynamic)x));
            result.ShouldBe([
                @"ALTER TABLE `TestTable1` ADD COLUMN `TestColumn1` DATETIME;",
                @"UPDATE `TestTable1` SET `TestColumn1` = CURRENT_TIMESTAMP WHERE 1 = 1;"
            ]);
        }

        [Test]
        public override void CanCreateDecimalColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateDecimalColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` ADD COLUMN `TestColumn1` DECIMAL(19,2) NOT NULL;");
        }

        [Test]
        public override void CanCreateDecimalColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateDecimalColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` ADD COLUMN `TestColumn1` DECIMAL(19,2) NOT NULL;");
        }

        [Test]
        public void CanCreateCurrencyColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateCurrencyColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` ADD COLUMN `TestColumn1` DECIMAL(19,4) NOT NULL;");
        }

        [Test]
        public void CanCreateCurrencyColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateCurrencyColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` ADD COLUMN `TestColumn1` DECIMAL(19,4) NOT NULL;");
        }

        [Test]
        public override void CanDropColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` DROP COLUMN `TestColumn1`;");
        }

        [Test]
        public override void CanDropColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` DROP COLUMN `TestColumn1`;");
        }

        [Test]
        public override void CanDropMultipleColumnsWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteColumnExpression(new[] { "TestColumn1", "TestColumn2" });
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` DROP COLUMN `TestColumn1`;ALTER TABLE `TestTable1` DROP COLUMN `TestColumn2`;");
        }

        [Test]
        public override void CanDropMultipleColumnsWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteColumnExpression(new[] { "TestColumn1", "TestColumn2" });

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` DROP COLUMN `TestColumn1`;ALTER TABLE `TestTable1` DROP COLUMN `TestColumn2`;");
        }

        [Test]
        public override void CanRenameColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` CHANGE `TestColumn1` `TestColumn2`;");
        }

        [Test]
        public override void CanRenameColumnWithDefaultSchema()
        {
            // MySql does not appear to have a way to change column without re-specifying the existing column definition
            var expression = GeneratorTestHelper.GetRenameColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` CHANGE `TestColumn1` `TestColumn2`;");
        }

        [Test]
        public void CanRenameColumnWithCustomSchemaAndDefaultValue()
        {
            CanRenameColumnWithCustomSchema();
        }

        [Test]
        public void CanRenameColumnWithDefaultSchemaAndDefaultValue()
        {
            CanRenameColumnWithDefaultSchema();
        }

        [Test]
        public void CanAlterColumnWithDescription()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpressionWithDescription();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` MODIFY COLUMN `TestColumn1` VARCHAR(20) NOT NULL COMMENT 'Description:TestColumn1Description';");
        }

        [Test]
        public void CanCreateColumnWithDescription()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpressionWithDescription();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` ADD COLUMN `TestColumn1` VARCHAR(5) NOT NULL COMMENT 'Description:TestColumn1Description';");
        }

        [Test]
        public void CanAlterColumnWithDescriptionWithAdditionalDescriptions()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpressionWithDescriptionWithAdditionalDescriptions();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` MODIFY COLUMN `TestColumn1` VARCHAR(20) NOT NULL COMMENT 'Description:TestColumn1Description" + Environment.NewLine + "AdditionalColumnDescriptionKey1:AdditionalColumnDescriptionValue1';");
        }

        [Test]
        public void CanCreateColumnWithDescriptionWithAdditionalDescriptions()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpressionWithDescriptionWithAdditionalDescriptions();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` ADD COLUMN `TestColumn1` VARCHAR(5) NOT NULL COMMENT 'Description:TestColumn1Description" + Environment.NewLine +
                            "AdditionalColumnDescriptionKey1:AdditionalColumnDescriptionValue1';");
        }

        [Test]
        public void CanCreateColumnWithBinaryIntMax()
        {
            var column = new ColumnDefinition { Name = GeneratorTestHelper.TestColumnName1, Type = DbType.Binary, Size = int.MaxValue };
            var expression = new CreateColumnExpression { TableName = GeneratorTestHelper.TestTableName1, Column = column };
            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` ADD COLUMN `TestColumn1` LONGBLOB NOT NULL;");
        }
    }
}
