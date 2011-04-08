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
using FluentMigrator.Builders.Execute;
using MySql.Data.MySqlClient;

namespace FluentMigrator.Runner.Processors.MySql
{
	public class MySqlProcessor : ProcessorBase
	{
		public MySqlConnection Connection { get; set; }

		public MySqlProcessor(MySqlConnection connection, IMigrationGenerator generator, IAnnouncer announcer, IMigrationProcessorOptions options)
			: base(generator, announcer, options)
		{
			Connection = connection;
		}

		public override bool SchemaExists(string schemaName)
		{
			throw new NotImplementedException();
		}

        public override bool TableExists(string schemaName, string tableName)
		{
			return Exists(@"select table_name from information_schema.tables 
							where table_schema = SCHEMA() and table_name='{0}'", tableName);
		}

        public override bool ColumnExists(string schemaName, string tableName, string columnName)
		{
			string sql = @"select column_name from information_schema.columns
							where table_schema = SCHEMA() and table_name='{0}'
							and column_name='{1}'";
			return Exists(sql, tableName, columnName);
		}

        public override bool ConstraintExists(string schemaName, string tableName, string constraintName)
		{
			string sql = @"select constraint_name from information_schema.table_constraints
							where table_schema = SCHEMA() and table_name='{0}'
							and constraint_name='{1}'";
			return Exists(sql, tableName, constraintName);
		}

        public override bool IndexExists(string schemaName, string tableName, string indexName)
		{
			string sql = @"select index_name from information_schema.statistics
							where table_schema = SCHEMA() and table_name='{0}'
							and index_name='{1}'";
			return Exists(sql, tableName, indexName);
		}

		public override void Execute(string template, params object[] args)
		{
			if (Connection.State != ConnectionState.Open) Connection.Open();

			using (var command = new MySqlCommand(String.Format(template, args), Connection))
			{
				command.CommandTimeout = Options.Timeout;
				command.ExecuteNonQuery();
			}
		}

		public override bool Exists(string template, params object[] args)
		{
			if (Connection.State != ConnectionState.Open) Connection.Open();

			using (var command = new MySqlCommand(String.Format(template, args), Connection))
			{
				command.CommandTimeout = Options.Timeout;
				using (var reader = command.ExecuteReader())
				{
					try
					{
						if (!reader.Read())
							return false;

						return true;
					}
					catch
					{
						return false;
					}
				}
			}
		}

        public override DataSet ReadTableData(string schemaName, string tableName)
		{
			return Read("select * from {0}", tableName);
		}

		public override DataSet Read(string template, params object[] args)
		{
			if (Connection.State != ConnectionState.Open) Connection.Open();

			DataSet ds = new DataSet();
			using (var command = new MySqlCommand(String.Format(template, args), Connection))
			{
				command.CommandTimeout = Options.Timeout;

				using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
				{
					adapter.Fill(ds);
					return ds;
				}
			}
		}

		protected override void Process(string sql)
		{
			Announcer.Sql(sql);

			if (Options.PreviewOnly || string.IsNullOrEmpty(sql))
				return;

			if (Connection.State != ConnectionState.Open)
				Connection.Open();

			using (var command = new MySqlCommand(sql, Connection))
			{
				command.CommandTimeout = Options.Timeout;
				command.ExecuteNonQuery();
			}
		}

		public override void Process(PerformDBOperationExpression expression)
		{
			if (Connection.State != ConnectionState.Open) Connection.Open();

			if (expression.Operation != null)
				expression.Operation(Connection, null);
		}
	}
}