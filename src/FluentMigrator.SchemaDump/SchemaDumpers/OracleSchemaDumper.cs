using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using FluentMigrator.Model;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Processors.Oracle;

namespace FluentMigrator.SchemaDump.SchemaDumpers
{
   public class OracleSchemaDumper : ISchemaDumper
   {
       public IAnnouncer Announcer { get; set; }
        public OracleProcessor Processor { get; set; }

        public OracleSchemaDumper(OracleProcessor processor, IAnnouncer announcer)
		{
            this.Announcer = announcer;
            this.Processor = processor;
		}	

      public IList<TableDefinition> ReadDbSchema()
      {
         Announcer.Say("Reading table schema");
         IList<TableDefinition> tables = ReadTables();
         foreach (TableDefinition table in tables)
         {
            Announcer.Say(string.Format("Reading indexes and foreign keys for {0}.{1}", table.SchemaName, table.Name));
            //table.Indexes = ReadIndexes(table.SchemaName, table.Name);
            //table.ForeignKeys = ReadForeignKeys(table.SchemaName, table.Name);
         }

         return tables as IList<TableDefinition>;
      }

      protected virtual IList<FluentMigrator.Model.TableDefinition> ReadTables()
      {
         var tables = new List<TableDefinition>();
         var oracleTables = Processor.Read("SELECT TABLE_NAME FROM USER_TABLES");

         foreach (DataRow table in oracleTables.Tables[0].Rows)
         {
            var tableName = table["TABLE_NAME"] as string;
            var schema = GetTableSchema(tableName);

            var definition = new TableDefinition();
            definition.Name = tableName;

            foreach (DataRow schemaRow in schema.Rows)
            {
               var column = new ColumnDefinition()
                               {
                                  Name = schemaRow["ColumnName"] as string
                               };

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
                  default:
                     Debug.WriteLine(providerType);
                     break;
               }

               if (column.Type != null)
               {
                  definition.Columns.Add(column);
                  continue;
               }
               


               switch (((Type)schemaRow["DataType"]).FullName)
               {
                  case "System.String":
                     column.Type = DbType.String;
                     column.Size = int.Parse(schemaRow["ColumnSize"].ToString());
                     break;
                  case "System.Decimal":
                     var precision = int.Parse(schemaRow["NumericPrecision"].ToString());
                     var scale = int.Parse(schemaRow["NumericScale"].ToString());
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

                     if ( column.Type == null )
                     {
                        column.Type = DbType.Decimal;
                        column.Precision = precision;
                        column.Scale = scale;
                     }
                     break;
                  case "System.Byte[]":
                     var size = int.Parse(schemaRow["ColumnSize"].ToString());
                     if (size == 16)
                        column.Type = DbType.Guid;
                     else
                        column.Type = DbType.Binary;
                     column.Size = int.Parse(schemaRow["ColumnSize"].ToString());
                     break;
                  case "System.DateTime":
                     column.Type = DbType.DateTime;
                     break;
                  case "System.Double":
                     column.Type = DbType.Double;
                     break;

              }
               definition.Columns.Add(column);
            }

            tables.Add(definition);

         }

         return tables;
      }

      

      public DataTable GetTableSchema(string tableName)
      {
         try
         {
            string strQuery = string.Format("SELECT * FROM {0} WHERE 1 = 2", tableName);

            var command = Processor.Connection.CreateCommand();
            command.CommandText = strQuery;
            var rdr = command.ExecuteReader();
            // Get the schema table.
            return rdr.GetSchemaTable();
         }
         catch (Exception)
         {
            return null;
         }
      }

      public IList<ViewDefinition> ReadViews()
      {
         throw new NotImplementedException();
      }

      public DataSet ReadTableData(string schemaName, string tableName)
      {
         throw new NotImplementedException();
      }
   }

   public class OracleTypes
   {
      public const int Char = 3;
      public const int Date = 6;
      public const int NChar = 11;
      public const int Varchar = 22;
   }
}
