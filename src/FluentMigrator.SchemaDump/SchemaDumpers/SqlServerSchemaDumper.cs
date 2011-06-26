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
using System.Linq;
using FluentMigrator.Builders.Execute;
using FluentMigrator.Model;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Processors.SqlServer;

namespace FluentMigrator.SchemaDump.SchemaDumpers
{
	public class SqlServerSchemaDumper : ISchemaDumper
	{
        public virtual IAnnouncer Announcer { get; set; }
        public SqlServerProcessor Processor { get; set; }
		public bool WasCommitted { get; private set; }

        public SqlServerSchemaDumper(SqlServerProcessor processor, IAnnouncer announcer)
		{
            this.Announcer = announcer;
            this.Processor = processor;
		}		

		public virtual void Execute(string template, params object[] args)
		{
            Processor.Execute(template, args);
		}

        public virtual bool Exists(string template, params object[] args)
		{
            return Processor.Exists(template, args);
		}

        public virtual DataSet ReadTableData(string tableName)
		{
            return Processor.Read("SELECT * FROM [{0}]", tableName);
		}

        public virtual DataSet ReadTableData(string schemaName, string tableName)
        {
           return Processor.Read("SELECT * FROM [{0}].[{1}]", schemaName,tableName);
        }

        public virtual DataSet Read(string template, params object[] args)
		{
            return Processor.Read(template, args);
		}        

        public virtual void Process(PerformDBOperationExpression expression)
		{
            Processor.Process(expression);
		}

        protected string FormatSqlEscape(string sql)
        {
            return sql.Replace("'", "''");
        }

        public virtual IList<TableDefinition> ReadDbSchema() {
           Announcer.Say("Reading table schema");
            IList<TableDefinition> tables = ReadTables();
            foreach(TableDefinition table in tables)
            {
               Announcer.Say(string.Format("Reading indexes and foreign keys for {0}.{1}", table.SchemaName, table.Name));
                table.Indexes = ReadIndexes(table.SchemaName, table.Name);
                table.ForeignKeys = ReadForeignKeys(table.SchemaName, table.Name);
            }

            return tables as IList<TableDefinition>;
        }



        public virtual IList<ViewDefinition> ReadViewSchema()
        {
           Announcer.Say("Reading view schema");
           return ReadViews();
        }

