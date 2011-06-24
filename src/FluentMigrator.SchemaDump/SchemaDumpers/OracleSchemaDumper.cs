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
            table.Indexes = ReadIndexes(table.Name);
            table.ForeignKeys = ReadForeignKeys(table.Name);
         }

         return tables;
      }

      private ICollection<ForeignKeyDefinition> ReadForeignKeys(string name)
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

      private static ICollection<ForeignKeyDefinition> ParseForeignKeyDefinition(string table, string sql)
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

      private static ICollection<string> GetColumnNames(string sql)
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

      private static string GetForeignKeyName(string sql)
      {
         var start = sql.IndexOf("ADD CONSTRAINT ") + "ADD CONSTRAINT ".Length;
         var end = sql.IndexOf("FOREIGN KEY ", start);

         return sql.Substring(start, end - start).Replace("\"", "").Trim();

      }

      private static string GetPrimaryTable(string sql)
      {
         var start = sql.IndexOf("REFERENCES ") + "REFERENCES ".Length;
         var end = sql.IndexOf("(", start);

         return sql.Substring(start, end - start).Replace("\"", "").Trim();

      }

      private ICollection<IndexDefinition> ReadIndexes(string name)
      {
         var indexDefinitions = Processor.Read("SELECT u.index_name, DBMS_METADATA.get_ddl('INDEX',u.index_name) As Sql FROM USER_INDEXES u WHERE TABLE_NAME = '{0}'", name);

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
          var dataSet = Processor.Read("SELECT OBJECT_NAME FROM USER_OBJECTS WHERE object_type = 'TABLE' ORDER BY OBJECT_NAME");

         return (from DataRow table in dataSet.Tables[0].Rows
                         select new TableDefinition
                                   {
                                      Name = table["OBJECT_NAME"].ToString()
                                      ,Columns = ParseColumns(table["OBJECT_NAME"].ToString(), GetTableDefintion(table["OBJECT_NAME"].ToString()))
                                   }).ToList();

      }

       private string GetTableDefintion(string tableName)
       {
           //TODO: Look at using dbms_metadata.get_xml instead and parse xml as text may be too brittle and have too many edge cases
           var dataSet = Processor.Read("SELECT dbms_metadata.get_sxml_ddl(object_type,object_name) AS SQL FROM USER_OBJECTS WHERE object_type = 'TABLE' AND OBJECT_NAME = '{0}'", tableName);
           return dataSet.Tables[0].Rows.Count == 1 ? dataSet.Tables[0].Rows[0]["SQL"].ToString() : string.Empty;
       }

       private static ICollection<ColumnDefinition> ParseColumns(string table, string sql)
      {
         var columns = new List<ColumnDefinition>();
         using ( var reader = new StringReader(sql))
         {
            var foundCreate = false;
            while (reader.Peek() > 0)
            {
               var line = reader.ReadLine();

               if ( string.IsNullOrEmpty(line))
                  continue;

               line = line.Trim();

               if ( line.StartsWith("CREATE TABLE"))
               {
                  foundCreate = true;
                  continue;
               }

               if ( foundCreate)
               {
                  // Check if start of column definitions
                  if (line.StartsWith("("))
                     line = line.Substring(1).Trim();

                  if (line.StartsWith(")"))
                     break; // End of column definitions stop now

                  if (!line.StartsWith("\""))
                      break; // End of column definitions stop now

                  var column = GetColumnFromSql(line);
                  column.TableName = table;
                  columns.Add(column);
               }
               
            }
         
         }
         return columns;
      }

      private static ColumnDefinition GetColumnFromSql(string line)
      {
         var column = new ColumnDefinition();
         var firstSpace = line.IndexOf(" ");
         column.Name = line.Substring(0, firstSpace).Trim().Replace("\"", "");

         var remainingLine = line.Substring(firstSpace).Trim();
         var nextSpace = remainingLine.IndexOf(" ");

         var type = remainingLine.Substring(0, nextSpace == -1 ? remainingLine.Length : nextSpace).Trim();
         var args = string.Empty;
         if (type.Contains("(") && type.Contains(")")) {
            args = remainingLine.Substring(type.IndexOf("(")+1, type.IndexOf(")") - type.IndexOf("(")-1);
            type = type.Substring(0,type.IndexOf("("));
         }

         switch ( type )
         {

            case "BLOB":
               column.Type = DbType.Binary;
               column.Size = int.MaxValue;
               break;
           
            case "CHAR":
               column.Type = DbType.AnsiStringFixedLength;
               column.Size = int.Parse(args);
               break;
            case "CLOB":
               column.Type = DbType.String;
               column.Size = int.MaxValue;
               break;

            case "DATE":
               column.Type = DbType.Date;
               break;
            case "DOUBLE":
               column.Type = DbType.Double;
               break;
            case "FLOAT":
               var floatSize = int.Parse(args);
               column.Type = floatSize == 126 ? DbType.Double : DbType.Single;
               break;
            case "NCHAR":
               column.Type = DbType.StringFixedLength;
               column.Size = int.Parse(args);
               break;
            case "NCLOB":
               column.Type = DbType.String;
               column.Size = int.MaxValue;
               break;
            case "NUMBER":

                 if ( string.IsNullOrEmpty(args) )
                 {
                     column.Type = DbType.Int32;
                 }
                 else
                 {
                     var scale = int.Parse(args.Split(',')[0]);
                     var precision = int.Parse(args.Split(',')[1]);

                     if (scale == 1)
                     {
                         column.Type = DbType.Boolean;
                         column.Size = 1;
                     }

                     if (scale == 3)
                     {
                         column.Type = DbType.Byte;
                     }

                     if (column.Type == null && scale <= 5 && precision == 0)
                     {
                         column.Type = DbType.Int16;
                     }

                     if (column.Type == null && scale <= 10 && precision == 0)
                     {
                         column.Type = DbType.Int32;
                     }

                     if (column.Type == null && scale <= 20 && precision == 0)
                     {
                         column.Type = DbType.Int64;
                     }

                     if (column.Type == null)
                     {
                         column.Type = DbType.Decimal;
                         column.Scale = scale;
                         column.Precision = precision;
                     }
                 }
               break;
            case "RAW":
               var rawSize = int.Parse(args);

               if (rawSize == 16)
                  column.Type = DbType.Guid;
               else
               {
                  column.Type = DbType.Binary;
                  column.Size = rawSize;
               }
               break;
            case "TIMESTAMP":
               column.Type = DbType.DateTime;
               break;
            case "NVARCHAR2":
               column.Type = DbType.String;
               column.Size = int.Parse(args);
               break;
            case "VARCHAR2":
               column.Type = DbType.AnsiString;
               column.Size = int.Parse(args);
               break;
         }

         return column;
      }

      /// <summary>
      /// Gets the views that exist in the Oracle database
      /// </summary>
      /// <remarks>The CreateViewSql includes schema refences to the schema it cam from which may need to be replaced</remarks>
      /// <returns>The view definition</returns>
      public IList<ViewDefinition> ReadViews()
      {
         var views = Processor.Read("SELECT OBJECT_NAME, dbms_metadata.get_sxml_ddl(object_type,object_name) AS SQL FROM USER_OBJECTS WHERE object_type = 'VIEW'");

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
   }
}
