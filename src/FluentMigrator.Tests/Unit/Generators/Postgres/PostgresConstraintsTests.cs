using System.Data;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Postgres;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.Postgres
{
    [TestFixture]
    public class PostgresConstraintsTests
    {
        protected PostgresGenerator generator;

        [SetUp]
        public void Setup()
        {
            generator = new PostgresGenerator();
        }

        [Test]
        public void CanCreateNamedForeignKeyWithDefaultSchema()
        {
            var expression = new CreateForeignKeyExpression();
            expression.ForeignKey.Name = "FK_Test";
            expression.ForeignKey.PrimaryTable = "TestPrimaryTable";
            expression.ForeignKey.ForeignTable = "TestForeignTable";
            expression.ForeignKey.PrimaryColumns = new[] { "Column1", "Column2" };
            expression.ForeignKey.ForeignColumns = new[] { "Column3", "Column4" };

            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"public\".\"TestForeignTable\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"Column3\",\"Column4\") REFERENCES \"public\".\"TestPrimaryTable\" (\"Column1\",\"Column2\")");
        }

        [Test]
        public void CanCreateNamedForeignKeyWithDifferentSchemas()
        {
            var expression = new CreateForeignKeyExpression();
            expression.ForeignKey.Name = "FK_Test";
            expression.ForeignKey.PrimaryTable = "TestPrimaryTable";
            expression.ForeignKey.ForeignTable = "TestForeignTable";
            expression.ForeignKey.PrimaryColumns = new[] { "Column1", "Column2" };
            expression.ForeignKey.ForeignColumns = new[] { "Column3", "Column4" };
            expression.ForeignKey.PrimaryTableSchema = "wibble";

            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"public\".\"TestForeignTable\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"Column3\",\"Column4\") REFERENCES \"wibble\".\"TestPrimaryTable\" (\"Column1\",\"Column2\")");
        }

        [Test]
        public void CanCreateNamedForeignKeyWithOnDeleteAndOnUpdateOptions()
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
            expression.ForeignKey.OnDelete = Rule.Cascade;
            expression.ForeignKey.OnUpdate = Rule.SetDefault;
            var sql = generator.Generate(expression);
            sql.ShouldBe(
                "ALTER TABLE \"public\".\"TestTable1\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"TestColumn1\") REFERENCES \"public\".\"TestTable2\" (\"TestColumn2\") ON DELETE CASCADE ON UPDATE SET DEFAULT");
        }

        [TestCase(Rule.SetDefault, "SET DEFAULT"), TestCase(Rule.SetNull, "SET NULL"), TestCase(Rule.Cascade, "CASCADE")]
        public void CanCreateNamedForeignKeyWithOnDeleteOptions(Rule rule, string output)
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
            expression.ForeignKey.OnDelete = rule;
            var sql = generator.Generate(expression);
            sql.ShouldBe(
                string.Format(
                    "ALTER TABLE \"public\".\"TestTable1\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"TestColumn1\") REFERENCES \"public\".\"TestTable2\" (\"TestColumn2\") ON DELETE {0}",
                    output));
        }

        [TestCase(Rule.SetDefault, "SET DEFAULT"), TestCase(Rule.SetNull, "SET NULL"), TestCase(Rule.Cascade, "CASCADE")]
        public void CanCreateNamedForeignKeyWithOnUpdateOptions(Rule rule, string output)
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
            expression.ForeignKey.OnUpdate = rule;
            var sql = generator.Generate(expression);
            sql.ShouldBe(
                string.Format("ALTER TABLE \"public\".\"TestTable1\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"TestColumn1\") REFERENCES \"public\".\"TestTable2\" (\"TestColumn2\") ON UPDATE {0}", output));
        }

        [Test]
        public void CanCreateNamedMultiColumnForeignKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
            expression.ForeignKey.PrimaryColumns = new[] { "Column1", "Column2" };
            expression.ForeignKey.ForeignColumns = new[] { "Column3", "Column4" };

            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"public\".\"TestTable1\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"Column3\",\"Column4\") REFERENCES \"public\".\"TestTable2\" (\"Column1\",\"Column2\")");
        }

        [Test]
        public void CanCreatePrimaryKeyConstraintWithCustomSchema()
        {
            var expression = new CreateConstraintExpression(ConstraintType.PrimaryKey);
            expression.Constraint.SchemaName = "Schema";
            expression.Constraint.TableName = "ConstraintTable";
            expression.Constraint.ConstraintName = "PK_Name";
            expression.Constraint.Columns.Add("column1");

            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"Schema\".\"ConstraintTable\" ADD CONSTRAINT \"PK_Name\" PRIMARY KEY (\"column1\")");
        }

        [Test]
        public void CanCreatePrimaryKeyConstraintWithDefaultSchema()
        {
            var expression = new CreateConstraintExpression(ConstraintType.PrimaryKey);
            expression.Constraint.TableName = "ConstraintTable";
            expression.Constraint.ConstraintName = "PK_Name";
            expression.Constraint.Columns.Add("column1");

            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"public\".\"ConstraintTable\" ADD CONSTRAINT \"PK_Name\" PRIMARY KEY (\"column1\")");
        }

        [Test]
        public void CanCreateUniqueConstraintWithCustomSchema()
        {
            var expression = new CreateConstraintExpression(ConstraintType.Unique);
            expression.Constraint.TableName = "ConstraintTable";
            expression.Constraint.SchemaName = "Schema";
            expression.Constraint.ConstraintName = "Constraint";
            expression.Constraint.Columns.Add("column1");

            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"Schema\".\"ConstraintTable\" ADD CONSTRAINT \"Constraint\" UNIQUE (\"column1\")");
        }

        [Test]
        public void CanCreateUniqueConstraintWithDefaultSchema()
        {
            var expression = new CreateConstraintExpression(ConstraintType.Unique);
            expression.Constraint.TableName = "ConstraintTable";
            expression.Constraint.ConstraintName = "Constraint";
            expression.Constraint.Columns.Add("column1");

            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"public\".\"ConstraintTable\" ADD CONSTRAINT \"Constraint\" UNIQUE (\"column1\")");
        }

        [Test]
        public void CanDropForeignKeyWithDefaultSchema()
        {
            var expression = new DeleteForeignKeyExpression();
            expression.ForeignKey.Name = "FK_Test";
            expression.ForeignKey.ForeignTable = "TestPrimaryTable";

            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"public\".\"TestPrimaryTable\" DROP CONSTRAINT \"FK_Test\"");
        }

        [Test]
        public void CanDropPrimaryKeyConstraintWithDefaultSchema()
        {
            var expression = new DeleteConstraintExpression(ConstraintType.Unique);
            expression.Constraint.TableName = "ConstraintTable";
            expression.Constraint.ConstraintName = "Constraint";

            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"public\".\"ConstraintTable\" DROP CONSTRAINT \"Constraint\"");
        }

        [Test]
        public void CanDropUniqueConstraintWithDefaultSchema()
        {
            var expression = new DeleteConstraintExpression(ConstraintType.Unique);
            expression.Constraint.TableName = "ConstraintTable";
            expression.Constraint.SchemaName = "Schema";
            expression.Constraint.ConstraintName = "Constraint";

            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"Schema\".\"ConstraintTable\" DROP CONSTRAINT \"Constraint\"");
        }
    }
}