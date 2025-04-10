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
using System.Linq;

using FluentMigrator.Runner.Generators.Postgres;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.Postgres
{
    public abstract class PostgresBaseColumnTests<TGenerator> : BaseColumnTests
        where TGenerator : PostgresGenerator
    {
        protected TGenerator Generator;

        protected abstract TGenerator ConstructGenerator();

        [SetUp]
        public void Setup()
        {
            Generator = ConstructGenerator();
        }

        [Test]
        public override void CanCreateNullableColumnWithCustomDomainTypeAndCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpressionWithNullableCustomType();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" ADD \"TestColumn1\" MyDomainType NULL;");
        }

        [Test]
        public override void CanCreateNullableColumnWithCustomDomainTypeAndDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpressionWithNullableCustomType();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"public\".\"TestTable1\" ADD \"TestColumn1\" MyDomainType NULL;");
        }

        [Test]
        public override void CanAlterColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpression();
            expression.Column.IsNullable = null;
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" ALTER \"TestColumn1\" TYPE varchar(20);");
        }

        [Test]
        public override void CanAlterColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpression();
            expression.Column.IsNullable = null;

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"public\".\"TestTable1\" ALTER \"TestColumn1\" TYPE varchar(20);");
        }

        [Test]
        public override void CanCreateColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" ADD \"TestColumn1\" varchar(5) NOT NULL;");
        }

        [Test]
        public override void CanCreateColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"public\".\"TestTable1\" ADD \"TestColumn1\" varchar(5) NOT NULL;");
        }

        [Test]
        public override void CanCreateColumnWithSystemMethodAndCustomSchema()
        {
            var expressions = GeneratorTestHelper.GetCreateColumnWithSystemMethodExpression("TestSchema");
            var result = expressions.Select(x => (string)Generator.Generate((dynamic)x));
            result.ShouldBe([
                @"ALTER TABLE ""TestSchema"".""TestTable1"" ADD ""TestColumn1"" timestamp;",
                @"UPDATE ""TestSchema"".""TestTable1"" SET ""TestColumn1"" = now() WHERE 1 = 1;",
            ]);
        }

        [Test]
        public override void CanCreateColumnWithSystemMethodAndDefaultSchema()
        {
            var expressions = GeneratorTestHelper.GetCreateColumnWithSystemMethodExpression();
            var result = expressions.Select(x => (string)Generator.Generate((dynamic)x));
            result.ShouldBe([
                @"ALTER TABLE ""public"".""TestTable1"" ADD ""TestColumn1"" timestamp;",
                @"UPDATE ""public"".""TestTable1"" SET ""TestColumn1"" = now() WHERE 1 = 1;",
            ]);
        }

        [Test]
        public override void CanCreateDecimalColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateDecimalColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" ADD \"TestColumn1\" decimal(19,2) NOT NULL;");
        }

        [Test]
        public override void CanCreateDecimalColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateDecimalColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"public\".\"TestTable1\" ADD \"TestColumn1\" decimal(19,2) NOT NULL;");
        }

        [Test]
        public override void CanDropColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" DROP COLUMN \"TestColumn1\";");
        }

        [Test]
        public override void CanDropColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"public\".\"TestTable1\" DROP COLUMN \"TestColumn1\";");
        }

        [Test]
        public override void CanDropMultipleColumnsWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteColumnExpression(new[] { "TestColumn1", "TestColumn2" });
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" DROP COLUMN \"TestColumn1\";ALTER TABLE \"TestSchema\".\"TestTable1\" DROP COLUMN \"TestColumn2\";");
        }

        [Test]
        public override void CanDropMultipleColumnsWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteColumnExpression(new[] { "TestColumn1", "TestColumn2" });

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"public\".\"TestTable1\" DROP COLUMN \"TestColumn1\";ALTER TABLE \"public\".\"TestTable1\" DROP COLUMN \"TestColumn2\";");
        }

        [Test]
        public override void CanRenameColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" RENAME COLUMN \"TestColumn1\" TO \"TestColumn2\";");
        }

        [Test]
        public override void CanRenameColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"public\".\"TestTable1\" RENAME COLUMN \"TestColumn1\" TO \"TestColumn2\";");
        }

        [Test]
        public void CannotCreateColumnWithDateTimeOffsetPrecisionOutOfRange()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpressionWithDateTimeOffsetType();
            expression.Column.Precision = 7;

            Should.Throw(() => Generator.Generate(expression), typeof(ArgumentOutOfRangeException));
        }

        public abstract void CanCreateTableWithIdentityWithCustomSchema();
        public abstract void CanCreateTableWithIdentityWithDefaultSchema();
        public abstract void CanCreateColumnWithAutoIncrementAndCustomSchema();
        public abstract void CanCreateColumnWithAutoIncrementAndDefaultSchema();
    }
}
