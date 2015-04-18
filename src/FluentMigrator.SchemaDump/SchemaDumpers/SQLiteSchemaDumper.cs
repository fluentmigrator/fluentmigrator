using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using FluentMigrator.Builders.Execute;
using FluentMigrator.Model;
using FluentMigrator.Runner;
using System;
using FluentMigrator.Runner.Processors.SQLite;

namespace FluentMigrator.SchemaDump.SchemaDumpers
{
    public class SQLiteSchemaDumper : ISchemaDumper
    {
        public SQLiteSchemaDumper(SQLiteProcessor processor, IAnnouncer announcer)
        {
            Announcer = announcer;
            Processor = processor;
        }

        public virtual IAnnouncer Announcer { get; set; }
        public SQLiteProcessor Processor { get; set; }

        #region ISchemaDumper Members

        public virtual IList<TableDefinition> ReadDbSchema()
        {
            IList<TableDefinition> tables = ReadTables();
            foreach (var table in tables)
            {
                table.Indexes = ReadIndexes(table.SchemaName, table.Name);
                table.ForeignKeys = ReadForeignKeys(table.SchemaName, table.Name);
            }

            return tables;
        }

        #endregion

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

        public virtual DataSet Read(string template, params object[] args)
        {
            return Processor.Read(template, args);
        }

        public virtual void Process(PerformDBOperationExpression expression)
        {
            Processor.Process(expression);
        }
        
        protected virtual IList<TableDefinition> ReadTables()
        {
            var dtTable = GetTableNamesAndDDL();
            var tableDefinitionList = new List<TableDefinition>();

            foreach (DataRow dr in dtTable.Rows)
            {
                var tableDef = new TableDefinition
                {
                    Name = dr["name"].ToString(),
                    Columns = GetColumnDefinitionsForTableDDL(dr["sql"].ToString())
                };
                tableDefinitionList.Add(tableDef);
            }
            return tableDefinitionList;
        }

        public static bool HasValidSimpleTableDDL(ref string simpleTableDDL)
        {
            var cleanUp = NormalizeSpaces(simpleTableDDL.Replace(Environment.NewLine, " ").Trim());
            var tableUPPER = cleanUp.ToUpper();

            simpleTableDDL = cleanUp;
            var haveCreateTable = tableUPPER.StartsWith("CREATE TABLE");
            var skip = "CREATE TABLE ".Length;
            var tableName = cleanUp.Substring(skip, cleanUp.IndexOf("(", StringComparison.InvariantCulture) - skip)
                .Replace("\"", "").Replace("'", "")
                .Replace("[", "").Replace("]", "")
                .Trim();
            var haveOpeningAndClosingBracket = cleanUp.Contains("(") && cleanUp.Contains(")");

            return haveCreateTable && !string.IsNullOrEmpty(tableName) && haveOpeningAndClosingBracket;
        }
        private static string CleanName(string ddlString)
        {
            return ddlString
                .Replace("\"", "")
                .Replace("'", "")
                .Replace("[", "").Replace("]", "");
        }

        /// <summary>
        /// Get table names and their DDL from database
        /// </summary>
        /// <returns>DataTable with 2 columns ( string name, string sql)</returns>
        public virtual DataTable GetTableNamesAndDDL()
        {
            const string sqlCommand = @"SELECT name, sql FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%'ORDER BY name;";
            return Read(sqlCommand).Tables[0];
        }

        /// <summary>
        /// Get table DDL for table
        /// </summary>
        /// <param name="tableName">Table name to get DDL for</param>
        /// <returns>Table DDL or string.empty if table not found</returns>
        public virtual string GetTableDDL(string tableName)
        {
            foreach (DataRow row in GetTableNamesAndDDL().Rows)
            {
                var name = row["name"].ToString();
                if (name.ToUpper().Equals(tableName.ToUpper())) return row["sql"].ToString();
            }
            return string.Empty;
        }

        /// <summary>
        /// Get column definitions DDL from table DDL, all text in brackets : create table x ( THIS )
        /// </summary>
        /// <param name="tableDDL">Table DDL</param>
        /// <returns> </returns>
        public static string GetColumnDefinitionsDDLFromTableDDL(ref string tableDDL)
        {
            if (!HasValidSimpleTableDDL(ref tableDDL)) throw new Exception("Invalid table DDL");
            //Console.WriteLine( tableDDL );// test snippet
            //Console.WriteLine( "start " + tableDDL.IndexOf( "(", StringComparison.InvariantCulture ) );
            //Console.WriteLine( "start " + tableDDL.LastIndexOf( ")", StringComparison.InvariantCulture ) );
            var start = tableDDL.IndexOf("(", StringComparison.InvariantCulture) + 1;
            var length = tableDDL.LastIndexOf(")", StringComparison.InvariantCulture) - start;
            var columnsDDL = tableDDL.Substring(start, length);
            var upper = columnsDDL.ToUpper();
            // strip "UNIQUE (FirstName, LastName)"
            while (upper.Contains("UNIQUE"))
            {
                var uStart = upper.IndexOf("UNIQUE");
                uStart = upper.Substring(0, uStart).LastIndexOf(',');

                var uLength = upper.Substring(uStart, upper.Length - uStart).IndexOf(")") + 1;
                columnsDDL = columnsDDL.Remove(uStart, uLength);
                upper = columnsDDL.ToUpper();
            }

            return columnsDDL;
        }

