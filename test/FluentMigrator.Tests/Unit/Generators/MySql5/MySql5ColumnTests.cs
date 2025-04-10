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

using FluentMigrator.Runner.Generators.MySql;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.MySql5
{
    [TestFixture]
    [Category("Generator")]
    [Category("MySql5")]
    public class MySql5ColumnTest
    {
        protected MySql4Generator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new MySql5Generator();
        }

        [Test]
        public void CanAlterColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` MODIFY COLUMN `TestColumn1` NVARCHAR(20) NOT NULL;");
        }

        [Test]
        public void CanAlterColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` MODIFY COLUMN `TestColumn1` NVARCHAR(20) NOT NULL;");
        }

        [Test]
        public void CanCreateColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` ADD COLUMN `TestColumn1` NVARCHAR(5) NOT NULL;");
        }

        [Test]
        public void CanCreateColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` ADD COLUMN `TestColumn1` NVARCHAR(5) NOT NULL;");
        }

        [Test]
        public void CanAlterColumnWithDescription()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpressionWithDescription();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` MODIFY COLUMN `TestColumn1` NVARCHAR(20) NOT NULL COMMENT 'Description:TestColumn1Description';");
        }

        [Test]
        public void CanAlterColumnWithDescriptionWithAdditionalDescriptions()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpressionWithDescriptionWithAdditionalDescriptions();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` MODIFY COLUMN `TestColumn1` NVARCHAR(20) NOT NULL COMMENT 'Description:TestColumn1Description" + Environment.NewLine +
                            "AdditionalColumnDescriptionKey1:AdditionalColumnDescriptionValue1';");
        }

        [Test]
        public void CanCreateColumnWithDescription()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpressionWithDescription();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` ADD COLUMN `TestColumn1` NVARCHAR(5) NOT NULL COMMENT 'Description:TestColumn1Description';");
        }

        [Test]
        public void CanCreateColumnWithDescriptionWithAdditionalDescriptions()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpressionWithDescriptionWithAdditionalDescriptions();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` ADD COLUMN `TestColumn1` NVARCHAR(5) NOT NULL COMMENT 'Description:TestColumn1Description" + Environment.NewLine +
                            "AdditionalColumnDescriptionKey1:AdditionalColumnDescriptionValue1';");
        }
    }
}
