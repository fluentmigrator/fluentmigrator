using System.Data;
using FluentMigrator.Runner.Generators.Oracle;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.OracleWithQuotedIdentifier
{
    [TestFixture]
    public class OracleConstraintsTests
    {
        protected OracleGenerator _generator;
        protected OracleGenerator generator;

        [SetUp]
        public void Setup()
        {
            _generator = new OracleGenerator(useQuotedIdentifiers: true);
            generator = new OracleGenerator(useQuotedIdentifiers: true);
        }

        [Test]
        public void CanCreateMultiColmnPrimaryKeyConstraint()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnPrimaryKeyExpression();
            var result = generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" ADD CONSTRAINT \"PK_TestTable1_TestColumn1_TestColumn2\" PRIMARY KEY (\"TestColumn1\", \"TestColumn2\")");
        }

        [Test]
        public void CanCreateMultiColumnUniqueConstraint()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnUniqueConstraintExpression();
            var result = generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" ADD CONSTRAINT \"UC_TestTable1_TestColumn1_TestColumn2\" UNIQUE (\"TestColumn1\", \"TestColumn2\")");
        }

        [Test]
        public void CanCreateForeignKey()
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"TestTable1\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"TestColumn1\") REFERENCES \"TestTable2\" (\"TestColumn2\")");

        }

        [Test]
        public void CanCreateForeignKeyWithOnDeleteAndOnUpdateOptions()
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
            expression.ForeignKey.OnDelete = Rule.Cascade;
            expression.ForeignKey.OnUpdate = Rule.SetDefault;
            var sql = generator.Generate(expression);
            sql.ShouldBe(
                "ALTER TABLE \"TestTable1\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"TestColumn1\") REFERENCES \"TestTable2\" (\"TestColumn2\") ON DELETE CASCADE ON UPDATE SET DEFAULT");
        }

        [TestCase(Rule.SetDefault, "SET DEFAULT"), TestCase(Rule.SetNull, "SET NULL"), TestCase(Rule.Cascade, "CASCADE")]
        public void CanCreateForeignKeyWithOnDeleteOptions(Rule rule, string output)
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
            expression.ForeignKey.OnDelete = rule;
            var sql = generator.Generate(expression);
            sql.ShouldBe(
                string.Format("ALTER TABLE \"TestTable1\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"TestColumn1\") REFERENCES \"TestTable2\" (\"TestColumn2\") ON DELETE {0}", output));
        }

        [TestCase(Rule.SetDefault, "SET DEFAULT"), TestCase(Rule.SetNull, "SET NULL"), TestCase(Rule.Cascade, "CASCADE")]
        public void CanCreateForeignKeyWithOnUpdateOptions(Rule rule, string output)
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
            expression.ForeignKey.OnUpdate = rule;
            var sql = generator.Generate(expression);
            sql.ShouldBe(
                string.Format("ALTER TABLE \"TestTable1\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"TestColumn1\") REFERENCES \"TestTable2\" (\"TestColumn2\") ON UPDATE {0}", output));
        }

        [Test]
        public void CanCreateMulitColumnForeignKey()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnForeignKeyExpression();
            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"TestTable1\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"TestColumn1\", \"TestColumn3\") REFERENCES \"TestTable2\" (\"TestColumn2\", \"TestColumn4\")");
        }

        [Test]
        public void CanCreateMultiColmnNamedPrimaryKeyConstraint()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnNamedPrimaryKeyExpression();
            var result = generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" ADD CONSTRAINT \"TESTPRIMARYKEY\" PRIMARY KEY (\"TestColumn1\", \"TestColumn2\")");
        }

        [Test]
        public void CanCreateMultiColumnNamedUniqueConstraint()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnNamedUniqueConstraintExpression();
            var result = generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" ADD CONSTRAINT \"TESTUNIQUECONSTRAINT\" UNIQUE (\"TestColumn1\", \"TestColumn2\")");
        }

        [Test]
        public void CanCreateNamedPrimaryKeyConstraint()
        {
            var expression = GeneratorTestHelper.GetCreateNamedPrimaryKeyExpression();
            var result = generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" ADD CONSTRAINT \"TESTPRIMARYKEY\" PRIMARY KEY (\"TestColumn1\")");
        }

        [Test]
        public void CanCreateNamedUniqueConstraint()
        {
            var expression = GeneratorTestHelper.GetCreateNamedUniqueConstraintExpression();
            var result = generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" ADD CONSTRAINT \"TESTUNIQUECONSTRAINT\" UNIQUE (\"TestColumn1\")");
        }

        [Test]
        public void CanCreatePrimaryKeyConstraint()
        {
            var expression = GeneratorTestHelper.GetCreatePrimaryKeyExpression();
            var result = generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" ADD CONSTRAINT \"PK_TestTable1_TestColumn1\" PRIMARY KEY (\"TestColumn1\")");
        }

        [Test]
        public void CanCreateUniqueConstraint()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueConstraintExpression();
            var result = generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" ADD CONSTRAINT \"UC_TestTable1_TestColumn1\" UNIQUE (\"TestColumn1\")");
        }

        [Test]
        public void CanDropForeignKey()
        {
            var expression = GeneratorTestHelper.GetDeleteForeignKeyExpression();
            string sql = _generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"TestTable1\" DROP CONSTRAINT \"FK_Test\"");
        }

        [Test]
        public void CanDropPrimaryKeyConstraint()
        {
            var expression = GeneratorTestHelper.GetDeletePrimaryKeyExpression();
            var result = generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" DROP CONSTRAINT \"TESTPRIMARYKEY\"");
        }

        [Test]
        public void CanDropUniqueConstraint()
        {
            var expression = GeneratorTestHelper.GetDeleteUniqueConstraintExpression();
            var result = generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" DROP CONSTRAINT \"TESTUNIQUECONSTRAINT\"");
        }
    }
}