        /// <summary>
        /// Get list<string> of columns DDL
        /// </summary>
        /// <param name="tableDDL"></param>
        /// <returns>list of columns DDL</returns>
        public static IList<string> GetColumnDefinitionsDDLAsList(string tableDDL)
        {
            if (!HasValidSimpleTableDDL(ref tableDDL)) throw new Exception("Invalid table DDL");
            var columnsDDL = GetColumnDefinitionsDDLFromTableDDL(ref tableDDL)
                .Split(',').Select(x => x.Trim()).ToList();
            return columnsDDL;
        }

        /// <summary>
        /// Parse sqlite table DDL into ColumnDefinitions
        /// </summary>
        /// <param name="tableDDL"> </param>
        /// <returns> </returns>
        public static List<ColumnDefinition> GetColumnDefinitionsForTableDDL(string tableDDL)
        {
            var cleanUp = tableDDL;

            var columnList = new List<ColumnDefinition>();
            if (HasValidSimpleTableDDL(ref cleanUp))
            {
                var columnStringList = GetColumnDefinitionsDDLAsList(cleanUp);
                foreach (var columnString in columnStringList)
                {
                    var upper = columnString.ToUpper();
                    if (upper.Contains("CONSTRAINT"))
                    {
                        continue;
                    }
                    // [0]rowID [1]INTEGER PRIMARY KEY AUTOINCREMENT,
                    // [0]NAME [1]TEXT NOT NULL UNIQUE
                    var column = new ColumnDefinition();
                    var splited = columnString.Split(new char[] { ' ' });
                    column.Name = splited[0];
                    column.Type = GetDbType(splited[1]);
                    column.IsPrimaryKey = upper.Contains("PRIMARY KEY");
                    column.IsIdentity = upper.Contains("AUTOINCREMENT");
                    column.IsNullable = !upper.Contains("NOT NULL") && !upper.Contains("PRIMARY KEY");
                    column.IsUnique = upper.Contains("UNIQUE") || upper.Contains("PRIMARY KEY");
                    columnList.Add(column);
                }
            }
            return columnList;
        }

        private List<ColumnDefinition> GetColumnDefinitionsForTableUsingPragma(string tableName)
        {
            var query = String.Format(@"PRAGMA table_info({0})", tableName);
            var dtTables = Read(query).Tables[0];

            var columns = new List<ColumnDefinition>();

            foreach (DataRow dr in dtTables.Rows)
            {
                columns.Add(new ColumnDefinition
                {
                    Name = dr["name"].ToString(),
                    //CustomType = "type", //TODO: set this property
                    DefaultValue = dr["dflt_value"].ToString(),
                    IsNullable = dr["notnull"].ToString() == "0",
                    IsPrimaryKey = dr["IsPrimaryKey"].ToString() == "1",
                    //IsUnique = dr["IsUnique"].ToString() == "1",
                    //Precision = int.Parse( dr["Precision"].ToString() ),
                    //PrimaryKeyName = dr.IsNull( "PrimaryKeyName" ) ? "" : dr["PrimaryKeyName"].ToString(),
                    //Size = int.Parse( dr["Length"].ToString() ),
                    TableName = tableName,
                    Type = GetDbType(dr["type"].ToString()), //TODO: set this property
                    ModificationType = ColumnModificationType.Create
                });
            }
            return columns;
        }

        /// <summary>
        /// Get Type from string type definition
        /// </summary>
        /// <param name="typeNum"></param>
        /// <returns></returns>
        public static Type GetType(string typeNum)
        {
            switch (typeNum.ToUpper())
            {
                case "BLOB": //'byte[]'
                    return typeof(Byte[]);
                case "CLOB": //'String'
                    return typeof(string);
                case "BOOLEAN": //'Double'
                    return typeof(bool);
                case "FLOAT": //'Double'
                    return typeof(double);
                case "REAL": //'Double'
                    return typeof(double);
                case "TIMESTAMP": //'DateTime'
                    return typeof(DateTime);
                case "DATETIME": //'DateTime
                    return typeof(DateTime);
                case "TIME": //'DateTime
                    return typeof(TimeSpan);
                case "INT": //'Int32'
                    return typeof(Int64);
                case "INTEGER": //'Int32'
                    return typeof(Int64);
                case "NUMERIC": //'String'
                    return typeof(decimal);
                case "DECIMAL": //'String'
                    return typeof(decimal);
                case "CHAR": //'String'
                    return typeof(string);
                case "VARCHAR": //'String'
                    return typeof(string);
                case "NVARCHAR": //'String'
                    return typeof(string);
                case "VARYINGCHARACTER": //'String'
                    return typeof(string);
                case "NATIONALVARYINGCHARACTER": //'String'
                    return typeof(string);
                case "TEXT": //'String'
                    return typeof(string);
                case "": //'string'
                default:
                    return typeof(string);
            }
        }

