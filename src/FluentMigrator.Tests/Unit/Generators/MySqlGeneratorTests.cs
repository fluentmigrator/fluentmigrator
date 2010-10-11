#region License

// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

#endregion

using System;
using System.Collections.Generic;
using System.Data;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators
{
	public class MySqlGeneratorTests
	{
		protected MySqlGenerator generator;

		[SetUp]
		public void SetUp()
		{
			generator = new MySqlGenerator();
		}
	}

	[TestFixture]
	public class MySqlGeneratorOtherTests : MySqlGeneratorTests
	{
		private DeleteTableExpression GetDeleteTableExpression(string tableName)
		{
			return new DeleteTableExpression { TableName = tableName };
		}

		[Test]
		public void CanAddColumn()
		{
			var tableName = "NewTable";

			var columnDefinition = new ColumnDefinition();
			columnDefinition.Name = "NewColumn";
			columnDefinition.Size = 5;
			columnDefinition.Type = DbType.String;

			var expression = new CreateColumnExpression();
			expression.Column = columnDefinition;
			expression.TableName = tableName;

			var sql = generator.Generate(expression);
			sql.ShouldBe("ALTER TABLE `NewTable` ADD NewColumn VARCHAR(5) NOT NULL");
		}

		[Test]
		public void CanAddDecimalColumn()
		{
			var tableName = "NewTable";

			var columnDefinition = new ColumnDefinition();
			columnDefinition.Name = "NewColumn";
			columnDefinition.Size = 19;
			columnDefinition.Precision = 2;
			columnDefinition.Type = DbType.Decimal;

			var expression = new CreateColumnExpression();
			expression.Column = columnDefinition;
			expression.TableName = tableName;

			var sql = generator.Generate(expression);
			sql.ShouldBe("ALTER TABLE `NewTable` ADD NewColumn DECIMAL(19,2) NOT NULL");
		}

		[Test]
		public void CanCreateForeignKey()
		{
			var expression = new CreateForeignKeyExpression();
			expression.ForeignKey.Name = "FK_Test";
			expression.ForeignKey.PrimaryTable = "TestPrimaryTable";
			expression.ForeignKey.ForeignTable = "TestForeignTable";
			expression.ForeignKey.PrimaryColumns = new[] { "Column1", "Column2" };
			expression.ForeignKey.ForeignColumns = new[] { "Column3", "Column4" };

			var sql = generator.Generate(expression);
			sql.ShouldBe(
				"ALTER TABLE `TestForeignTable` ADD CONSTRAINT FK_Test FOREIGN KEY (Column3,Column4) REFERENCES TestPrimaryTable (Column1,Column2)");
		}

		[Test]
		public void CanCreateIndex()
		{
			var expression = new CreateIndexExpression();
			expression.Index.Name = "IX_TEST";
			expression.Index.TableName = "TEST_TABLE";
			expression.Index.IsUnique = true;
			expression.Index.Columns.Add(new IndexColumnDefinition { Direction = Direction.Ascending, Name = "Column1" });
			expression.Index.Columns.Add(new IndexColumnDefinition { Direction = Direction.Descending, Name = "Column2" });

			var sql = generator.Generate(expression);
			sql.ShouldBe("CREATE UNIQUE INDEX IX_TEST ON TEST_TABLE (Column1 ASC,Column2 DESC)");
		}

		[Test]
		public void CanDropColumn()
		{
			var tableName = "NewTable";
			var columnName = "NewColumn";

			var expression = new DeleteColumnExpression();
			expression.TableName = tableName;
			expression.ColumnName = columnName;

			var sql = generator.Generate(expression);
			sql.ShouldBe("ALTER TABLE `NewTable` DROP COLUMN NewColumn");
		}

		[Test]
		public void CanDropForeignKey()
		{
			var expression = new DeleteForeignKeyExpression();
			expression.ForeignKey.Name = "FK_Test";
			expression.ForeignKey.ForeignTable = "TestPrimaryTable";

			var sql = generator.Generate(expression);
			sql.ShouldBe("ALTER TABLE `TestPrimaryTable` DROP FOREIGN KEY `FK_Test`");
		}

		[Test]
		public void CanDropTable()
		{
			var tableName = "NewTable";
			var expression = GetDeleteTableExpression(tableName);
			var sql = generator.Generate(expression);
			sql.ShouldBe("DROP TABLE `NewTable`");
		}

		[Test]
		public void CanInsertData()
		{
			var expression = new InsertDataExpression();
			expression.TableName = "TestTable";
			expression.Rows.Add(new InsertionDataDefinition
			                    	{
			                    		new KeyValuePair<string, object>("Id", 1),
			                    		new KeyValuePair<string, object>("Name", @"Just'in"),
			                    		new KeyValuePair<string, object>("Website", "codethinked.com")
			                    	});
			expression.Rows.Add(new InsertionDataDefinition
			                    	{
			                    		new KeyValuePair<string, object>("Id", 2),
			                    		new KeyValuePair<string, object>("Name", @"Na\te"),
			                    		new KeyValuePair<string, object>("Website", "kohari.org")
			                    	});

			var sql = generator.Generate(expression);

			var expected = @"INSERT INTO `TestTable` (Id,Name,Website) VALUES (1,'Just''in','codethinked.com');";
			expected += @"INSERT INTO `TestTable` (Id,Name,Website) VALUES (2,'Na\\te','kohari.org');";

			sql.ShouldBe(expected);
		}

		[Test]
		public void CanInsertGuidData()
		{
			var gid = Guid.NewGuid();
			var expression = new InsertDataExpression { TableName = "TestTable" };
			expression.Rows.Add(new InsertionDataDefinition { new KeyValuePair<string, object>("guid", gid) });

			var sql = generator.Generate(expression);

			var expected = String.Format("INSERT INTO `TestTable` (guid) VALUES ('{0}');", gid);

			sql.ShouldBe(expected);
		}

		[Test, ExpectedException(typeof(NotImplementedException))]
		public void CanRenameColumn()
		{
			var expression = new RenameColumnExpression();
			expression.TableName = "Table1";
			expression.OldName = "Column1";
			expression.NewName = "Column2";

			var sql = generator.Generate(expression);
			// MySql does not appear to have a way to change column without re-specifying the existing column definition
			sql.ShouldBe("ALTER TABLE `Table1` CHANGE COLUMN `Column1` `Column2` EXISTING_DEFINITION_HERE");
		}

		[Test]
		public void CanRenameTable()
		{
			var expression = new RenameTableExpression();
			expression.OldName = "Table1";
			expression.NewName = "Table2";

			var sql = generator.Generate(expression);
			sql.ShouldBe("RENAME TABLE `Table1` TO `Table2`");
		}
	}

	[TestFixture]
	public class MySqlGeneratorCreateTableTests : MySqlGeneratorTests
	{
		private string tableName = "NewTable";

		[Test]
		public void CanCreateTable()
		{
			var expression = GetCreateTableExpression(tableName);
			var sql = generator.Generate(expression);
			sql.ShouldBe("CREATE TABLE `NewTable` (ColumnName1 VARCHAR(255) NOT NULL, ColumnName2 INTEGER NOT NULL) ENGINE = INNODB");
		}

		[Test]
		public void CanCreateTableWithCustomColumnType()
		{
			var expression = GetCreateTableExpression(tableName);
			expression.Columns[0].IsPrimaryKey = true;
			expression.Columns[1].Type = null;
			expression.Columns[1].CustomType = "[timestamp]";
			var sql = generator.Generate(expression);
			sql.ShouldBe(
				"CREATE TABLE `NewTable` (ColumnName1 VARCHAR(255) NOT NULL, PRIMARY KEY (`ColumnName1`), ColumnName2 [timestamp] NOT NULL) ENGINE = INNODB");
		}

		[Test]
		public void CanCreateTableWithPrimaryKey()
		{
			var expression = GetCreateTableExpression(tableName);
			expression.Columns[0].IsPrimaryKey = true;
			var sql = generator.Generate(expression);
			sql.ShouldBe(
				"CREATE TABLE `NewTable` (ColumnName1 VARCHAR(255) NOT NULL, PRIMARY KEY (`ColumnName1`), ColumnName2 INTEGER NOT NULL) ENGINE = INNODB");
		}

		[Test]
		public void CanCreateTableWithIdentity()
		{
			var expression = GetCreateTableExpression(tableName);
			expression.Columns[0].IsIdentity = true;
			var sql = generator.Generate(expression);
			sql.ShouldBe(
				"CREATE TABLE `NewTable` (ColumnName1 VARCHAR(255) NOT NULL AUTO_INCREMENT, ColumnName2 INTEGER NOT NULL) ENGINE = INNODB");
		}

		[Test]
		public void CanCreateTableWithNullField()
		{
			var expression = GetCreateTableExpression(tableName);
			expression.Columns[0].IsNullable = true;
			var sql = generator.Generate(expression);
			sql.ShouldBe(
				"CREATE TABLE `NewTable` (ColumnName1 VARCHAR(255), ColumnName2 INTEGER NOT NULL) ENGINE = INNODB");
		}

		[Test]
		public void CanCreateTableWithDefaultValue()
		{
			var expression = GetCreateTableExpression(tableName);
			expression.Columns[0].DefaultValue = "Default";
			expression.Columns[1].DefaultValue = 0;
			var sql = generator.Generate(expression);
			sql.ShouldBe(
				"CREATE TABLE `NewTable` (ColumnName1 VARCHAR(255) NOT NULL DEFAULT 'Default', ColumnName2 INTEGER NOT NULL DEFAULT 0) ENGINE = INNODB");
		}


		protected CreateTableExpression GetCreateTableExpression(string tableName)
		{
			var columnName1 = "ColumnName1";
			var columnName2 = "ColumnName2";

			var column1 = new ColumnDefinition { Name = columnName1, Type = DbType.String };
			var column2 = new ColumnDefinition { Name = columnName2, Type = DbType.Int32 };

			var expression = new CreateTableExpression { TableName = tableName };
			expression.Columns.Add(column1);
			expression.Columns.Add(column2);
			return expression;
		}
	}
}