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
	[TestFixture]
	public class SqlServer2000GeneratorTests
	{
		protected SqlServer2000Generator generator;

		[SetUp]
		public void SetUp()
		{
			generator = new SqlServer2000Generator();
		}

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
			sql.ShouldBe("ALTER TABLE [NewTable] ADD NewColumn NVARCHAR(5) NOT NULL");
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
			sql.ShouldBe("ALTER TABLE [NewTable] ADD NewColumn DECIMAL(19,2) NOT NULL");
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
				"ALTER TABLE [TestForeignTable] ADD CONSTRAINT FK_Test FOREIGN KEY ([Column3],[Column4]) REFERENCES [TestPrimaryTable] ([Column1],[Column2])");
		}

		[Test]
		public void CanCreateIndex()
		{
			var expression = new CreateIndexExpression();
			expression.Index.Name = "IX_TEST";
			expression.Index.TableName = "TEST_TABLE";
			expression.Index.IsUnique = true;
			expression.Index.IsClustered = true;
			expression.Index.Columns.Add(new IndexColumnDefinition { Direction = Direction.Ascending, Name = "Column1" });
			expression.Index.Columns.Add(new IndexColumnDefinition { Direction = Direction.Descending, Name = "Column2" });

			var sql = generator.Generate(expression);
			sql.ShouldBe("CREATE UNIQUE CLUSTERED INDEX IX_TEST ON [TEST_TABLE] (Column1 ASC,Column2 DESC)");
		}

		[Test]
		[Ignore("need better way to test this")]
		public void CanDropColumn()
		{
			var tableName = "NewTable";
			var columnName = "NewColumn";

			var expression = new DeleteColumnExpression();
			expression.TableName = tableName;
			expression.ColumnName = columnName;

			var sql = generator.Generate(expression);

			var expectedSql =
				@"
			DECLARE @default sysname, @sql nvarchar(max);

			-- get name of default constraint
			SELECT @default = name 
			FROM sys.default_constraints 
			WHERE parent_object_id = object_id('NewTable')
			AND type = 'D'
			AND parent_column_id = (
				SELECT column_id 
				FROM sys.columns 
				WHERE object_id = object_id('NewTable')
				AND name = 'NewColumn'
			);

			-- create alter table command as string and run it
			SET @sql = N'ALTER TABLE [NewTable] DROP CONSTRAINT ' + @default;
			EXEC sp_executesql @sql;

			-- now we can finally drop column
			ALTER TABLE [NewTable] DROP COLUMN [NewColumn];";

			sql.ShouldBe(expectedSql);
		}

		[Test]
		public void CanDropForeignKey()
		{
			var expression = new DeleteForeignKeyExpression();
			expression.ForeignKey.Name = "FK_Test";
			expression.ForeignKey.PrimaryTable = "TestPrimaryTable";

			var sql = generator.Generate(expression);
			sql.ShouldBe("ALTER TABLE [TestPrimaryTable] DROP CONSTRAINT FK_Test");
		}

		[Test]
		public void CanDropTable()
		{
			var tableName = "NewTable";
			var expression = GetDeleteTableExpression(tableName);
			var sql = generator.Generate(expression);
			sql.ShouldBe("DROP TABLE [NewTable]");
		}

		[Test]
		public void CanInsertData()
		{
			var expression = new InsertDataExpression();
			expression.TableName = "TestTable";
			expression.Rows.Add(new InsertionDataDefinition
			                    	{
			                    		new KeyValuePair<string, object>("Id", 1),
			                    		new KeyValuePair<string, object>("Name", "Justin"),
			                    		new KeyValuePair<string, object>("Website", "codethinked.com")
			                    	});
			expression.Rows.Add(new InsertionDataDefinition
			                    	{
			                    		new KeyValuePair<string, object>("Id", 2),
			                    		new KeyValuePair<string, object>("Name", "Nate"),
			                    		new KeyValuePair<string, object>("Website", "kohari.org")
			                    	});

			var sql = generator.Generate(expression);

			var expected = "INSERT INTO [TestTable] ([Id],[Name],[Website]) VALUES (1,'Justin','codethinked.com');";
			expected += "INSERT INTO [TestTable] ([Id],[Name],[Website]) VALUES (2,'Nate','kohari.org');";

			sql.ShouldBe(expected);
		}

		[Test]
		public void CanInsertGuidData()
		{
			var gid = Guid.NewGuid();
			var expression = new InsertDataExpression { TableName = "TestTable" };
			expression.Rows.Add(new InsertionDataDefinition { new KeyValuePair<string, object>("guid", gid) });

			var sql = generator.Generate(expression);

			var expected = String.Format("INSERT INTO [TestTable] ([guid]) VALUES ('{0}');", gid);

			sql.ShouldBe(expected);
		}

		[Test]
		public void CanRenameColumn()
		{
			var expression = new RenameColumnExpression();
			expression.TableName = "Table1";
			expression.OldName = "Column1";
			expression.NewName = "Column2";

			var sql = generator.Generate(expression);
			sql.ShouldBe("sp_rename '[Table1].[Column1]', [Column2]");
		}

		[Test]
		public void CanRenameTable()
		{
			var expression = new RenameTableExpression();
			expression.OldName = "Table1";
			expression.NewName = "Table2";

			var sql = generator.Generate(expression);
			sql.ShouldBe("sp_rename [Table1], [Table2]");
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

	[TestFixture]
	public class SqlServer200GeneratorCreateTableTests : SqlServer2000GeneratorTests
	{
		[Test]
		public void CanCreateTable()
		{
			var tableName = "NewTable";
			var expression = GetCreateTableExpression(tableName);
			var sql = generator.Generate(expression);
			sql.ShouldBe("CREATE TABLE [NewTable] (ColumnName1 NVARCHAR(255) NOT NULL, ColumnName2 INT NOT NULL)");
		}

		[Test]
		public void CanCreateTableWithCustomColumnType()
		{
			var tableName = "NewTable";
			var expression = GetCreateTableExpression(tableName);
			expression.Columns[0].IsPrimaryKey = true;
			expression.Columns[1].Type = null;
			expression.Columns[1].CustomType = "[timestamp]";
			var sql = generator.Generate(expression);
			sql.ShouldBe(
				"CREATE TABLE [NewTable] (ColumnName1 NVARCHAR(255) NOT NULL PRIMARY KEY CLUSTERED, ColumnName2 [timestamp] NOT NULL)");
		}

		[Test]
		public void CanCreateTableWithPrimaryKey()
		{
			var tableName = "NewTable";
			var expression = GetCreateTableExpression(tableName);
			expression.Columns[0].IsPrimaryKey = true;
			var sql = generator.Generate(expression);
			sql.ShouldBe(
				"CREATE TABLE [NewTable] (ColumnName1 NVARCHAR(255) NOT NULL PRIMARY KEY CLUSTERED, ColumnName2 INT NOT NULL)");
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