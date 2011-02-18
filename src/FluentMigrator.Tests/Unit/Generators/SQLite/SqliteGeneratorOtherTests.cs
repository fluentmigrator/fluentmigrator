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



namespace FluentMigrator.Tests.Unit.Generators
{
    using System.Collections.Generic;
    using System.Data;
    using FluentMigrator.Expressions;
    using FluentMigrator.Model;
    using NUnit.Framework;
    using NUnit.Should;
    using FluentMigrator.Runner.Generators.SQLite;

    public class SqliteGeneratorTests : SQLiteTestBase
	{
		
		[Test]
		public void CanCreateTable()
		{
			CreateTableExpression expression = GetCreateTableExpression();
			string sql = generator.Generate(expression);
			sql.ShouldBe(string.Format("CREATE TABLE [{0}] (NewColumn TEXT NOT NULL)", this.TestTableName1));
		}

		[Test]
		public void CanCreateTableWithAutoIncrement()
		{
			var expression = GetCreateTableWithPrimaryKeyIdentityExpression();
			var sql = generator.Generate(expression);
            sql.ShouldBe(string.Format("CREATE TABLE [{0}] (NewColumn INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT)", this.TestTableName1));
		}

		[Test]
		public void CanRenameTable()
		{
            RenameTableExpression expression = new RenameTableExpression { OldName = this.TestTableName1, NewName = this.TestTableName2 };
			string sql = generator.Generate(expression);
            sql.ShouldBe(string.Format("ALTER TABLE [{0}] RENAME TO [{1}]", this.TestTableName1, this.TestTableName2));
		}

		[Test]
		public void CanDeleteTable()
		{
            DeleteTableExpression expression = new DeleteTableExpression { TableName = this.TestTableName1 };
			string sql = generator.Generate(expression);
            sql.ShouldBe(string.Format("DROP TABLE [{0}]", this.TestTableName1));
		}

		[Test]
		public void CanCreateColumn()
		{
			CreateColumnExpression expression = GetCreateColumnExpression();
			string sql = generator.Generate(expression);
            sql.ShouldBe(string.Format("ALTER TABLE [{0}] ADD COLUMN [{1}] TEXT NOT NULL", this.TestTableName1, this.TestColumnName1));
		}

		[Test]
		public void CanAddDecimalColumn()
		{
			string tableName = "NewTable";

			var columnDefinition = new ColumnDefinition();
			columnDefinition.Name = "NewColumn";
			columnDefinition.Size = 19;
			columnDefinition.Precision = 2;
			columnDefinition.Type = DbType.Decimal;

			var expression = new CreateColumnExpression();
			expression.Column = columnDefinition;
			expression.TableName = tableName;

			string sql = generator.Generate(expression);
			sql.ShouldBe("ALTER TABLE [NewTable] ADD COLUMN [NewColumn] NUMERIC NOT NULL");
		}

		[Test]
		public void CanCreateAutoIncrementColumn()
		{
			CreateColumnExpression expression = GetCreateAutoIncrementColumnExpression();
			string sql = generator.Generate(expression);
			sql.ShouldBe(string.Format("ALTER TABLE [{0}] ADD COLUMN [{1}] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT", this.TestTableName1, this.TestColumnName1));
		}

		//[Test]
		//public void CanRenameColumn()
		//{
		//	RenameColumnExpression expression = GetRenameColumnExpression();
		//	string sql = Generator.Generate(expression);
		//	sql.ShouldBe(string.Format("UPDATE {0} SET {1}={2}", table, oldColumn, newColumn));
		//}

		[Test]
		public void CanDeleteColumn()
		{
            DeleteColumnExpression expression = GetDeleteColumnExpression();
			string sql = generator.Generate(expression);
            string.Format("ALTER TABLE [{0}] DROP COLUMN [{1}]", this.TestTableName1, this.TestColumnName1).ShouldBe(sql);
		}

		// CreateForeignKey -- Not supported in Sqlite
		// DeleteForeignKey -- Not supported in Sqlite

		[Test]
		public void CanCreateBasicIndex()
		{
			CreateIndexExpression expression = GetCreateIndexExpression();
			string sql = generator.Generate(expression);
			sql.ShouldBe(string.Format("CREATE INDEX IF NOT EXISTS [{0}] ON [{1}] ([{2}])", this.TestIndexName, this.TestTableName1, this.TestColumnName1));
		}

		[Test]
		public void CanDeleteBasicIndex()
		{
			DeleteIndexExpression expression = GetDeleteIndexExpression();
			string sql = generator.Generate(expression);
            sql.ShouldBe(string.Format("DROP INDEX IF EXISTS [{0}]", this.TestIndexName));
		}

		// DeleteIndex

		[Test]
		public void CanInsertData()
		{
			var expression = new InsertDataExpression();
			expression.TableName = "TestTable";
			expression.Rows.Add(new InsertionDataDefinition { new KeyValuePair<string, object>("Id", 1), 
													new KeyValuePair<string, object>("Name", "Justin"),
													new KeyValuePair<string, object>("Website", "codethinked.com") });
			expression.Rows.Add(new InsertionDataDefinition { new KeyValuePair<string, object>("Id", 2), 
													new KeyValuePair<string, object>("Name", "Nate"),
													new KeyValuePair<string, object>("Website", "kohari.org") });

			string sql = generator.Generate(expression);

			string expected = "INSERT INTO [TestTable] (Id,Name,Website) VALUES (1,'Justin','codethinked.com');";
			expected += "INSERT INTO [TestTable] (Id,Name,Website) VALUES (2,'Nate','kohari.org');";

			sql.ShouldBe(expected);
		}

		
	}
}