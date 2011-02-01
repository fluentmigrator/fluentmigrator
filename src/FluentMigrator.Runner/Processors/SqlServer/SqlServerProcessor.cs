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
using System.Collections.Generic;
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

		public override bool SchemaExists(string schemaName)
		{
            return Exists("SELECT * FROM SYS.SCHEMAS WHERE NAME = '{0}'", FormatSqlEscape(schemaName));
		}

		public override bool TableExists(string tableName)
		{
            return Exists("SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}'", FormatSqlEscape(tableName));
		}

		public override bool ColumnExists(string tableName, string columnName)
		{
            return Exists("SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{0}' AND COLUMN_NAME = '{1}'", FormatSqlEscape(tableName), FormatSqlEscape(columnName));
		}

		public override bool ConstraintExists(string tableName, string constraintName)
		{
            return Exists("SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_CATALOG = DB_NAME() AND TABLE_NAME = '{0}' AND CONSTRAINT_NAME = '{1}'", FormatSqlEscape(tableName), FormatSqlEscape(constraintName));
		}

        public override bool IndexExists(string tableName, string indexName)
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

		public override DataSet ReadTableData(string tableName)
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

        public override List<FluentMigrator.Model.TableDefinition> ReadDbSchema() {
            throw new NotImplementedException();
        }

        protected virtual ICollection<FluentMigrator.Model.TableDefinition> ReadTables() {
            /*
             * --get columns for a given table; still needs to determined if a columns IsUnique
                SELECT OBJECT_SCHEMA_NAME(t.[object_id],DB_ID()) AS [Schema], t.name AS [Table], 
                c.[Name] AS ColumnName,
                t.object_id AS [TableID],
                c.column_id AS [ColumnID],
                def.definition AS [DefaultValue],
                c.[system_type_id] AS [TypeID],
                c.[user_type_id] AS [UserTypeID],
                c.[max_length] AS [Length],
                c.[precision] AS [Precision],
                c.[scale] AS [Scale],
                c.[is_identity] AS [IsIdentity],
                c.[is_nullable] AS [IsNullable],
                CASE WHEN EXISTS(SELECT 1 FROM sys.foreign_key_columns fkc WHERE t.object_id = fkc.parent_object_id AND c.column_id = fkc.parent_column_id) THEN 1 ELSE 0 END AS IsForiegnKey,
                CASE WHEN EXISTS(select 1 from sys.index_columns ic WHERE t.object_id = ic.object_id AND c.column_id = ic.column_id) THEN 1 ELSE 0 END AS IsIndexed 
                --,CASE WHEN EXISTS(select 1 from sys.key_constraints kc where [type] = 'UQ' AND c.object_id = kc.parent_object_id) THEN 1 ELSE 0 END AS IsUniqueKey
                FROM sys.all_columns c
                JOIN sys.tables t ON c.object_id = t.object_id AND t.type = 'u'
                LEFT JOIN sys.default_constraints def ON c.default_object_id = def.object_id
                ORDER BY t.name, c.name
             */
            throw new NotImplementedException();
        }

        protected virtual ICollection<FluentMigrator.Model.ColumnDefinition> ReadIndexes() 
        {
            /*
             * SELECT OBJECT_SCHEMA_NAME(T.[object_id],DB_ID()) AS [Schema],  
              T.[name] AS [table_name], I.[name] AS [index_name], AC.[name] AS [column_name],  
              I.[type_desc], I.[is_unique], I.[data_space_id], I.[ignore_dup_key], I.[is_primary_key], 
              I.[is_unique_constraint], I.[fill_factor],    I.[is_padded], I.[is_disabled], I.[is_hypothetical], 
              I.[allow_row_locks], I.[allow_page_locks], IC.[is_descending_key], IC.[is_included_column] 
            FROM sys.[tables] AS T  
              INNER JOIN sys.[indexes] I ON T.[object_id] = I.[object_id]  
              INNER JOIN sys.[index_columns] IC ON I.[object_id] = IC.[object_id] 
              INNER JOIN sys.[all_columns] AC ON T.[object_id] = AC.[object_id] AND IC.[column_id] = AC.[column_id] 
            WHERE T.[is_ms_shipped] = 0 AND I.[type_desc] <> 'HEAP' 
            ORDER BY T.[name], I.[index_id], IC.[key_ordinal]
             */
            throw new NotImplementedException();
        }

        protected virtual ICollection<FluentMigrator.Model.ForeignKeyDefinition> ReadForeignKeys() 
        {
            /*
             * SELECT C.CONSTRAINT_NAME AS Constraint_Name,
	                FK.CONSTRAINT_SCHEMA AS ForeignTableSchema,
	                FK.TABLE_NAME AS FK_Table,
	                CU.COLUMN_NAME AS FK_Column,
	                PK.CONSTRAINT_SCHEMA as PrimaryTableSchema,
	                PK.TABLE_NAME AS PK_Table,
	                PT.COLUMN_NAME AS PK_Column
                FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS C
                INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS FK ON C.CONSTRAINT_NAME = FK.CONSTRAINT_NAME
                INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS PK ON C.UNIQUE_CONSTRAINT_NAME = PK.CONSTRAINT_NAME
                INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE CU ON C.CONSTRAINT_NAME = CU.CONSTRAINT_NAME
                INNER JOIN (
                SELECT i1.TABLE_NAME, i2.COLUMN_NAME
                FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS i1
                INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE i2 ON i1.CONSTRAINT_NAME = i2.CONSTRAINT_NAME
                WHERE i1.CONSTRAINT_TYPE = 'PRIMARY KEY'
                ) PT ON PT.TABLE_NAME = PK.TABLE_NAME
                ORDER BY Constraint_Name
             */
            throw new NotImplementedException();
        }
	}
}
