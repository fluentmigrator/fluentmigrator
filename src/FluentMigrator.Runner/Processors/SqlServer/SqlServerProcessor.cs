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
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace FluentMigrator.Runner.Processors.SqlServer
{
	public class SqlServerProcessor : ProcessorBase
	{
		public virtual SqlConnection Connection { get; set; }
		public SqlTransaction Transaction { get; private set; }

		public SqlServerProcessor(SqlConnection connection, IMigrationGenerator generator)
		{
			this.generator = generator;
			Connection = connection;
			Transaction = Connection.BeginTransaction();
		}

		public override bool TableExists(string tableName)
		{
			return Exists("SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}'", tableName);
		}

		public override bool ColumnExists(string tableName, string columnName)
		{
			return Exists("SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{0}' AND COLUMN_NAME = '{1}'", tableName, columnName);
		}

		public override bool ConstraintExists(string tableName, string constraintName)
		{
			return Exists("SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_CATALOG = DB_NAME() AND TABLE_NAME = '{0}' AND CONSTRAINT_NAME = '{1}'", tableName, constraintName);
		}
		
		public override void Execute(string template, params object[] args)
		{
			if (Connection.State != ConnectionState.Open)
				Connection.Open();

			using (var command = new SqlCommand(String.Format(template, args), Connection, Transaction))
			{
				command.ExecuteNonQuery();
			}
		}

		public override bool Exists(string template, params object[] args)
		{
			if (Connection.State != ConnectionState.Open)
				Connection.Open();

			using (var command = new SqlCommand(String.Format(template, args), Connection, Transaction))
			using (var reader = command.ExecuteReader())
			{
				return reader.Read();
			}
		}

		public override DataSet ReadTableData(string tableName)
		{
			return Read("SELECT * FROM {0}", tableName);
		}

		public override DataSet Read(string template, params object[] args)
		{
			if (Connection.State != ConnectionState.Open) Connection.Open();

			DataSet ds = new DataSet();
			using (var command = new SqlCommand(String.Format(template, args), Connection, Transaction))
			using (SqlDataAdapter adapter = new SqlDataAdapter(command))
			{
				adapter.Fill(ds);
				return ds;
			}
		}

		public override void CommitTransaction()
		{
			Transaction.Commit();
		}

		public override void RollbackTransaction()
		{
			Transaction.Rollback();
		}

		protected override void Process(string sql)
		{
			if (Connection.State != ConnectionState.Open)
				Connection.Open();

			using (var command = new SqlCommand(sql, Connection, Transaction)) {
				try {
					command.ExecuteNonQuery();
				}
				catch(Exception ex) {
					using (StringWriter message = new StringWriter()) {
						message.WriteLine("An error occured executing the following sql:");
						message.WriteLine(sql);
						message.WriteLine("The error was {0}", ex.Message);

						throw new Exception(message.ToString(), ex);
					}
				}	
			}
		}
	}
}