        public virtual IList<TableDefinition> ReadTables() {
            string query = @"SELECT OBJECT_SCHEMA_NAME(t.[object_id],DB_ID()) AS [Schema], t.name AS [Table], 
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
                CASE WHEN EXISTS(SELECT 1 FROM sys.foreign_key_columns fkc WHERE t.object_id = fkc.parent_object_id AND c.column_id = fkc.parent_column_id) THEN 1 ELSE 0 END AS IsForeignKey,
                CASE WHEN EXISTS(select 1 from sys.index_columns ic WHERE t.object_id = ic.object_id AND c.column_id = ic.column_id) THEN 1 ELSE 0 END AS IsIndexed 
                ,CASE WHEN kcu.CONSTRAINT_NAME IS NOT NULL THEN 1 ELSE 0 END AS IsPrimaryKey
                , CASE WHEN EXISTS(select stc.CONSTRAINT_NAME, skcu.TABLE_NAME, skcu.COLUMN_NAME from INFORMATION_SCHEMA.TABLE_CONSTRAINTS stc
                JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE skcu ON skcu.CONSTRAINT_NAME = stc.CONSTRAINT_NAME WHERE stc.CONSTRAINT_TYPE = 'UNIQUE'
                AND skcu.TABLE_NAME = t.name AND skcu.COLUMN_NAME = c.name) THEN 1 ELSE 0 END AS IsUnique
                ,pk.name AS PrimaryKeyName
                FROM sys.all_columns c
                JOIN sys.tables t ON c.object_id = t.object_id AND t.type = 'u'
                LEFT JOIN sys.default_constraints def ON c.default_object_id = def.object_id
                LEFT JOIN sys.key_constraints pk ON t.object_id = pk.parent_object_id AND pk.type = 'PK'
                LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu ON t.name = kcu.TABLE_NAME AND c.name = kcu.COLUMN_NAME AND pk.name = kcu.CONSTRAINT_NAME
                ORDER BY t.name, c.column_id";
            DataSet ds = Read(query);
            DataTable dt = ds.Tables[0];
            IList<TableDefinition> tables = new List<TableDefinition>();

            foreach (DataRow dr in dt.Rows) {
                List<TableDefinition> matches = (from t in tables
                            where t.Name == dr["Table"].ToString()
                            && t.SchemaName == dr["Schema"].ToString()
                            select t).ToList();

                TableDefinition tableDef = null;
                if (matches.Count > 0) tableDef = matches[0];

                // create the table if not found
                if (tableDef == null) {
                    tableDef = new TableDefinition()
                    {
                        Name = dr["Table"].ToString(),
                        SchemaName = dr["Schema"].ToString()
                    };
                    tables.Add(tableDef);
                }
                //find the column
                List<ColumnDefinition> cmatches = (from c in tableDef.Columns
                                           where c.Name == dr["ColumnName"].ToString()
                                           select c).ToList();
                ColumnDefinition colDef = null;
                if (cmatches.Count > 0) colDef = cmatches[0];

                if (colDef == null) {
                    //need to create and add the column
                    tableDef.Columns.Add( new ColumnDefinition() { 
                        Name = dr["ColumnName"].ToString(),
                        CustomType = "", //TODO: set this property
                        DefaultValue = dr.IsNull("DefaultValue") ? "" : dr["DefaultValue"].ToString(),
                        IsForeignKey = AsBoolean(dr, "IsForeignKey"),
                        IsIdentity = AsBoolean(dr, "IsIdentity"),
                        IsIndexed = AsBoolean(dr, "IsIndexed"),
                        IsNullable = AsBoolean(dr, "IsNullable"),
                        IsPrimaryKey = AsBoolean(dr, "IsPrimaryKey"),
                        IsUnique = AsBoolean(dr, "IsUnique"),
                        Precision = int.Parse(dr["Precision"].ToString()),
                        Scale = int.Parse(dr["Scale"].ToString()),
                        PrimaryKeyName = dr.IsNull("PrimaryKeyName") ? "" : dr["PrimaryKeyName"].ToString(), 
                        Size = GetDbSize(int.Parse(dr["TypeID"].ToString()), int.Parse(dr["Length"].ToString())),
                        TableName = dr["Table"].ToString(),
                        Type = GetDbType(int.Parse(dr["TypeID"].ToString())) //TODO: set this property
                    });
                }
            }

            return tables;
        }

	   private int GetDbSize(int databaseType, int databaseLength)
	   {
         // Source http://msdn.microsoft.com/en-us/library/ms187993.aspx (text,ntext,image)
         // Source http://msdn.microsoft.com/en-us/library/ms176089.aspx (char, varchar)
         // Source http://msdn.microsoft.com/en-us/library/ms186939.aspx (nchar, nvarchar)
	      switch ( databaseType)
	      {
            case SqlTypes.NChar:
	            return databaseLength/2;
	         case SqlTypes.Image:
            case SqlTypes.Text:
	            return int.MaxValue;
            case SqlTypes.NText:
	            return int.MaxValue/2;
            case SqlTypes.Varchar:
            case SqlTypes.Varbinary:
               return databaseLength == -1 ? int.MaxValue : databaseLength;
            case SqlTypes.NVarchar:
               //TODO Review to ensure that we do not loose precison when migrating UNICODE characters
               return databaseLength == -1 ? int.MaxValue : databaseLength / 2;
            default:
	            return databaseLength;
	      }
	   }

	   public virtual IList<ViewDefinition> ReadViews()
        {
           string query = @"select  OBJECT_SCHEMA_NAME(v.[object_id],DB_ID()) AS [Schema], v.name AS [View], sc.text As [Sql]
FROM sys.views v
INNER JOIN syscomments sc ON v.[object_id] = sc.[id]
ORDER BY v.name, sc.colid
";
           var ds = Read(query);
           var dt = ds.Tables[0];
           IList<ViewDefinition> views = new List<ViewDefinition>();

           foreach (DataRow dr in dt.Rows)
           {
              var matches = (from t in views
                                               where t.Name == dr["View"].ToString()
                                               && t.SchemaName == dr["Schema"].ToString()
                                               select t).ToList();

              ViewDefinition viewDefinition = null;
              if (matches.Count > 0) viewDefinition = matches[0];

              // create the view if not found
              if (viewDefinition == null)
              {
                 viewDefinition = new ViewDefinition()
                 {
                    Name = dr["View"].ToString(),
                    SchemaName = dr["Schema"].ToString()
                 };
                 viewDefinition.CreateViewSql = dr["Sql"].ToString();
                 views.Add(viewDefinition);
              }
              else
              {
                 // Append the sql as the create sql statement may span more than one row
                 viewDefinition.CreateViewSql += dr["Sql"];
              }
              
           }

           return views;
        }

