using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator.Runner.Generators;
using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Generators
{
	public class SqliteGeneratorTests : GeneratorTestsBase<SqliteGenerator,SqliteExpectedTestResults>
	{
		[ExpectedException(typeof(NotImplementedException))]
		public override void CanRenameColumn()
		{
			throw new NotImplementedException();
		}
	}

	public class SqliteExpectedTestResults : IExpectedTestResults
	{
		public string AddColumn()
		{
			return "ALTER TABLE [NewTable] ADD COLUMN NewColumn TEXT NOT NULL";
		}

		public string AddDecimalColumn()
		{
			return "ALTER TABLE [NewTable] ADD COLUMN NewColumn NUMERIC NOT NULL";
		}

		public string CreateForeignKey()
		{
			// The current implementation chooses to do nothing with FK's instead of throwing
			return string.Empty;
		}

		public string CreateUniqueClusteredIndex()
		{
			return "CREATE UNIQUE INDEX IF NOT EXISTS IX_TEST ON TEST_TABLE (Column1,Column2)";
		}

		public string DropForeignKey()
		{
			// The current implementation chooses to do nothing with FK's instead of throwing
			return string.Empty;
		}

		public string DropTable()
		{
			return "DROP TABLE NewTable";
		}

		public string InsertData()
		{
			return @"INSERT INTO [TestTable] (Id,Name,Website) VALUES (1,'Just''in','codethinked.com');" +
				@"INSERT INTO [TestTable] (Id,Name,Website) VALUES (2,'Na\te','kohari.org');";
		}

		public string InsertGuidData()
		{
			return @"INSERT INTO [TestTable] (guid) VALUES ('12345678-1234-1234-1234-123456789012');";
		}

		public string RenameColumn()
		{
			return string.Empty;
		}

		public string RenameTable()
		{
			return "ALTER TABLE Table1 RENAME TO Table2";
		}

		public string DropColumn()
		{
			return "ALTER TABLE NewTable DROP COLUMN NewColumn";
		}
	}
}
