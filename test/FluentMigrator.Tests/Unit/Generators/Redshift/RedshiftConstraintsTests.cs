#region License
// Copyright (c) 2007-2018, FluentMigrator Project
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

using System.Data;

using FluentMigrator.Runner.Generators.Redshift;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.Redshift
{
    [TestFixture]
    public sealed class RedshiftConstraintsTests : BaseConstraintsTests
    {
        private RedshiftGenerator _generator;

        [SetUp]
        public void Setup()
        {
            _generator = new RedshiftGenerator();
        }

        [Test]
        public override void CanCreateForeignKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";
            expression.ForeignKey.PrimaryTableSchema = "TestSchema";

            var result = _generator.Generate(expression);
            result.ShouldBe(
                "ALTER TABLE \"TestSchema\".\"TestTable1\" ADD CONSTRAINT \"FK_TestTable1_TestColumn1_TestTable2_TestColumn2\" FOREIGN KEY (\"TestColumn1\") REFERENCES \"TestSchema\".\"TestTable2\" (\"TestColumn2\");");
        }

        [Test]
        public override void CanCreateForeignKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();

            var result = _generator.Generate(expression);
            result.ShouldBe(
                "ALTER TABLE \"public\".\"TestTable1\" ADD CONSTRAINT \"FK_TestTable1_TestColumn1_TestTable2_TestColumn2\" FOREIGN KEY (\"TestColumn1\") REFERENCES \"public\".\"TestTable2\" (\"TestColumn2\");");
        }

        [Test]
        public override void CanCreateForeignKeyWithDifferentSchemas()
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";

            var result = _generator.Generate(expression);
            result.ShouldBe(
                "ALTER TABLE \"TestSchema\".\"TestTable1\" ADD CONSTRAINT \"FK_TestTable1_TestColumn1_TestTable2_TestColumn2\" FOREIGN KEY (\"TestColumn1\") REFERENCES \"public\".\"TestTable2\" (\"TestColumn2\");");
        }

        [Test]
        public override void CanCreateMultiColumnForeignKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";
            expression.ForeignKey.PrimaryTableSchema = "TestSchema";

            var result = _generator.Generate(expression);
            result.ShouldBe(
                "ALTER TABLE \"TestSchema\".\"TestTable1\" ADD CONSTRAINT \"FK_TestTable1_TestColumn1_TestColumn3_TestTable2_TestColumn2_TestColumn4\" FOREIGN KEY (\"TestColumn1\",\"TestColumn3\") REFERENCES \"TestSchema\".\"TestTable2\" (\"TestColumn2\",\"TestColumn4\");");
        }

        [Test]
        public override void CanCreateMultiColumnForeignKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnForeignKeyExpression();

            var result = _generator.Generate(expression);
            result.ShouldBe(
                "ALTER TABLE \"public\".\"TestTable1\" ADD CONSTRAINT \"FK_TestTable1_TestColumn1_TestColumn3_TestTable2_TestColumn2_TestColumn4\" FOREIGN KEY (\"TestColumn1\",\"TestColumn3\") REFERENCES \"public\".\"TestTable2\" (\"TestColumn2\",\"TestColumn4\");");
        }

        [Test]
        public override void CanCreateMultiColumnForeignKeyWithDifferentSchemas()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";

            var result = _generator.Generate(expression);
            result.ShouldBe(
                "ALTER TABLE \"TestSchema\".\"TestTable1\" ADD CONSTRAINT \"FK_TestTable1_TestColumn1_TestColumn3_TestTable2_TestColumn2_TestColumn4\" FOREIGN KEY (\"TestColumn1\",\"TestColumn3\") REFERENCES \"public\".\"TestTable2\" (\"TestColumn2\",\"TestColumn4\");");
        }

        [Test]
        public override void CanCreateMultiColumnPrimaryKeyConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnPrimaryKeyExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = _generator.Generate(expression);
            result.ShouldBe(
                "ALTER TABLE \"TestSchema\".\"TestTable1\" ADD CONSTRAINT \"PK_TestTable1_TestColumn1_TestColumn2\" PRIMARY KEY (\"TestColumn1\", \"TestColumn2\");");
        }

        [Test]
        public override void CanCreateMultiColumnPrimaryKeyConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnPrimaryKeyExpression();

            var result = _generator.Generate(expression);
            result.ShouldBe(
                "ALTER TABLE \"public\".\"TestTable1\" ADD CONSTRAINT \"PK_TestTable1_TestColumn1_TestColumn2\" PRIMARY KEY (\"TestColumn1\", \"TestColumn2\");");
        }

        [Test]
        public override void CanCreateMultiColumnUniqueConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = _generator.Generate(expression);
            result.ShouldBe(
                "ALTER TABLE \"TestSchema\".\"TestTable1\" ADD CONSTRAINT \"UC_TestTable1_TestColumn1_TestColumn2\" UNIQUE (\"TestColumn1\", \"TestColumn2\");");
        }

        [Test]
        public override void CanCreateMultiColumnUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnUniqueConstraintExpression();

            var result = _generator.Generate(expression);
            result.ShouldBe(
                "ALTER TABLE \"public\".\"TestTable1\" ADD CONSTRAINT \"UC_TestTable1_TestColumn1_TestColumn2\" UNIQUE (\"TestColumn1\", \"TestColumn2\");");
        }

        [Test]
        public override void CanCreateNamedForeignKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";
            expression.ForeignKey.PrimaryTableSchema = "TestSchema";

            var result = _generator.Generate(expression);
            result.ShouldBe(
                "ALTER TABLE \"TestSchema\".\"TestTable1\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"TestColumn1\") REFERENCES \"TestSchema\".\"TestTable2\" (\"TestColumn2\");");
        }

        [Test]
        public override void CanCreateNamedForeignKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedForeignKeyExpression();

            var result = _generator.Generate(expression);
            result.ShouldBe(
                "ALTER TABLE \"public\".\"TestTable1\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"TestColumn1\") REFERENCES \"public\".\"TestTable2\" (\"TestColumn2\");");
        }

        [Test]
        public override void CanCreateNamedForeignKeyWithDifferentSchemas()
        {
            var expression = GeneratorTestHelper.GetCreateNamedForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";

            var result = _generator.Generate(expression);
            result.ShouldBe(
                "ALTER TABLE \"TestSchema\".\"TestTable1\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"TestColumn1\") REFERENCES \"public\".\"TestTable2\" (\"TestColumn2\");");
        }

        [Test]
        public override void CanCreateNamedForeignKeyWithOnDeleteAndOnUpdateOptions()
        {
            var expression = GeneratorTestHelper.GetCreateNamedForeignKeyExpression();
            expression.ForeignKey.OnDelete = Rule.Cascade;
            expression.ForeignKey.OnUpdate = Rule.SetDefault;

            var result = _generator.Generate(expression);
            result.ShouldBe(
                "ALTER TABLE \"public\".\"TestTable1\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"TestColumn1\") REFERENCES \"public\".\"TestTable2\" (\"TestColumn2\") ON DELETE CASCADE ON UPDATE SET DEFAULT;");
        }

        [TestCase(Rule.SetDefault, "SET DEFAULT"), TestCase(Rule.SetNull, "SET NULL"),
         TestCase(Rule.Cascade, "CASCADE")]
        public override void CanCreateNamedForeignKeyWithOnDeleteOptions(Rule rule, string output)
        {
            var expression = GeneratorTestHelper.GetCreateNamedForeignKeyExpression();
            expression.ForeignKey.OnDelete = rule;

            var result = _generator.Generate(expression);
            result.ShouldBe(string.Format(
                                "ALTER TABLE \"public\".\"TestTable1\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"TestColumn1\") REFERENCES \"public\".\"TestTable2\" (\"TestColumn2\") ON DELETE {0};",
                                output));
        }

        [TestCase(Rule.SetDefault, "SET DEFAULT"), TestCase(Rule.SetNull, "SET NULL"),
         TestCase(Rule.Cascade, "CASCADE")]
        public override void CanCreateNamedForeignKeyWithOnUpdateOptions(Rule rule, string output)
        {
            var expression = GeneratorTestHelper.GetCreateNamedForeignKeyExpression();
            expression.ForeignKey.OnUpdate = rule;

            var result = _generator.Generate(expression);
            result.ShouldBe(string.Format(
                                "ALTER TABLE \"public\".\"TestTable1\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"TestColumn1\") REFERENCES \"public\".\"TestTable2\" (\"TestColumn2\") ON UPDATE {0};",
                                output));
        }

        [Test]
        public override void CanCreateNamedMultiColumnForeignKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";
            expression.ForeignKey.PrimaryTableSchema = "TestSchema";

            var result = _generator.Generate(expression);
            result.ShouldBe(
                "ALTER TABLE \"TestSchema\".\"TestTable1\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"TestColumn1\",\"TestColumn3\") REFERENCES \"TestSchema\".\"TestTable2\" (\"TestColumn2\",\"TestColumn4\");");
        }

        [Test]
        public override void CanCreateNamedMultiColumnForeignKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnForeignKeyExpression();

            var result = _generator.Generate(expression);
            result.ShouldBe(
                "ALTER TABLE \"public\".\"TestTable1\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"TestColumn1\",\"TestColumn3\") REFERENCES \"public\".\"TestTable2\" (\"TestColumn2\",\"TestColumn4\");");
        }

        [Test]
        public override void CanCreateNamedMultiColumnForeignKeyWithDifferentSchemas()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";

            var result = _generator.Generate(expression);
            result.ShouldBe(
                "ALTER TABLE \"TestSchema\".\"TestTable1\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"TestColumn1\",\"TestColumn3\") REFERENCES \"public\".\"TestTable2\" (\"TestColumn2\",\"TestColumn4\");");
        }

        [Test]
        public override void CanCreateNamedMultiColumnPrimaryKeyConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnPrimaryKeyExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = _generator.Generate(expression);
            result.ShouldBe(
                "ALTER TABLE \"TestSchema\".\"TestTable1\" ADD CONSTRAINT \"TESTPRIMARYKEY\" PRIMARY KEY (\"TestColumn1\", \"TestColumn2\");");
        }

        [Test]
        public override void CanCreateNamedMultiColumnPrimaryKeyConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnPrimaryKeyExpression();

            var result = _generator.Generate(expression);
            result.ShouldBe(
                "ALTER TABLE \"public\".\"TestTable1\" ADD CONSTRAINT \"TESTPRIMARYKEY\" PRIMARY KEY (\"TestColumn1\", \"TestColumn2\");");
        }

        [Test]
        public override void CanCreateNamedMultiColumnUniqueConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = _generator.Generate(expression);
            result.ShouldBe(
                "ALTER TABLE \"TestSchema\".\"TestTable1\" ADD CONSTRAINT \"TESTUNIQUECONSTRAINT\" UNIQUE (\"TestColumn1\", \"TestColumn2\");");
        }

        [Test]
        public override void CanCreateNamedMultiColumnUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnUniqueConstraintExpression();

            var result = _generator.Generate(expression);
            result.ShouldBe(
                "ALTER TABLE \"public\".\"TestTable1\" ADD CONSTRAINT \"TESTUNIQUECONSTRAINT\" UNIQUE (\"TestColumn1\", \"TestColumn2\");");
        }

        [Test]
        public override void CanCreateNamedPrimaryKeyConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedPrimaryKeyExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = _generator.Generate(expression);
            result.ShouldBe(
                "ALTER TABLE \"TestSchema\".\"TestTable1\" ADD CONSTRAINT \"TESTPRIMARYKEY\" PRIMARY KEY (\"TestColumn1\");");
        }

        [Test]
        public override void CanCreateNamedPrimaryKeyConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedPrimaryKeyExpression();

            var result = _generator.Generate(expression);
            result.ShouldBe(
                "ALTER TABLE \"public\".\"TestTable1\" ADD CONSTRAINT \"TESTPRIMARYKEY\" PRIMARY KEY (\"TestColumn1\");");
        }

        [Test]
        public override void CanCreateNamedUniqueConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = _generator.Generate(expression);
            result.ShouldBe(
                "ALTER TABLE \"TestSchema\".\"TestTable1\" ADD CONSTRAINT \"TESTUNIQUECONSTRAINT\" UNIQUE (\"TestColumn1\");");
        }

        [Test]
        public override void CanCreateNamedUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedUniqueConstraintExpression();

            var result = _generator.Generate(expression);
            result.ShouldBe(
                "ALTER TABLE \"public\".\"TestTable1\" ADD CONSTRAINT \"TESTUNIQUECONSTRAINT\" UNIQUE (\"TestColumn1\");");
        }

        [Test]
        public override void CanCreatePrimaryKeyConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreatePrimaryKeyExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = _generator.Generate(expression);
            result.ShouldBe(
                "ALTER TABLE \"TestSchema\".\"TestTable1\" ADD CONSTRAINT \"PK_TestTable1_TestColumn1\" PRIMARY KEY (\"TestColumn1\");");
        }

        [Test]
        public override void CanCreatePrimaryKeyConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreatePrimaryKeyExpression();

            var result = _generator.Generate(expression);
            result.ShouldBe(
                "ALTER TABLE \"public\".\"TestTable1\" ADD CONSTRAINT \"PK_TestTable1_TestColumn1\" PRIMARY KEY (\"TestColumn1\");");
        }

        [Test]
        public override void CanCreateUniqueConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = _generator.Generate(expression);
            result.ShouldBe(
                "ALTER TABLE \"TestSchema\".\"TestTable1\" ADD CONSTRAINT \"UC_TestTable1_TestColumn1\" UNIQUE (\"TestColumn1\");");
        }

        [Test]
        public override void CanCreateUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueConstraintExpression();

            var result = _generator.Generate(expression);
            result.ShouldBe(
                "ALTER TABLE \"public\".\"TestTable1\" ADD CONSTRAINT \"UC_TestTable1_TestColumn1\" UNIQUE (\"TestColumn1\");");
        }

        [Test]
        public override void CanDropForeignKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";

            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" DROP CONSTRAINT \"FK_Test\";");
        }

        [Test]
        public override void CanDropForeignKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteForeignKeyExpression();

            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"public\".\"TestTable1\" DROP CONSTRAINT \"FK_Test\";");
        }

        [Test]
        public override void CanDropPrimaryKeyConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeletePrimaryKeyExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" DROP CONSTRAINT \"TESTPRIMARYKEY\";");
        }

        [Test]
        public override void CanDropPrimaryKeyConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeletePrimaryKeyExpression();

            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"public\".\"TestTable1\" DROP CONSTRAINT \"TESTPRIMARYKEY\";");
        }

        [Test]
        public override void CanDropUniqueConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" DROP CONSTRAINT \"TESTUNIQUECONSTRAINT\";");
        }

        [Test]
        public override void CanDropUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteUniqueConstraintExpression();

            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"public\".\"TestTable1\" DROP CONSTRAINT \"TESTUNIQUECONSTRAINT\";");

        }
    }
}