      public bool AsBoolean(DataRow dr, string column)
      {
         if ( dr.Table.Columns[column].DataType.Equals(typeof(bool)) )
         {
            return (bool) dr[column];
         }
         return dr[column].ToString() == "1";
      }

	   protected virtual DbType GetDbType(int typeNum)
        {
           //http://www.sqlservercurry.com/2008/06/find-all-columns-with-varchar-and.html
            switch (typeNum)
            {
                case SqlTypes.Image: // 'byte[]'
                    return DbType.Binary;
                case SqlTypes.Text: // 'string'
                    return DbType.AnsiString;
                case SqlTypes.Uniqueidentifier: //UniqueIdentifier -> 'System.Guid'
                    return DbType.Guid;
                case SqlTypes.Tinyint: // 'byte'
                    return DbType.Byte;
                case SqlTypes.Smallint: // 'short'
                    return DbType.Int16;
                case SqlTypes.Int: // 'int'
                    return DbType.Int32;
                case SqlTypes.Smalldatetime: // 'System.DateTime'
                    return DbType.Date;
                case SqlTypes.Real: // 'float'
                    return DbType.Int64;
                case SqlTypes.Money: // 'decimal'
                    return DbType.Decimal;
                case SqlTypes.Datetime: // 'System.DateTime'
                    return DbType.DateTime;
                case SqlTypes.Float: // 'double'
                    return DbType.Double;
                case SqlTypes.SqlVariant: // 'object'
                    return DbType.Object;
                case SqlTypes.NText: // 'string'
                    return DbType.String;
                case SqlTypes.Bit: //bit -> 'bool'
                    return DbType.Boolean;
                case SqlTypes.Decimal: // 'decimal'
                    return DbType.Decimal;
                case SqlTypes.Numeric: // 'decimal'
                    return DbType.Decimal;
                case SqlTypes.Smallmoney: // 'decimal'
                    return DbType.Decimal;
                case SqlTypes.Bigint: // 'long'
                    return DbType.Int64;
                case SqlTypes.Varbinary: // 'byte[]'
                    return DbType.Binary;
                case SqlTypes.Varchar: // 'string'
                    return DbType.AnsiString;
                case SqlTypes.Binary: // 'byte[]'
                    return DbType.Binary;
                case SqlTypes.Char: //'string'
                    return DbType.AnsiStringFixedLength;
                case SqlTypes.Timestamp: //'long'
                    return DbType.Int64;
                case SqlTypes.NChar: //'string'
                    return DbType.StringFixedLength;
                case SqlTypes.NVarchar: //'string'
                case SqlTypes.Xml: //'string'
                default:
                    return DbType.String;
            }
        }

