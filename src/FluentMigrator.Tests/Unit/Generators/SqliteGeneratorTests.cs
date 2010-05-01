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

using System.Collections.Generic;
using System.Data;
using FluentMigrator.Builders.Insert;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators
{
	[TestFixture]
	public class SqliteGeneratorTests
	{
		SqliteGenerator generator;
		string table = "Table";
		private string oldTable = "OldTable";
		string newTable = "NewTable";

		string column = "Column";
		string oldColumn = "OldColumn";
		string newColumn = "NewColumn";

		string indexName = "indexed-column";
		string indexColumn = "IndexColumn";

		public SqliteGeneratorTests()
		{
			generator = new SqliteGenerator();
		}

		[Test]
		public void CanCreateTable()
		{
			CreateTableExpression expression = GetCreateTableExpression();
			string sql = generator.Generate(expression);
			sql.ShouldBe(string.Format("CREATE TABLE {0} (NewColumn TEXT NOT NULL)", table));
		}

		[Test]
		public void CanRenameTable()
		{
			RenameTableExpression expression = new RenameTableExpression { OldName = oldTable, NewName = newTable };
			string sql = generator.Generate(expression);
			sql.ShouldBe(string.Format("ALTER TABLE {0} RENAME TO {1}", oldTable, newTable));
		}

		[Test]
		public void CanDeleteTable()
		{
			DeleteTableExpression expression = new DeleteTableExpression { TableName = table };
			string sql = generator.Generate(expression);
			sql.ShouldBe(string.Format("DROP TABLE {0}", table));
		}

		[Test]
		public void CanCreateColumn()
		{
			CreateColumnExpression expression = GetCreateColumnExpression();
			string sql = generator.Generate(expression);
			sql.ShouldBe(string.Format("ALTER TABLE [{0}] ADD COLUMN {1} TEXT NOT NULL", table, newColumn));
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
			sql.ShouldBe("ALTER TABLE [NewTable] ADD COLUMN NewColumn NUMERIC NOT NULL");
		}

		[Test]
		public void CanCreateAutoIncrementColumn()
		{
			CreateColumnExpression expression = GetCreateAutoIncrementColumnExpression();
			string sql = generator.Generate(expression);
			sql.ShouldBe(string.Format("ALTER TABLE [{0}] ADD COLUMN {1} INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT", table, newColumn));
		}

		//[Test]
		//public void CanRenameColumn()
		//{
		//	RenameColumnExpression expression = GetRenameColumnExpression();
		//	string sql = generator.Generate(expression);
		//	sql.ShouldBe(string.Format("UPDATE {0} SET {1}={2}", table, oldColumn, newColumn));
		//}

		[Test]
		public void CanDeleteColumn()
		{
			DeleteColumnExpression expression = new DeleteColumnExpression { TableName = table, ColumnName = column };
			string sql = generator.Generate(expression);
			sql.ShouldBe(string.Format("ALTER TABLE {0} DROP COLUMN {1}", table, column));
		}

		// CreateForeignKey -- Not supported in Sqlite
		// DeleteForeignKey -- Not supported in Sqlite

		[Test]
		public void CanCreateBasicIndex()
		{
			CreateIndexExpression expression = GetCreateIndexExpression();
			string sql = generator.Generate(expression);
			sql.ShouldBe(string.Format("CREATE INDEX IF NOT EXISTS {0} ON {1} ({2})", indexName, table, indexColumn));
		}

		// DeleteIndex

		[Test]
		public void CanInsertData()
		{
			var expression = new InsertDataExpression();
			expression.TableName = "TestTable";
			expression.Rows.Add(new InsertionData { new KeyValuePair<string, object>("Id", 1), 
													new KeyValuePair<string, object>("Name", "Justin"),
													new KeyValuePair<string, object>("Website", "codethinked.com") });
			expression.Rows.Add(new InsertionData { new KeyValuePair<string, object>("Id", 2), 
													new KeyValuePair<string, object>("Name", "Nate"),
													new KeyValuePair<string, object>("Website", "kohari.org") });

			string sql = generator.Generate(expression);

			string expected = "INSERT INTO [TestTable] (Id,Name,Website) VALUES (1,'Justin','codethinked.com');";
			expected += "INSERT INTO [TestTable] (Id,Name,Website) VALUES (2,'Nate','kohari.org');";

			sql.ShouldBe(expected);
		}

		private CreateIndexExpression GetCreateIndexExpression()
		{
			IndexColumnDefinition indexColumnDefinition = new IndexColumnDefinition { Name = indexColumn };
			IndexDefinition indexDefinition = new IndexDefinition { TableName = table, Name = indexName, Columns = new List<IndexColumnDefinition> { indexColumnDefinition } };
			return new CreateIndexExpression { Index = indexDefinition };
		}

		private RenameColumnExpression GetRenameColumnExpression()
		{
			return new RenameColumnExpression { OldName = oldColumn, NewName = newColumn, TableName = table };
		}

		private CreateColumnExpression GetCreateColumnExpression()
		{
			ColumnDefinition column = new ColumnDefinition { Name = newColumn, Type = DbType.String };
			return new CreateColumnExpression { TableName = table, Column = column };
		}

		private CreateColumnExpression GetCreateAutoIncrementColumnExpression()
		{
			ColumnDefinition column = new ColumnDefinition { Name = newColumn, IsIdentity = true, IsPrimaryKey = true, Type = DbType.String };
			return new CreateColumnExpression { TableName = table, Column = column };
		}

		private CreateTableExpression GetCreateTableExpression()
		{
			CreateTableExpression expression = new CreateTableExpression() { TableName = table, };
			expression.Columns.Add(new ColumnDefinition { Name = newColumn, Type = DbType.String });
			return expression;
		}
	}
}