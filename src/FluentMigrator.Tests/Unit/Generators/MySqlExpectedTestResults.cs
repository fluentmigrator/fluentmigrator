using System;
using FluentMigrator.Runner.Generators;
using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Generators
{
	public class MySqlGeneratorTests : GeneratorTestsBase<MySqlGenerator, MySqlExpectedTestResults>
	{
		[ExpectedException(typeof(NotImplementedException))]
		public override void CanRenameColumn()
		{
			base.CanRenameColumn();
		}
	}

	public class MySqlExpectedTestResults : IExpectedTestResults
	{
		public string AddColumn()
		{
			return "ALTER TABLE `NewTable` ADD NewColumn VARCHAR(5) NOT NULL";
		}

		public string AddDecimalColumn()
		{
			return "ALTER TABLE `NewTable` ADD NewColumn DECIMAL(19,2) NOT NULL";
		}

		public string CreateForeignKey()
		{
			return
				"ALTER TABLE `TestForeignTable` ADD CONSTRAINT FK_Test FOREIGN KEY (Column3,Column4) REFERENCES TestPrimaryTable (Column1,Column2)";
		}

		public string CreateUniqueClusteredIndex()
		{
			return "CREATE UNIQUE INDEX IX_TEST ON TEST_TABLE (Column1 ASC,Column2 DESC)";
		}

		public string DropForeignKey()
		{
			return "ALTER TABLE `TestPrimaryTable` DROP FOREIGN KEY `FK_Test`";
		}

		public string DropTable()
		{
			return "DROP TABLE `NewTable`";
		}

		public string InsertData()
		{
			return @"INSERT INTO `TestTable` (Id,Name,Website) VALUES (1,'Just''in','codethinked.com');" +
					@"INSERT INTO `TestTable` (Id,Name,Website) VALUES (2,'Na\\te','kohari.org');";
		}

		public string InsertGuidData()
		{
			return "INSERT INTO `TestTable` (guid) VALUES ('12345678-1234-1234-1234-123456789012');";
		}

		public string RenameColumn()
		{
			return "not implemented";
		}

		public string RenameTable()
		{
			return "RENAME TABLE `Table1` TO `Table2`";
		}

		public string DropColumn()
		{
			return "ALTER TABLE `NewTable` DROP COLUMN NewColumn";
		}

		public string AddIdentityColumn()
		{
			return "ALTER TABLE `NewTable` ADD NewColumn VARCHAR(255) NOT NULL AUTO_INCREMENT";
		}
	}
}
