using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using FluentMigrator.Runner.Generators.Oracle;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.Oracle
{
    public class OracleCreateTableTests : BaseTableCreateTests
    {
        private OracleGenerator generator;

        [SetUp]
		public void Setup()
		{
			generator = new OracleGenerator();
		}

        [Test]
        public override void CanCreateTable()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            string sql = generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 NVARCHAR2(255) NOT NULL, TestColumn2 NUMBER(10,0) NOT NULL)");
        }

        [Test]
        public override void CanCreateTableWithCustomColumnType()
        {
            throw new NotImplementedException();
        }

        [Test]
        public override void CanCreateTableWithPrimaryKey()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithPrimaryKeyExpression();
            string sql = generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 NVARCHAR2(255) NOT NULL PRIMARY KEY, TestColumn2 NUMBER(10,0) NOT NULL)");

        }

        [Test]
        public override void CanCreateTableWithIdentity()
        {
            throw new NotImplementedException();
        }

        [Test]
        public override void CanCreateTableWithNullField()
        {
            throw new NotImplementedException();
        }

        [Test]
        public override void CanCreateTableWithDefaultValue()
        {

            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].DefaultValue = "abc";
            string sql = generator.Generate(expression);

            // Oracle requires the DEFAULT clause to appear before the NOT NULL clause
            sql.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 NVARCHAR2(255) DEFAULT 'abc' NOT NULL, TestColumn2 NUMBER(10,0) NOT NULL)");
        }

        [Test]
        public override void CanCreateTableWithDefaultValueExplicitlySetToNull()
        {
            throw new NotImplementedException();
        }

        [Test]
        public override void CanCreateIndex()
        {
            var expression = GeneratorTestHelper.GetCreateIndexExpression();
            string sql = generator.Generate(expression);
            sql.ShouldBe("CREATE UNIQUE INDEX IX_TEST ON TestTable1 (TestColumn1 ASC,TestColumn2 DESC)");
        }

        [Test]
        public override void CanCreateMultiColumnIndex()
        {
            var expression = GeneratorTestHelper.GetMultiColumnCreateIndexExpression();
            string sql = generator.Generate(expression);
            sql.ShouldBe("CREATE UNIQUE INDEX IX_TEST ON TestTable1 (TestColumn1 ASC,TestColumn2 DESC)");
        }

        [Test]
        public override void CanCreateTableWithMultipartKey()
        {

            var expression = GeneratorTestHelper.GetCreateTableWithMultipartKeyExpression();
               string sql = generator.Generate(expression);
            // See the note in OracleColumn about why the PK should not be named
               sql.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 NVARCHAR2(255) NOT NULL, TestColumn2 NUMBER(10,0) NOT NULL,  PRIMARY KEY (TestColumn1,TestColumn2))");

        }
    }
}
