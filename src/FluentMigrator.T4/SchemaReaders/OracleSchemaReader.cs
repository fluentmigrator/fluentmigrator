using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System;

namespace FluentMigrator.T4
{
    class OracleSchemaReader : SchemaReader
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
            cmd.GetType().GetProperty("BindByName").SetValue(cmd, true, null);

            //pull the tables in a reader
            using(cmd)
            {

                using (var rdr=cmd.ExecuteReader())
                {
                    while(rdr.Read())
                    {
                        Table tbl=new Table();
                        tbl.Name=rdr["TABLE_NAME"].ToString();
                        tbl.Schema = rdr["TABLE_SCHEMA"].ToString();
                        tbl.IsView=String.Compare(rdr["TABLE_TYPE"].ToString(), "View", true)==0;
                        tbl.CleanName=CleanUp(tbl.Name);
                        tbl.ClassName=Inflector.MakeSingular(tbl.CleanName);
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
                    pkColumn.IsPK=true;
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
                cmd.GetType().GetProperty("BindByName").SetValue(cmd, true, null);

                var p = cmd.CreateParameter();
                p.ParameterName = ":tableName";
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
                        col.PropertyType=this.GetPropertyType(rdr["DataType"].ToString(), (rdr["DataType"] == DBNull.Value ? null : rdr["DataType"].ToString()));
                        col.Size=GetDatatypeSize(rdr["DataType"].ToString());
                        col.Precision=GetDatatypePrecision(rdr["DataType"].ToString());
                        col.IsNullable=rdr["IsNullable"].ToString()=="YES";
                        col.IsAutoIncrement=false;
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
        
            string sql=@"select column_name from USER_CONSTRAINTS uc
  inner join USER_CONS_COLUMNS ucc on uc.constraint_name = ucc.constraint_name
where uc.constraint_type = 'P'
and uc.table_name = upper(:tableName)
and ucc.position = 1";

            using (var cmd=this._factory.CreateCommand())
            {
                cmd.Connection=this._connection;
                cmd.CommandText=sql;
                cmd.GetType().GetProperty("BindByName").SetValue(cmd, true, null);

                var p = cmd.CreateParameter();
                p.ParameterName = ":tableName";
                p.Value=table;
                cmd.Parameters.Add(p);

                var result=cmd.ExecuteScalar();

                if(result!=null)
                    return result.ToString();    
            }	         
        
            return "";
        }
    
        string GetPropertyType(string sqlType, string dataScale)
        {
            string sysType="string";
            switch (sqlType.ToLower()) 
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
                    sysType=  "DateTime";
                    break;
                case "float":
                    sysType="double";
                    break;
                case "real":
                case "numeric":
                case "smallmoney":
                case "decimal":
                case "money":
                case "number":
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
        
            if (sqlType == "number" && dataScale == "0")
                return "long";
        
            return sysType;
        }



        const string TABLE_SQL=@"select TABLE_NAME from USER_TABLES";

        const string COLUMN_SQL=@"select table_name TableName, 
 column_name ColumnName, 
 data_type DataType, 
 data_scale DataScale,
 nullable IsNullable
 from USER_TAB_COLS utc 
 where table_name = upper(:tableName)
 order by column_id";
      
    }
}