using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer
{
    public class SqlServer2000CreateTableTests : BaseTableCreateTests
    {
        protected SqlServer2000Generator generator;

        [SetUp]
        public void Setup()
        {
            generator = new SqlServer2000Generator();


        }

        [Test]
        public override void CanCreateTable()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE [NewTable] (ColumnName1 NVARCHAR(255) NOT NULL, ColumnName2 INT NOT NULL)");
        }

        [Test]
        public override void CanCreateTableWithCustomColumnType()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].IsPrimaryKey = true;
            expression.Columns[1].Type = null;
            expression.Columns[1].CustomType = "[timestamp]";

            var sql = generator.Generate(expression);
            sql.ShouldBe(
                "CREATE TABLE [NewTable] (ColumnName1 NVARCHAR(255) NOT NULL PRIMARY KEY CLUSTERED, ColumnName2 [timestamp] NOT NULL)");
        }

        [Test]
        public override void CanCreateTableWithPrimaryKey()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithPrimaryKeyExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe(
                "CREATE TABLE [NewTable] (ColumnName1 NVARCHAR(255) NOT NULL PRIMARY KEY CLUSTERED, ColumnName2 INT NOT NULL)");
        }

        [Test]
        public override void CanCreateTableWithIdentity()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithGetAutoIncrementExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe(
                "CREATE TABLE [NewTable] (ColumnName1 NVARCHAR(255) NOT NULL IDENTITY(1,1), ColumnName2 INT NOT NULL)");
        }

        [Test]
        public override void CanCreateTableWithNullField()
        {
            //var expression = G
            //var sql = generator.Generate(expression);
            //sql.ShouldBe(
            //    "CREATE TABLE [NewTable] (ColumnName1 NVARCHAR(255), ColumnName2 INT NOT NULL)");
        }

        [Test]
        public override void CanCreateTableWithDefaultValue()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();
            var sql = generator.Generate(expression);
            sql.ShouldBe(
                "CREATE TABLE [NewTable] (ColumnName1 NVARCHAR(255) NOT NULL CONSTRAINT DF_NewTable_ColumnName1 DEFAULT 'Default', ColumnName2 INT NOT NULL CONSTRAINT DF_NewTable_ColumnName2 DEFAULT 0)");
  
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
            var sql = generator.Generate(expression);
            sql.ShouldBe("CREATE UNIQUE CLUSTERED INDEX [IX_TEST] ON [TEST_TABLE] ([Column1] ASC,[Column2] DESC)");
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
