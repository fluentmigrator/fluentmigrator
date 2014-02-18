#region Apache License
// 
// Copyright (c) 2014, Tony O'Hagan <tony@ohagan.name>
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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using FluentMigrator.Model;
using FluentMigrator.SchemaGen.Extensions;
using FluentMigrator.SchemaGen.Model;


namespace FluentMigrator.SchemaGen.SchemaReaders
{
    /// <summary>
    /// Reads a SQL Server 2008 database schema.  
    /// </summary>
    /// <remarks>
    /// Does not use SMO API.
    /// 
    /// <see cref="ScriptsInDependencyOrder"/>() is probably the only method that breaks for SQL Server 2005
    /// and at the time of writing this remark, it's not yet used.
    /// </remarks>
    public class SqlServerSchemaReader : IDbSchemaReader
    {
        private readonly IDbConnection connection;
        private readonly IOptions options;
        private readonly DbProviderFactory factory;
        private IDictionary<string, TableDefinitionExt> tables;

        //public IAnnouncer Announcer { get; set; }
        //public SqlServerProcessor Processor { get; set; }

        public SqlServerSchemaReader(IDbConnection connection, IOptions options)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (options == null) throw new ArgumentNullException("options");
            Debug.Assert(connection.State == ConnectionState.Open);
            this.connection = connection;
            this.options = options;
            this.factory = SqlClientFactory.Instance;
            //this.Announcer = announcer;
            //this.Processor = processor;
        }

        #region Mapping Tables
        private enum SqlServerType
        {
            Image = 34,
            Text = 35,
            UniqueIdentifier = 36,
            Date = 40,
            Time = 41,
            DateTime2 = 42,
            DateTimeOffset = 43,
            Tinyint = 48,
            SmallInt = 52,
            Int = 56,
            SmallDatetime = 58,
            Real = 59,
            Money = 60,
            DateTime = 61,
            Float = 62,
            SqlVariant = 98,
            Ntext = 99,
            Bit = 104,
            Decimal = 106,
            Numeric = 108,
            SmallMoney = 122,
            BigInt = 127,
            HierarchyId = 240, //Geometry = 240, Geography = 240,
            VarBinary = 165,
            VarChar = 167,
            Binary = 173,
            Char = 175,
            Timestamp = 189,
            NVarChar = 231,
            NChar = 239,
            Xml = 241,
            SysName = 231,
        }

        // from FluentMigrator.Runner.Generators.SqlServer2000TypeMap
        static IDictionary<SqlServerType, DbType> typeMap = new Dictionary<SqlServerType, DbType>()
            {
                {SqlServerType.Image, DbType.Binary},
                {SqlServerType.Text, DbType.AnsiString},
                {SqlServerType.UniqueIdentifier, DbType.Guid},
                {SqlServerType.Date, DbType.Date},
                {SqlServerType.Time, DbType.Time},
                {SqlServerType.DateTime2, DbType.DateTime2},
                {SqlServerType.DateTimeOffset, DbType.DateTimeOffset},
                {SqlServerType.Tinyint, DbType.Byte},
                {SqlServerType.SmallInt, DbType.Int16},
                {SqlServerType.Int, DbType.Int32},
                //{SqlServerType.SmallDatetime, DbType.},
                {SqlServerType.Real, DbType.Single},
                {SqlServerType.Money, DbType.Currency},
                {SqlServerType.DateTime, DbType.DateTime},
                {SqlServerType.Float, DbType.Double},
                //{SqlServerType.SqlVariant, DbType.}, 
                {SqlServerType.Ntext, DbType.String}, 
                {SqlServerType.Bit, DbType.Boolean},
                {SqlServerType.Decimal, DbType.Decimal},
                //{SqlServerType.Numeric, DbType.},
                //{SqlServerType.SmallMoney, DbType.},
                {SqlServerType.BigInt, DbType.Int64},
                //{240, DbType.}, //hierarchyid
                //{240, DbType.}, //geometry
                //{240, DbType.}, //geography
                {SqlServerType.VarBinary, DbType.Binary},
                {SqlServerType.VarChar, DbType.AnsiString},
                {SqlServerType.Binary, DbType.Binary},
                {SqlServerType.Char, DbType.AnsiStringFixedLength},
                //{SqlServerType.Timestamp, DbType.},
                {SqlServerType.NVarChar, DbType.String},
                {SqlServerType.NChar, DbType.StringFixedLength},
                {SqlServerType.Xml, DbType.Xml}
                //{SqlServerType.Sysname, DbType.} //Sysname
            };

