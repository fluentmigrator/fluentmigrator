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

using System.Data;
using System.Data.SQLite;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Processors.Sqlite;
using Moq;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors
{
	[TestFixture]
	public class SqliteProcessorTests
	{
		private SQLiteConnection connection;
		private SqliteProcessor processor;
		private Mock<ColumnDefinition> column;
		private SQLiteCommand command;
		private string columnName;
		private string tableName;

		[SetUp]
		public void SetUp()
		{
			// This connection used in the tests
			connection = new SQLiteConnection { ConnectionString = "Data Source=:memory:;Version=3;New=True;" };
			connection.Open();
			command = connection.CreateCommand();

			// SUT
			processor = new SqliteProcessor(connection, new SqliteGenerator(), new Announcer(System.Console.Out));

			column = new Mock<ColumnDefinition>();
			tableName = "NewTable";
			columnName = "ColumnName";
			column.SetupGet(c => c.Name).Returns(columnName);
			column.SetupGet(c => c.IsNullable).Returns(true);
			column.SetupGet(c => c.Type).Returns(DbType.Int32);
		}

		[Test]
		public void CanDefaultAutoIncrementColumnTypeToInteger()
		{
			ColumnDefinition column = new ColumnDefinition();
			column.Name = "Id";
			column.IsIdentity = true;
			column.IsPrimaryKey = true;
			column.Type = DbType.Int64;
			column.IsNullable = false;

			CreateTableExpression expression = new CreateTableExpression { TableName = tableName };
			expression.Columns.Add(column);

			using (command)
			{
				processor.Process(expression);
				command.CommandText = string.Format("SELECT name FROM sqlite_master WHERE type='table' and name='{0}'", tableName);
				command.ExecuteReader().Read().ShouldBeTrue();
			}
		}

		[Test]
		public void CanCreateTableExpression()
		{
			CreateTableExpression expression = new CreateTableExpression { TableName = tableName };
			expression.Columns.Add(column.Object);

			using (command)
			{
				processor.Process(expression);
				command.CommandText = string.Format("SELECT name FROM sqlite_master WHERE type='table' and name='{0}'", tableName);
				command.ExecuteReader().Read().ShouldBeTrue();
			}
		}

		public void InsertTable()
		{
			SQLiteCommand cmd = connection.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = string.Format("CREATE TABLE {0} ({1})", tableName, columnName);
			cmd.ExecuteNonQuery();
		}
	}
}