        protected virtual IList<IndexDefinition> ReadIndexes(string schemaName, string tableName) 
        {
            string query = @"SELECT OBJECT_SCHEMA_NAME(T.[object_id],DB_ID()) AS [Schema],  
              T.[name] AS [table_name], I.[name] AS [index_name], AC.[name] AS [column_name],  
              I.[type_desc], I.[is_unique], I.[data_space_id], I.[ignore_dup_key], I.[is_primary_key], 
              I.[is_unique_constraint], I.[fill_factor],    I.[is_padded], I.[is_disabled], I.[is_hypothetical], 
              I.[allow_row_locks], I.[allow_page_locks], IC.[is_descending_key], IC.[is_included_column] 
            FROM sys.[tables] AS T  
              INNER JOIN sys.[indexes] I ON T.[object_id] = I.[object_id]  
              INNER JOIN sys.[index_columns] IC ON I.[object_id] = IC.[object_id] AND I.index_id = IC.index_id
              INNER JOIN sys.[all_columns] AC ON T.[object_id] = AC.[object_id] AND IC.[column_id] = AC.[column_id] 
            WHERE T.[is_ms_shipped] = 0 AND I.[type_desc] <> 'HEAP' 
            AND T.object_id = OBJECT_ID('[{0}].[{1}]')
            ORDER BY T.[name], I.[index_id], IC.[key_ordinal]";
            DataSet ds = Read(query, schemaName, tableName);
            DataTable dt = ds.Tables[0];
            IList<IndexDefinition> indexes = new List<IndexDefinition>();

            foreach (DataRow dr in dt.Rows) 
            {
                List<IndexDefinition> matches = (from i in indexes
                                                 where i.Name == dr["index_name"].ToString()
                                                 && i.SchemaName == dr["Schema"].ToString()
                                                 select i).ToList();

                IndexDefinition iDef = null;
                if (matches.Count > 0) iDef = matches[0];

                // create the index if not found
                if (iDef == null) {
                    iDef = new IndexDefinition()
                    {
                        Name = dr["index_name"].ToString(),
                        SchemaName = dr["Schema"].ToString(),
                        IsClustered = dr["type_desc"].ToString()=="CLUSTERED",
                        IsUnique = dr["is_unique"].ToString() == "1",
                        TableName = dr["table_name"].ToString()
                    };
                    indexes.Add(iDef);
                }

                ICollection<IndexColumnDefinition> ms;
                // columns
                ms = (from m in iDef.Columns
                      where m.Name == dr["column_name"].ToString()
                      select m).ToList();
                if (ms.Count == 0) 
                {
                    iDef.Columns.Add(new IndexColumnDefinition() {
                        Name = dr["column_name"].ToString(),
                        Direction = dr["is_descending_key"].ToString()=="1" ? Direction.Descending : Direction.Ascending
                    });                    
                }
            }

            return indexes;
        }

        protected virtual IList<ForeignKeyDefinition> ReadForeignKeys(string schemaName, string tableName)
        {
            // Source http://blog.sqlauthority.com/2006/11/01/sql-server-query-to-display-foreign-key-relationships-and-name-of-the-constraint-for-each-table-in-database/
            string query =
                @"SELECT
C.CONSTRAINT_NAME As Constraint_Name,
FK.CONSTRAINT_SCHEMA AS ForeignTableSchema,
FK.TABLE_NAME As FK_Table,
CU.COLUMN_NAME As FK_Column ,
PK.CONSTRAINT_SCHEMA as PrimaryTableSchema,
PK.TABLE_NAME As PK_Table,
PT.COLUMN_NAME As PK_Column
FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS As C
INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS As FK ON C.CONSTRAINT_NAME = FK.CONSTRAINT_NAME
INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS As PK ON C.UNIQUE_CONSTRAINT_NAME = PK.CONSTRAINT_NAME
INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE As CU ON C.CONSTRAINT_NAME = CU.CONSTRAINT_NAME
INNER JOIN (
SELECT i1.TABLE_NAME, i2.COLUMN_NAME
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS i1
INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE i2 ON i1.CONSTRAINT_NAME = i2.CONSTRAINT_NAME
WHERE i1.CONSTRAINT_TYPE = 'PRIMARY KEY'
) As PT ON PT.TABLE_NAME = PK.TABLE_NAME
WHERE FK.TABLE_NAME = '{1}'
AND FK.CONSTRAINT_SCHEMA = '{0}'
ORDER BY 1,2,3,4";


            DataSet ds = Read(query, schemaName, tableName);
            DataTable dt = ds.Tables[0];
            IList<ForeignKeyDefinition> keys = new List<ForeignKeyDefinition>();

            foreach (DataRow dr in dt.Rows) {
                List<ForeignKeyDefinition> matches = (from i in keys
                                                      where i.Name == dr["Constraint_Name"].ToString()
                                                      select i).ToList();

                ForeignKeyDefinition d = null;
                if (matches.Count > 0) d = matches[0];

                // create the table if not found
                if (d == null) {
                    d = new ForeignKeyDefinition()
                    {
                        Name = dr["Constraint_Name"].ToString(),
                        ForeignTableSchema = dr["ForeignTableSchema"].ToString(),
                        ForeignTable = dr["FK_Table"].ToString(),
                        PrimaryTable = dr["PK_Table"].ToString(),
                        PrimaryTableSchema = dr["PrimaryTableSchema"].ToString()
                    };
                    keys.Add(d);
                }

                ICollection<string> ms;
                // Foreign Columns
                ms = (from m in d.ForeignColumns
                           where m == dr["FK_Table"].ToString()
                           select m).ToList();
                if (ms.Count == 0) d.ForeignColumns.Add(dr["FK_Column"].ToString());

                // Primary Columns
                ms = (from m in d.PrimaryColumns
                           where m == dr["PK_Table"].ToString()
                           select m).ToList();
                if (ms.Count == 0) d.PrimaryColumns.Add(dr["PK_Column"].ToString());
            }

            return keys;
        }

