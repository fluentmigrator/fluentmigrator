using System.Data;
using FluentMigrator.Runner.Generators.Postgres;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.Postgres
{
    [TestFixture]
    public class PostgresConstraintsTests
    {
        protected PostgresGenerator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new PostgresGenerator();
        }

        [Test]
        public void CanCreateNamedForeignKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedForeignKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"public\".\"TestTable1\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"TestColumn1\") REFERENCES \"public\".\"TestTable2\" (\"TestColumn2\")");
        }

        [Test]
        public void CanCreateNamedForeignKeyWithDifferentSchemas()
        {
            var expression = GeneratorTestHelper.GetCreateNamedForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"TestColumn1\") REFERENCES \"public\".\"TestTable2\" (\"TestColumn2\")");
        }

        [Test]
        public void CanCreateNamedForeignKeyWithOnDeleteAndOnUpdateOptions()
        {
            var expression = GeneratorTestHelper.GetCreateNamedForeignKeyExpression();
            expression.ForeignKey.OnDelete = Rule.Cascade;
            expression.ForeignKey.OnUpdate = Rule.SetDefault;

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"public\".\"TestTable1\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"TestColumn1\") REFERENCES \"public\".\"TestTable2\" (\"TestColumn2\") ON DELETE CASCADE ON UPDATE SET DEFAULT");
        }

        [TestCase(Rule.SetDefault, "SET DEFAULT"), TestCase(Rule.SetNull, "SET NULL"), TestCase(Rule.Cascade, "CASCADE")]
        public void CanCreateNamedForeignKeyWithOnDeleteOptions(Rule rule, string output)
        {
            var expression = GeneratorTestHelper.GetCreateNamedForeignKeyExpression();
            expression.ForeignKey.OnDelete = rule;

            var result = Generator.Generate(expression);
            result.ShouldBe(string.Format("ALTER TABLE \"public\".\"TestTable1\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"TestColumn1\") REFERENCES \"public\".\"TestTable2\" (\"TestColumn2\") ON DELETE {0}", output));
        }

        [TestCase(Rule.SetDefault, "SET DEFAULT"), TestCase(Rule.SetNull, "SET NULL"), TestCase(Rule.Cascade, "CASCADE")]
        public void CanCreateNamedForeignKeyWithOnUpdateOptions(Rule rule, string output)
        {
            var expression = GeneratorTestHelper.GetCreateNamedForeignKeyExpression();
            expression.ForeignKey.OnUpdate = rule;

            var result = Generator.Generate(expression);
            result.ShouldBe(string.Format("ALTER TABLE \"public\".\"TestTable1\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"TestColumn1\") REFERENCES \"public\".\"TestTable2\" (\"TestColumn2\") ON UPDATE {0}", output));
        }

        [Test]
        public void CanCreateNamedMultiColumnForeignKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnForeignKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"public\".\"TestTable1\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"TestColumn1\",\"TestColumn3\") REFERENCES \"public\".\"TestTable2\" (\"TestColumn2\",\"TestColumn4\")");
        }

        [Test]
        public void CanCreatePrimaryKeyConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreatePrimaryKeyExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" ADD CONSTRAINT \"PK_TestTable1_TestColumn1\" PRIMARY KEY (\"TestColumn1\")");
        }

        [Test]
        public void CanCreatePrimaryKeyConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreatePrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"public\".\"TestTable1\" ADD CONSTRAINT \"PK_TestTable1_TestColumn1\" PRIMARY KEY (\"TestColumn1\")");
        }

        [Test]
        public void CanCreateUniqueConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" ADD CONSTRAINT \"UC_TestTable1_TestColumn1\" UNIQUE (\"TestColumn1\")");
        }

        [Test]
        public void CanCreateUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueConstraintExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"public\".\"TestTable1\" ADD CONSTRAINT \"UC_TestTable1_TestColumn1\" UNIQUE (\"TestColumn1\")");
        }

        [Test]
        public void CanDropForeignKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteForeignKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"public\".\"TestTable1\" DROP CONSTRAINT \"FK_Test\"");
        }

        [Test]
        public void CanDropPrimaryKeyConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeletePrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"public\".\"TestTable1\" DROP CONSTRAINT \"TESTPRIMARYKEY\"");
        }

        [Test]
        public void CanDropUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteUniqueConstraintExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"public\".\"TestTable1\" DROP CONSTRAINT \"TESTUNIQUECONSTRAINT\"");
        }
    }
}