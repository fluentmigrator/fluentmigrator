using System;
using FluentMigrator.Runner.Generators;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.CreateTable
{
	public class OracleCreateTableGeneratorTests : GeneratorCreateTableTestsBase<OracleGenerator,OracleExpectedCreateTableResults>
	{
		[ExpectedException(typeof(NotImplementedException))]
		public override void CanCreateTableWithIdentity()
		{
			base.CanCreateTableWithIdentity();
		}
	}

	public class OracleExpectedCreateTableResults : IExpectedCreateTableTestResults
	{
		public string CreateTable()
		{
			return "CREATE TABLE NewTable (ColumnName1 NVARCHAR2(255) NOT NULL)";
		}

		public string CreateTableWithCustomColumnType()
		{
			return "CREATE TABLE NewTable (ColumnName1 timestamp NOT NULL)";
		}

		public string CreateTableWithPrimaryKey()
		{
			return "CREATE TABLE NewTable (ColumnName1 NVARCHAR2(255) NOT NULL PRIMARY KEY)";
		}

		public string CreateTableWithIdentity()
		{
			throw new NotImplementedException();
		}

		public string CreateTableWithNullField()
		{
			return "CREATE TABLE NewTable (ColumnName1 NVARCHAR2(255))";
		}

		public string CreateTableWithDefaultValue()
		{
			return "CREATE TABLE NewTable (ColumnName1 NVARCHAR2(255) DEFAULT 'Default' NOT NULL, ColumnName2 NUMBER(10,0) DEFAULT 0 NOT NULL)";
		}

		public string CreateTableWithDefaultValueExplicitlySetToNull()
		{
			return "CREATE TABLE NewTable (ColumnName1 NVARCHAR2(255) DEFAULT NULL)";
		}

		public string CreateTableWithMultipleColumns()
		{
			return "CREATE TABLE NewTable (ColumnName1 NVARCHAR2(255) NOT NULL, ColumnName2 NUMBER(10,0) NOT NULL, ColumnName3 NVARCHAR2(255) NOT NULL)";
		}

		public string CreateTableWithMultipartPrimaryKey()
		{
			// See the note in OracleColumn about why the PK should not be named
			return
				"CREATE TABLE NewTable (ColumnName1 NVARCHAR2(255) NOT NULL, ColumnName2 NUMBER(10,0) NOT NULL,  PRIMARY KEY (ColumnName1,ColumnName2))";
		}
	}
}