        #endregion

        private int GetColumnSize(DataRow row)
        {
            // Reference: FluentMigrator.Runner.Generators.SqlServer2000TypeMap.SetupTypeMaps()

            int size = int.Parse(row["Length"].ToString());
            int typeNum = int.Parse(row["TypeID"].ToString());

            switch (typeNum)
            {
                case (int)SqlServerType.Image: 
                    size = DbTypeSizes.ImageCapacity;
                    break;

                case (int)SqlServerType.Text:  
                    size = DbTypeSizes.AnsiTextCapacity;
                    break;

                case (int)SqlServerType.Ntext: 
                    size = DbTypeSizes.UnicodeTextCapacity;
                    break;

                case (int)SqlServerType.NVarChar:   
                case (int)SqlServerType.NChar:
                    if (size != -1) size /= 2;  // Unicode string length is doubled
                    break;
            }

            return size;
        }

        private string GetColumnDefault(DataRow row)
        {
            string strDefault = null;
            if (!row.IsNull("DefaultValue"))
            {
                strDefault = row["DefaultValue"].ToString();
                // "((0))"  -> "0"
                if (strDefault.StartsWith("((") && strDefault.EndsWith("))"))
                {
                    strDefault = strDefault.Substring(2, strDefault.Length - 4);
                }
            }
            return strDefault;
        }

        protected virtual DbType GetColumnType(DataRow row)
        {
            int typeNum = int.Parse(row["TypeID"].ToString());

            DbType value;
            if (typeMap.TryGetValue((SqlServerType)typeNum, out value))
            {
                return value;
            }
            else
            {
                throw new KeyNotFoundException(typeNum + " was not found!");
            }
        }


