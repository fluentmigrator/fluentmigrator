using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using FluentMigrator.Runner.Generators.SQLite;
using NUnit.Should;
using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators;

namespace FluentMigrator.Tests.Unit.Generators.SQLite
{
    public class SQLiteCreateTableTests : BaseTableCreateTests
    {
        protected SqliteGenerator generator;

        [SetUp]
        public void Setup()
        {
            generator = new SqliteGenerator();
        }


        [Test]
        public override void CanCreateTable()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            string sql = generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE 'TestTable1' ('TestColumn1' TEXT NOT NULL, 'TestColumn2' INTEGER NOT NULL)");
        }

        [Test]
        public override void CanCreateTableWithCustomColumnType()
        {
            //Not sure what custom column types there are in sqlite so not testing
        }

        [Test]
        public override void CanCreateTableWithPrimaryKey()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithPrimaryKeyExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE 'TestTable1' ('TestColumn1' TEXT NOT NULL, 'TestColumn2' INTEGER NOT NULL, PRIMARY KEY ('TestColumn1'))");
        }

        [Test]
        public override void CanCreateTableNamedPrimaryKey()
        {
            //Should work. I think from the docs
            var expression = GeneratorTestHelper.GetCreateTableWithNamedPrimaryKeyExpression();
            string sql = generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE 'TestTable1' ('TestColumn1' TEXT NOT NULL, 'TestColumn2' INTEGER NOT NULL, CONSTRAINT 'TestKey' PRIMARY KEY ('TestColumn1'))");

        }

        [Test]
        public override void CanCreateTableNamedMultiColumnPrimaryKey()
        {
            //Should work. I think from the docs
            var expression = GeneratorTestHelper.GetCreateTableWithMultiColumNamedPrimaryKeyExpression();
            string sql = generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE 'TestTable1' ('TestColumn1' TEXT NOT NULL, 'TestColumn2' INTEGER NOT NULL, CONSTRAINT 'TestKey' PRIMARY KEY ('TestColumn1', 'TestColumn2'))");

        }

        [Test]
        public override void CanCreateTableWithIdentity()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression();
            //Have to force it to primary key for SQLite
            expression.Columns[0].IsPrimaryKey = true;

            var sql = generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE 'TestTable1' ('TestColumn1' INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 'TestColumn2' INTEGER NOT NULL)");

        }

        [Test]
        public override void CanCreateTableWithNullableField()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNullableColumn();
           

            var result = generator.Generate(expression);

            result.ShouldBe(
                "CREATE TABLE 'TestTable1' ('TestColumn1' TEXT, 'TestColumn2' INTEGER NOT NULL)");
        }

        [Test]
        public override void CanCreateTableWithDefaultValue()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();

            var result = generator.Generate(expression);

            result.ShouldBe(
                "CREATE TABLE 'TestTable1' ('TestColumn1' TEXT NOT NULL DEFAULT 'Default', 'TestColumn2' INTEGER NOT NULL DEFAULT 0)");
     
        }

        [Test]
        public override void CanCreateTableWithDefaultValueExplicitlySetToNull()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();
            expression.Columns[0].DefaultValue = null;

            var result = generator.Generate(expression);

            result.ShouldBe(
                "CREATE TABLE 'TestTable1' ('TestColumn1' TEXT NOT NULL DEFAULT NULL, 'TestColumn2' INTEGER NOT NULL DEFAULT 0)");
        }

        [Test]
        public override void CanCreateIndex()
        {
            var expression = GeneratorTestHelper.GetCreateIndexExpression();
            string sql = generator.Generate(expression);
            sql.ShouldBe("CREATE INDEX 'TestIndex' ON 'TestTable1' ('TestColumn1' ASC)");

        }

        [Test]
        public override void CanCreateMultiColumnIndex()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnCreateIndexExpression();
            string sql = generator.Generate(expression);
            sql.ShouldBe("CREATE INDEX 'TestIndex' ON 'TestTable1' ('TestColumn1' ASC, 'TestColumn2' DESC)");
        }

        [Test]
        public override void CanCreateTableWithMultiColumnPrimaryKey()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithMultiColumnPrimaryKeyExpression();
            var result = generator.Generate(expression);
            result.ShouldBe("CREATE TABLE 'TestTable1' ('TestColumn1' TEXT NOT NULL, 'TestColumn2' INTEGER NOT NULL, PRIMARY KEY ('TestColumn1', 'TestColumn2'))");
   
        }

        [Test]
        public override void CanCreateUniqueIndex()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueIndexExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe("CREATE UNIQUE INDEX 'TestIndex' ON 'TestTable1' ('TestColumn1' ASC)");

        }

        [Test]
        public override void CanCreateMultiColumnUniqueIndex()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueMultiColumnIndexExpression();

            var sql = generator.Generate(expression);
            sql.ShouldBe("CREATE UNIQUE INDEX 'TestIndex' ON 'TestTable1' ('TestColumn1' ASC, 'TestColumn2' DESC)");

        }

        [Test]
        public override void CanCreateSchema()
        {
            var expression = new CreateSchemaExpression() { SchemaName = "TestSchema" };
            var result = generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }

        [Test]
        public void CanCreateSchemaInStrictMode()
        {
            generator.compatabilityMode = Runner.CompatabilityMode.STRICT;
            Assert.Throws<DatabaseOperationNotSupportedExecption>(() => generator.Generate(new CreateSchemaExpression()));
        }

        [Test]
        public override void CanCreateTableWithIFNotExists()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.IfNotExists = true;
            var result = generator.Generate(expression);
            result.ShouldBe("CREATE TABLE IF NOT EXISTS 'TestTable1' ('TestColumn1' TEXT NOT NULL, 'TestColumn2' INTEGER NOT NULL)");
        }
    }
}
