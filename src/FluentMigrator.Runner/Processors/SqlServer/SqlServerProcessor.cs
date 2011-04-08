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
using System.Data;
using System.Data.SqlClient;
using System.IO;
using FluentMigrator.Builders.Execute;

namespace FluentMigrator.Runner.Processors.SqlServer
{
	public class SqlServerProcessor : ProcessorBase
	{
		public virtual SqlConnection Connection { get; set; }
		public SqlTransaction Transaction { get; private set; }
		public bool WasCommitted { get; private set; }

		public SqlServerProcessor(SqlConnection connection, IMigrationGenerator generator, IAnnouncer announcer, IMigrationProcessorOptions options)
			: base(generator, announcer, options)
		{
			Connection = connection;
			connection.Open();
			Transaction = connection.BeginTransaction();
		}

		private string SafeSchemaName(string schemaName)
		{
			return string.IsNullOrEmpty(schemaName) ? "dbo" : FormatSqlEscape(schemaName);
		}

		public override bool SchemaExists(string schemaName)
		{
			return Exists("SELECT * FROM SYS.SCHEMAS WHERE NAME = '{0}'", SafeSchemaName(schemaName));
		}

        public override bool TableExists(string schemaName, string tableName)
		{
        	try
        	{
        		return Exists("SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{0}' AND TABLE_NAME = '{1}'", SafeSchemaName(schemaName), FormatSqlEscape(tableName));
        	}
        	catch (Exception e)
        	{
        		Console.WriteLine(e);
        	}
        	return false;
		}

        public override bool ColumnExists(string schemaName, string tableName, string columnName)
		{
			return Exists("SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = '{0}' AND TABLE_NAME = '{1}' AND COLUMN_NAME = '{2}'", SafeSchemaName(schemaName), FormatSqlEscape(tableName), FormatSqlEscape(columnName));
		}

        public override bool ConstraintExists(string schemaName, string tableName, string constraintName)
		{
			return Exists("SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_CATALOG = DB_NAME() AND TABLE_SCHEMA = '{0}' AND TABLE_NAME = '{1}' AND CONSTRAINT_NAME = '{2}'", SafeSchemaName(schemaName), FormatSqlEscape(tableName), FormatSqlEscape(constraintName));
		}

        public override bool IndexExists(string schemaName, string tableName, string indexName)
        {
            return Exists("SELECT NULL FROM sysindexes WHERE name = '{0}'", FormatSqlEscape(indexName));
        }

		public override void Execute(string template, params object[] args)
		{
			Process(String.Format(template, args));
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

        public override DataSet ReadTableData(string schemaName, string tableName)
		{
			return Read("SELECT * FROM [{0}]", tableName);
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

		public override void BeginTransaction()
		{
			Announcer.Say( "Beginning Transaction" );
			Transaction = Connection.BeginTransaction();
		}

		public override void CommitTransaction()
		{
			Announcer.Say("Committing Transaction");
			Transaction.Commit();
			WasCommitted = true;
			if (Connection.State != ConnectionState.Closed)
			{
				Connection.Close();
			}
		}

		public override void RollbackTransaction()
		{
			Announcer.Say("Rolling back transaction");
			Transaction.Rollback();
			WasCommitted = true;
			if (Connection.State != ConnectionState.Closed)
			{
				Connection.Close();
			}
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

            }else{
                ExecuteNonQuery(sql, Connection, Transaction);
			}
		}

        private void ExecuteNonQuery(string sql, SqlConnection connection,SqlTransaction transaction)
        {
            using (var command = new SqlCommand(sql, Connection, Transaction))
            {
                try
                {
                    command.CommandTimeout = Options.Timeout;
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    using (StringWriter message = new StringWriter())
                    {
                        message.WriteLine("An error occured executing the following sql:");
                        message.WriteLine(sql);
                        message.WriteLine("The error was {0}", ex.Message);

                        throw new Exception(message.ToString(), ex);
                    }
                }
            }
        }

        private void ExecuteBatchNonQuery(string sql, SqlConnection conn)
        {
             sql += "\nGO";   // make sure last batch is executed.
            string sqlBatch = string.Empty;

            using (var command = new SqlCommand(string.Empty, Connection, Transaction))
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
                catch (Exception ex)
                {
                    using (StringWriter message = new StringWriter())
                    {
                        message.WriteLine("An error occured executing the following sql:");
                        message.WriteLine(sql);
                        message.WriteLine("The error was {0}", ex.Message);

                        throw new Exception(message.ToString(), ex);
                    }
                }
            }
        }


		public override void Process(PerformDBOperationExpression expression)
		{
			if (Connection.State != ConnectionState.Open) Connection.Open();

			if (expression.Operation != null)
				expression.Operation(Connection, Transaction);
		}

        protected string FormatSqlEscape(string sql)
        {
            return sql.Replace("'", "''");
        }
	}
}
