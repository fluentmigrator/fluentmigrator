using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Should;
using FluentMigrator.Runner.Generators.SQLite;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Expressions;

namespace FluentMigrator.Tests.Unit.Generators.SQLite
{
    public class SQLiteAlterTableTests : BaseTableAlterTests
    {
        protected SqliteGenerator generator;

        [SetUp]
        public void Setup()
        {
            generator = new SqliteGenerator();
        }


        [Test]
        public override void CanAddColumn()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();
            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE 'TestTable1' ADD COLUMN 'TestColumn1' TEXT NOT NULL");
	
        }

        [Test]
        public override void CanAddDecimalColumn()
        {
            var expression = GeneratorTestHelper.GetCreateDecimalColumnExpression();
            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE 'TestTable1' ADD COLUMN 'TestColumn1' NUMERIC NOT NULL");
        }

        [Test]
        public override void CanRenameColumn()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();
            var result = generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }

        [Test]
        public void CanRenameColumnInStrictMode()
        {
            generator.compatabilityMode = Runner.CompatabilityMode.STRICT;
            Assert.Throws<DatabaseOperationNotSupportedExecption>(() => generator.Generate(GeneratorTestHelper.GetRenameColumnExpression()));
        }

        [Test]
        public override void CanRenameTable()
        {
            var expression = GeneratorTestHelper.GetRenameTableExpression();
            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE 'TestTable1' RENAME TO 'TestTable2'");
	
        }

        [Test]
        public override void CanAlterColumn()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();
            var result = generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }

        [Test]
        public void CanAlterColumnInStrictMode()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();
            generator.compatabilityMode = Runner.CompatabilityMode.STRICT;
            Assert.Throws<DatabaseOperationNotSupportedExecption>(() => generator.Generate(expression));
        }

        [Test]
        public override void CanCreateForeignKey()
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
            var result = generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }

        [Test]
        public void CanCreateForeignKeyInStrictMode()
        {
            generator.compatabilityMode = Runner.CompatabilityMode.STRICT;
            Assert.Throws<DatabaseOperationNotSupportedExecption>(() => generator.Generate(GeneratorTestHelper.GetCreateForeignKeyExpression()));
        }

        [Test]
        public override void CanCreateMulitColumnForeignKey()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnForeignKeyExpression();
            var result = generator.Generate(expression);
            result.ShouldBe(string.Empty);
            
        }

        [Test]
        public void CanCreateMulitColumnForeignKeyInStrictMode()
        {

            generator.compatabilityMode = Runner.CompatabilityMode.STRICT;
            Assert.Throws<DatabaseOperationNotSupportedExecption>(() => generator.Generate(GeneratorTestHelper.GetCreateMultiColumnForeignKeyExpression()));
        }

        [Test]
        public override void CanCreateAutoIncrementColumn()
        {
            var expression = GeneratorTestHelper.GetAlterTableAutoIncrementColumnExpression();
            expression.Column.IsPrimaryKey = true;

            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE 'TestTable1' ADD COLUMN 'TestColumn1' INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT");
    
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
            Assert.Throws<DatabaseOperationNotSupportedExecption>(() => generator.Generate(new CreateSchemaExpression()));
        }
    }
}
