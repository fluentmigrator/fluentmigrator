#region License
// 
// Copyright (c) 2011, Grant Archibald
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
using System.IO;
using System.Linq;
using System.Reflection;
using FluentMigrator.Model;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Processors.Oracle;

namespace FluentMigrator.SchemaDump.SchemaDumpers
{
   /// <summary>
   /// Reads the schema from an oracle database
   /// </summary>
   public class OracleSchemaDumper : ISchemaDumper
   {
      /// <summary>
      /// The announcer instance that will report information about the steps beging performed
      /// </summary>
      public IAnnouncer Announcer { get; set; }

      /// <summary>
      /// The processor instance that will be used to read oracle data from
      /// </summary>
      public OracleProcessor Processor { get; set; }

      /// <summary>
      /// Creates a new instance of a <see cref="OracleSchemaDumper"/>
      /// </summary>
      /// <param name="processor">The processor that will be used to obtain schema information from</param>
      /// <param name="announcer">The anncouncer instance that will report progress</param>
      public OracleSchemaDumper(OracleProcessor processor, IAnnouncer announcer)
      {
         Announcer = announcer;
         Processor = processor;
      }

      /// <summary>
      /// Reads the table schema and assoacted indexes and foreign keys from the database for the USER
      /// </summary>
      /// <returns>A list of <see cref="TableDefinition"/> that where found</returns>
      public IList<TableDefinition> ReadDbSchema()
      {
         Announcer.Say("Reading table schema");
         var tables = ReadTables();
         foreach (var table in tables)
         {
            Announcer.Say(string.Format("Reading indexes and foreign keys for {0}", table.Name));
            table.Indexes = ReadIndexes(table.SchemaName, table.Name);
            table.ForeignKeys = ReadForeignKeys(table.SchemaName, table.Name);
         }

         return tables;
      }

      private ICollection<ForeignKeyDefinition> ReadForeignKeys(string schemaName, string name)
      {
         try
         {
            var tableDefinition = Processor.Read("SELECT  dbms_metadata.GET_DEPENDENT_DDL( 'REF_CONSTRAINT', '{0}' ) As SQL FROM dual", name);

            if (tableDefinition.Tables[0].Rows.Count == 1)
            {
               return ParseForeignKeyDefinition(name, tableDefinition.Tables[0].Rows[0]["SQL"].ToString());
            }

            throw new ApplicationException("More than one table found");
         }
         catch ( Exception )
         {
            // The object may not have any referances.. in this case an exception will be thrown
            return new List<ForeignKeyDefinition>();
         }
         
      }

      private ICollection<ForeignKeyDefinition> ParseForeignKeyDefinition(string table, string sql)
      {
         var definitions = new List<ForeignKeyDefinition>();
         using (var reader = new StringReader(sql))
         {
            while (  reader.Peek() > 0)
            {
               var line = reader.ReadLine();

               if (string.IsNullOrEmpty(line) || !line.Contains(" FOREIGN KEY ")) continue;

               var references = reader.ReadLine();
               var definition = new ForeignKeyDefinition
                                   {
                                      Name = GetForeignKeyName(line)
                                      ,ForeignTable = table
                                      ,PrimaryTable = GetPrimaryTable(references)
                                      ,PrimaryColumns = GetColumnNames(references)
                                      ,ForeignColumns = GetColumnNames(line)
                                   };

               definitions.Add(definition);
            }
            
         }

         return definitions;
      }

      private ICollection<string> GetColumnNames(string sql)
      {
         var columnsStart = sql.IndexOf("(");
         var columnsEnd = sql.IndexOf(")", columnsStart);

         var columnsText = sql.Substring(columnsStart + 1, columnsEnd - columnsStart - 1);
         if ( !string.IsNullOrEmpty(columnsText))
         {
            return columnsText.Replace("\"", "").Split(',');
         }
         return null;
      }

      private string GetForeignKeyName(string sql)
      {
         var start = sql.IndexOf("ADD CONSTRAINT ") + "ADD CONSTRAINT ".Length;
         var end = sql.IndexOf("FOREIGN KEY ", start);

         return sql.Substring(start, end - start).Replace("\"", "").Trim();

      }

      private string GetPrimaryTable(string sql)
      {
         var start = sql.IndexOf("REFERENCES ") + "REFERENCES ".Length;
         var end = sql.IndexOf("(", start);

         return sql.Substring(start, end - start).Replace("\"", "").Trim();

      }

