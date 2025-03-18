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

using FluentMigrator.Exceptions;
using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators.Oracle;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.Oracle
{
    [TestFixture]
    [Category("Generator")]
    [Category("Oracle")]
    public class OracleGeneratorTests
    {
        protected OracleGenerator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new OracleGenerator();
        }

        [Test]
        public void CanAlterColumnNoNullSettings()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpression();
            expression.Column.IsNullable = null;

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 MODIFY TestColumn1 NVARCHAR2(20);");
        }

        [Test]
        public void CanAlterColumnNull()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpression();
            expression.Column.IsNullable = true;

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 MODIFY TestColumn1 NVARCHAR2(20) NULL;");
        }

        [Test]
        public void CanAlterColumnNotNull()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpression();
            expression.Column.IsNullable = false;

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 MODIFY TestColumn1 NVARCHAR2(20) NOT NULL;");
        }

        [Test]
        public void CanAlterSchemaInStrictMode()
        {
            Generator.CompatibilityMode = Runner.CompatibilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(new CreateSchemaExpression()));
        }

        [Test]
        public void CanCreateSchemaInStrictMode()
        {
            Generator.CompatibilityMode = Runner.CompatibilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(new CreateSchemaExpression()));
        }

        [Test]
        public void CanDropSchemaInStrictMode()
        {
            Generator.CompatibilityMode = Runner.CompatibilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(new DeleteSchemaExpression()));
        }

        [Test]
        public void CanCreateTableWithoutAnyDescriptions()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();

            var result = Generator.Generate(expression);

            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 NVARCHAR2(255) NOT NULL, TestColumn2 NUMBER(10,0) NOT NULL);");
        }

        [Test]
        public void CanCreateTableWithDescriptionAndColumnDescription()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithTableDescriptionAndColumnDescriptions();

            var result = Generator.Generate(expression);

            result.ShouldBe("BEGIN EXECUTE IMMEDIATE 'CREATE TABLE TestTable1 (TestColumn1 NVARCHAR2(255), TestColumn2 NUMBER(10,0) NOT NULL);';EXECUTE IMMEDIATE 'COMMENT ON TABLE TestTable1 IS ''TestDescription''';EXECUTE IMMEDIATE 'COMMENT ON COLUMN TestTable1.TestColumn1 IS ''Description:TestColumn1Description''';EXECUTE IMMEDIATE 'COMMENT ON COLUMN TestTable1.TestColumn2 IS ''Description:TestColumn2Description'''; END;");
        }

        [Test]
        public void CanCreateTableWithDescriptionAndColumnDescriptionWithAdditionalDescriptions()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithTableDescriptionAndColumnDescriptionsAndAdditionalDescriptions();

            var result = Generator.Generate(expression);

            result.ShouldBe("BEGIN EXECUTE IMMEDIATE 'CREATE TABLE TestTable1 (TestColumn1 NVARCHAR2(255), TestColumn2 NUMBER(10,0) NOT NULL);';EXECUTE IMMEDIATE 'COMMENT ON TABLE TestTable1 IS ''TestDescription''';EXECUTE IMMEDIATE 'COMMENT ON COLUMN TestTable1.TestColumn1 IS ''Description:TestColumn1Description" + Environment.NewLine +
                "AdditionalColumnDescriptionKey1:AdditionalColumnDescriptionValue1''';EXECUTE IMMEDIATE 'COMMENT ON COLUMN TestTable1.TestColumn2 IS ''Description:TestColumn2Description" + Environment.NewLine +
                "AdditionalColumnDescriptionKey2:AdditionalColumnDescriptionValue2'''; END;");
        }

        [Test]
        public void CanAlterTableWithDescription()
        {
            var expression = GeneratorTestHelper.GetAlterTableWithDescriptionExpression();

            var result = Generator.Generate(expression);

            result.ShouldBe("COMMENT ON TABLE TestTable1 IS 'TestDescription'");
        }

        [Test]
        public void CanAlterTableWithoutAnyDescripion()
        {
            var expression = GeneratorTestHelper.GetAlterTable();

            var result = Generator.Generate(expression);

            result.ShouldBe(string.Empty);
        }

        [Test]
        public void CanCreateColumnWithDescription()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpressionWithDescription();

            var result = Generator.Generate(expression);

            result.ShouldBe("BEGIN EXECUTE IMMEDIATE 'ALTER TABLE TestTable1 ADD TestColumn1 NVARCHAR2(5) NOT NULL;';EXECUTE IMMEDIATE 'COMMENT ON COLUMN TestTable1.TestColumn1 IS ''Description:TestColumn1Description'''; END;");
        }

        [Test]
        public void CanCreateColumnWithDescriptionWithAdditionalDescriptions()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpressionWithDescriptionWithAdditionalDescriptions();

            var result = Generator.Generate(expression);

            result.ShouldBe("BEGIN EXECUTE IMMEDIATE 'ALTER TABLE TestTable1 ADD TestColumn1 NVARCHAR2(5) NOT NULL;';EXECUTE IMMEDIATE 'COMMENT ON COLUMN TestTable1.TestColumn1 IS ''Description:TestColumn1Description"+Environment.NewLine+
                "AdditionalColumnDescriptionKey1:AdditionalColumnDescriptionValue1'''; END;");
        }

        [Test]
        public void CanCreateColumnWithoutDescription()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();

            var result = Generator.Generate(expression);

            result.ShouldBe("ALTER TABLE TestTable1 ADD TestColumn1 NVARCHAR2(5) NOT NULL;");
        }

        [Test]
        public void CanAlterColumnWithDescription()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpressionWithDescription();

            var result = Generator.Generate(expression);

            result.ShouldBe("BEGIN EXECUTE IMMEDIATE 'ALTER TABLE TestTable1 MODIFY TestColumn1 NVARCHAR2(20) NOT NULL;';EXECUTE IMMEDIATE 'COMMENT ON COLUMN TestTable1.TestColumn1 IS ''Description:TestColumn1Description'''; END;");
        }

        [Test]
        public void CanAlterColumnWithDescriptionWithAdditionalDescriptions()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpressionWithDescriptionWithAdditionalDescriptions();

            var result = Generator.Generate(expression);

            result.ShouldBe("BEGIN EXECUTE IMMEDIATE 'ALTER TABLE TestTable1 MODIFY TestColumn1 NVARCHAR2(20) NOT NULL;';EXECUTE IMMEDIATE 'COMMENT ON COLUMN TestTable1.TestColumn1 IS ''Description:TestColumn1Description"+Environment.NewLine+
                "AdditionalColumnDescriptionKey1:AdditionalColumnDescriptionValue1'''; END;");
        }

        [Test]
        public void CanAlterColumnWithoutDescription()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpression();

            var result = Generator.Generate(expression);

            result.ShouldBe("ALTER TABLE TestTable1 MODIFY TestColumn1 NVARCHAR2(20) NOT NULL;");
        }
    }
}
