

namespace FluentMigrator.Tests.Unit.Generators
{
    using System;
    using System.Data;
    using FluentMigrator.Expressions;
    using FluentMigrator.Model;
    using NUnit.Framework;
    using FluentMigrator.Runner.Generators.SqlServer;
    using NUnit.Should;


	public class SqlServer2005GeneratorTests
	{
		private SqlServer2005Generator generator;

		[SetUp]
		public void SetUp()
		{
			generator = new SqlServer2005Generator();
		}



		[Test]
		public void CanCreateTableWithNvarcharMax()
		{
            var expression = GeneratorTestHelper.GetCreateTableExpression();
			expression.Columns[0].Type = DbType.String;
			expression.Columns[0].Size = Int32.MaxValue;
			var sql = generator.Generate(expression);
			sql.ShouldBe(
                "CREATE TABLE [TestTable1] ([TestColumn1] NVARCHAR(MAX) NOT NULL, [TestColumn2] INT NOT NULL)");
		}

        [Test]
        public void CanDeleteIndex()
        {
            var expression = GeneratorTestHelper.GetDeleteIndexExpression();
 
            var sql = generator.Generate(expression);
            sql.ShouldBe("DROP INDEX [TestIndex] ON [TestTable1]");
        }

        [Test]
        public void CanAlterSchema()
        {
          var expression = new AlterSchemaExpression
          {
            DestinationSchemaName = "DEST",
            SourceSchemaName = "SOURCE",
            TableName = "TABLE"
          };

          var sql = generator.Generate( expression );
          sql.ShouldBe(
            "ALTER SCHEMA [DEST] TRANSFER [SOURCE].[TABLE]" );
        }

        [Test]
        public void CanRenameTable()
        {

            var expression = GeneratorTestHelper.GetRenameTableExpression();

            var sql = generator.Generate(expression);
            sql.ShouldBe("sp_rename '[dbo].[TestTable1]', '[TestTable2]'");
        }

        [Test]
        public void CanCreateTableWithDateTimeOffsetColumn()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].Type = DbType.DateTimeOffset;
            var sql = generator.Generate(expression);
            sql.ShouldBe(
                "CREATE TABLE [dbo].[TestTable1] ([TestColumn1] DATETIMEOFFSET NOT NULL)");
        }

        [Test]
        public void CanRenameColumn()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe("sp_rename '[Schema1].[TestTable1].[TestColumn1]', 'TestColumn2'");
        }
	}
}