

namespace FluentMigrator.Tests.Unit.Generators.Jet
{
    using System;
    using NUnit.Should;
    using NUnit.Framework;
    using FluentMigrator.Runner.Generators.Jet;

    public class JetCreateTableTests : BaseTableCreateTests
    {
        protected JetGenerator SUT;

        [SetUp]
        public void SetUp()
        {
            SUT = new JetGenerator();
        }

        [Test]
        public override void CanCreateTable()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();

            var result = SUT.Generate(expression);

            result.ShouldBe("CREATE TABLE [TestTable1] ([TestColumn1] VARCHAR(255) NOT NULL, [ColumnName2] INTEGER NOT NULL)");
        }

        [Test]
        public override void CanCreateTableWithCustomColumnType()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].IsPrimaryKey = true;
            expression.Columns[1].Type = null;
            expression.Columns[1].CustomType = "[timestamp]";

            var restul = SUT.Generate(expression);

            restul.ShouldBe("CREATE TABLE [TestTable1] ([TestColumn1] VARCHAR(255) NOT NULL PRIMARY KEY, [ColumnName2] INTEGER NOT NULL)");
        }

        [Test]
        public override void CanCreateTableWithPrimaryKey()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithPrimaryKeyExpression();

            var result = SUT.Generate(expression);

            result.ShouldBe(
                "CREATE TABLE [TestTable1] ([TestColumn1] VARCHAR(255) NOT NULL PRIMARY KEY, [TestColumn2] INTEGER NOT NULL)");
     
        }

        [Test]
        public override void CanCreateTableWithIdentity()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithGetAutoIncrementExpression();
            var result = SUT.Generate(expression);

            result.ShouldBe(
                "CREATE TABLE [TestTable1] ([TestColumn1] COUNTER NOT NULL, [TestColumn2] INTEGER NOT NULL)");
        }

        [Test]
        public override void CanCreateTableWithNullField()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].IsNullable = true;

            var result = SUT.Generate(expression);

            result.ShouldBe(
                "CREATE TABLE [TestTable1] ([TestColumn1] VARCHAR(255), [TestColumn2] INTEGER NOT NULL)");
        }

        [Test]
        public override void CanCreateTableWithDefaultValue()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].DefaultValue = "Default";
            expression.Columns[1].DefaultValue = 0;

            var result = SUT.Generate(expression);

            result.ShouldBe(
                "CREATE TABLE [TestTable1] ([TestColumn1] VARCHAR(255) NOT NULL DEFAULT 'Default', [TestColumn2] INTEGER NOT NULL DEFAULT 0)");
     
        }

        [Test]
        public override void CanCreateTableWithDefaultValueExplicitlySetToNull()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].DefaultValue = null;

            var result = SUT.Generate(expression);

            result.ShouldBe(
                "CREATE TABLE [TestTable1] ([TestColumn1] VARCHAR(255) NOT NULL DEFAULT NULL, [TestColumnName2] INTEGER NOT NULL)");
        }

        [Test]
        public override void CanCreateIndex()
        {
            var expression = GeneratorTestHelper.GetCreateIndexExpression();

            var sql = SUT.Generate(expression);

            sql.ShouldBe("CREATE UNIQUE INDEX IX_TEST ON [TestTable1] ([TestColumn1] ASC)");
        }

        [Test]
        public override void CanCreateMultiColumnIndex()
        {
            var expression = GeneratorTestHelper.GetMultiColumnCreateIndexExpression();

            var sql = SUT.Generate(expression);

            sql.ShouldBe("CREATE UNIQUE INDEX IX_TEST ON [TestTable1] ([TestColumn1] ASC,[TestColumn2] DESC)");
        }


        [Test]
        public override void CanCreateTableWithMultipartKey()
        {
            throw new NotImplementedException();
        }
    }
}
