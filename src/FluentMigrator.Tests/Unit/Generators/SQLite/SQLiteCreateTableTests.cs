using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using FluentMigrator.Runner.Generators.SQLite;
using NUnit.Should;

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
            sql.ShouldBe("CREATE TABLE [TestTable1] ([TestColumn1] TEXT NOT NULL)");
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
            var sql = generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE [TestTable1] ([TestColumn1] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT)");
        }

        [Test]
        public override void CanCreateTableWithIdentity()
        {


            var expression = GeneratorTestHelper.GetCreateTableWithGetAutoIncrementExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE [TestTable1] ([TestColumn1] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT)");

        }

        [Test]
        public override void CanCreateTableWithNullField()
        {
            throw new NotImplementedException();
        }

        [Test]
        public override void CanCreateTableWithDefaultValue()
        {
            throw new NotImplementedException();
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
            sql.ShouldBe("CREATE INDEX IF NOT EXISTS [TestIndex] ON [TestTable1] ([TestColumn1])");

        }

        [Test]
        public override void CanCreateMultiColumnIndex()
        {
            throw new NotImplementedException();
        }

        [Test]
        public override void CanCreateTableWithMultipartKey()
        {
            throw new NotImplementedException();
        }
    }
}
