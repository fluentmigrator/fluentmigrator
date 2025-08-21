#region License
// Copyright (c) 2007-2024, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Linq;

using FluentMigrator.Runner.Generators.Redshift;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.Redshift
{
    [TestFixture]
    public sealed class RedshiftColumnTests : BaseColumnTests
    {
        private RedshiftGenerator _generator;

        [SetUp]
        public void Setup()
        {
            _generator = new RedshiftGenerator();
        }

        [Test]
        public override void CanCreateNullableColumnWithCustomDomainTypeAndCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpressionWithNullableCustomType();
            expression.SchemaName = "TestSchema";

            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" ADD \"TestColumn1\" MyDomainType;");
        }

        [Test]
        public override void CanCreateNullableColumnWithCustomDomainTypeAndDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpressionWithNullableCustomType();

            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"public\".\"TestTable1\" ADD \"TestColumn1\" MyDomainType;");
        }

        [Test]
        public override void CanAlterColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpression();
            expression.Column.IsNullable = null;
            expression.SchemaName = "TestSchema";

            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" ALTER \"TestColumn1\" TYPE varchar(20);");
        }

        [Test]
        public override void CanAlterColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpression();
            expression.Column.IsNullable = null;

            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"public\".\"TestTable1\" ALTER \"TestColumn1\" TYPE varchar(20);");
        }

        [Test]
        public override void CanCreateAutoIncrementColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetAlterColumnAddAutoIncrementExpression();
            expression.SchemaName = "TestSchema";

            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" ALTER \"TestColumn1\" TYPE integer;");
        }

        [Test]
        public override void CanCreateAutoIncrementColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetAlterColumnAddAutoIncrementExpression();

            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"public\".\"TestTable1\" ALTER \"TestColumn1\" TYPE integer;");
        }

        [Test]
        public override void CanCreateColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" ADD \"TestColumn1\" varchar(5) NOT NULL;");
        }

        [Test]
        public override void CanCreateColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();

            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"public\".\"TestTable1\" ADD \"TestColumn1\" varchar(5) NOT NULL;");
        }

        [Test]
        public override void CanCreateColumnWithSystemMethodAndCustomSchema()
        {
            var expressions = GeneratorTestHelper.GetCreateColumnWithSystemMethodExpression("TestSchema");
            var result = expressions.Select(x => (string)_generator.Generate((dynamic)x));
            result.ShouldBe([
                @"ALTER TABLE ""TestSchema"".""TestTable1"" ADD ""TestColumn1"" timestamp;",
                @"UPDATE ""TestSchema"".""TestTable1"" SET ""TestColumn1"" = SYSDATE WHERE 1 = 1;",
            ]);
        }

        [Test]
        public override void CanCreateColumnWithSystemMethodAndDefaultSchema()
        {
            var expressions = GeneratorTestHelper.GetCreateColumnWithSystemMethodExpression();
            var result = expressions.Select(x => (string)_generator.Generate((dynamic)x));
            result.ShouldBe([
                @"ALTER TABLE ""public"".""TestTable1"" ADD ""TestColumn1"" timestamp;",
                @"UPDATE ""public"".""TestTable1"" SET ""TestColumn1"" = SYSDATE WHERE 1 = 1;",
            ]);
        }

        [Test]
        public override void CanCreateDecimalColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateDecimalColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" ADD \"TestColumn1\" decimal(19,2) NOT NULL;");
        }

        [Test]
        public override void CanCreateDecimalColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateDecimalColumnExpression();

            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"public\".\"TestTable1\" ADD \"TestColumn1\" decimal(19,2) NOT NULL;");
        }

        [Test]
        public override void CanDropColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" DROP COLUMN \"TestColumn1\";");
        }

        [Test]
        public override void CanDropColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteColumnExpression();

            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"public\".\"TestTable1\" DROP COLUMN \"TestColumn1\";");
        }

        [Test]
        public override void CanDropMultipleColumnsWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteColumnExpression(new[] {"TestColumn1", "TestColumn2"});
            expression.SchemaName = "TestSchema";

            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" DROP COLUMN \"TestColumn1\";" +
                            Environment.NewLine +
                            "ALTER TABLE \"TestSchema\".\"TestTable1\" DROP COLUMN \"TestColumn2\";");
        }

        [Test]
        public override void CanDropMultipleColumnsWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteColumnExpression(new[] {"TestColumn1", "TestColumn2"});

            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"public\".\"TestTable1\" DROP COLUMN \"TestColumn1\";" +
                            Environment.NewLine +
                            "ALTER TABLE \"public\".\"TestTable1\" DROP COLUMN \"TestColumn2\";");
        }

        [Test]
        public override void CanRenameColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = _generator.Generate(expression);
            result.ShouldBe(
                "ALTER TABLE \"TestSchema\".\"TestTable1\" RENAME COLUMN \"TestColumn1\" TO \"TestColumn2\";");
        }

        [Test]
        public override void CanRenameColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();

            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"public\".\"TestTable1\" RENAME COLUMN \"TestColumn1\" TO \"TestColumn2\";");
        }

        [Test]
        public override void CanCreateColumnWithComputedExpression()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpressionWithComputed();
            
            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"public\".\"TestTable1\" ADD \"TestColumn1\" GENERATED ALWAYS AS (Price * Quantity) NOT NULL;");
        }

        [Test]
        public override void CanCreateColumnWithStoredComputedExpression()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpressionWithStoredComputed();
            
            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"public\".\"TestTable1\" ADD \"TestColumn1\" GENERATED ALWAYS AS (Price * Quantity) STORED NOT NULL;");
        }

        [Test]
        public override void CanAlterColumnToAddComputedExpression()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpressionWithComputed();
            
            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"public\".\"TestTable1\" ALTER \"TestColumn1\" TYPE ;");
        }

        [Test]
        public override void CanAlterColumnToAddStoredComputedExpression()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpressionWithStoredComputed();
            
            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"public\".\"TestTable1\" ALTER \"TestColumn1\" TYPE ;");
        }
    }
}
