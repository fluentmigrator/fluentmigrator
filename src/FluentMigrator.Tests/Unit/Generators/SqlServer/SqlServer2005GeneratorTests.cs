

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
				"CREATE TABLE [dbo].[NewTable] (ColumnName1 NVARCHAR(MAX) NOT NULL)");
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
	}
}