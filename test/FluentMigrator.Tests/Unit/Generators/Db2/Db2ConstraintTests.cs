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

using System.Data;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.DB2;
using FluentMigrator.Runner.Generators.DB2.iSeries;

using Microsoft.Extensions.Options;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.Db2
{
    [TestFixture]
    public class Db2ConstraintTests : BaseConstraintsTests
    {
        protected Db2Generator Generator;

        [SetUp]
        public void Setup()
        {
            var generatorOptions = new OptionsWrapper<GeneratorOptions>(new GeneratorOptions());
            Generator = new Db2Generator(new Db2ISeriesQuoter(), generatorOptions);
        }

        [Test]
        public override void CanCreateForeignKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";
            expression.ForeignKey.PrimaryTableSchema = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestSchema.TestTable1 ADD CONSTRAINT TestSchema.FK_TestTable1_TestColumn1_TestTable2_TestColumn2 FOREIGN KEY (TestColumn1) REFERENCES TestSchema.TestTable2 (TestColumn2);");
        }

        [Test]
        public override void CanCreateForeignKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 ADD CONSTRAINT FK_TestTable1_TestColumn1_TestTable2_TestColumn2 FOREIGN KEY (TestColumn1) REFERENCES TestTable2 (TestColumn2);");
        }

        [Test]
        public override void CanCreateForeignKeyWithDifferentSchemas()
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestSchema.TestTable1 ADD CONSTRAINT TestSchema.FK_TestTable1_TestColumn1_TestTable2_TestColumn2 FOREIGN KEY (TestColumn1) REFERENCES TestTable2 (TestColumn2);");
        }

        [Test]
        public override void CanCreateMultiColumnForeignKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";
            expression.ForeignKey.PrimaryTableSchema = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestSchema.TestTable1 ADD CONSTRAINT TestSchema.FK_TestTable1_TestColumn1_TestColumn3_TestTable2_TestColumn2_TestColumn4 FOREIGN KEY (TestColumn1, TestColumn3) REFERENCES TestSchema.TestTable2 (TestColumn2, TestColumn4);");
        }

        [Test]
        public override void CanCreateMultiColumnForeignKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnForeignKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 ADD CONSTRAINT FK_TestTable1_TestColumn1_TestColumn3_TestTable2_TestColumn2_TestColumn4 FOREIGN KEY (TestColumn1, TestColumn3) REFERENCES TestTable2 (TestColumn2, TestColumn4);");
        }

        [Test]
        public override void CanCreateMultiColumnForeignKeyWithDifferentSchemas()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestSchema.TestTable1 ADD CONSTRAINT TestSchema.FK_TestTable1_TestColumn1_TestColumn3_TestTable2_TestColumn2_TestColumn4 FOREIGN KEY (TestColumn1, TestColumn3) REFERENCES TestTable2 (TestColumn2, TestColumn4);");
        }

        [Test]
        public override void CanCreateMultiColumnPrimaryKeyConstraintWithCustomSchema([Values] CompatibilityMode compatibilityMode)
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnPrimaryKeyExpression();
            expression.Constraint.SchemaName = "TestSchema";

            Generator.CompatibilityMode = compatibilityMode;
            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestSchema.TestTable1 ADD CONSTRAINT TestSchema.PK_TestTable1_TestColumn1_TestColumn2 PRIMARY KEY (TestColumn1, TestColumn2);");
        }

        [Test]
        public override void CanCreateMultiColumnPrimaryKeyConstraintWithDefaultSchema([Values] CompatibilityMode compatibilityMode)
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnPrimaryKeyExpression();

            Generator.CompatibilityMode = compatibilityMode;
            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 ADD CONSTRAINT PK_TestTable1_TestColumn1_TestColumn2 PRIMARY KEY (TestColumn1, TestColumn2);");
        }

        [Test]
        public override void CanCreateMultiColumnUniqueConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestSchema.TestTable1 ADD CONSTRAINT TestSchema.UC_TestTable1_TestColumn1_TestColumn2 UNIQUE (TestColumn1, TestColumn2);");
        }

        [Test]
        public override void CanCreateMultiColumnUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnUniqueConstraintExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 ADD CONSTRAINT UC_TestTable1_TestColumn1_TestColumn2 UNIQUE (TestColumn1, TestColumn2);");
        }

        [Test]
        public override void CanCreateNamedForeignKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";
            expression.ForeignKey.PrimaryTableSchema = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestSchema.TestTable1 ADD CONSTRAINT TestSchema.FK_Test FOREIGN KEY (TestColumn1) REFERENCES TestSchema.TestTable2 (TestColumn2);");
        }

        [Test]
        public override void CanCreateNamedForeignKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedForeignKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 ADD CONSTRAINT FK_Test FOREIGN KEY (TestColumn1) REFERENCES TestTable2 (TestColumn2);");
        }

        [Test]
        public override void CanCreateNamedForeignKeyWithDifferentSchemas()
        {
            var expression = GeneratorTestHelper.GetCreateNamedForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestSchema.TestTable1 ADD CONSTRAINT TestSchema.FK_Test FOREIGN KEY (TestColumn1) REFERENCES TestTable2 (TestColumn2);");
        }

        [Test]
        public override void CanCreateNamedForeignKeyWithOnDeleteAndOnUpdateOptions()
        {
            var expression = GeneratorTestHelper.GetCreateNamedForeignKeyExpression();
            expression.ForeignKey.OnDelete = Rule.Cascade;
            expression.ForeignKey.OnUpdate = Rule.None;

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 ADD CONSTRAINT FK_Test FOREIGN KEY (TestColumn1) REFERENCES TestTable2 (TestColumn2) ON DELETE CASCADE;");
        }

        [TestCase(Rule.SetDefault, "SET DEFAULT"), TestCase(Rule.SetNull, "SET NULL"), TestCase(Rule.Cascade, "CASCADE")]
        public override void CanCreateNamedForeignKeyWithOnDeleteOptions(Rule rule, string output)
        {
            var expression = GeneratorTestHelper.GetCreateNamedForeignKeyExpression();
            expression.ForeignKey.OnDelete = rule;

            var result = Generator.Generate(expression);
            result.ShouldBe(string.Format("ALTER TABLE TestTable1 ADD CONSTRAINT FK_Test FOREIGN KEY (TestColumn1) REFERENCES TestTable2 (TestColumn2) ON DELETE {0};", output));
        }

        [TestCase(Rule.None, "NO ACTION")]
        public override void CanCreateNamedForeignKeyWithOnUpdateOptions(Rule rule, string output)
        {
            var expression = GeneratorTestHelper.GetCreateNamedForeignKeyExpression();
            expression.ForeignKey.OnUpdate = rule;

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 ADD CONSTRAINT FK_Test FOREIGN KEY (TestColumn1) REFERENCES TestTable2 (TestColumn2);");
        }

        [Test]
        public override void CanCreateNamedMultiColumnForeignKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";
            expression.ForeignKey.PrimaryTableSchema = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestSchema.TestTable1 ADD CONSTRAINT TestSchema.FK_Test FOREIGN KEY (TestColumn1, TestColumn3) REFERENCES TestSchema.TestTable2 (TestColumn2, TestColumn4);");
        }

        [Test]
        public override void CanCreateNamedMultiColumnForeignKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnForeignKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 ADD CONSTRAINT FK_Test FOREIGN KEY (TestColumn1, TestColumn3) REFERENCES TestTable2 (TestColumn2, TestColumn4);");
        }

        [Test]
        public override void CanCreateNamedMultiColumnForeignKeyWithDifferentSchemas()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestSchema.TestTable1 ADD CONSTRAINT TestSchema.FK_Test FOREIGN KEY (TestColumn1, TestColumn3) REFERENCES TestTable2 (TestColumn2, TestColumn4);");
        }

        [Test]
        public override void CanCreateNamedMultiColumnPrimaryKeyConstraintWithCustomSchema([Values] CompatibilityMode compatibilityMode)
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnPrimaryKeyExpression();
            expression.Constraint.SchemaName = "TestSchema";

            Generator.CompatibilityMode = compatibilityMode;
            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestSchema.TestTable1 ADD CONSTRAINT TestSchema.TESTPRIMARYKEY PRIMARY KEY (TestColumn1, TestColumn2);");
        }

        [Test]
        public override void CanCreateNamedMultiColumnPrimaryKeyConstraintWithDefaultSchema([Values] CompatibilityMode compatibilityMode)
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnPrimaryKeyExpression();

            Generator.CompatibilityMode = compatibilityMode;
            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 ADD CONSTRAINT TESTPRIMARYKEY PRIMARY KEY (TestColumn1, TestColumn2);");
        }

        [Test]
        public override void CanCreateNamedMultiColumnUniqueConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestSchema.TestTable1 ADD CONSTRAINT TestSchema.TESTUNIQUECONSTRAINT UNIQUE (TestColumn1, TestColumn2);");
        }

        [Test]
        public override void CanCreateNamedMultiColumnUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnUniqueConstraintExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 ADD CONSTRAINT TESTUNIQUECONSTRAINT UNIQUE (TestColumn1, TestColumn2);");
        }

        [Test]
        public override void CanCreateNamedPrimaryKeyConstraintWithCustomSchema([Values] CompatibilityMode compatibilityMode)
        {
            var expression = GeneratorTestHelper.GetCreateNamedPrimaryKeyExpression();
            expression.Constraint.SchemaName = "TestSchema";

            Generator.CompatibilityMode = compatibilityMode;
            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestSchema.TestTable1 ADD CONSTRAINT TestSchema.TESTPRIMARYKEY PRIMARY KEY (TestColumn1);");
        }

        [Test]
        public override void CanCreateNamedPrimaryKeyConstraintWithDefaultSchema([Values] CompatibilityMode compatibilityMode)
        {
            var expression = GeneratorTestHelper.GetCreateNamedPrimaryKeyExpression();

            Generator.CompatibilityMode = compatibilityMode;
            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 ADD CONSTRAINT TESTPRIMARYKEY PRIMARY KEY (TestColumn1);");
        }

        [Test]
        public override void CanCreateNamedUniqueConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestSchema.TestTable1 ADD CONSTRAINT TestSchema.TESTUNIQUECONSTRAINT UNIQUE (TestColumn1);");
        }

        [Test]
        public override void CanCreateNamedUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedUniqueConstraintExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 ADD CONSTRAINT TESTUNIQUECONSTRAINT UNIQUE (TestColumn1);");
        }

        [Test]
        public override void CanCreatePrimaryKeyConstraintWithCustomSchema([Values] CompatibilityMode compatibilityMode)
        {
            var expression = GeneratorTestHelper.GetCreatePrimaryKeyExpression();
            expression.Constraint.SchemaName = "TestSchema";

            Generator.CompatibilityMode = compatibilityMode;
            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestSchema.TestTable1 ADD CONSTRAINT TestSchema.PK_TestTable1_TestColumn1 PRIMARY KEY (TestColumn1);");
        }

        [Test]
        public override void CanCreatePrimaryKeyConstraintWithDefaultSchema([Values] CompatibilityMode compatibilityMode)
        {
            var expression = GeneratorTestHelper.GetCreatePrimaryKeyExpression();

            Generator.CompatibilityMode = compatibilityMode;
            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 ADD CONSTRAINT PK_TestTable1_TestColumn1 PRIMARY KEY (TestColumn1);");
        }

        [Test]
        public override void CanCreateUniqueConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestSchema.TestTable1 ADD CONSTRAINT TestSchema.UC_TestTable1_TestColumn1 UNIQUE (TestColumn1);");
        }

        [Test]
        public override void CanCreateUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueConstraintExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 ADD CONSTRAINT UC_TestTable1_TestColumn1 UNIQUE (TestColumn1);");
        }

        [Test]
        public override void CanDropForeignKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestSchema.TestTable1 DROP FOREIGN KEY TestSchema.FK_Test;");
        }

        [Test]
        public override void CanDropForeignKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteForeignKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 DROP FOREIGN KEY FK_Test;");
        }

        [Test]
        public override void CanDropPrimaryKeyConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeletePrimaryKeyExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestSchema.TestTable1 DROP CONSTRAINT TestSchema.TESTPRIMARYKEY;");
        }

        [Test]
        public override void CanDropPrimaryKeyConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeletePrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 DROP CONSTRAINT TESTPRIMARYKEY;");
        }

        [Test]
        public override void CanDropUniqueConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestSchema.TestTable1 DROP CONSTRAINT TestSchema.TESTUNIQUECONSTRAINT;");
        }

        [Test]
        public override void CanDropUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteUniqueConstraintExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 DROP CONSTRAINT TESTUNIQUECONSTRAINT;");
        }
    }
}
