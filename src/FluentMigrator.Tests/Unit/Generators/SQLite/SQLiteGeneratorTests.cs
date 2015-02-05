using FluentMigrator.Exceptions;
using FluentMigrator.Expressions;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Extensions;
using FluentMigrator.Runner.Generators.SQLite;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.SQLite
{
    [TestFixture]
    public class SQLiteGeneratorTests
    {
        protected SQLiteGenerator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new SQLiteGenerator();
        }

        [Test]
        public void CanAlterColumnInStrictMode()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();
            Generator.compatabilityMode = CompatabilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(expression));
        }

        [Test]
        public void CanAlterSchemaInStrictMode()
        {
            Generator.compatabilityMode = CompatabilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(new CreateSchemaExpression()));
        }

        [Test]
        public void CanCreateForeignKeyInStrictMode()
        {
            Generator.compatabilityMode = CompatabilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(GeneratorTestHelper.GetCreateNamedForeignKeyExpression()));
        }

        [Test]
        public void CanCreateMulitColumnForeignKeyInStrictMode()
        {
            Generator.compatabilityMode = CompatabilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(GeneratorTestHelper.GetCreateNamedMultiColumnForeignKeyExpression()));
        }

        [Test]
        public void CanCreateSchemaInStrictMode()
        {
            Generator.compatabilityMode = CompatabilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(new CreateSchemaExpression()));
        }

        [Test]
        public void CanCreateTableWithSeededIdentityAndLooseCompatibility()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression();
            expression.Columns[0].IsPrimaryKey = true;
            expression.Columns[0].AdditionalFeatures.Add(SqlServerExtensions.IdentitySeed, 3);
            expression.Columns[0].AdditionalFeatures.Add(SqlServerExtensions.IdentityIncrement, 3);
            Generator.compatabilityMode = CompatabilityMode.LOOSE;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"TestTable1\" (\"TestColumn1\" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, \"TestColumn2\" INTEGER NOT NULL)");
        }

        [Test]
        public void CanDropForeignKeyInStrictMode()
        {
            var expression = GeneratorTestHelper.GetDeleteForeignKeyExpression();
            Generator.compatabilityMode = CompatabilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(expression));
        }

        [Test]
        public void CanDropSchemaInStrictMode()
        {
            Generator.compatabilityMode = CompatabilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(new DeleteSchemaExpression()));
        }

        [Test]
        public void CanNotCreateTableWithSeededIdentityAndStrictCompatibility()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression();
            expression.Columns[0].IsPrimaryKey = true;
            expression.Columns[0].AdditionalFeatures.Add(SqlServerExtensions.IdentitySeed, 3);
            expression.Columns[0].AdditionalFeatures.Add(SqlServerExtensions.IdentityIncrement, 3);
            Generator.compatabilityMode = CompatabilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(expression));
        }

        [Test]
        public void CanRenameColumnInStrictMode()
        {
            Generator.compatabilityMode = CompatabilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(GeneratorTestHelper.GetRenameColumnExpression()));
        }
    }
}