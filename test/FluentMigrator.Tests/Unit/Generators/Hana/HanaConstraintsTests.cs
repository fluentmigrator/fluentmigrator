using System.Data;
using FluentMigrator.Runner.Generators.Hana;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.Hana
{
    [TestFixture]
    public class HanaConstraintsTests : BaseConstraintsTests
    {
        protected HanaGenerator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new HanaGenerator();
        }

        [Test]
        public override void CanCreateForeignKeyWithCustomSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";
            expression.ForeignKey.PrimaryTableSchema = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" ADD CONSTRAINT \"FK_TestTable1_TestColumn1_TestTable2_TestColumn2\" " +
                            "FOREIGN KEY (\"TestColumn1\") REFERENCES \"TestTable2\" (\"TestColumn2\")");
        }

        [Test]
        public override void CanCreateForeignKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" ADD CONSTRAINT \"FK_TestTable1_TestColumn1_TestTable2_TestColumn2\" " +
                            "FOREIGN KEY (\"TestColumn1\") REFERENCES \"TestTable2\" (\"TestColumn2\");");
        }

        [Test]
        public override void CanCreateForeignKeyWithDifferentSchemas()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" ADD CONSTRAINT \"FK_TestTable1_TestColumn1_TestTable2_TestColumn2\" " +
                            "FOREIGN KEY (\"TestColumn1\") REFERENCES \"TestTable2\" (\"TestColumn2\")");
        }

        [Test]
        public override void CanCreateMultiColumnForeignKeyWithCustomSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            var expression = GeneratorTestHelper.GetCreateMultiColumnForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";
            expression.ForeignKey.PrimaryTableSchema = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" ADD CONSTRAINT " +
                            "\"FK_TestTable1_TestColumn1_TestColumn3_TestTable2_TestColumn2_TestColumn4\" " +
                            "FOREIGN KEY (\"TestColumn1\",\"TestColumn3\") " +
                            "REFERENCES \"TestSchema\".\"TestTable2\" (\"TestColumn2\",\"TestColumn4\")");
        }

        [Test]
        public override void CanCreateMultiColumnForeignKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnForeignKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" ADD CONSTRAINT " +
                           "\"FK_TestTable1_TestColumn1_TestColumn3_TestTable2_TestColumn2_TestColumn4\" " +
                           "FOREIGN KEY (\"TestColumn1\", \"TestColumn3\") " +
                           "REFERENCES \"TestTable2\" (\"TestColumn2\", \"TestColumn4\");");
        }

        [Test]
        public override void CanCreateMultiColumnForeignKeyWithDifferentSchemas()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            var expression = GeneratorTestHelper.GetCreateMultiColumnForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" ADD CONSTRAINT " +
                           "\"FK_TestTable1_TestColumn1_TestColumn3_TestTable2_TestColumn2_TestColumn4\" " +
                           "FOREIGN KEY (\"TestColumn1\", \"TestColumn3\") " +
                           "REFERENCES \"TestTable2\" (\"TestColumn2\", \"TestColumn4\")");
        }

        [Test]
        public override void CanCreateMultiColumnPrimaryKeyConstraintWithCustomSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            var expression = GeneratorTestHelper.GetCreateMultiColumnPrimaryKeyExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" ADD CONSTRAINT \"PK_TestTable1_TestColumn1_TestColumn2\" " +
                            "PRIMARY KEY (\"TestColumn1\", \"TestColumn2\")");
        }

        [Test]
        public override void CanCreateMultiColumnPrimaryKeyConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnPrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" ADD CONSTRAINT \"PK_TestTable1_TestColumn1_TestColumn2\" " +
                            "PRIMARY KEY (\"TestColumn1\", \"TestColumn2\");");
        }

        [Test]
        public override void CanCreateMultiColumnUniqueConstraintWithCustomSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            var expression = GeneratorTestHelper.GetCreateMultiColumnUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" ADD CONSTRAINT \"UC_TestTable1_TestColumn1_TestColumn2\" " +
                            "UNIQUE (\"TestColumn1\", \"TestColumn2\")");
        }

        [Test]
        public override void CanCreateMultiColumnUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnUniqueConstraintExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" ADD CONSTRAINT \"UC_TestTable1_TestColumn1_TestColumn2\" " +
                            "UNIQUE (\"TestColumn1\", \"TestColumn2\");");
        }

        [Test]
        public override void CanCreateNamedForeignKeyWithCustomSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            var expression = GeneratorTestHelper.GetCreateNamedForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";
            expression.ForeignKey.PrimaryTableSchema = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" ADD CONSTRAINT \"FK_Test\" " +
                           "FOREIGN KEY (\"TestColumn1\") " +
                           "REFERENCES \"TestTable2\" (\"TestColumn2\")");
        }

        [Test]
        public override void CanCreateNamedForeignKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedForeignKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" ADD CONSTRAINT \"FK_Test\" " +
                            "FOREIGN KEY (\"TestColumn1\") " +
                            "REFERENCES \"TestTable2\" (\"TestColumn2\");");
        }

        [Test]
        public override void CanCreateNamedForeignKeyWithDifferentSchemas()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");
            
            var expression = GeneratorTestHelper.GetCreateNamedForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" ADD CONSTRAINT \"FK_Test\" " +
                            "FOREIGN KEY (\"TestColumn1\") " +
                            "REFERENCES \"TestTable2\" (\"TestColumn2\")");
        }

        [Test]
        public override void CanCreateNamedForeignKeyWithOnDeleteAndOnUpdateOptions()
        {
            var expression = GeneratorTestHelper.GetCreateNamedForeignKeyExpression();
            expression.ForeignKey.OnDelete = Rule.Cascade;
            expression.ForeignKey.OnUpdate = Rule.SetDefault;

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" ADD CONSTRAINT \"FK_Test\" " +
                            "FOREIGN KEY (\"TestColumn1\") REFERENCES \"TestTable2\" (\"TestColumn2\") " +
                            "ON DELETE CASCADE ON UPDATE SET DEFAULT;");
        }

        [TestCase(Rule.SetDefault, "SET DEFAULT"), TestCase(Rule.SetNull, "SET NULL"), TestCase(Rule.Cascade, "CASCADE")]
        public override void CanCreateNamedForeignKeyWithOnDeleteOptions(Rule rule, string output)
        {
            var expression = GeneratorTestHelper.GetCreateNamedForeignKeyExpression();
            expression.ForeignKey.OnDelete = rule;

            var result = Generator.Generate(expression);
            result.ShouldBe(string.Format(
                "ALTER TABLE \"TestTable1\" ADD CONSTRAINT \"FK_Test\" " +
                "FOREIGN KEY (\"TestColumn1\") REFERENCES \"TestTable2\" (\"TestColumn2\") " +
                "ON DELETE {0};", output));
        }

        [TestCase(Rule.SetDefault, "SET DEFAULT"), TestCase(Rule.SetNull, "SET NULL"), TestCase(Rule.Cascade, "CASCADE")]
        public override void CanCreateNamedForeignKeyWithOnUpdateOptions(Rule rule, string output)
        {
            var expression = GeneratorTestHelper.GetCreateNamedForeignKeyExpression();
            expression.ForeignKey.OnUpdate = rule;

            var result = Generator.Generate(expression);

            result.ShouldBe(string.Format(
                "ALTER TABLE \"TestTable1\" ADD CONSTRAINT \"FK_Test\" " +
                "FOREIGN KEY (\"TestColumn1\") REFERENCES \"TestTable2\" (\"TestColumn2\") " +
                "ON UPDATE {0};", output));
        }

        [Test]
        public override void CanCreateNamedMultiColumnForeignKeyWithCustomSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";
            expression.ForeignKey.PrimaryTableSchema = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" " +
                           "ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"TestColumn1\", \"TestColumn3\") " +
                           "REFERENCES \"TestTable2\" (\"TestColumn2\", \"TestColumn4\")");
        }

        [Test]
        public override void CanCreateNamedMultiColumnForeignKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnForeignKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" " +
                            "ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"TestColumn1\", \"TestColumn3\") " +
                            "REFERENCES \"TestTable2\" (\"TestColumn2\", \"TestColumn4\");");
        }

        [Test]
        public override void CanCreateNamedMultiColumnForeignKeyWithDifferentSchemas()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" " +
                           "ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"TestColumn1\", \"TestColumn3\") " +
                           "REFERENCES \"TestTable2\" (\"TestColumn2\", \"TestColumn4\")");
        }

        [Test]
        public override void CanCreateNamedMultiColumnPrimaryKeyConstraintWithCustomSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnPrimaryKeyExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" " +
                           "ADD CONSTRAINT \"TESTPRIMARYKEY\" " +
                           "PRIMARY KEY (\"TestColumn1\", \"TestColumn2\")");
        }

        [Test]
        public override void CanCreateNamedMultiColumnPrimaryKeyConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnPrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" " +
                            "ADD CONSTRAINT \"TESTPRIMARYKEY\" " +
                            "PRIMARY KEY (\"TestColumn1\", \"TestColumn2\");");
        }

        [Test]
        public override void CanCreateNamedMultiColumnUniqueConstraintWithCustomSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" " +
                            "ADD CONSTRAINT \"TESTUNIQUECONSTRAINT\" " +
                            "UNIQUE (\"TestColumn1\", \"TestColumn2\")");
        }

        [Test]
        public override void CanCreateNamedMultiColumnUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnUniqueConstraintExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" " +
                            "ADD CONSTRAINT \"TESTUNIQUECONSTRAINT\" " +
                            "UNIQUE (\"TestColumn1\", \"TestColumn2\");");
        }

        [Test]
        public override void CanCreateNamedPrimaryKeyConstraintWithCustomSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            var expression = GeneratorTestHelper.GetCreateNamedPrimaryKeyExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" " +
                            "ADD CONSTRAINT \"TESTPRIMARYKEY\" " +
                            "PRIMARY KEY (\"TestColumn1\")");
        }

        [Test]
        public override void CanCreateNamedPrimaryKeyConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedPrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" " +
                            "ADD CONSTRAINT \"TESTPRIMARYKEY\" " +
                            "PRIMARY KEY (\"TestColumn1\");");
        }

        [Test]
        public override void CanCreateNamedUniqueConstraintWithCustomSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            var expression = GeneratorTestHelper.GetCreateNamedUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" " +
                            "ADD CONSTRAINT \"TESTUNIQUECONSTRAINT\" " +
                            "UNIQUE (\"TestColumn1\")");
        }

        [Test]
        public override void CanCreateNamedUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedUniqueConstraintExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" " +
                            "ADD CONSTRAINT \"TESTUNIQUECONSTRAINT\" " +
                            "UNIQUE (\"TestColumn1\");");
        }

        [Test]
        public override void CanCreatePrimaryKeyConstraintWithCustomSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            var expression = GeneratorTestHelper.GetCreatePrimaryKeyExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" ADD CONSTRAINT \"PK_TestTable1_TestColumn1\" PRIMARY KEY (\"TestColumn1\")");
        }

        [Test]
        public override void CanCreatePrimaryKeyConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreatePrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" ADD CONSTRAINT \"PK_TestTable1_TestColumn1\" PRIMARY KEY (\"TestColumn1\");");
        }

        [Test]
        public override void CanCreateUniqueConstraintWithCustomSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            var expression = GeneratorTestHelper.GetCreateUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" ADD CONSTRAINT \"UC_TestTable1_TestColumn1\" UNIQUE (\"TestColumn1\")");
        }

        [Test]
        public override void CanCreateUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueConstraintExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" ADD CONSTRAINT \"UC_TestTable1_TestColumn1\" UNIQUE (\"TestColumn1\");");
        }

        [Test]
        public override void CanDropForeignKeyWithCustomSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            var expression = GeneratorTestHelper.GetDeleteForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" DROP CONSTRAINT \"FK_Test\"");
        }

        [Test]
        public override void CanDropForeignKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteForeignKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" DROP CONSTRAINT \"FK_Test\";");
        }

        [Test]
        public override void CanDropPrimaryKeyConstraintWithCustomSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            var expression = GeneratorTestHelper.GetDeletePrimaryKeyExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" DROP CONSTRAINT \"TESTPRIMARYKEY\"");
        }

        [Test]
        public override void CanDropPrimaryKeyConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeletePrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" DROP CONSTRAINT \"TESTPRIMARYKEY\";");
        }

        [Test]
        public override void CanDropUniqueConstraintWithCustomSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            var expression = GeneratorTestHelper.GetDeleteUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" DROP CONSTRAINT \"TESTUNIQUECONSTRAINT\"");
        }

        [Test]
        public override void CanDropUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteUniqueConstraintExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" DROP CONSTRAINT \"TESTUNIQUECONSTRAINT\";");
        }
    }
}