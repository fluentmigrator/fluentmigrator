using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.Oracle;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.Oracle
{
    public class OracleCreateTableTests : BaseTableCreateTests
    {
        private OracleGenerator _generator;

        [SetUp]
        public void Setup()
        {
            _generator = new OracleGenerator();
        }

        [Test]
        public override void CanCreateTable()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            string sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 NVARCHAR2(255) NOT NULL, TestColumn2 NUMBER(10,0) NOT NULL)");
        }

        [Test]
        public override void CanCreateTableWithCustomColumnType()
        {
            //Do not knows any oracle custom types. Not testing for it.  If someone else knows please add a test
        }

        [Test]
        public override void CanCreateTableWithPrimaryKey()
        {
            //After a quick google the below tested statment should work.
            //CONSTRAINT constraint_name PRIMARY KEY (column1, column2, . column_n)
            var expression = GeneratorTestHelper.GetCreateTableWithPrimaryKeyExpression();
            string sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 NVARCHAR2(255) NOT NULL, TestColumn2 NUMBER(10,0) NOT NULL, PRIMARY KEY (TestColumn1))");
        }

        [Test]
        public override void CanCreateTableNamedPrimaryKey()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedPrimaryKeyExpression();
            string sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 NVARCHAR2(255) NOT NULL, TestColumn2 NUMBER(10,0) NOT NULL, CONSTRAINT TestKey PRIMARY KEY (TestColumn1))");
        }

        [Test]
        public override void CanCreateTableNamedMultiColumnPrimaryKey()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithMultiColumNamedPrimaryKeyExpression();
            string sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 NVARCHAR2(255) NOT NULL, TestColumn2 NUMBER(10,0) NOT NULL, CONSTRAINT TestKey PRIMARY KEY (TestColumn1, TestColumn2))");
        }

        [Test]
        public override void CanCreateTableWithIdentity()
        {
            Assert.Throws<DatabaseOperationNotSupportedException>(() => _generator.Generate(GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression()));
        }

        [Test]
        public override void CanCreateTableWithNullableField()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNullableColumn();
            string sql = _generator.Generate(expression);
            sql.ShouldBe(
                "CREATE TABLE TestTable1 (TestColumn1 NVARCHAR2(255), TestColumn2 NUMBER(10,0) NOT NULL)");
        }

        [Test]
        public override void CanCreateTableWithDefaultValue()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();
            string sql = _generator.Generate(expression);

            // Oracle requires the DEFAULT clause to appear before the NOT NULL clause
            sql.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 NVARCHAR2(255) DEFAULT 'Default' NOT NULL, TestColumn2 NUMBER(10,0) DEFAULT 0 NOT NULL)");
        }

        [Test]
        public override void CanCreateTableWithDefaultValueExplicitlySetToNull()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();
            expression.Columns[0].DefaultValue = null;
            string sql = _generator.Generate(expression);

            sql.ShouldBe(
                "CREATE TABLE TestTable1 (TestColumn1 NVARCHAR2(255) DEFAULT NULL NOT NULL, TestColumn2 NUMBER(10,0) DEFAULT 0 NOT NULL)");
        }

        [Test]
        public void CanCreateTableWithDefaultFunctionValue()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultFunctionValue();
            string sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 NVARCHAR2(50) DEFAULT TestFunction NOT NULL)");
        }

        [Test]
        public void CanCreateTableWithDefaultGuidValue()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultGuidValue();
            string sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 NVARCHAR2(50) DEFAULT sys_guid() NOT NULL)");
        }

        [Test]
        public void CanCreateTableWithDefaultCurrentDateValue()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultCurrentDateTimeValue();
            string sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 NVARCHAR2(50) DEFAULT sysdate NOT NULL)");
        }

        [Test]
        public override void CanCreateIndex()
        {
            var expression = GeneratorTestHelper.GetCreateIndexExpression();
            string sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE INDEX TestIndex ON TestTable1 (TestColumn1 ASC)");
        }

        [Test]
        public override void CanCreateMultiColumnIndex()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnCreateIndexExpression();
            string sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE INDEX TestIndex ON TestTable1 (TestColumn1 ASC, TestColumn2 DESC)");
        }

        [Test]
        public override void CanCreateUniqueIndex()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueIndexExpression();
            string sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE UNIQUE INDEX TestIndex ON TestTable1 (TestColumn1 ASC)");
        }

        [Test]
        public override void CanCreateMultiColumnUniqueIndex()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueMultiColumnIndexExpression();
            string sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE UNIQUE INDEX TestIndex ON TestTable1 (TestColumn1 ASC, TestColumn2 DESC)");
        }

        [Test]
        public override void CanCreateTableWithMultiColumnPrimaryKey()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithMultiColumnPrimaryKeyExpression();
            string sql = _generator.Generate(expression);
            // See the note in OracleColumn about why the PK should not be named
            sql.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 NVARCHAR2(255) NOT NULL, TestColumn2 NUMBER(10,0) NOT NULL, PRIMARY KEY (TestColumn1, TestColumn2))");
        }

        [Test]
        public override void CanCreateSchema()
        {
            var expression = new CreateSchemaExpression { SchemaName = "TestSchema" };
            var sql = _generator.Generate(expression);
            sql.ShouldBe(string.Empty);
        }

        [Test]
        public void CanCreateSchemaInStrictMode()
        {
            _generator.compatabilityMode = Runner.CompatabilityMode.STRICT;
            Assert.Throws<DatabaseOperationNotSupportedException>(() => _generator.Generate(new CreateSchemaExpression()));
        }
    }
}