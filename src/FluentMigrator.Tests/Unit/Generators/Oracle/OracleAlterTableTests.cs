﻿using System.Data;
using FluentMigrator.Exceptions;
using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators.Oracle;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.Oracle
{
    public class OracleAlterTableTests : BaseTableAlterTests
    {
        private OracleGenerator generator;
	    private OracleGenerator quotedIdentiferGenerator;

	    [SetUp]
        public void Setup()
        {
            generator = new OracleGenerator();
			quotedIdentiferGenerator = new OracleGenerator(true);
        }

        [Test]
        public override void CanAddColumn()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();
            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE TestTable1 ADD TestColumn1 NVARCHAR2(5) NOT NULL");

			sql = quotedIdentiferGenerator.Generate(expression);
			sql.ShouldBe("ALTER TABLE \"TestTable1\" ADD \"TestColumn1\" NVARCHAR2(5) NOT NULL");
        }

        [Test]
        public override void CanAddDecimalColumn()
        {
            var expression = GeneratorTestHelper.GetCreateDecimalColumnExpression();
            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE TestTable1 ADD TestColumn1 NUMBER(19,2) NOT NULL");

			sql = quotedIdentiferGenerator.Generate(expression);
			sql.ShouldBe("ALTER TABLE \"TestTable1\" ADD \"TestColumn1\" NUMBER(19,2) NOT NULL");
        }

        [Test]
        public override void CanRenameColumn()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();
            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE TestTable1 RENAME COLUMN TestColumn1 TO TestColumn2");
			
			sql = quotedIdentiferGenerator.Generate(expression);
			sql.ShouldBe("ALTER TABLE \"TestTable1\" RENAME COLUMN \"TestColumn1\" TO \"TestColumn2\"");
        }

        [Test]
        public override void CanRenameTable()
        {
            var expression = GeneratorTestHelper.GetRenameTableExpression();
            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE TestTable1 RENAME TO TestTable2");

			sql = quotedIdentiferGenerator.Generate(expression);
			sql.ShouldBe("ALTER TABLE \"TestTable1\" RENAME TO \"TestTable2\"");
        }

        [Test]
        public override void CanAlterColumn()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpression();
            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE TestTable1 MODIFY TestColumn1 NVARCHAR2(20) NOT NULL");

			
			sql = quotedIdentiferGenerator.Generate(expression);
			sql.ShouldBe("ALTER TABLE \"TestTable1\" MODIFY \"TestColumn1\" NVARCHAR2(20) NOT NULL");
        }

		[Test]
		public void CanAlterColumnNoNullSettings()
		{
			var expression = GeneratorTestHelper.GetAlterColumnExpression();
			expression.Column.IsNullable = null;
			string sql = generator.Generate(expression);
			sql.ShouldBe("ALTER TABLE TestTable1 MODIFY TestColumn1 NVARCHAR2(20)");

			sql = quotedIdentiferGenerator.Generate(expression);
			sql.ShouldBe("ALTER TABLE \"TestTable1\" MODIFY \"TestColumn1\" NVARCHAR2(20)");
		}

        [Test]
        public override void CanCreateForeignKey()
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
            string sql = generator.Generate(expression);
			sql.ShouldBe("ALTER TABLE TestTable1 ADD CONSTRAINT FK_Test FOREIGN KEY (TestColumn1) REFERENCES TestTable2 (TestColumn2)");
            

			sql = quotedIdentiferGenerator.Generate(expression);
			sql.ShouldBe("ALTER TABLE \"TestTable1\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"TestColumn1\") REFERENCES \"TestTable2\" (\"TestColumn2\")");

        }

        [TestCase(Rule.SetDefault, "SET DEFAULT"), TestCase(Rule.SetNull, "SET NULL"), TestCase(Rule.Cascade, "CASCADE")]
        public void CanCreateForeignKeyWithOnUpdateOptions(Rule rule, string output) 
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
            expression.ForeignKey.OnUpdate = rule;
            var sql = generator.Generate(expression);
            sql.ShouldBe(
                string.Format("ALTER TABLE TestTable1 ADD CONSTRAINT FK_Test FOREIGN KEY (TestColumn1) REFERENCES TestTable2 (TestColumn2) ON UPDATE {0}", output));

			sql = quotedIdentiferGenerator.Generate(expression);
			sql.ShouldBe(
				string.Format("ALTER TABLE \"TestTable1\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"TestColumn1\") REFERENCES \"TestTable2\" (\"TestColumn2\") ON UPDATE {0}", output));

        }

        [TestCase(Rule.SetDefault, "SET DEFAULT"), TestCase(Rule.SetNull, "SET NULL"), TestCase(Rule.Cascade, "CASCADE")]
        public void CanCreateForeignKeyWithOnDeleteOptions(Rule rule, string output) 
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
            expression.ForeignKey.OnDelete = rule;
            var sql = generator.Generate(expression);
            sql.ShouldBe(
                string.Format("ALTER TABLE TestTable1 ADD CONSTRAINT FK_Test FOREIGN KEY (TestColumn1) REFERENCES TestTable2 (TestColumn2) ON DELETE {0}", output));

			sql = quotedIdentiferGenerator.Generate(expression);
			sql.ShouldBe(
			  string.Format("ALTER TABLE \"TestTable1\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"TestColumn1\") REFERENCES \"TestTable2\" (\"TestColumn2\") ON DELETE {0}", output));

        }

        [Test]
        public void CanCreateForeignKeyWithOnDeleteAndOnUpdateOptions() 
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
            expression.ForeignKey.OnDelete = Rule.Cascade;
            expression.ForeignKey.OnUpdate = Rule.SetDefault;
            var sql = generator.Generate(expression);
            sql.ShouldBe(
                "ALTER TABLE TestTable1 ADD CONSTRAINT FK_Test FOREIGN KEY (TestColumn1) REFERENCES TestTable2 (TestColumn2) ON DELETE CASCADE ON UPDATE SET DEFAULT");

			sql = quotedIdentiferGenerator.Generate(expression);
			sql.ShouldBe(
            	"ALTER TABLE \"TestTable1\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"TestColumn1\") REFERENCES \"TestTable2\" (\"TestColumn2\") ON DELETE CASCADE ON UPDATE SET DEFAULT");

        }

        [Test]
        public override void CanCreateMulitColumnForeignKey()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnForeignKeyExpression();
            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE TestTable1 ADD CONSTRAINT FK_Test FOREIGN KEY (TestColumn1, TestColumn3) REFERENCES TestTable2 (TestColumn2, TestColumn4)");

			sql = quotedIdentiferGenerator.Generate(expression);
			sql.ShouldBe("ALTER TABLE \"TestTable1\" ADD CONSTRAINT \"FK_Test\" FOREIGN KEY (\"TestColumn1\", \"TestColumn3\") REFERENCES \"TestTable2\" (\"TestColumn2\", \"TestColumn4\")");
        }

        [Test]
        public override void CanCreateAutoIncrementColumn()
        {
        }

        [Test]
        public override void CanAlterSchema()
        {
            var expression = new AlterSchemaExpression();
            var result = generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }

        [Test]
        public void CanAlterSchemaInStrictMode()
        {
            generator.compatabilityMode = Runner.CompatabilityMode.STRICT;
            Assert.Throws<DatabaseOperationNotSupportedException>(() => generator.Generate(new CreateSchemaExpression()));
        }

        [Test]
        public void CanDropPrimaryKeyConstraint()
        {
            var expression = GeneratorTestHelper.GetDeletePrimaryKeyExpression();
            var result = generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 DROP CONSTRAINT TESTPRIMARYKEY");

			result = quotedIdentiferGenerator.Generate(expression);
			result.ShouldBe("ALTER TABLE \"TestTable1\" DROP CONSTRAINT \"TESTPRIMARYKEY\"");
        }

        [Test]
        public void CanDropUniqueConstraint()
        {
            var expression = GeneratorTestHelper.GetDeleteUniqueConstraintExpression();
            var result = generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 DROP CONSTRAINT TESTUNIQUECONSTRAINT");

			result = quotedIdentiferGenerator.Generate(expression);
			result.ShouldBe("ALTER TABLE \"TestTable1\" DROP CONSTRAINT \"TESTUNIQUECONSTRAINT\"");
        }

        [Test]
        public void CanCreatePrimaryKeyConstraint()
        {
            var expression = GeneratorTestHelper.GetCreatePrimaryKeyExpression();
            var result = generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 ADD CONSTRAINT PK_TestTable1_TestColumn1 PRIMARY KEY (TestColumn1)");

			result = quotedIdentiferGenerator.Generate(expression);
			result.ShouldBe("ALTER TABLE \"TestTable1\" ADD CONSTRAINT \"PK_TestTable1_TestColumn1\" PRIMARY KEY (\"TestColumn1\")");
        }

        [Test]
        public void CanCreateNamedPrimaryKeyConstraint()
        {
            var expression = GeneratorTestHelper.GetCreateNamedPrimaryKeyExpression();
            var result = generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 ADD CONSTRAINT TESTPRIMARYKEY PRIMARY KEY (TestColumn1)");

			result = quotedIdentiferGenerator.Generate(expression);
			result.ShouldBe("ALTER TABLE \"TestTable1\" ADD CONSTRAINT \"TESTPRIMARYKEY\" PRIMARY KEY (\"TestColumn1\")");
        }

        [Test]
        public void CanCreateMultiColmnPrimaryKeyConstraint()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnPrimaryKeyExpression();
            var result = generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 ADD CONSTRAINT PK_TestTable1_TestColumn1_TestColumn2 PRIMARY KEY (TestColumn1, TestColumn2)");

			result = quotedIdentiferGenerator.Generate(expression);
			result.ShouldBe("ALTER TABLE \"TestTable1\" ADD CONSTRAINT \"PK_TestTable1_TestColumn1_TestColumn2\" PRIMARY KEY (\"TestColumn1\", \"TestColumn2\")");

        }

        [Test]
        public void CanCreateMultiColmnNamedPrimaryKeyConstraint()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnNamedPrimaryKeyExpression();
            var result = generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 ADD CONSTRAINT TESTPRIMARYKEY PRIMARY KEY (TestColumn1, TestColumn2)");

			result = quotedIdentiferGenerator.Generate(expression);
			result.ShouldBe("ALTER TABLE \"TestTable1\" ADD CONSTRAINT \"TESTPRIMARYKEY\" PRIMARY KEY (\"TestColumn1\", \"TestColumn2\")");
        }

        [Test]
        public void CanCreateUniqueConstraint()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueConstraintExpression();
            var result = generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 ADD CONSTRAINT UC_TestTable1_TestColumn1 UNIQUE (TestColumn1)");

			result = quotedIdentiferGenerator.Generate(expression);
			result.ShouldBe("ALTER TABLE \"TestTable1\" ADD CONSTRAINT \"UC_TestTable1_TestColumn1\" UNIQUE (\"TestColumn1\")");
        }

        [Test]
        public void CanCreateNamedUniqueConstraint()
        {
            var expression = GeneratorTestHelper.GetCreateNamedUniqueConstraintExpression();
            var result = generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 ADD CONSTRAINT TESTUNIQUECONSTRAINT UNIQUE (TestColumn1)");

			result = quotedIdentiferGenerator.Generate(expression);
			result.ShouldBe("ALTER TABLE \"TestTable1\" ADD CONSTRAINT \"TESTUNIQUECONSTRAINT\" UNIQUE (\"TestColumn1\")");
        }

        [Test]
        public void CanCreateMultiColumnUniqueConstraint()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnUniqueConstraintExpression();
            var result = generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 ADD CONSTRAINT UC_TestTable1_TestColumn1_TestColumn2 UNIQUE (TestColumn1, TestColumn2)");

			result = quotedIdentiferGenerator.Generate(expression);
			result.ShouldBe("ALTER TABLE \"TestTable1\" ADD CONSTRAINT \"UC_TestTable1_TestColumn1_TestColumn2\" UNIQUE (\"TestColumn1\", \"TestColumn2\")");
        }

        [Test]
        public void CanCreateMultiColumnNamedUniqueConstraint()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnNamedUniqueConstraintExpression();
            var result = generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 ADD CONSTRAINT TESTUNIQUECONSTRAINT UNIQUE (TestColumn1, TestColumn2)");

			result = quotedIdentiferGenerator.Generate(expression);
			result.ShouldBe("ALTER TABLE \"TestTable1\" ADD CONSTRAINT \"TESTUNIQUECONSTRAINT\" UNIQUE (\"TestColumn1\", \"TestColumn2\")");
        }
    }
}
