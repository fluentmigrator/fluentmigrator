using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System;

namespace FluentMigrator.T4
{
    public class MySqlSchemaReader : SchemaReader
    {
        // SchemaReader.ReadSchema
        public override Tables ReadSchema(DbConnection connection, DbProviderFactory factory)
        {
            var result = new Tables();


            var cmd = factory.CreateCommand();
            cmd.Connection = connection;
            cmd.CommandText = TABLE_SQL;

            //pull the tables in a reader
            using (cmd)
            {
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        Table tbl = new Table();
                        tbl.Name = rdr["TABLE_NAME"].ToString();
                        tbl.Schema = rdr["TABLE_SCHEMA"].ToString();
                        tbl.IsView = String.Compare(rdr["TABLE_TYPE"].ToString(), "View", true) == 0;
                        tbl.CleanName = CleanUp(tbl.Name);
                        tbl.ClassName = Inflector.MakeSingular(tbl.CleanName);
                        result.Add(tbl);
                    }
                }
            }

            //this will return everything for the DB
            var schema = connection.GetSchema("COLUMNS");

            //loop again - but this time pull by table name
            foreach (var item in result)
            {
                item.Columns = new List<Column>();

                //pull the columns from the schema
                var columns = schema.Select("TABLE_NAME='" + item.Name + "'");
                foreach (var row in columns)
                {
                    var type = GetPropertyType(row);
                    Column col = new Column();
                    col.Name = row["COLUMN_NAME"].ToString();
                    col.PropertyName = CleanUp(col.Name);
                    col.PropertyType = type;
                    col.CustomType = type == null 
                        ? row["DATA_TYPE"].ToString().ToLowerInvariant() 
                        : null;
                    col.Size = GetDatatypeSize(row["DATA_TYPE"].ToString());
                    col.Precision = GetDatatypePrecision(row["DATA_TYPE"].ToString());
                    col.IsNullable = row["IS_NULLABLE"].ToString() == "YES";
                    col.IsPrimaryKey = row["COLUMN_KEY"].ToString() == "PRI";
                    col.IsAutoIncrement = row["extra"].ToString().ToLower().IndexOf("auto_increment") >= 0;

                    item.Columns.Add(col);
                }
            }

            return result;

        }

        private static readonly IDictionary<string, DbType?> _typeMap =
            new Dictionary<string, DbType?>()
            {
                {"bigint", DbType.Int64}
                ,{"ubigint", DbType.UInt64}
                ,{"smallint",DbType.Int16}
                ,{"usmallint",DbType.UInt16}
                ,{"int",DbType.Int32}
                ,{"uint",DbType.UInt32}
                ,{"guid",DbType.Guid}
                ,{"smalldatetime",DbType.DateTime}
                ,{"datetime",DbType.DateTime}
                ,{"datetime2",DbType.DateTime2}
                ,{"date",DbType.Date}
                ,{"time",DbType.Time}
                ,{"timestamp",DbType.Byte}//TODO: CHECK
                ,{"float",DbType.Double}
                ,{"double",DbType.Double}
                ,{"numeric",DbType.Decimal}
                ,{"smallmoney",DbType.Decimal}
                ,{"decimal",DbType.Decimal}
                ,{"money",DbType.Currency}
                ,{"binary",DbType.Binary}
                ,{"bit",DbType.Boolean}
                ,{"bool",DbType.Boolean}
                ,{"boolean",DbType.Boolean}
                ,{"tinyint",DbType.Byte}
                ,{"utinyint",DbType.SByte}
                ,{"image",DbType.Binary}
                ,{"binary",DbType.Binary}
                ,{"blob",DbType.Binary}
                ,{"mediumblob",DbType.Binary}
                ,{"longblob",DbType.Binary}
                ,{"varbinary",DbType.Binary}
                ,{"char",DbType.AnsiStringFixedLength}
                ,{"varchar",DbType.AnsiString}
                ,{"text",DbType.AnsiString}
                ,{"nchar",DbType.StringFixedLength}
                ,{"nvarchar",DbType.String}
                ,{"ntext",DbType.String}

            };
        static DbType? GetPropertyType(string sqlType)
        {
            var sysType = default(DbType?);
            _typeMap.TryGetValue(sqlType, out sysType);
            return sysType;
        }

        static DbType? GetPropertyType(DataRow row)
        {
            var sing = row["COLUMN_TYPE"].ToString().IndexOf("unsigned") >= 0 ? "u" : string.Empty;
            var typeName = sing + row["DATA_TYPE"].ToString().ToLowerInvariant();
            var type = default(DbType?);
            _typeMap.TryGetValue(typeName, out type);
            return type;
        }

        const string TABLE_SQL = @"
            SELECT * 
            FROM information_schema.tables 
            WHERE ((table_type='BASE TABLE' OR table_type='VIEW') AND table_schema <> 'mysql')
            ";

    }
}