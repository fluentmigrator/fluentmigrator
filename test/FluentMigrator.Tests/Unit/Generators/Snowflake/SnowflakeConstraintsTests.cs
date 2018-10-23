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
using FluentMigrator.Runner.Generators.Snowflake;
using FluentMigrator.Runner.Processors.Snowflake;
using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Generators.Snowflake
{
    [TestFixture(true)]
    [TestFixture(false)]
    [Category("Generator")]
    [Category("Snowflake")]
    [Category("Constraint")]
    public class SnowflakeConstraintsTests : BaseConstraintsTests
    {
        protected SnowflakeGenerator Generator;
        private readonly bool _quotingEnabled;

        public SnowflakeConstraintsTests(bool quotingEnabled)
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
        public override void CanCreateForeignKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";
            expression.ForeignKey.PrimaryTableSchema = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe(@"ALTER TABLE ""TestSchema"".""TestTable1"" ADD CONSTRAINT ""FK_TestTable1_TestColumn1_TestTable2_TestColumn2"" FOREIGN KEY (""TestColumn1"") REFERENCES ""TestSchema"".""TestTable2"" (""TestColumn2"")", _quotingEnabled);
        }

        [Test]
        public override void CanCreateForeignKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
            Assert.Throws<ArgumentException>(() => Generator.Generate(expression));
        }

        [Test]
        public override void CanCreateForeignKeyWithDifferentSchemas()
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";
            expression.ForeignKey.PrimaryTableSchema = "TestSchema2";

            var result = Generator.Generate(expression);
            result.ShouldBe(@"ALTER TABLE ""TestSchema"".""TestTable1"" ADD CONSTRAINT ""FK_TestTable1_TestColumn1_TestTable2_TestColumn2"" FOREIGN KEY (""TestColumn1"") REFERENCES ""TestSchema2"".""TestTable2"" (""TestColumn2"")", _quotingEnabled);
        }

        [Test]
        public override void CanCreateMultiColumnForeignKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";
            expression.ForeignKey.PrimaryTableSchema = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe(@"ALTER TABLE ""TestSchema"".""TestTable1"" ADD CONSTRAINT ""FK_TestTable1_TestColumn1_TestColumn3_TestTable2_TestColumn2_TestColumn4"" FOREIGN KEY (""TestColumn1"", ""TestColumn3"") REFERENCES ""TestSchema"".""TestTable2"" (""TestColumn2"", ""TestColumn4"")", _quotingEnabled);
        }

        [Test]
        public override void CanCreateMultiColumnForeignKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnForeignKeyExpression();
            Assert.Throws<ArgumentException>(() => Generator.Generate(expression));
        }

        [Test]
        public override void CanCreateMultiColumnForeignKeyWithDifferentSchemas()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";
            expression.ForeignKey.PrimaryTableSchema = "TestSchema2";

            var result = Generator.Generate(expression);
            result.ShouldBe(@"ALTER TABLE ""TestSchema"".""TestTable1"" ADD CONSTRAINT ""FK_TestTable1_TestColumn1_TestColumn3_TestTable2_TestColumn2_TestColumn4"" FOREIGN KEY (""TestColumn1"", ""TestColumn3"") REFERENCES ""TestSchema2"".""TestTable2"" (""TestColumn2"", ""TestColumn4"")", _quotingEnabled);
        }

        [Test]
        public override void CanCreateMultiColumnPrimaryKeyConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnPrimaryKeyExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe(@"ALTER TABLE ""TestSchema"".""TestTable1"" ADD CONSTRAINT ""PK_TestTable1_TestColumn1_TestColumn2"" PRIMARY KEY (""TestColumn1"", ""TestColumn2"")", _quotingEnabled);
        }

        [Test]
        public override void CanCreateMultiColumnPrimaryKeyConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnPrimaryKeyExpression();
            Assert.Throws<ArgumentException>(() => Generator.Generate(expression));
        }

        [Test]
        public override void CanCreateMultiColumnUniqueConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe(@"ALTER TABLE ""TestSchema"".""TestTable1"" ADD CONSTRAINT ""UC_TestTable1_TestColumn1_TestColumn2"" UNIQUE (""TestColumn1"", ""TestColumn2"")", _quotingEnabled);
        }

        [Test]
        public override void CanCreateMultiColumnUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnUniqueConstraintExpression();
            Assert.Throws<ArgumentException>(() => Generator.Generate(expression));
        }

        [Test]
        public override void CanCreateNamedForeignKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";
            expression.ForeignKey.PrimaryTableSchema = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe(@"ALTER TABLE ""TestSchema"".""TestTable1"" ADD CONSTRAINT ""FK_Test"" FOREIGN KEY (""TestColumn1"") REFERENCES ""TestSchema"".""TestTable2"" (""TestColumn2"")", _quotingEnabled);
        }

        [Test]
        public override void CanCreateNamedForeignKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedForeignKeyExpression();
            Assert.Throws<ArgumentException>(() => Generator.Generate(expression));
        }

        [Test]
        public override void CanCreateNamedForeignKeyWithDifferentSchemas()
        {
            var expression = GeneratorTestHelper.GetCreateNamedForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";
            expression.ForeignKey.PrimaryTableSchema = "TestSchema2";

            var result = Generator.Generate(expression);
            result.ShouldBe(@"ALTER TABLE ""TestSchema"".""TestTable1"" ADD CONSTRAINT ""FK_Test"" FOREIGN KEY (""TestColumn1"") REFERENCES ""TestSchema2"".""TestTable2"" (""TestColumn2"")", _quotingEnabled);
        }

        [Test]
        public override void CanCreateNamedForeignKeyWithOnDeleteAndOnUpdateOptions()
        {
            var expression = GeneratorTestHelper.GetCreateNamedForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";
            expression.ForeignKey.PrimaryTableSchema = "TestSchema";
            expression.ForeignKey.OnDelete = Rule.Cascade;
            expression.ForeignKey.OnUpdate = Rule.SetDefault;

            var result = Generator.Generate(expression);
            result.ShouldBe(@"ALTER TABLE ""TestSchema"".""TestTable1"" ADD CONSTRAINT ""FK_Test"" FOREIGN KEY (""TestColumn1"") REFERENCES ""TestSchema"".""TestTable2"" (""TestColumn2"") ON DELETE CASCADE ON UPDATE SET DEFAULT", _quotingEnabled);
        }

        [TestCase(Rule.SetDefault, "SET DEFAULT"), TestCase(Rule.SetNull, "SET NULL"), TestCase(Rule.Cascade, "CASCADE")]
        public override void CanCreateNamedForeignKeyWithOnDeleteOptions(Rule rule, string output)
        {
            var expression = GeneratorTestHelper.GetCreateNamedForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";
            expression.ForeignKey.PrimaryTableSchema = "TestSchema";
            expression.ForeignKey.OnDelete = rule;

            var result = Generator.Generate(expression);
            result.ShouldBe($@"ALTER TABLE ""TestSchema"".""TestTable1"" ADD CONSTRAINT ""FK_Test"" FOREIGN KEY (""TestColumn1"") REFERENCES ""TestSchema"".""TestTable2"" (""TestColumn2"") ON DELETE {output}", _quotingEnabled);
        }

        [TestCase(Rule.SetDefault, "SET DEFAULT"), TestCase(Rule.SetNull, "SET NULL"), TestCase(Rule.Cascade, "CASCADE")]
        public override void CanCreateNamedForeignKeyWithOnUpdateOptions(Rule rule, string output)
        {
            var expression = GeneratorTestHelper.GetCreateNamedForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";
            expression.ForeignKey.PrimaryTableSchema = "TestSchema";
            expression.ForeignKey.OnUpdate = rule;

            var result = Generator.Generate(expression);
            result.ShouldBe($@"ALTER TABLE ""TestSchema"".""TestTable1"" ADD CONSTRAINT ""FK_Test"" FOREIGN KEY (""TestColumn1"") REFERENCES ""TestSchema"".""TestTable2"" (""TestColumn2"") ON UPDATE {output}", _quotingEnabled);
        }

        [Test]
        public override void CanCreateNamedMultiColumnForeignKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";
            expression.ForeignKey.PrimaryTableSchema = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe(@"ALTER TABLE ""TestSchema"".""TestTable1"" ADD CONSTRAINT ""FK_Test"" FOREIGN KEY (""TestColumn1"", ""TestColumn3"") REFERENCES ""TestSchema"".""TestTable2"" (""TestColumn2"", ""TestColumn4"")", _quotingEnabled);
        }

        [Test]
        public override void CanCreateNamedMultiColumnForeignKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnForeignKeyExpression();
            Assert.Throws<ArgumentException>(() => Generator.Generate(expression));
        }

        [Test]
        public override void CanCreateNamedMultiColumnForeignKeyWithDifferentSchemas()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";
            expression.ForeignKey.PrimaryTableSchema = "TestSchema2";

            var result = Generator.Generate(expression);
            result.ShouldBe(@"ALTER TABLE ""TestSchema"".""TestTable1"" ADD CONSTRAINT ""FK_Test"" FOREIGN KEY (""TestColumn1"", ""TestColumn3"") REFERENCES ""TestSchema2"".""TestTable2"" (""TestColumn2"", ""TestColumn4"")", _quotingEnabled);
        }

        [Test]
        public override void CanCreateNamedMultiColumnPrimaryKeyConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnPrimaryKeyExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe(@"ALTER TABLE ""TestSchema"".""TestTable1"" ADD CONSTRAINT ""TESTPRIMARYKEY"" PRIMARY KEY (""TestColumn1"", ""TestColumn2"")", _quotingEnabled);
        }

        [Test]
        public override void CanCreateNamedMultiColumnPrimaryKeyConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnPrimaryKeyExpression();
            Assert.Throws<ArgumentException>(() => Generator.Generate(expression));
        }

        [Test]
        public override void CanCreateNamedMultiColumnUniqueConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe(@"ALTER TABLE ""TestSchema"".""TestTable1"" ADD CONSTRAINT ""TESTUNIQUECONSTRAINT"" UNIQUE (""TestColumn1"", ""TestColumn2"")", _quotingEnabled);
        }

        [Test]
        public override void CanCreateNamedMultiColumnUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnUniqueConstraintExpression();
            Assert.Throws<ArgumentException>(() => Generator.Generate(expression));
        }

        [Test]
        public override void CanCreateNamedPrimaryKeyConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedPrimaryKeyExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe(@"ALTER TABLE ""TestSchema"".""TestTable1"" ADD CONSTRAINT ""TESTPRIMARYKEY"" PRIMARY KEY (""TestColumn1"")", _quotingEnabled);
        }

        [Test]
        public override void CanCreateNamedPrimaryKeyConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedPrimaryKeyExpression();
            Assert.Throws<ArgumentException>(() => Generator.Generate(expression));
        }

        [Test]
        public override void CanCreateNamedUniqueConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe(@"ALTER TABLE ""TestSchema"".""TestTable1"" ADD CONSTRAINT ""TESTUNIQUECONSTRAINT"" UNIQUE (""TestColumn1"")", _quotingEnabled);
        }

        [Test]
        public override void CanCreateNamedUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedUniqueConstraintExpression();
            Assert.Throws<ArgumentException>(() => Generator.Generate(expression));
        }

        [Test]
        public override void CanCreatePrimaryKeyConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreatePrimaryKeyExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe(@"ALTER TABLE ""TestSchema"".""TestTable1"" ADD CONSTRAINT ""PK_TestTable1_TestColumn1"" PRIMARY KEY (""TestColumn1"")", _quotingEnabled);
        }

        [Test]
        public override void CanCreatePrimaryKeyConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreatePrimaryKeyExpression();
            Assert.Throws<ArgumentException>(() => Generator.Generate(expression));
        }

        [Test]
        public override void CanCreateUniqueConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe(@"ALTER TABLE ""TestSchema"".""TestTable1"" ADD CONSTRAINT ""UC_TestTable1_TestColumn1"" UNIQUE (""TestColumn1"")", _quotingEnabled);
        }

        [Test]
        public override void CanCreateUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueConstraintExpression();
            Assert.Throws<ArgumentException>(() => Generator.Generate(expression));
        }

        [Test]
        public override void CanDropForeignKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe(@"ALTER TABLE ""TestSchema"".""TestTable1"" DROP CONSTRAINT ""FK_Test""", _quotingEnabled);
        }

        [Test]
        public override void CanDropForeignKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteForeignKeyExpression();
            Assert.Throws<ArgumentException>(() => Generator.Generate(expression));
        }

        [Test]
        public override void CanDropPrimaryKeyConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeletePrimaryKeyExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe(@"ALTER TABLE ""TestSchema"".""TestTable1"" DROP CONSTRAINT ""TESTPRIMARYKEY""", _quotingEnabled);
        }

        [Test]
        public override void CanDropPrimaryKeyConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeletePrimaryKeyExpression();
            Assert.Throws<ArgumentException>(() => Generator.Generate(expression));
        }

        [Test]
        public override void CanDropUniqueConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe(@"ALTER TABLE ""TestSchema"".""TestTable1"" DROP CONSTRAINT ""TESTUNIQUECONSTRAINT""", _quotingEnabled);
        }

        [Test]
        public override void CanDropUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteUniqueConstraintExpression();
            Assert.Throws<ArgumentException>(() => Generator.Generate(expression));
        }
    }
}
