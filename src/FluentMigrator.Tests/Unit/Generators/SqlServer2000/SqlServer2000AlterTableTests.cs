using System;
using System.Data;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer
{
    public class SqlServer2000AlterTableTests : BaseTableAlterTests
    {
        private SqlServer2000Generator _generator;

        [SetUp]
        public void Setup()
        {
            _generator = new SqlServer2000Generator();
        }

        [Test]
        public override void CanAddColumn()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE [TestTable1] ADD [TestColumn1] NVARCHAR(5) NOT NULL");
        }

        [Test]
        public void CanAddColumnWithGetDateDefault()
        {
            ColumnDefinition column = new ColumnDefinition
            {
                Name = "TestColumn1",
                Type = DbType.String,
                Size = 5,
                DefaultValue = "GetDate()"
            };
            var expression = new CreateColumnExpression { TableName = "TestTable1", Column = column };
            var sql = _generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE [TestTable1] ADD [TestColumn1] NVARCHAR(5) NOT NULL DEFAULT GetDate()");
        }

        [Test]
        public override void CanAddDecimalColumn()
        {
            var expression = GeneratorTestHelper.GetCreateDecimalColumnExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE [TestTable1] ADD [TestColumn1] DECIMAL(19,2) NOT NULL");
        }

        [Test]
        public override void CanRenameTable()
        {
            var expression = GeneratorTestHelper.GetRenameTableExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("sp_rename '[TestTable1]', 'TestTable2'");
        }

        [Test]
        public override void CanRenameColumn()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("sp_rename '[TestTable1].[TestColumn1]', 'TestColumn2'");
        }

        [Test]
        public override void CanAlterColumn()
        {
            //TODO: This will fail if there are any keys attached 
            var expression = GeneratorTestHelper.GetAlterColumnExpression();

            var sql = _generator.Generate(expression);

            sql.ShouldBe("ALTER TABLE [TestTable1] ALTER COLUMN [TestColumn1] NVARCHAR(20) NOT NULL");
        }

        [Test]
        public void CanAlterColumnWithDefaultValue() {
            //TODO: This will fail if there are any keys attached 
            var expression = GeneratorTestHelper.GetAlterColumnExpression();
            expression.Column.DefaultValue = "Foo";

            var sql = _generator.Generate(expression);

            sql.ShouldBe("ALTER TABLE [TestTable1] ALTER COLUMN [TestColumn1] NVARCHAR(20) NOT NULL");
        }

        [Test]
        public void CanAlterDefaultConstraint()
        {
            var expression = GeneratorTestHelper.GetAlterDefaultConstraintExpression();
            var sql = _generator.Generate(expression);

            const string expected = "DECLARE @default sysname, @sql nvarchar(4000);\r\n\r\n" +
            "-- get name of default constraint\r\n" +
            "SELECT @default = name\r\n" +
            "FROM sys.default_constraints\r\n" +
            "WHERE parent_object_id = object_id('[TestTable1]')\r\n" + "" +
            "AND type = 'D'\r\n" + "" +
            "AND parent_column_id = (\r\n" + "" +
            "SELECT column_id\r\n" +
            "FROM sys.columns\r\n" +
            "WHERE object_id = object_id('[TestTable1]')\r\n" +
            "AND name = 'TestColumn1'\r\n" +
            ");\r\n\r\n" +
            "-- create alter table command to drop contraint as string and run it\r\n" +
            "SET @sql = N'ALTER TABLE [TestTable1] DROP CONSTRAINT ' + @default;\r\n" +
            "EXEC sp_executesql @sql;\r\n\r\n" +
            "-- create alter table command to create new default constraint as string and run it\r\n" +
            "ALTER TABLE [TestTable1] WITH NOCHECK ADD CONSTRAINT DF_TestTable1_TestColumn1 DEFAULT(1) FOR [TestColumn1];";

            sql.ShouldBe(expected);
        }

        [Test]
        public override void CanCreateForeignKey()
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe(
                "ALTER TABLE [TestTable1] ADD CONSTRAINT [FK_Test] FOREIGN KEY ([TestColumn1]) REFERENCES [TestTable2] ([TestColumn2])");
        }

        [Test]
        public override void CanCreateMulitColumnForeignKey()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnForeignKeyExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe(
                "ALTER TABLE [TestTable1] ADD CONSTRAINT [FK_Test] FOREIGN KEY ([TestColumn1], [TestColumn3]) REFERENCES [TestTable2] ([TestColumn2], [TestColumn4])");
        }

        [Test]
        public void CanCreateXmlColumn()
        {
            var expression = new CreateColumnExpression();
            expression.TableName = "Table1";

            expression.Column = new ColumnDefinition();
            expression.Column.Name = "MyXmlColumn";
            expression.Column.Type = DbType.Xml;

            var sql = _generator.Generate(expression);
            sql.ShouldNotBeNull();
        }

        public override void CanCreateAutoIncrementColumn()
        {
            throw new NotImplementedException();
        }

        [Test]
        public override void CanAlterSchema()
        {
            var expression = new AlterSchemaExpression();
            var result = _generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }

        [Test]
        public void CanAlterSchemaInStrictMode()
        {
            _generator.StrictCompatibility = true;
            Assert.Throws<DatabaseOperationNotSupportedException>(() => _generator.Generate(new CreateSchemaExpression()));
        }

        [Test]
        public void CanDropPrimaryKeyConstraint()
        {
            var expression = GeneratorTestHelper.GetDeletePrimaryKeyExpression();
            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] DROP CONSTRAINT [TESTPRIMARYKEY]");
        }

        [Test]
        public void CanDropUniqueConstraint()
        {
            var expression = GeneratorTestHelper.GetDeleteUniqueConstraintExpression();
            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] DROP CONSTRAINT [TESTUNIQUECONSTRAINT]");
        }

        [Test]
        public void CanCreatePrimaryKeyConstraint()
        {
            var expression = GeneratorTestHelper.GetCreatePrimaryKeyExpression();
            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ADD CONSTRAINT [PK_TestTable1_TestColumn1] PRIMARY KEY ([TestColumn1])");
        }

        [Test]
        public void CanCreateNamedPrimaryKeyConstraint()
        {
            var expression = GeneratorTestHelper.GetCreateNamedPrimaryKeyExpression();
            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ADD CONSTRAINT [TESTPRIMARYKEY] PRIMARY KEY ([TestColumn1])");
        }

        [Test]
        public void CanCreateMultiColumnPrimaryKeyConstraint()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnPrimaryKeyExpression();
            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ADD CONSTRAINT [PK_TestTable1_TestColumn1_TestColumn2] PRIMARY KEY ([TestColumn1], [TestColumn2])");
        }

        [Test]
        public void CanCreateMultiColumnNamedPrimaryKeyConstraint()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnNamedPrimaryKeyExpression();
            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ADD CONSTRAINT [TESTPRIMARYKEY] PRIMARY KEY ([TestColumn1], [TestColumn2])");
        }

        [Test]
        public void CanCreateUniqueConstraint()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueConstraintExpression();
            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ADD CONSTRAINT [UC_TestTable1_TestColumn1] UNIQUE ([TestColumn1])");
        }

        [Test]
        public void CanCreateNamedUniqueConstraint()
        {
            var expression = GeneratorTestHelper.GetCreateNamedUniqueConstraintExpression();
            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ADD CONSTRAINT [TESTUNIQUECONSTRAINT] UNIQUE ([TestColumn1])");
        }

        [Test]
        public void CanCreateMultiColumnUniqueConstraint()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnUniqueConstraintExpression();
            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ADD CONSTRAINT [UC_TestTable1_TestColumn1_TestColumn2] UNIQUE ([TestColumn1], [TestColumn2])");
        }

        [Test]
        public void CanCreateMultiColumnNamedUniqueConstraint()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnNamedUniqueConstraintExpression();
            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ADD CONSTRAINT [TESTUNIQUECONSTRAINT] UNIQUE ([TestColumn1], [TestColumn2])");
        }
    }
}