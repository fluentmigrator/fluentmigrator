using FluentMigrator.Runner.Processors.Firebird;
using FluentMigrator.Runner.Generators.Firebird;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.Firebird
{
    [TestFixture]
    public class FirebirdTableTests
    {
        protected FirebirdGenerator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new FirebirdGenerator(FirebirdOptions.StandardBehaviour());
        }

        [Test]
        public void CanCreateTableWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"TestTable1\" (\"TestColumn1\" BLOB SUB_TYPE TEXT NOT NULL, \"TestColumn2\" INTEGER NOT NULL)");
        }

        [Test]
        public void CanCreateTableWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"TestTable1\" (\"TestColumn1\" BLOB SUB_TYPE TEXT NOT NULL, \"TestColumn2\" INTEGER NOT NULL)");
        }

        [Test]
        public void CanCreateTableWithDefaultValueExplicitlySetToNullWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();
            expression.Columns[0].DefaultValue = null;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"TestTable1\" (\"TestColumn1\" BLOB SUB_TYPE TEXT DEFAULT NULL NOT NULL, \"TestColumn2\" INTEGER DEFAULT 0 NOT NULL)");

        }

        [Test]
        public void CanCreateTableWithDefaultValueWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"TestTable1\" (\"TestColumn1\" BLOB SUB_TYPE TEXT DEFAULT 'Default' NOT NULL, \"TestColumn2\" INTEGER DEFAULT 0 NOT NULL)");
        }

        [Test]
        public void CanCreateTableWithMultiColumnPrimaryKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithMultiColumnPrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"TestTable1\" (\"TestColumn1\" BLOB SUB_TYPE TEXT NOT NULL, \"TestColumn2\" INTEGER NOT NULL, PRIMARY KEY (\"TestColumn1\", \"TestColumn2\"))");
        }

        [Test]
        public void CanCreateTableWithNamedMultiColumnPrimaryKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedMultiColumnPrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"TestTable1\" (\"TestColumn1\" BLOB SUB_TYPE TEXT NOT NULL, \"TestColumn2\" INTEGER NOT NULL, CONSTRAINT \"TestKey\" PRIMARY KEY (\"TestColumn1\", \"TestColumn2\"))");
        }

        [Test]
        public void CanCreateTableWithNamedPrimaryKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedPrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"TestTable1\" (\"TestColumn1\" BLOB SUB_TYPE TEXT NOT NULL, \"TestColumn2\" INTEGER NOT NULL, CONSTRAINT \"TestKey\" PRIMARY KEY (\"TestColumn1\"))");
        }

        [Test]
        public void CanCreateTableWithPrimaryKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithPrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"TestTable1\" (\"TestColumn1\" BLOB SUB_TYPE TEXT NOT NULL, \"TestColumn2\" INTEGER NOT NULL, PRIMARY KEY (\"TestColumn1\"))");
        }

        [Test]
        public void CanDropTableWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteTableExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("DROP TABLE \"TestTable1\"");
        }

        [Test]
        public void CanDropTableWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteTableExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("DROP TABLE \"TestTable1\"");
        }

        [Test]
        public void CanRenameTableWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetRenameTableExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }
    }
}
