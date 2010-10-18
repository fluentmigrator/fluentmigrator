using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator.Runner.Generators;
using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Generators
{
	public class OracleGeneratorTests : GeneratorTestsBase<OracleGenerator,OracleExpectedTestResults>
	{
	}

	public class OracleExpectedTestResults : IExpectedTestResults
	{
		public string AddColumn()
		{
			return "ALTER TABLE NewTable ADD NewColumn NVARCHAR2(5) NOT NULL";
		}

		public string AddDecimalColumn()
		{
			return "ALTER TABLE NewTable ADD NewColumn NUMBER(19,2) NOT NULL";
		}

		public string CreateForeignKey()
		{
			return "ALTER TABLE TestForeignTable ADD CONSTRAINT FK_Test FOREIGN KEY (Column3,Column4) REFERENCES TestPrimaryTable (Column1,Column2)";
		}

		public string CreateUniqueClusteredIndex()
		{
			return "CREATE UNIQUE INDEX IX_TEST ON TEST_TABLE (Column1 ASC,Column2 DESC)";
		}

		public string DropForeignKey()
		{
			return "ALTER TABLE TestPrimaryTable DROP CONSTRAINT FK_Test";
		}

		public string DropTable()
		{
			return "DROP TABLE NewTable";
		}

		public string InsertData()
		{
			return @"INSERT ALL INTO TestTable (Id,Name,Website) VALUES (1,'Just''in','codethinked.com')" +
					@" INTO TestTable (Id,Name,Website) VALUES (2,'Na\te','kohari.org')" +
					@" SELECT 1 FROM DUAL";
		}

		public string InsertGuidData()
		{
			return "INSERT ALL INTO TestTable (guid) VALUES ('12345678-1234-1234-1234-123456789012') SELECT 1 FROM DUAL";
		}

		public string RenameColumn()
		{
			return "ALTER TABLE Table1 RENAME COLUMN Column1 TO Column2";
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