	   public IList<ProcedureDefinition> ReadProcedures()
	   {
	      var procedures = new List<ProcedureDefinition>();

         // NOTE: Query for SQL 2005 for greater ... would need to be refactored to support SQL Server 2000
         var procedureQuery = Processor.Read("SELECT schema_Name(so.schema_id)  as  SchemaName, o.name, c.text FROM syscomments c, sysobjects o, sys.objects so WHERE c.id = o.id AND xtype = 'P' AND so.object_id = o.id ORDER BY colid");

         foreach ( DataRow dr in procedureQuery.Tables[0].Rows )
         {
            var matches = (from t in procedures
                                             where t.Name == dr["name"].ToString()
                                             select t).ToList();

            ProcedureDefinition procedureDef = null;
            if (matches.Count > 0) procedureDef = matches[0];

            // create the procedure if not found
            if (procedureDef == null)
            {
               procedureDef = new ProcedureDefinition()
               {
                  SchemaName = dr["SchemaName"].ToString(),
                  Name = dr["Name"].ToString(),
                  Sql = dr["Text"].ToString()
               };
               procedures.Add(procedureDef);
            }
            else
            {
               procedureDef.Sql += dr["Text"].ToString();
            }
         }
         
	      return procedures;
	   }

      public IList<FunctionDefinition> ReadFunctions()
      {
         var procedures = new List<FunctionDefinition>();

         // NOTE: Query for SQL 2005 for greater ... would need to be refactored to support SQL Server 2000
         var query = Processor.Read("SELECT schema_Name(so.schema_id)  as  SchemaName, o.name, c.text FROM syscomments c, sysobjects o, sys.objects so WHERE c.id = o.id AND xtype = 'FN' AND so.object_id = o.id ORDER BY colid");

         foreach (DataRow dr in query.Tables[0].Rows)
         {
            var matches = (from t in procedures
                           where t.Name == dr["name"].ToString()
                           select t).ToList();

            FunctionDefinition functionDefinition = null;
            if (matches.Count > 0) functionDefinition = matches[0];

            // create the procedure if not found
            if (functionDefinition == null)
            {
               functionDefinition = new FunctionDefinition()
               {
                  SchemaName = dr["SchemaName"].ToString(),
                  Name = dr["Name"].ToString(),
                  Sql = dr["Text"].ToString()
               };
               procedures.Add(functionDefinition);
            }
            else
            {
               functionDefinition.Sql += dr["Text"].ToString();
            }
         }

         return procedures;
      }
	}

   public class SqlTypes
   {
      public const int Image = 34;
      public const int Text = 35;
      public const int Uniqueidentifier = 36;
      public const int Tinyint = 48;
      public const int Smallint = 52;
      public const int Int = 56;
      public const int Smalldatetime = 58;
      public const int Real = 59;
      public const int Money = 60;
      public const int Datetime = 61;
      public const int Float = 62;
      public const int SqlVariant = 98;
      public const int NText = 99;
      public const int Bit = 104;
      public const int Decimal = 106;
      public const int Numeric = 108;
      public const int Smallmoney = 122;
      public const int Bigint = 127;
      public const int Varbinary = 165;
      public const int Varchar = 167;
      public const int Binary = 173;
      public const int Char = 175;
      public const int Timestamp = 189;
      public const int NVarchar = 231;
      public const int NChar = 239;
      public const int Xml = 241;
      public const int Sysname = 256;
   }
}