        public DataSet Read(string template, params object[] args)
        {
            // TODO: Replace DataSet with a reader and return IEnumerable<DataRow>

            var ds = new DataSet();
            var sql = String.Format(template, args);
            //Announcer.Sql(sql);
            using (IDbCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = sql;
                IDbDataAdapter adapter = factory.CreateDataAdapter();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds);
                return ds;
            }
        }

        public virtual DataSet ReadTableData(string tableName)
        {
            return Read("SELECT * FROM [{0}]", tableName);
        }

        #region Table name filtering
        private Regex MapFilter(string filter)
        {
            string[] patterns = (from pat in filter.Split(';') select "^" + pat.Replace(".", "\\.").Replace("*", ".*") + "$").ToArray();
            return new Regex(String.Join("|", patterns));
        }

        protected IEnumerable<TableDefinitionExt> ApplyTableFilter(IEnumerable<TableDefinitionExt> tables)
        {
            if (options.IncludeTables != null)
            {
                var pattern = MapFilter(options.IncludeTables);
                tables = tables.Where(table => pattern.IsMatch(table.Name));
            }

            if (options.ExcludeTables != null)
            {
                var pattern = MapFilter(options.ExcludeTables);
                tables = tables.Where(table => !pattern.IsMatch(table.Name));
            }

            return tables;
        }

        protected IEnumerable<string> ApplyTableFilter(IEnumerable<string> tableNames)
        {
            if (options.IncludeTables != null)
            {
                var pattern = MapFilter(options.IncludeTables);
                tableNames = tableNames.Where(name => pattern.IsMatch(name));
            }

            if (options.ExcludeTables != null)
            {
                var pattern = MapFilter(options.ExcludeTables);
                tableNames = tableNames.Where(name => !pattern.IsMatch(name));
            }

            return tableNames;
        }
        #endregion

        public IDictionary<string, TableDefinitionExt> Tables
        {
            get
            {
                if (tables == null)
                {
                    Console.WriteLine("Loading table schema ...");
                    this.tables = GetTables().ToDictionary(table => table.Name);
                    // TODO: Added SchemaName to all dictionaries 
                    // this.tables = GetTables().ToDictionary(table => (table.SchemaName ?? "dbo") + "." + table.Name);
                }
                return tables;
            }
        }

        public IEnumerable<string> TableNames
        {
            get { return ApplyTableFilter(GetNameList("SELECT name FROM sys.tables WHERE type = 'U' ORDER BY name")); }
        }

        public IEnumerable<string> UserDefinedDataTypes
        {
            get { return GetNameList("SELECT name FROM sys.procedures WHERE LEFT(name, 3) NOT IN ('sp_', 'xp_', 'ms_', 'dt_') ORDER BY name"); }
        }

        public IEnumerable<string> UserDefinedFunctions
        {
            get { return GetNameList("SELECT specific_name AS name FROM information_schema.routines WHERE routine_type = 'FUNCTION' ORDER BY name"); }
        }

        public IEnumerable<string> StoredProcedures
        {
            get { return GetNameList("SELECT specific_name AS name FROM information_schema.routines WHERE routine_type = 'PROCEDURE' AND Left(specific_name, 3) NOT IN ('sp_', 'xp_', 'ms_', 'dt_') ORDER BY name"); }
        }

        public IEnumerable<string> Views
        {
            get { return GetNameList("SELECT table_name AS name FROM information_schema.views ORDER BY name"); }
        }

        private IEnumerable<string> GetNameList(string query)
        {
            using (DataSet ds = Read(query))
            using (DataTable dt = ds.Tables[0])
            {
                return (from row in dt.Rows.Cast<DataRow>() select row["name"].ToString()).ToList();
            }
        }

        public class ObjectOrdering
        {
            public int Ordinal { get; set; }
            public string SchemaName { get; set; }
            public string ObjectName { get; set; }
            public long ObjectId { get; set; }
        }

        /// <summary>
        /// Ordering used to create or drop tables and foreign keys.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, int> TablesInForeignKeyOrder(bool ascending)
        {
            // Computes a dependency depth (Ordinal).
            // Level 0 = Depends on NO table FKs.
            // Level 1 = Depends on Level 0 tables
            // Level 2 = Depends on Level 0 or 1 tables
            // Level N = Depends on Level 0 .. N-1 tables.

            #region query = Magic Query
            // http://stackoverflow.com/questions/352176/sqlserver-how-to-sort-table-names-ordered-by-their-foreign-key-dependency
            const string query = @"WITH TablesCTE(SchemaName, TableName, TableID, Ordinal) AS
                (
                    SELECT OBJECT_SCHEMA_NAME(so.object_id) AS SchemaName,
                        OBJECT_NAME(so.object_id) AS TableName,
                        so.object_id AS TableID,
                        0 AS Ordinal
                    FROM sys.objects AS so
                    WHERE so.type = 'U' AND so.is_ms_Shipped = 0

                    UNION ALL
                    SELECT OBJECT_SCHEMA_NAME(so.object_id) AS SchemaName,
                        OBJECT_NAME(so.object_id) AS TableName,
                        so.object_id AS TableID,
                        tt.Ordinal + 1 AS Ordinal
                    FROM sys.objects AS so
                    INNER JOIN sys.foreign_keys AS f
                        ON f.parent_object_id = so.object_id
                        AND f.parent_object_id != f.referenced_object_id
                    INNER JOIN TablesCTE AS tt ON f.referenced_object_id = tt.TableID
                    WHERE so.type = 'U' AND so.is_ms_Shipped = 0
                ) 
                SELECT DISTINCT t.Ordinal, t.SchemaName, t.TableName, t.TableID
                    FROM TablesCTE AS t
                    INNER JOIN
                        (
                            SELECT
                                itt.SchemaName AS SchemaName,
                                itt.TableName AS TableName,
                                itt.TableID AS TableID,
                                Max(itt.Ordinal) AS Ordinal
                            FROM TablesCTE AS itt
                            GROUP BY itt.SchemaName, itt.TableName, itt.TableID
                        ) AS tt
                        ON t.TableID = tt.TableID
                        AND t.Ordinal = tt.Ordinal
                ORDER BY t.Ordinal {0}, t.TableName {0}";

            #endregion

            using (DataSet ds = Read(query, ascending ? "ASC" : "DESC"))
            using (DataTable dt = ds.Tables[0])
            {
                return (from row in dt.Rows.Cast<DataRow>()
                        select new ObjectOrdering
                        {
                            Ordinal = int.Parse(row["Ordinal"].ToString()),
                            SchemaName = row["SchemaName"].ToString(),
                            ObjectName = row["TableName"].ToString(),
                            ObjectId = long.Parse(row["TableID"].ToString())
                        }).ToDictionary(to => to.ObjectName, to => to.Ordinal);
            }
        }

        /// <summary>
        /// Ordering of dependent Views, Stored Procedures and Functions.
        /// </summary>
        /// <param name="ascending"></param>
        /// <returns>Mapping from ObjectName -> Ordering. Lower numbered objects should be declared first.</returns>
        /// <remarks>
        /// Object's schema name is not (yet) included in the dictionary key.
        /// Uses sys.sql_expression_dependencies view that requires SQL Server 2008
        /// </remarks>
        public IDictionary<string, int> ScriptsInDependencyOrder(bool ascending)
        {
            const string query = @"WITH TablesCTE(ObjType, SchemaName, ObjectName, ObjectID, Ordinal) AS
                (
                    SELECT  so.type AS ObjType, OBJECT_SCHEMA_NAME(so.object_id) AS SchemaName, OBJECT_NAME(so.object_id) AS ObjectName, so.object_id AS ObjectID, 
                        0 AS Ordinal
                    FROM sys.objects AS so
                    WHERE so.type IN ( 'IF', 'V', 'P'  ) AND  so.is_ms_Shipped = 0
 
                    UNION ALL
 
                    SELECT so.type AS ObjType, OBJECT_SCHEMA_NAME(so.object_id) AS SchemaName, OBJECT_NAME(so.object_id) AS ObjectName, so.object_id AS ObjectID,
                        tt.Ordinal + 1 AS Ordinal
                    FROM sys.objects AS so
                    INNER JOIN sys.sql_expression_dependencies AS dep ON dep.referencing_id = so.object_id 
                    INNER JOIN TablesCTE AS tt ON dep.referenced_id = tt.ObjectID
                    WHERE so.type IN ( 'IF', 'V', 'P' ) AND so.is_ms_Shipped = 0
                )
 
                SELECT DISTINCT tt.Ordinal, t.ObjType, t.SchemaName, t.ObjectName, t.ObjectID
                    FROM TablesCTE AS t
                    INNER JOIN
                        (
                            SELECT itt.ObjType, itt.SchemaName AS SchemaName, itt.ObjectName, itt.ObjectID, 
                                   Max(itt.Ordinal) AS Ordinal
                            FROM TablesCTE AS itt
                            GROUP BY itt.ObjType, itt.SchemaName, itt.ObjectName, itt.ObjectID
                        ) AS tt
                        ON t.ObjectID = tt.ObjectID AND t.Ordinal = tt.Ordinal
                ORDER BY tt.Ordinal {0}, t.ObjType, t.SchemaName {0}, t.ObjectName {0}, t.ObjectID";

            using (DataSet ds = Read(query, ascending ? "ASC" : "DESC"))
            using (DataTable dt = ds.Tables[0])
            {
                return (from row in dt.Rows.Cast<DataRow>()
                        select new ObjectOrdering
                        {
                            Ordinal = int.Parse(row["Ordinal"].ToString()),
                            SchemaName = row["SchemaName"].ToString(),
                            ObjectName = row["ObjectName"].ToString(),
                            ObjectId = long.Parse(row["ObjectID"].ToString())
                        }).ToDictionary(to => to.ObjectName, to => to.Ordinal);
            }
        }

        private IEnumerable<ForeignKeyDefinitionExt> FindForeignKeysWithColumns(IEnumerable<ForeignKeyDefinitionExt> fks, IEnumerable<IndexColumnDefinition> cols)
        {
            string[] colNames = cols.Select(col => col.Name).ToArray();
            return fks.Where(fk => fk.ForeignColumns.SequenceEqual(colNames));
        }

        public IEnumerable<TableDefinitionExt> GetTables()
        {
            foreach (TableDefinitionExt table in ReadTableDefs())
            {
                table.Indexes = ReadIndexes(table.SchemaName, table.Name);
                table.ForeignKeys = ReadForeignKeys(table.SchemaName, table.Name);

                // transfer index properties to table columns
                foreach (IndexDefinitionExt index in table.Indexes)
                {
                    index.SchemaName = table.SchemaName;

                    if (index.IsPrimary)  // Now only declaring Primary keys on the column
                    {
                        if (index.Columns.Count() == 1 && index.Columns.First().Direction == Direction.Ascending)
                        {
                            IndexColumnDefinition indexColumn = index.Columns.First();

                            ColumnDefinitionExt tableColumn =
                                (table.Columns.Where(col => col.Name == indexColumn.Name)).First();

                            tableColumn.IsIndexed = true;
                            tableColumn.IsPrimaryKey = index.IsPrimary;
                            tableColumn.IsUnique = index.IsUnique;
                            if (index.IsPrimary)
                            {
                                tableColumn.PrimaryKeyName = index.Name;
                            }
                            else
                            {
                                tableColumn.IndexName = index.Name;
                            }

                        }
                    } else
                    {
                        if (table.ForeignKeys.Any())
                        {
                            // Added property to FM API
                            index.IsFkIndex = FindForeignKeysWithColumns(table.ForeignKeys, index.Columns).Any();
                        }
                    }
                }

                yield return table;
            }
        }

        protected virtual IList<TableDefinitionExt> ReadTableDefs()
        {
            const string query = @"SELECT OBJECT_SCHEMA_NAME(t.[object_id],DB_ID()) AS [Schema], t.name AS [Table], 
                c.[name] AS ColumnName,
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
                pk.name AS PrimaryKeyName
                FROM sys.tables t 
                JOIN sys.all_columns c ON c.object_id = t.object_id
                LEFT JOIN sys.default_constraints def ON c.default_object_id = def.object_id
                LEFT JOIN sys.key_constraints pk ON t.object_id = pk.parent_object_id AND pk.type = 'PK'
                WHERE NOT t.name IN ('dtproperties', 'sysdiagrams') AND t.type = 'u'
                ORDER BY t.name, c.column_id";

            using (DataSet ds = Read(query))
            using (DataTable dt = ds.Tables[0])
            {
                IEnumerable<TableDefinitionExt> tables = from row in dt.Rows.Cast<DataRow>()
                    group new ColumnDefinitionExt
                        {
                            Name = row["ColumnName"].ToString(),
                            CustomType = "", //TODO: set this property
                            DefaultValue = GetColumnDefault(row),
                            IsForeignKey = Convert.ToBoolean(row["IsForeignKey"]),
                            IsIdentity = Convert.ToBoolean(row["IsIdentity"]),
                            //IsIndexed = Convert.ToBoolean(row["IsIndexed"]),
                            //IndexName = row["IndexName"].ToString(),
                            IsNullable = Convert.ToBoolean(row["IsNullable"]),
                            //IsPrimaryKey = Convert.ToBoolean(row["IsPrimaryKey"]),
                            //IsUnique = Convert.ToBoolean(row["IsUnique"]),
                            Precision = int.Parse(row["Precision"].ToString()),
                            PrimaryKeyName = row.IsNull("PrimaryKeyName") ? "" : row["PrimaryKeyName"].ToString(),
                            TableName = row["Table"].ToString(),
                            Type = GetColumnType(row),
                            Size = GetColumnSize(row),
                            ModificationType = ColumnModificationType.Create
                        } 
                    by new
                        {
                            TableName = row["Table"].ToString(),
                            SchemaName = row["Schema"].ToString()
                        } into g
                    select new TableDefinitionExt 
                        {
                            Columns = g.ToList(),
                            SchemaName = g.Key.SchemaName,
                            Name = g.Key.TableName,
                        };

                // TODO: Not very efficient if there are large numbers of tables. Should alter the SQL query instead.
                tables = ApplyTableFilter(tables);

                return tables.ToList();
            }
        }

        protected virtual IList<IndexDefinitionExt> ReadIndexes(string schemaName, string tableName)
        {
            const string query = @"SELECT DISTINCT OBJECT_SCHEMA_NAME(T.[object_id],DB_ID()) AS [Schema],  
              T.[name] AS [table_name], I.[name] AS [index_name], AC.[name] AS [column_name],  
              I.[type_desc], I.[is_unique], I.[data_space_id], I.[ignore_dup_key], I.[is_primary_key], 
              I.[is_unique_constraint], I.[fill_factor], I.[is_padded], I.[is_disabled], I.[is_hypothetical], 
              I.[allow_row_locks], I.[allow_page_locks], IC.[is_descending_key], IC.[is_included_column] 
            FROM sys.[tables] AS T  
              INNER JOIN sys.[indexes] I ON T.[object_id] = I.[object_id] 
              INNER JOIN sys.[index_columns] IC ON I.[object_id] = IC.[object_id] AND I.index_id = IC.index_id 
              INNER JOIN sys.[all_columns] AC ON T.[object_id] = AC.[object_id] AND IC.[column_id] = AC.[column_id] 
            WHERE T.[is_ms_shipped] = 0 AND I.[type_desc] <> 'HEAP' 
            AND T.[name] = '{1}' and OBJECT_SCHEMA_NAME(T.[object_id],DB_ID()) = '{0}'";

            using (DataSet ds = Read(query, schemaName, tableName))
            using (DataTable dt = ds.Tables[0])
            {
                IEnumerable<IndexDefinitionExt> indexes = from row in dt.Rows.Cast<DataRow>()
                    group new IndexColumnDefinition
                        {
                            Name = row["column_name"].ToString(), 
                            Direction = Convert.ToBoolean(row["is_descending_key"]) ? Direction.Descending : Direction.Ascending
                        } 
                    by new
                        {
                            SchemaName = row["Schema"].ToString(),
                            TableName = row["table_name"].ToString(), 
                            IndexName = row["index_name"].ToString(),
                            IsClustered = row["type_desc"].ToString() == "CLUSTERED",
                            IsUnique = Convert.ToBoolean(row["is_unique"]),
                            IsPrimary = Convert.ToBoolean(row["is_primary_key"])
                        } into g
                    select new IndexDefinitionExt 
                        {
                            Columns = g.ToList(),
                            SchemaName = g.Key.SchemaName,
                            TableName = g.Key.TableName,
                            Name = g.Key.IndexName,
                            IsClustered= g.Key.IsClustered,
                            IsUnique = g.Key.IsUnique,
                            IsPrimary = g.Key.IsPrimary
                        };

                return indexes.ToList();
            }
        }

        protected virtual IList<ForeignKeyDefinitionExt> ReadForeignKeys(string schemaName, string tableName)
        {
            const string query = @"SELECT 
                FK.CONSTRAINT_NAME AS ForeignConstraintName, 
                FK.TABLE_SCHEMA AS ForeignTableSchema, 
                FK.TABLE_NAME AS ForeignTable, 
                FKC.COLUMN_NAME AS ForeignColumn, 
                PK.CONSTRAINT_NAME AS PrimaryConstraintName, 
                PK.TABLE_SCHEMA AS PrimaryTableSchema, 
                PK.TABLE_NAME AS PrimaryTable, 
                PKC.COLUMN_NAME AS PrimaryColumn, 
                RC.UPDATE_RULE AS UpdateRule, 
                RC.DELETE_RULE AS DeleteRule
            FROM         
                INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS FK INNER JOIN
                INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS FKC ON FK.CONSTRAINT_SCHEMA = FKC.CONSTRAINT_SCHEMA AND 
                FK.CONSTRAINT_NAME = FKC.CONSTRAINT_NAME INNER JOIN
                INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS AS RC ON FK.CONSTRAINT_SCHEMA = RC.CONSTRAINT_SCHEMA AND 
                FK.CONSTRAINT_NAME = RC.CONSTRAINT_NAME INNER JOIN
                INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS PK ON RC.UNIQUE_CONSTRAINT_SCHEMA = PK.CONSTRAINT_SCHEMA AND 
                RC.UNIQUE_CONSTRAINT_NAME = PK.CONSTRAINT_NAME INNER JOIN
                INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS PKC ON PK.CONSTRAINT_SCHEMA = PKC.CONSTRAINT_SCHEMA AND 
                PK.CONSTRAINT_NAME = PKC.CONSTRAINT_NAME AND FKC.ORDINAL_POSITION = PKC.ORDINAL_POSITION
            WHERE     
                (FK.CONSTRAINT_TYPE = 'FOREIGN KEY') 
                AND (FK.TABLE_NAME = '{1}') 
                AND (FK.TABLE_SCHEMA = '{0}')
            ORDER BY ForeignConstraintName, ForeignTableSchema, ForeignTable, FKC.ORDINAL_POSITION";

            using (DataSet ds = Read(query, schemaName, tableName))
            using (DataTable dt = ds.Tables[0])
            {
                var keys = dt.Rows.Cast<DataRow>().Select(row => new
                {
                    ForeignConstraintName = row["ForeignConstraintName"].ToString(),
                    ForeignTableSchema = row["ForeignTableSchema"].ToString(),
                    ForeignTable = row["ForeignTable"].ToString(),
                    ForeignColumn = row["ForeignColumn"].ToString(),
                    PrimaryConstraintName = row["PrimaryConstraintName"].ToString(),
                    PrimaryTableSchema = row["PrimaryTableSchema"].ToString(),
                    PrimaryTable = row["PrimaryTable"].ToString(),
                    PrimaryColumn = row["PrimaryColumn"].ToString(),
                    UpdateRule = DecodeRule(row["UpdateRule"].ToString()),
                    DeleteRule = DecodeRule(row["DeleteRule"].ToString()),
                });

                return keys.GroupBy(key => new
                {
                    key.ForeignConstraintName,
                    key.ForeignTableSchema,
                    key.ForeignTable,
                    key.PrimaryTableSchema,
                    key.PrimaryTable,
                    key.UpdateRule,
                    key.DeleteRule
                }).Select(fkGroup => new ForeignKeyDefinitionExt
                {
                    Name = fkGroup.Key.ForeignConstraintName,
                    ForeignTable = fkGroup.Key.ForeignTable,
                    ForeignTableSchema = fkGroup.Key.ForeignTableSchema,
                    ForeignColumns = fkGroup.Select(f => f.ForeignColumn).ToList(),
                    PrimaryTable = fkGroup.Key.PrimaryTable,
                    PrimaryTableSchema = fkGroup.Key.PrimaryTableSchema,
                    PrimaryColumns = fkGroup.Select(f => f.PrimaryColumn).ToList(),
                    OnUpdate = fkGroup.Key.UpdateRule,
                    OnDelete = fkGroup.Key.DeleteRule
                }).ToList();
            }
        }

        private static System.Data.Rule DecodeRule(string rule)
        {
            return new Switch<string, System.Data.Rule>(rule)
                .Case("CASCADE", System.Data.Rule.Cascade)
                .Case("SET NULL", System.Data.Rule.SetDefault)
                .Case("SET DEFAULT", System.Data.Rule.SetNull)
                .Default(System.Data.Rule.None);
        }
    }
}
