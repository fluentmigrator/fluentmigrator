using FluentMigrator.Exceptions;
using FluentMigrator.Runner.Generators.Oracle;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.Oracle
{
    [TestFixture]
    public class OracleTableTests
    {
        protected OracleGenerator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new OracleGenerator();
        }

        [Test]
        public void CanCreateTableWithCustomColumnTypeWithDefaultSchema()
        {
            //Do not knows any oracle custom types. Not testing for it.  If someone else knows please add a test
        }

        [Test]
        public void CanCreateTableWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 NVARCHAR2(255) NOT NULL, TestColumn2 NUMBER(10,0) NOT NULL)");
        }

        [Test]
        public void CanCreateTableWithDefaultValueExplicitlySetToNullWithDefaultSchema()
        {
            //Not sure how this would work in oracle.  Someone please add a test
        }

        [Test]
        public void CanCreateTableWithDefaultValueWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();

            // Oracle requires the DEFAULT clause to appear before the NOT NULL clause

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 NVARCHAR2(255) DEFAULT 'Default' NOT NULL, TestColumn2 NUMBER(10,0) DEFAULT 0 NOT NULL)");
        }

        [Test]
        public void CanCreateTableWithIdentityWithDefaultSchema()
        {
            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression()));
        }

        [Test]
        public void CanCreateTableWithMultiColumnPrimaryKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithMultiColumnPrimaryKeyExpression();

            // See the note in OracleColumn about why the PK should not be named

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 NVARCHAR2(255) NOT NULL, TestColumn2 NUMBER(10,0) NOT NULL, PRIMARY KEY (TestColumn1, TestColumn2))");
        }

        [Test]
        public void CanCreateTableWithNamedMultiColumnPrimaryKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedMultiColumnPrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 NVARCHAR2(255) NOT NULL, TestColumn2 NUMBER(10,0) NOT NULL, CONSTRAINT TestKey PRIMARY KEY (TestColumn1, TestColumn2))");
        }

        [Test]
        public void CanCreateTableWithNamedPrimaryKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedPrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 NVARCHAR2(255) NOT NULL, TestColumn2 NUMBER(10,0) NOT NULL, CONSTRAINT TestKey PRIMARY KEY (TestColumn1))");
        }

        [Test]
        public void CanCreateTableWithNullableFieldWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNullableColumn();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 NVARCHAR2(255), TestColumn2 NUMBER(10,0) NOT NULL)");
        }

        [Test]
        public void CanCreateTableWithPrimaryKeyWithDefaultSchema()
        {
            //After a quick goolge the below tested statment should work.
            //CONSTRAINT constraint_name PRIMARY KEY (column1, column2, . column_n)
            var expression = GeneratorTestHelper.GetCreateTableWithPrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 NVARCHAR2(255) NOT NULL, TestColumn2 NUMBER(10,0) NOT NULL, PRIMARY KEY (TestColumn1))");
        }

        [Test]
        public void CanDropTableWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteTableExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("DROP TABLE TestTable1");
        }

        [Test]
        public void CanRenameTableWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetRenameTableExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 RENAME TO TestTable2");
        }
    }
}