using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System;

namespace FluentMigrator.T4
{
    class SqlServerCeSchemaReader : SchemaReader
    {
        // SchemaReader.ReadSchema
        public override Tables ReadSchema(DbConnection connection, DbProviderFactory factory)
        {
            var result=new Tables();
        
            this._connection=connection;
            this._factory=factory;

            var cmd=this._factory.CreateCommand();
            cmd.Connection=connection;
            cmd.CommandText=TABLE_SQL;

            //pull the tables in a reader
            using(cmd)
            {
                using (var rdr=cmd.ExecuteReader())
                {
                    while(rdr.Read())
                    {
                        Table tbl=new Table();
                        tbl.Name=rdr["TABLE_NAME"].ToString();
                        tbl.CleanName=CleanUp(tbl.Name);
                        tbl.ClassName=Inflector.MakeSingular(tbl.CleanName);
                        tbl.Schema=null;
                        tbl.IsView=false;
                        result.Add(tbl);
                    }
                }
            }

            foreach (var tbl in result)
            {
                tbl.Columns=this.LoadColumns(tbl);
                tbl.Indexes = this.LoadIndices(tbl.Name);
                tbl.ForeignKeys = this.LoadFKeys(tbl.Name);			
                    
                // Mark the primary key
                string PrimaryKey=this.GetPK(tbl.Name);
                var pkColumn=tbl.Columns.SingleOrDefault(x=>x.Name.ToLower().Trim()==PrimaryKey.ToLower().Trim());
                if(pkColumn!=null)
                    pkColumn.IsPrimaryKey=true;
            }
        

            return result;
        }
    
        DbConnection _connection;
        DbProviderFactory _factory;
    

        List<Column> LoadColumns(Table tbl)
        {
    
            using (var cmd=this._factory.CreateCommand())
            {
                cmd.Connection=this._connection;
                cmd.CommandText=COLUMN_SQL;

                var p = cmd.CreateParameter();
                p.ParameterName = "@tableName";
                p.Value=tbl.Name;
                cmd.Parameters.Add(p);

                var result=new List<Column>();
                using (IDataReader rdr=cmd.ExecuteReader())
                {
                    while(rdr.Read())
                    {
                        var type = GetPropertyType(rdr["DataType"].ToString());
                        Column col=new Column();
                        col.Name=rdr["ColumnName"].ToString();
                        col.PropertyName=CleanUp(col.Name);
                        col.PropertyType=type;
                        col.CustomType = type == null
                            ? rdr["DataType"].ToString().ToLowerInvariant()
                            : null;
                        col.Size=GetDatatypeSize(rdr["DataType"].ToString());
                        col.Precision=GetDatatypePrecision(rdr["DataType"].ToString());
                        col.IsNullable=rdr["IsNullable"].ToString()=="YES";
                        col.IsAutoIncrement=rdr["AUTOINC_INCREMENT"]!=DBNull.Value;
                        result.Add(col);
                    }
                }

                return result;
            }
        }
	
        List<TableIndex> LoadIndices(string tableName)
        {
            var result=new List<TableIndex>();
            return result;
        }

        List<ForeignKey> LoadFKeys(string tblName)
        {
            var result=new List<ForeignKey>();
            return result;		
        }	

        string GetPK(string table){
        
            string sql=@"SELECT KCU.COLUMN_NAME 
            FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE KCU
            JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS TC
            ON KCU.CONSTRAINT_NAME=TC.CONSTRAINT_NAME
            WHERE TC.CONSTRAINT_TYPE='PRIMARY KEY'
            AND KCU.TABLE_NAME=@tableName";

            using (var cmd=this._factory.CreateCommand())
            {
                cmd.Connection=this._connection;
                cmd.CommandText=sql;

                var p = cmd.CreateParameter();
                p.ParameterName = "@tableName";
                p.Value=table;
                cmd.Parameters.Add(p);

                var result=cmd.ExecuteScalar();

                if(result!=null)
                    return result.ToString();    
            }	         
        
            return "";
        }

        private static readonly IDictionary<string, DbType?> _typeMap =
            new Dictionary<string, DbType?>()
            {
                {"bigint", DbType.Int64}
                ,{"smallint",DbType.Int16}
                ,{"int",DbType.Int32}
                ,{"uniqueidentifier",DbType.Guid}
                ,{"smalldatetime",DbType.DateTime}
                ,{"datetime",DbType.DateTime}
                ,{"datetime2",DbType.DateTime2}
                ,{"date",DbType.Date}
                ,{"time",DbType.Time}
                ,{"float",DbType.Double}
                ,{"real",DbType.Single}
                ,{"numeric",DbType.Decimal}
                ,{"smallmoney",DbType.Decimal}
                ,{"decimal",DbType.Decimal}
                ,{"money",DbType.Currency}
                ,{"tinyint",DbType.Byte}
                ,{"image",DbType.Binary}
                ,{"binary",DbType.Binary}
                ,{"varbinary",DbType.Binary}
                ,{"bit",DbType.Boolean}
                ,{"datetimeoffset",DbType.DateTimeOffset}
                ,{"tinyint",DbType.Byte}
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



        const string TABLE_SQL=@"SELECT *
        FROM  INFORMATION_SCHEMA.TABLES
        WHERE TABLE_TYPE='TABLE'";

        const string COLUMN_SQL=@"SELECT 
            TABLE_CATALOG AS [Database],
            TABLE_SCHEMA AS Owner, 
            TABLE_NAME AS TableName, 
            COLUMN_NAME AS ColumnName, 
            ORDINAL_POSITION AS OrdinalPosition, 
            COLUMN_DEFAULT AS DefaultSetting, 
            IS_NULLABLE AS IsNullable, DATA_TYPE AS DataType, 
            AUTOINC_INCREMENT,
            CHARACTER_MAXIMUM_LENGTH AS MaxLength, 
            DATETIME_PRECISION AS DatePrecision
        FROM  INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_NAME=@tableName
        ORDER BY OrdinalPosition ASC";
      
    }
}