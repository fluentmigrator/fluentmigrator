﻿using FluentMigrator.Runner.Generators.Firebird;
using FluentMigrator.Runner.Processors.Firebird;
using Xunit;

namespace FluentMigrator.Tests.Unit.Generators.Firebird
{
    public class FirebirdColumnTests : BaseColumnTests
    {
        protected FirebirdGenerator Generator;

        public FirebirdColumnTests()
        {
            Generator = new FirebirdGenerator(FirebirdOptions.StandardBehaviour());
        }

        [Fact]
        public override void CanAlterColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }

        [Fact]
        public override void CanAlterColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }

        [Fact]
        public override void CanCreateAutoIncrementColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetAlterColumnAddAutoIncrementExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }

        [Fact]
        public override void CanCreateAutoIncrementColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetAlterColumnAddAutoIncrementExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }

        [Fact]
        public override void CanCreateColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 ADD TestColumn1 VARCHAR(5) NOT NULL");
        }

        [Fact]
        public override void CanCreateColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 ADD TestColumn1 VARCHAR(5) NOT NULL");
        }

        [Fact]
        public override void CanCreateDecimalColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateDecimalColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 ADD TestColumn1 DECIMAL(19, 2) NOT NULL");
        }

        [Fact]
        public override void CanCreateDecimalColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateDecimalColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 ADD TestColumn1 DECIMAL(19, 2) NOT NULL");
        }

        [Fact]
        public override void CanDropColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 DROP TestColumn1");
        }

        [Fact]
        public override void CanDropColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 DROP TestColumn1");
        }

        [Fact]
        public override void CanDropMultipleColumnsWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteColumnExpression(new[] { "TestColumn1", "TestColumn2" });
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 DROP TestColumn1;" + System.Environment.NewLine + "ALTER TABLE TestTable1 DROP TestColumn2");
        }

        [Fact]
        public override void CanDropMultipleColumnsWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteColumnExpression(new[] { "TestColumn1", "TestColumn2" });

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 DROP TestColumn1;" + System.Environment.NewLine + "ALTER TABLE TestTable1 DROP TestColumn2");
        }

        [Fact]
        public override void CanRenameColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 ALTER COLUMN TestColumn1 TO TestColumn2");
        }

        [Fact]
        public override void CanRenameColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 ALTER COLUMN TestColumn1 TO TestColumn2");
        }

        [Fact]
        public virtual void CanCreateDefaultString()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();
            expression.Column.Size = 0;

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 ADD TestColumn1 VARCHAR(255) NOT NULL");
        }

        [Fact]
        public virtual void CanCreateSizedString()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();
            expression.Column.Size = 10;

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 ADD TestColumn1 VARCHAR(10) NOT NULL");
        }

        [Fact]
        public virtual void CanCreateText()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();
            expression.Column.Size = 1048576;

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestTable1 ADD TestColumn1 BLOB SUB_TYPE TEXT NOT NULL");
        }

    }
}
