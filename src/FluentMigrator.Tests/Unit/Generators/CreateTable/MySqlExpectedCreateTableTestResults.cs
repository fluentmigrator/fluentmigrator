using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator.Runner.Generators;

namespace FluentMigrator.Tests.Unit.Generators.CreateTable
{
	public class MySqlCreateTableTests : GeneratorCreateTableTestsBase<MySqlGenerator,MySqlExpectedCreateTableTestResults>
	{
	}

	public class MySqlExpectedCreateTableTestResults : IExpectedCreateTableTestResults
	{
		public string CreateTable()
		{
			return "CREATE TABLE `NewTable` (ColumnName1 VARCHAR(255) NOT NULL) ENGINE = INNODB";
		}

		public string CreateTableWithCustomColumnType()
		{
			return
				"CREATE TABLE `NewTable` (ColumnName1 timestamp NOT NULL) ENGINE = INNODB";
		}

		public string CreateTableWithPrimaryKey()
		{
			return
				"CREATE TABLE `NewTable` (ColumnName1 VARCHAR(255) NOT NULL , PRIMARY KEY (`ColumnName1`)) ENGINE = INNODB";
		}

		public string CreateTableWithIdentity()
		{
			return
				"CREATE TABLE `NewTable` (ColumnName1 VARCHAR(255) NOT NULL AUTO_INCREMENT) ENGINE = INNODB";
		}

		public string CreateTableWithNullField()
		{
			return "CREATE TABLE `NewTable` (ColumnName1 VARCHAR(255)) ENGINE = INNODB";
		}

		public string CreateTableWithDefaultValue()
		{
			return
				"CREATE TABLE `NewTable` (ColumnName1 VARCHAR(255) NOT NULL DEFAULT 'Default', ColumnName2 INTEGER NOT NULL DEFAULT 0) ENGINE = INNODB";
		}

		public string CreateTableWithDefaultValueExplicitlySetToNull()
		{
			return "CREATE TABLE `NewTable` (ColumnName1 VARCHAR(255) DEFAULT NULL) ENGINE = INNODB";
		}

		public string CreateTableWithMultipleColumns()
		{
			return "CREATE TABLE `NewTable` (ColumnName1 VARCHAR(255) NOT NULL, ColumnName2 INTEGER NOT NULL, ColumnName3 VARCHAR(255) NOT NULL) ENGINE = INNODB";
		}

		public string CreateTableWithMultipartPrimaryKey()
		{
			return "CREATE TABLE `NewTable` (ColumnName1 VARCHAR(255) NOT NULL, ColumnName2 INTEGER NOT NULL, CONSTRAINT ColumnName1_ColumnName2_PK PRIMARY KEY (ColumnName1,ColumnName2)) ENGINE = INNODB";
		}
	}
}
