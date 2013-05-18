using System.Data;
using FluentMigrator.Expressions;
using FluentMigrator.Runner.Processors.Firebird;
using FluentMigrator.Runner.Generators.Firebird;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.Firebird
{
    [TestFixture]
    public class FirebirdConstraintsTests
    {
        protected FirebirdGenerator generator;

        [SetUp]
        public void Setup()
        {
            generator = new FirebirdGenerator(FirebirdOptions.StandardBehaviour());
        }
        
        [Test]
        public void CanCreateForeignKey()
        {
            var expression = new CreateForeignKeyExpression();
            expression.ForeignKey.Name = "FK_Test";
            expression.ForeignKey.PrimaryTable = "TestPrimaryTable";
            expression.ForeignKey.ForeignTable = "TestForeignTable";
            expression.ForeignKey.PrimaryColumns = new[] { "Column1", "Column2" };
            expression.ForeignKey.ForeignColumns = new[] { "Column3", "Column4" };

            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"TestForeignTable\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"Column3\", \"Column4\") REFERENCES \"TestPrimaryTable\" (\"Column1\", \"Column2\")");
        }

        [Test]
        public void CanCreateForeignKeyToDifferentSchema()
        {
            var expression = new CreateForeignKeyExpression();
            expression.ForeignKey.Name = "FK_Test";
            expression.ForeignKey.PrimaryTable = "TestPrimaryTable";
            expression.ForeignKey.ForeignTable = "TestForeignTable";
            expression.ForeignKey.PrimaryColumns = new[] { "Column1", "Column2" };
            expression.ForeignKey.ForeignColumns = new[] { "Column3", "Column4" };
            expression.ForeignKey.PrimaryTableSchema = "wibble";

            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"TestForeignTable\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"Column3\", \"Column4\") REFERENCES \"TestPrimaryTable\" (\"Column1\", \"Column2\")");
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
                string.Format(
                    "ALTER TABLE \"TestTable1\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"TestColumn1\") REFERENCES \"TestTable2\" (\"TestColumn2\") ON DELETE {0}",
                    output));
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
        public void CanCreateForeignKeyWithMultipleColumns()
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
            expression.ForeignKey.PrimaryColumns = new[] { "Column1", "Column2" };
            expression.ForeignKey.ForeignColumns = new[] { "Column3", "Column4" };

            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"TestTable1\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"Column3\", \"Column4\") REFERENCES \"TestTable2\" (\"Column1\", \"Column2\")");
        }

        [Test]
        public void CanDropForeignKey()
        {
            var expression = new DeleteForeignKeyExpression();
            expression.ForeignKey.Name = "FK_Test";
            expression.ForeignKey.ForeignTable = "TestPrimaryTable";

            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"TestPrimaryTable\" DROP CONSTRAINT \"FK_Test\"");
        }
    }
}
