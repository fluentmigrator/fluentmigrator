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
                tbl.Indices = this.LoadIndices(tbl.Name);
                tbl.FKeys = this.LoadFKeys(tbl.Name);			
                    
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
                        Column col=new Column();
                        col.Name=rdr["ColumnName"].ToString();
                        col.PropertyName=CleanUp(col.Name);
                        col.PropertyType=this.GetPropertyType(rdr["DataType"].ToString());
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
    
        string GetPropertyType(string sqlType)
        {
            string sysType="string";
            switch (sqlType) 
            {
                case "bigint":
                    sysType = "long";
                    break;
                case "smallint":
                    sysType= "short";
                    break;
                case "int":
                    sysType= "int";
                    break;
                case "uniqueidentifier":
                    sysType=  "Guid";
                    break;
                case "smalldatetime":
                case "datetime":
                case "date":
                case "time":
                    sysType=  "DateTime";
                    break;
                case "float":
                    sysType="double";
                    break;
                case "real":
                    sysType="float";
                    break;
                case "numeric":
                case "smallmoney":
                case "decimal":
                case "money":
                    sysType=  "decimal";
                    break;
                case "tinyint":
                    sysType = "byte";
                    break;
                case "bit":
                    sysType=  "bool";
                    break;
                case "image":
                case "binary":
                case "varbinary":
                case "timestamp":
                    sysType=  "byte[]";
                    break;
            }
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