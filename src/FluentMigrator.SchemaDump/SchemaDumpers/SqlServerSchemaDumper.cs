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

using System.Collections.Generic;
using System.Data;
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

        public SqlServerSchemaDumper(SqlServerProcessor processor, IAnnouncer announcer)
        {
            Announcer = announcer;
            Processor = processor;
        }

        public virtual void Execute(string template, params object[] args)
        {
            Processor.Execute(template, args);
        }

        public virtual bool Exists(string template, params object[] args)
        {
            return Processor.Exists(template, args);
        }

        public virtual IDataReader ReadTableData(string tableName)
        {
            return Processor.Read("SELECT * FROM [{0}]", tableName);
        }

        public virtual IDataReader Read(string template, params object[] args)
        {
            return Processor.Read(template, args);
        }

        public virtual void Process(PerformDBOperationExpression expression)
        {
            Processor.Process(expression);
        }

        public virtual IList<TableDefinition> ReadDbSchema()
        {
            IList<TableDefinition> tables = ReadTables();
            foreach (TableDefinition table in tables)
            {
                table.Indexes = ReadIndexes(table.SchemaName, table.Name);
                table.ForeignKeys = ReadForeignKeys(table.SchemaName, table.Name);
            }

            return tables;
        }

        protected virtual IList<TableDefinition> ReadTables()
        {
            const string query = @"SELECT OBJECT_SCHEMA_NAME(t.[object_id],DB_ID()) AS [Schema], t.name AS [Table], 
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
            IDataReader ds = Read(query);
            IList<TableDefinition> tables = new List<TableDefinition>();

            while (ds.Read())
            {
                List<TableDefinition> matches = (from t in tables
                                                 where t.Name == ds["Table"].ToString()
                                                 && t.SchemaName == ds["Schema"].ToString()
                                                 select t).ToList();

                TableDefinition tableDef = null;
                if (matches.Count > 0) tableDef = matches[0];

                // create the table if not found
                if (tableDef == null)
                {
                    tableDef = new TableDefinition
                                   {
                        Name = ds["Table"].ToString(),
                        SchemaName = ds["Schema"].ToString()
                    };
                    tables.Add(tableDef);
                }
                //find the column
                List<ColumnDefinition> cmatches = (from c in tableDef.Columns
                                                   where c.Name == ds["ColumnName"].ToString()
                                                   select c).ToList();
                ColumnDefinition colDef = null;
                if (cmatches.Count > 0) colDef = cmatches[0];

                if (colDef == null)
                {
                    //need to create and add the column
                    tableDef.Columns.Add(new ColumnDefinition
                                             {
                        Name = ds["ColumnName"].ToString(),
                        CustomType = "", //TODO: set this property
                        DefaultValue = ds.IsDBNull(ds.GetOrdinal("DefaultValue")) ? "" : ds["DefaultValue"].ToString(),
                        IsForeignKey = ds["IsForeignKey"].ToString() == "1",
                        IsIdentity = ds["IsIdentity"].ToString() == "True",
                        IsIndexed = ds["IsIndexed"].ToString() == "1",
                        IsNullable = ds["IsNullable"].ToString() == "True",
                        IsPrimaryKey = ds["IsPrimaryKey"].ToString() == "1",
                        IsUnique = ds["IsUnique"].ToString() == "1",
                        Precision = int.Parse(ds["Precision"].ToString()),
                        PrimaryKeyName = ds.IsDBNull(ds.GetOrdinal("PrimaryKeyName")) ? "" : ds["PrimaryKeyName"].ToString(),
                        Size = int.Parse(ds["Length"].ToString()),
                        TableName = ds["Table"].ToString(),
                        Type = GetDbType(int.Parse(ds["TypeID"].ToString())), //TODO: set this property
                        ModificationType = ColumnModificationType.Create
                    });
                }
            }

            return tables;
        }

        protected virtual DbType GetDbType(int typeNum)
        {
            var types = new Dictionary<int, DbType>()
            {
                {34, DbType.Binary},
                {35, DbType.AnsiString},
                {36, DbType.Guid},
                {40, DbType.Date},
                {41, DbType.Time},
                {42, DbType.DateTime2},
                {43, DbType.DateTimeOffset},
                {48, DbType.Byte},
                {52, DbType.Int16},
                {56, DbType.Int32},
                //{58, DbType.}, //smalldatetime
                {59, DbType.Single},
                {60, DbType.Currency},
                {61, DbType.DateTime},
                {62, DbType.Double},
                //{98, DbType.}, //sql_variant
                {99, DbType.String},
                {104, DbType.Boolean},
                {106, DbType.Decimal},
                //{108, DbType.}, //numeric
                //{122, DbType.}, //smallmoney
                {127, DbType.Int64},
                //{240, DbType.}, //hierarchyid
                //{240, DbType.}, //geometry
                //{240, DbType.}, //geography
                {165, DbType.Binary},
                {167, DbType.AnsiString},
                {173, DbType.Binary},
                {175, DbType.AnsiStringFixedLength},
                //{189, DbType.}, //Timestamp
                {231, DbType.String},
                {239, DbType.StringFixedLength},
                {241, DbType.Xml}
                //{231, DbType.} //Sysname
            };

            DbType value;
            if (types.TryGetValue(typeNum, out value))
            {
                return value;
            }
            else
            {
                throw new KeyNotFoundException(typeNum + " was not found!");
            }
        }

        protected virtual IList<IndexDefinition> ReadIndexes(string schemaName, string tableName)
        {
            const string query = @"SELECT OBJECT_SCHEMA_NAME(T.[object_id],DB_ID()) AS [Schema],  
              T.[name] AS [table_name], I.[name] AS [index_name], AC.[name] AS [column_name],  
              I.[type_desc], I.[is_unique], I.[data_space_id], I.[ignore_dup_key], I.[is_primary_key], 
              I.[is_unique_constraint], I.[fill_factor],    I.[is_padded], I.[is_disabled], I.[is_hypothetical], 
              I.[allow_row_locks], I.[allow_page_locks], IC.[is_descending_key], IC.[is_included_column] 
            FROM sys.[tables] AS T  
              INNER JOIN sys.[indexes] I ON T.[object_id] = I.[object_id]  
              INNER JOIN sys.[index_columns] IC ON I.[object_id] = IC.[object_id] 
              INNER JOIN sys.[all_columns] AC ON T.[object_id] = AC.[object_id] AND IC.[column_id] = AC.[column_id] 
            WHERE T.[is_ms_shipped] = 0 AND I.[type_desc] <> 'HEAP' 
            AND T.object_id = OBJECT_ID('[{0}].[{1}]')
            ORDER BY T.[name], I.[index_id], IC.[key_ordinal]";
            IDataReader ds = Read(query, schemaName, tableName);
            IList<IndexDefinition> indexes = new List<IndexDefinition>();

            while (ds.Read()) {
                {
                    List<IndexDefinition> matches = (from i in indexes
                                                     where i.Name == ds["index_name"].ToString()
                                                     && i.SchemaName == ds["Schema"].ToString()
                                                     select i).ToList();

                    IndexDefinition iDef = null;
                    if (matches.Count > 0)
                        iDef = matches[0];

                    // create the table if not found
                    if (iDef == null) {
                        iDef = new IndexDefinition
                        {
                            Name = ds["index_name"].ToString(),
                            SchemaName = ds["Schema"].ToString(),
                            IsClustered = ds["type_desc"].ToString() == "CLUSTERED",
                            IsUnique = ds["is_unique"].ToString() == "1",
                            TableName = ds["table_name"].ToString()
                        };
                        indexes.Add(iDef);
                    }

                    // columns
                    ICollection<IndexColumnDefinition> ms = (from m in iDef.Columns
                                                             where m.Name == ds["column_name"].ToString()
                                                             select m).ToList();
                    if (ms.Count == 0) {
                        iDef.Columns.Add(new IndexColumnDefinition
                        {
                            Name = ds["column_name"].ToString(),
                            Direction = ds["is_descending_key"].ToString() == "1" ? Direction.Descending : Direction.Ascending
                        });
                    }
                }
            }

            return indexes;
        }

        protected virtual IList<ForeignKeyDefinition> ReadForeignKeys(string schemaName, string tableName)
        {
            const string query = @"SELECT C.CONSTRAINT_SCHEMA AS Contraint_Schema, 
                    C.CONSTRAINT_NAME AS Constraint_Name,
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
                WHERE PK.TABLE_NAME = '{1}'
                AND PK.CONSTRAINT_SCHEMA = '{0}'
                ORDER BY Constraint_Name";
            IDataReader ds = Read(query, schemaName, tableName);
            IList<ForeignKeyDefinition> keys = new List<ForeignKeyDefinition>();

            while (ds.Read())
            {
                List<ForeignKeyDefinition> matches = (from i in keys
                                                      where i.Name == ds["Constraint_Name"].ToString()
                                                      select i).ToList();

                ForeignKeyDefinition d = null;
                if (matches.Count > 0) d = matches[0];

                // create the table if not found
                if (d == null)
                {
                    d = new ForeignKeyDefinition
                            {
                        Name = ds["Constraint_Name"].ToString(),
                        ForeignTableSchema = ds["ForeignTableSchema"].ToString(),
                        ForeignTable = ds["FK_Table"].ToString(),
                        PrimaryTable = ds["PK_Table"].ToString(),
                        PrimaryTableSchema = ds["PrimaryTableSchema"].ToString()
                    };
                    keys.Add(d);
                }

                // Foreign Columns
                ICollection<string> ms = (from m in d.ForeignColumns
                                  where m == ds["FK_Table"].ToString()
                                  select m).ToList();
                if (ms.Count == 0) d.ForeignColumns.Add(ds["FK_Column"].ToString());

                // Primary Columns
                ms = (from m in d.PrimaryColumns
                      where m == ds["PK_Table"].ToString()
                      select m).ToList();
                if (ms.Count == 0) d.PrimaryColumns.Add(ds["PK_Column"].ToString());
            }

            return keys;
        }
    }
}