      private ICollection<IndexDefinition> ReadIndexes(string schemaName, string name)
      {
         var indexDefinitions = Processor.Read("SELECT u.index_name, DBMS_METADATA.GET_DDL('INDEX',u.index_name) As Sql FROM USER_INDEXES u WHERE TABLE_NAME = '{0}'", name);

         return (from DataRow index in indexDefinitions.Tables[0].Rows
                 let sql = index["SQL"].ToString()
                 select new IndexDefinition
                           {
                              Name = index["INDEX_NAME"].ToString()
                              , IsUnique = sql.Contains("UNIQUE INDEX")
                              , TableName = name
                              , Columns = GetColumns(sql)
                           }).ToList();
      }

      private static ICollection<IndexColumnDefinition> GetColumns(string sql)
      {
         var columns = new List<IndexColumnDefinition>();
         
         using ( var reader = new StringReader(sql))
         {
            var firstLine = GetFirstNonEmptyLine(reader);

            if ( !string.IsNullOrEmpty(firstLine))
            {
               var columnsStart = firstLine.IndexOf("(");
               var columnsEnd = firstLine.IndexOf(")", columnsStart);

               // Not a valid syntax for normal CREATE INDEX on a column
               // ..... could be a CLOB or BLOB index generated by the system
               if ( columnsStart < 0 || columnsEnd <= columnsStart)
                  return columns; // complete no valid indexes to parse

               var columnsText = firstLine.Substring(columnsStart + 1, columnsEnd - columnsStart - 1);
               if ( !string.IsNullOrEmpty(columnsText))
               {
                  var names = columnsText.Replace("\"", "").Split(',');

                  columns.AddRange(names.Select(name =>
                                                   {
                                                      var isDecending = false;
                                                      if ( name.EndsWith(" DESC"))
                                                      {
                                                         name = name.Substring(0, name.Length - " DESC".Length);
                                                         isDecending = true;
                                                      }

                                                      return new IndexColumnDefinition
                                                                {
                                                                   Name = name,
                                                                   Direction =
                                                                      isDecending
                                                                         ? Direction.Descending
                                                                         : Direction.Ascending
                                                                };
                                                   }
                     ));
               }
            }
         }

         return columns;
      }

      private static string GetFirstNonEmptyLine(TextReader reader)
      {
         var firstLine = string.Empty;
         while ( reader.Peek() > 0 )
         {
            firstLine = reader.ReadLine();
            if ( !string.IsNullOrEmpty(firstLine))
               break;
         }
         return firstLine;
      }

      /// <summary>
      /// Gets a list of <see cref="TableDefinition"/> that exist for the user connection
      /// </summary>
      /// <returns>Matching <see cref="TableDefinition"/></returns>
      public virtual IList<TableDefinition> ReadTables()
      {
         var tables = new List<TableDefinition>();
         var oracleTables = Processor.Read("SELECT TABLE_NAME FROM USER_TABLES");

         foreach (DataRow table in oracleTables.Tables[0].Rows)
         {
            var tableName = table["TABLE_NAME"] as string;
            var schema = GetTableSchema(tableName);

            var definition = new TableDefinition {Name = tableName};

            foreach (DataRow schemaRow in schema.Rows)
            {
               var column = new ColumnDefinition
                               {
                                  Name = schemaRow["ColumnName"] as string
                               };

               MapColumnUsingProviderType(schemaRow, column);

               if (column.Type != null)
               {
                  definition.Columns.Add(column);
                  continue;
               }

               MapColumnUsingDataType(schemaRow, column);
               definition.Columns.Add(column);
            }

            tables.Add(definition);
         }

         return tables;
      }

      /// <summary>
      /// Attemps to map the <see cref="ColumnDefinition.Type"/> and <see cref="ColumnDefinition.Size"/> based on the DataType 
      /// </summary>
      /// <param name="schemaRow">The row containing the oracle schema information</param>
      /// <param name="column">The column to be mapped</param>
      private static void MapColumnUsingDataType(DataRow schemaRow, ColumnDefinition column)
      {
         switch (((Type) schemaRow["DataType"]).FullName)
         {
            case "System.String":
               column.Type = DbType.String;
               column.Size = int.Parse(schemaRow["ColumnSize"].ToString());
               break;
            case "System.Decimal":
               var precision = int.Parse(schemaRow["NumericPrecision"].ToString());
               int scale = int.Parse(schemaRow["NumericScale"].ToString());
               if (precision == 1 && scale == 0)
               {
                  column.Type = DbType.Boolean;
                  column.Size = precision;
               }

               if (precision == 3 && scale == 0)
                  column.Type = DbType.Byte;
               if (precision == 5 && scale == 0)
                  column.Type = DbType.Int16;
               if (precision == 10 && scale == 0)
                  column.Type = DbType.Int32;
               if (precision == 20 && scale == 0)
                  column.Type = DbType.Int64;
               if (precision == 24 && scale == 129)
                  column.Type = DbType.Single;

               if (precision == 126)
                  column.Type = DbType.Double;

               if (column.Type == null)
               {
                  column.Type = DbType.Decimal;
                  column.Precision = precision;
                  column.Scale = scale;
               }
               break;
            case "System.Byte[]":
               var size = int.Parse(schemaRow["ColumnSize"].ToString());
               // Special case assume that RAW(16) is a Guid
               column.Type = size == 16 ? DbType.Guid : DbType.Binary;
               column.Size = int.Parse(schemaRow["ColumnSize"].ToString());
               break;
            case "System.DateTime":
               column.Type = DbType.DateTime;
               break;
            case "System.Double":
               column.Type = DbType.Double;
               break;
         }
      }

