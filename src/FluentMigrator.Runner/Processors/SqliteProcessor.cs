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
using System.Data.SQLite;

namespace FluentMigrator.Runner.Processors
{
	public class SqliteProcessor: ProcessorBase
	{
		public SQLiteConnection Connection { get; set; }

		public SqliteProcessor(SQLiteConnection connection, IMigrationGenerator generator)
		{
			this.generator = generator;
			Connection = connection;
		}

        public override bool TableExists(string tableName)
        {
            //select count(*) from sqlite_master where name='tableName'
            //select * from sqlite_master where name={0}
            return Exists("select * from sqlite_master where name='{0}'", tableName);
        }

	    public override void Execute(string template, params object[] args)
        {
            if (Connection.State != ConnectionState.Open) Connection.Open();

            using (var command = new SQLiteCommand(String.Format(template, args), Connection))
            {
                command.ExecuteNonQuery();
            }
        }

        public override bool Exists(string template, params object[] args)
        {
            if (Connection.State != ConnectionState.Open) Connection.Open();

            using (var command = new SQLiteCommand(String.Format(template, args), Connection))
            using (var reader = command.ExecuteReader())
            {
                return reader.Read();
            }
        }

        public override DataSet ReadTableData(string tableName)
        {
            return Read("select * from {0}", tableName);
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
        
		protected override void Process(string sql)
		{
            if (Connection.State != ConnectionState.Open) Connection.Open();

			using (var command = new SQLiteCommand(sql, Connection))
				command.ExecuteNonQuery();
		}

        public override void UpdateTable(string tableName, List<string> columns, List<string> formattedValues)
        {
            var setParam = string.Empty;
            for (int index = 0; index < columns.Count; index++)
            {
                setParam += columns[index] + " = " + formattedValues[index];
                if (index < columns.Count - 1) setParam += ",";
            }

            Execute("update {0} set {1} ", tableName, setParam);
        }
	}
}