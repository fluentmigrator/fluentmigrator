#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
// Copyright (c) 2010, Nathan Brown
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
using System.Data.SQLite;

using FluentMigrator.Builders.Execute;

namespace FluentMigrator.Runner.Processors.Sqlite
{
	public class SqliteProcessor : ProcessorBase
	{
		public SQLiteConnection Connection { get; set; }

		public SqliteProcessor(SQLiteConnection connection, IMigrationGenerator generator, IAnnouncer announcer, IMigrationProcessorOptions options)
			: base(generator, announcer, options)
		{
			Connection = connection;
		}

		public override bool SchemaExists(string schemaName)
		{
            return true;
		}

        public override bool TableExists(string schemaName, string tableName)
		{
            return Exists("select count(*) from sqlite_master where name=\"{0}\" and type='table'", tableName);
		}

        public override bool ColumnExists(string schemaName, string tableName, string columnName)
		{
		    return Read("PRAGMA table_info([{0}])",tableName).Tables[0].Select(string.Format("Name='{0}'",columnName.Replace("'","''"))).Length>0;
        }

        public override bool ConstraintExists(string schemaName, string tableName, string constraintName)
		{
			return false;
		}

        public override bool IndexExists(string schemaName, string tableName, string indexName)
        {
            return Exists("select count(*) from sqlite_master where name='{0}' and tbl_name='{1}' and type='index'", indexName, tableName);
        }

		public override void Execute(string template, params object[] args)
		{
			Process(String.Format(template, args));
		}

		public override bool Exists(string template, params object[] args)
		{
			if (Connection.State != ConnectionState.Open) Connection.Open();

			using (var command = new SQLiteCommand(String.Format(template, args), Connection))
			using (var reader = command.ExecuteReader())
			{
				try
				{
					if (!reader.Read()) return false;
					if (int.Parse(reader[0].ToString()) <= 0) return false;
					return true;
				}
				catch
				{
					return false;
				}
			}
		}

        public override DataSet ReadTableData(string schemaName, string tableName)
		{
			return Read("select * from [{0}]", tableName);
		}

		public override void Process(PerformDBOperationExpression expression)
		{
			if (Connection.State != ConnectionState.Open) Connection.Open();
				
			if (expression.Operation != null)
				expression.Operation(Connection, null);
		}

		protected override void Process(string sql)
		{
			Announcer.Sql(sql);

			if (Options.PreviewOnly || string.IsNullOrEmpty(sql))
				return;

			if (Connection.State != ConnectionState.Open)
				Connection.Open();

            if (sql.Contains("GO"))
            {
                ExecuteBatchNonQuery(sql, Connection);

            }
            else
            {
                ExecuteNonQuery(sql, Connection);
            }

			
		}

        private void ExecuteNonQuery(string sql, SQLiteConnection connection)
        {
            using (var command = new SQLiteCommand(sql, Connection))
            {
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (SQLiteException ex)
                {
                    throw new SQLiteException(ex.Message + "\r\nWhile Processing:\r\n\"" + command.CommandText + "\"", ex);
                }
            }
        }

        private void ExecuteBatchNonQuery(string sql, SQLiteConnection conn)
        {
            sql += "\nGO";   // make sure last batch is executed.
            string sqlBatch = string.Empty;
            
            using (var command = new SQLiteCommand(sql, Connection))
            {
                try
                {
                    foreach (string line in sql.Split(new string[2] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (line.ToUpperInvariant().Trim() == "GO")
                        {
                            if (!string.IsNullOrEmpty(sqlBatch))
                            {
                                command.CommandText = sqlBatch;
                                command.ExecuteNonQuery();
                                sqlBatch = string.Empty;
                            }
                        }
                        else
                        {
                            sqlBatch += line + "\n";
                        }
                    }
                }
                 catch (SQLiteException ex)
                {
                    throw new SQLiteException(ex.Message + "\r\nWhile Processing:\r\n\"" + command.CommandText + "\"", ex);
                }
            }
        }

		public override DataSet Read(string template, params object[] args)
		{
			if (Connection.State != ConnectionState.Open) Connection.Open();

			DataSet ds = new DataSet();
			using (var command = new SQLiteCommand(String.Format(template, args), Connection))
			using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(command))
			{
				adapter.Fill(ds);
				return ds;
			}
		}
	}
}