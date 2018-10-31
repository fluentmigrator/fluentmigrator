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

using FluentMigrator.Exceptions;
using FluentMigrator.Runner.Generators.Snowflake;
using FluentMigrator.Runner.Processors.Snowflake;

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Generators.Snowflake
{
    [TestFixture(true)]
    [TestFixture(false)]
    [Category("Generator")]
    [Category("Snowflake")]
    public class SnowflakeColumnTests : BaseColumnTests
    {
        protected SnowflakeGenerator Generator;
        private readonly bool _quotingEnabled;

        public SnowflakeColumnTests(bool quotingEnabled)
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
        public override void CanCreateNullableColumnWithCustomDomainTypeAndCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpressionWithNullableCustomType();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe(@"ALTER TABLE ""TestSchema"".""TestTable1"" ADD COLUMN ""TestColumn1"" MyDomainType", _quotingEnabled);
        }

        [Test]
        public override void CanCreateNullableColumnWithCustomDomainTypeAndDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpressionWithNullableCustomType();
            Assert.Throws<ArgumentException>(() => Generator.Generate(expression));
        }

        [Test]
        public override void CanAlterColumnWithCustomSchema()
        {
            //TODO: This will fail if there are any keys attached
            var expression = GeneratorTestHelper.GetAlterColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe(@"ALTER TABLE ""TestSchema"".""TestTable1"" ALTER COLUMN ""TestColumn1"" SET NOT NULL, COLUMN ""TestColumn1"" VARCHAR(20), COLUMN ""TestColumn1"" COMMENT ''", _quotingEnabled);
        }

        [Test]
        public override void CanAlterColumnWithDefaultSchema()
        {
            //TODO: This will fail if there are any keys attached
            var expression = GeneratorTestHelper.GetAlterColumnExpression();
            Assert.Throws<ArgumentException>(() => Generator.Generate(expression));
        }

        [Test]
        public override void CanCreateAutoIncrementColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetAlterColumnAddAutoIncrementExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe(@"ALTER TABLE ""TestSchema"".""TestTable1"" ALTER COLUMN ""TestColumn1"" SET NOT NULL, COLUMN ""TestColumn1"" NUMBER, COLUMN ""TestColumn1"" COMMENT ''", _quotingEnabled);
        }

        [Test]
        public override void CanCreateAutoIncrementColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetAlterColumnAddAutoIncrementExpression();
            Assert.Throws<ArgumentException>(() => Generator.Generate(expression));
        }

        [Test]
        public override void CanCreateColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe(@"ALTER TABLE ""TestSchema"".""TestTable1"" ADD COLUMN ""TestColumn1"" VARCHAR(5) NOT NULL", _quotingEnabled);
        }

        [Test]
        public override void CanCreateColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();
            Assert.Throws<ArgumentException>(() => Generator.Generate(expression));
        }

        [Test]
        public override void CanCreateColumnWithSystemMethodAndCustomSchema()
        {
            var expressions = GeneratorTestHelper.GetCreateColumnWithSystemMethodExpression("TestSchema");
            var result = expressions.Select(x => (string)Generator.Generate((dynamic)x));
            result.ShouldBe(new [] { 
                @"ALTER TABLE ""TestSchema"".""TestTable1"" ADD COLUMN ""TestColumn1"" TIMESTAMP_NTZ",
                @"UPDATE ""TestSchema"".""TestTable1"" SET ""TestColumn1"" = CURRENT_TIMESTAMP() WHERE 1 = 1" }, _quotingEnabled);
        }

        [Test]
        public override void CanCreateColumnWithSystemMethodAndDefaultSchema()
        {
            var expressions = GeneratorTestHelper.GetCreateColumnWithSystemMethodExpression();
            foreach (var e in expressions)
            {
                Assert.Throws<ArgumentException>(() => Generator.Generate((dynamic) e));
            }
        }

        [Test]
        public override void CanCreateDecimalColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateDecimalColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe(@"ALTER TABLE ""TestSchema"".""TestTable1"" ADD COLUMN ""TestColumn1"" NUMBER(19,2) NOT NULL", _quotingEnabled);
        }

        [Test]
        public override void CanCreateDecimalColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateDecimalColumnExpression();
            Assert.Throws<ArgumentException>(() => Generator.Generate(expression));
        }

        [Test]
        public override void CanDropColumnWithCustomSchema()
        {
            //This does not work if it is a primary key
            var expression = GeneratorTestHelper.GetDeleteColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe(@"ALTER TABLE ""TestSchema"".""TestTable1"" DROP COLUMN ""TestColumn1""", _quotingEnabled);
        }

        [Test]
        public override void CanDropColumnWithDefaultSchema()
        {
            //This does not work if it is a primary key
            var expression = GeneratorTestHelper.GetDeleteColumnExpression();
            Assert.Throws<ArgumentException>(() => Generator.Generate(expression));
        }

        [Test]
        public override void CanDropMultipleColumnsWithCustomSchema()
        {
            //This does not work if it is a primary key
            var expression = GeneratorTestHelper.GetDeleteColumnExpression(new [] { "TestColumn1", "TestColumn2" });
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe(@"ALTER TABLE ""TestSchema"".""TestTable1"" DROP COLUMN ""TestColumn1""; ALTER TABLE ""TestSchema"".""TestTable1"" DROP COLUMN ""TestColumn2""", _quotingEnabled);
        }

        [Test]
        public override void CanDropMultipleColumnsWithDefaultSchema()
        {
            //This does not work if it is a primary key
            var expression = GeneratorTestHelper.GetDeleteColumnExpression(new [] { "TestColumn1", "TestColumn2" });
            Assert.Throws<ArgumentException>(() => Generator.Generate(expression));
        }

        [Test]
        public override void CanRenameColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe(@"ALTER TABLE ""TestSchema"".""TestTable1"" RENAME COLUMN ""TestColumn1"" TO ""TestColumn2""", _quotingEnabled);
        }

        [Test]
        public override void CanRenameColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();
            Assert.Throws<ArgumentException>(() => Generator.Generate(expression));
        }


        [Test]
        public void CollationNotSupported()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();
            expression.SchemaName = "TestSchema";
            expression.Column.CollationName = "Finnish_Swedish_CI_AS";

            var ex = Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(expression));
            Assert.AreEqual("Snowflake database does not support collation.", ex.Message);
        }
    }
}