        /// <summary>
        /// Get DbType from string type definition
        /// </summary>
        /// <param name="typeNum"></param>
        /// <returns></returns>
        public static DbType GetDbType(string typeNum)
        {
            // TODO : implement this !!!
            //throw new NotImplementedException( "sqlDDL parser not implemented!" );
            // MORE INFO ON : https://bitbucket.org/VahidN/codesmith/src/eaf97d87927f/Maps/Sqlite-CSharp.csmap?at=default
            switch (typeNum.ToUpper())
            {
                case "BLOB": //'byte[]'
                    return DbType.Byte;
                case "CLOB": //'String'
                    return DbType.String;
                case "BOOLEAN": //'Double'
                    return DbType.Boolean;
                case "FLOAT": //'Double'
                    return DbType.Double;
                case "REAL": //'Double'
                    return DbType.Double;
                case "TIMESTAMP": //'DateTime'
                    return DbType.DateTime;
                case "DATETIME": //'DateTime
                    return DbType.DateTime;
                case "TIME": //'DateTime
                    return DbType.Time;
                case "INT": //'Int32'
                    return DbType.Int64;
                case "INTEGER": //'Int32'
                    return DbType.Int64;
                case "NUMERIC": //'String'
                    return DbType.Decimal;
                case "DECIMAL": //'String'
                    return DbType.Decimal;
                case "CHAR": //'String'
                    return DbType.String;
                case "VARCHAR": //'String'
                    return DbType.String;
                case "NVARCHAR": //'String'
                    return DbType.String;
                case "VARYINGCHARACTER": //'String'
                    return DbType.String;
                case "NATIONALVARYINGCHARACTER": //'String'
                    return DbType.String;
                case "TEXT": //'String'
                    return DbType.String;
                case "": //'string'
                default:
                    return DbType.String;
            }
        }

        protected virtual DbType GetDbTypeFromPragma(string typeNum)
        {
            switch (typeNum.ToUpper())
            {
                case "BLOB": //'byte[]'
                    return DbType.Byte;
                case "TEXT": //'string'
                    return DbType.String;
                case "INT": //'int'
                    return DbType.Int64;
                case "INTEGER": //'int'
                    return DbType.Int64;
                case "BIGINT": //'float'
                    return DbType.Int64;
                case "DATETIME": //'System.DateTime'
                    return DbType.DateTime;
                case "": //'string'
                default:
                    return DbType.String;
            }
        }

        protected virtual IList<IndexDefinition> ReadIndexes(string schemaName, string tableName)
        {
            var sqlCommand = string.Format(@"SELECT type, name, sql FROM sqlite_master WHERE tbl_name = '{0}' AND type = 'index' AND name NOT LIKE 'sqlite_auto%';", tableName);
            DataTable table = Read(sqlCommand).Tables[0];

            IList<IndexDefinition> indexes = new List<IndexDefinition>();

            foreach (DataRow dr in table.Rows)
            {
                var sql = dr["sql"].ToString();
                var upper = sql.ToUpper();
                var columnsString = GetTextBeteenBrackets(sql);
                var iDef = new IndexDefinition
                {
                    Name = dr["name"].ToString(),
                    SchemaName = schemaName,
                    IsClustered = false,
                    IsUnique = upper.Contains("UNIQUE"),
                    TableName = tableName,
                    Columns = GetIndexColumnsFromIndexDDL(columnsString)
                };

                indexes.Add(iDef);
            }
            return indexes;
        }

        public static string GetTextBeteenBrackets(string sql)
        {
            //CREATE INDEX [IDX_USERS_LastLogin] ON [Users]([LastLogin]  DESC)
            var start = sql.IndexOf('(') + 1;
            var end = sql.LastIndexOf(')');
            return sql.Substring(start, end - start);
        }

        public static ICollection<IndexColumnDefinition> GetIndexColumnsFromIndexDDL(string IndexSqlDDL)
        {
            ICollection<IndexColumnDefinition> columnList = new Collection<IndexColumnDefinition>();
            var columns_string = IndexSqlDDL.Split(',');
            foreach (var column_Str in columns_string)
            {
                var columnDefinition = new IndexColumnDefinition();
                var split = NormalizeSpaces(column_Str).Trim().Split(' ');
                columnDefinition.Name = CleanName(split[0]);
                columnDefinition.Direction = split[1].ToUpper().Equals("ASC") ? Direction.Ascending : Direction.Descending;
                columnList.Add(columnDefinition);
            }
            return columnList;
        }
        /// <summary>
        /// replace multiple occurences of spaces into 1
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        private static string NormalizeSpaces(string inputString)
        {
            var returnStr = inputString;
            while (returnStr.Contains("  "))
            {
                returnStr = returnStr.Replace("  ", " ");
            }
            return returnStr;
        }

        protected virtual IList<ForeignKeyDefinition> ReadForeignKeys(string schemaName, string tableName)
        {
            return new List<ForeignKeyDefinition>();
        }
    }
}