      /// <summary>
      /// Attemps to map the <see cref="ColumnDefinition.Type"/> and <see cref="ColumnDefinition.Size"/> based on the ProviderType from Oracle
      /// </summary>
      /// <param name="schemaRow">The row containing the oracle schema information</param>
      /// <param name="column">The column to be mapped</param>
      private static void MapColumnUsingProviderType(DataRow schemaRow, ColumnDefinition column)
      {
         var providerType = int.Parse(schemaRow["ProviderType"].ToString());
         switch (providerType)
         {
            case OracleTypes.Char:
               column.Type = DbType.AnsiStringFixedLength;
               column.Size = int.Parse(schemaRow["ColumnSize"].ToString());
               break;
            case OracleTypes.NChar:
               column.Type = DbType.StringFixedLength;
               column.Size = int.Parse(schemaRow["ColumnSize"].ToString());
               break;
            case OracleTypes.Date:
               column.Type = DbType.Date;
               break;
            case OracleTypes.Varchar:
               column.Type = DbType.AnsiString;
               column.Size = int.Parse(schemaRow["ColumnSize"].ToString());
               break;
         }
      }


      public DataTable GetTableSchema(string tableName)
      {
         try
         {
            var strQuery = string.Format("SELECT * FROM {0} WHERE 1 = 2", tableName);

            using ( var command = Processor.Connection.CreateCommand() )
            {
               command.CommandText = strQuery;
               var rdr = command.ExecuteReader();
               // Get the schema table.
               return rdr.GetSchemaTable();   
            }
         }
         catch (Exception)
         {
            return null;
         }
      }

      public IList<ViewDefinition> ReadViews()
      {
         var views = Processor.Read("SELECT OBJECT_NAME, dbms_metadata.get_ddl(object_type,object_name) AS SQL FROM USER_OBJECTS WHERE object_type = 'VIEW'");

         return (from DataRow view in views.Tables[0].Rows
                 select new ViewDefinition
                 {
                    Name = view["OBJECT_NAME"].ToString()
                    ,CreateViewSql = view["SQL"].ToString()
                 }).ToList();
         
      }

      public DataSet ReadTableData(string schemaName, string tableName)
      {
         return Processor.ReadTableData(schemaName, tableName);
      }

      /// <summary>
      /// Gets a list of procedures that exist for the user
      /// </summary>
      /// <returns>The procedures found in the database</returns>
      public IList<ProcedureDefinition> ReadProcedures()
      {
         var oracleProcedures = Processor.Read("SELECT OBJECT_NAME, dbms_metadata.get_ddl(object_type,object_name) AS SQL FROM USER_OBJECTS WHERE object_type = 'PROCEDURE'");

         return (from DataRow view in oracleProcedures.Tables[0].Rows
                 select new ProcedureDefinition
                 {
                    Name = view["OBJECT_NAME"].ToString()
                    ,Sql = view["SQL"].ToString()
                 }).ToList();
      }

      /// <summary>
      /// Gets a list of functions that exist for the user
      /// </summary>
      /// <returns>The functions found in the database</returns>
      public IList<FunctionDefinition> ReadFunctions()
      {
         var procedures = Processor.Read("SELECT OBJECT_NAME, dbms_metadata.get_ddl(object_type,object_name) AS SQL FROM USER_OBJECTS WHERE object_type = 'FUNCTION'");

         return (from DataRow view in procedures.Tables[0].Rows
                 select new FunctionDefinition
                 {
                    Name = view["OBJECT_NAME"].ToString()
                    ,Sql = view["SQL"].ToString()
                 }).ToList();
      }

      private string GetProcedureSql(string toString)
      {
         throw new NotImplementedException();
      }
   }

   public class OracleTypes
   {
      public const int Char = 3;
      public const int Date = 6;
      public const int NChar = 11;
      public const int NVarchar2 = 14;
      public const int Varchar = 22;
   }
}
