using System;
using System.Data;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators
{
	public class SqlServer2000GeneratorTests : GeneratorTestsBase<SqlServer2000Generator, SqlServer2000ExpectedTestResults>
	{
		[Ignore("need better way to test this")]
		public override void CanDropColumn()
		{
			base.CanDropColumn();
		}

		[Test]
		public void CanCreateXmlColumn()
		{
			var expression = new CreateColumnExpression();
			expression.TableName = "Table1";

			expression.Column = new ColumnDefinition();
			expression.Column.Name = "MyXmlColumn";
			expression.Column.Type = DbType.Xml;

			var sql = generator.Generate(expression);
			sql.ShouldNotBeNull();
		}
	}

	public class SqlServer2000ExpectedTestResults : IExpectedTestResults
	{
		public string AddColumn()
		{
			return "ALTER TABLE [NewTable] ADD NewColumn NVARCHAR(5) NOT NULL";
		}

		public string AddDecimalColumn()
		{
			return "ALTER TABLE [NewTable] ADD NewColumn DECIMAL(19,2) NOT NULL";
		}

		public string CreateForeignKey()
		{
			return
				"ALTER TABLE [TestForeignTable] ADD CONSTRAINT FK_Test FOREIGN KEY ([Column3],[Column4]) REFERENCES [TestPrimaryTable] ([Column1],[Column2])";
		}

		public string CreateUniqueClusteredIndex()
		{
			return "CREATE UNIQUE CLUSTERED INDEX IX_TEST ON [TEST_TABLE] (Column1 ASC,Column2 DESC)";
		}

		public string DropForeignKey()
		{
			return "ALTER TABLE [TestPrimaryTable] DROP CONSTRAINT FK_Test";
		}

		public string DropTable()
		{
			return "DROP TABLE [NewTable]";
		}

		public string InsertData()
		{
			return "INSERT INTO [TestTable] ([Id],[Name],[Website]) VALUES (1,'Just''in','codethinked.com');" + 
					@"INSERT INTO [TestTable] ([Id],[Name],[Website]) VALUES (2,'Na\te','kohari.org');";
		}

		public string InsertGuidData()
		{
			return "INSERT INTO [TestTable] ([guid]) VALUES ('12345678-1234-1234-1234-123456789012');";
		}

		public string RenameColumn()
		{
			return "sp_rename '[Table1].[Column1]', [Column2]";
		}

		public string RenameTable()
		{
			return "sp_rename [Table1], [Table2]";
		}

		public string DropColumn()
		{
			throw new NotImplementedException();
		}
	}
}