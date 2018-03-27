using FluentMigrator.Exceptions;
using FluentMigrator.Runner.Generators.Oracle;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.Oracle
{
    [TestFixture]
    public class OracleColumnTests : BaseColumnTests
    {
        protected OracleGenerator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new OracleGenerator();
        }

        [Test]
        public override void CanAlterColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 MODIFY TestColumn1 NVARCHAR2(20) NOT NULL");
        }

        [Test]
        public override void CanAlterColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 MODIFY TestColumn1 NVARCHAR2(20) NOT NULL");
        }

        [Test]
        public override void CanCreateAutoIncrementColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetAlterColumnAddAutoIncrementExpression();
            expression.SchemaName = "TestSchema";

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(expression));
        }

        [Test]
        public override void CanCreateAutoIncrementColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetAlterColumnAddAutoIncrementExpression();

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(expression));
        }

        [Test]
        public override void CanCreateColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 ADD TestColumn1 NVARCHAR2(5) NOT NULL");
        }

        [Test]
        public override void CanCreateColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 ADD TestColumn1 NVARCHAR2(5) NOT NULL");
        }

        [Test]
        public override void CanCreateDecimalColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateDecimalColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 ADD TestColumn1 NUMBER(19,2) NOT NULL");
        }

        [Test]
        public override void CanCreateDecimalColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateDecimalColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 ADD TestColumn1 NUMBER(19,2) NOT NULL");
        }

        [Test]
        public override void CanDropColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 DROP COLUMN TestColumn1");
        }

        [Test]
        public override void CanDropColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 DROP COLUMN TestColumn1");
        }

        [Test]
        public override void CanDropMultipleColumnsWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteColumnExpression(new[] { "TestColumn1", "TestColumn2" });
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 DROP COLUMN TestColumn1;" + System.Environment.NewLine + "ALTER TABLE TestTable1 DROP COLUMN TestColumn2");
        }

        [Test]
        public override void CanDropMultipleColumnsWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteColumnExpression(new[] { "TestColumn1", "TestColumn2" });

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 DROP COLUMN TestColumn1;" + System.Environment.NewLine + "ALTER TABLE TestTable1 DROP COLUMN TestColumn2");
        }

        [Test]
        public override void CanRenameColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 RENAME COLUMN TestColumn1 TO TestColumn2");
        }

        [Test]
        public override void CanRenameColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 RENAME COLUMN TestColumn1 TO TestColumn2");
        }

        [Test]
        public void CanCreateColumnWithDefaultValue()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();
            expression.Column.DefaultValue = 1;

            var result = Generator.Generate(expression);

            result.ShouldBe("ALTER TABLE TestTable1 ADD TestColumn1 NVARCHAR2(5) DEFAULT 1 NOT NULL");
        }

        [Test]
        public void CanCreateColumnWithDefaultStringValue()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();
            expression.Column.DefaultValue = "1";

            var result = Generator.Generate(expression);

            result.ShouldBe("ALTER TABLE TestTable1 ADD TestColumn1 NVARCHAR2(5) DEFAULT '1' NOT NULL");
        }

        [Test]
        public void CanCreateColumnWithDefaultSystemMethodNewGuid()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();
            expression.Column.DefaultValue = SystemMethods.NewGuid;

            var result = Generator.Generate(expression);

            result.ShouldBe("ALTER TABLE TestTable1 ADD TestColumn1 NVARCHAR2(5) DEFAULT sys_guid() NOT NULL");
        }

        [Test]
        public void CanCreateColumnWithDefaultSystemMethodCurrentDateTime()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();
            expression.Column.DefaultValue = SystemMethods.CurrentDateTime;

            var result = Generator.Generate(expression);

            result.ShouldBe("ALTER TABLE TestTable1 ADD TestColumn1 NVARCHAR2(5) DEFAULT CURRENT_TIMESTAMP NOT NULL");
        }

        [Test]
        public void CanCreateColumnWithDefaultSystemMethodCurrentUser()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();
            expression.Column.DefaultValue = SystemMethods.CurrentUser;

            var result = Generator.Generate(expression);

            result.ShouldBe("ALTER TABLE TestTable1 ADD TestColumn1 NVARCHAR2(5) DEFAULT USER NOT NULL");
        }
    }
}