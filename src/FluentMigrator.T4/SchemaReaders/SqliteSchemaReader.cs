using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System;

namespace FluentMigrator.T4
{
    class SqliteSchemaReader : SchemaReader
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
            //cmd.GetType().GetProperty("BindByName").SetValue(cmd, true, null);
            //pull the tables in a reader
            using(cmd)
            {
                using (var rdr=cmd.ExecuteReader())
                {
                    while(rdr.Read())
                    {
                        Table tbl=new Table();
                        tbl.Name=rdr["name"].ToString();
                        tbl.Schema = "";
                        tbl.IsView=String.Compare(rdr["type"].ToString(), "view", true)==0;
                        tbl.CleanName=CleanUp(tbl.Name);
                        tbl.ClassName=Inflector.MakeSingular(tbl.CleanName);
                        tbl.SQL = rdr["sql"].ToString();
                        result.Add(tbl);
                    }
                }
            }
            foreach (var tbl in result)
            {
                tbl.Columns=this.LoadColumns(tbl);
                tbl.Indexes = this.LoadIndices(tbl.Name);
                tbl.ForeignKeys = this.LoadForeignKeys(tbl.Name);
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
                cmd.CommandText=String.Format(COLUMN_SQL,tbl.Name);

                var result=new List<Column>();
                using (IDataReader rdr=cmd.ExecuteReader())
                {
                    while(rdr.Read())
                    {
                        var type=this.GetPropertyType(rdr["type"].ToString(), (rdr["type"] == DBNull.Value ? null : rdr["type"].ToString()));
                        Column col=new Column();
                        col.Name=rdr["name"].ToString();
                        col.PropertyName=CleanUp(col.Name);
                        col.PropertyType= type;
                        col.CustomType = type == null
                            ? rdr["type"].ToString().ToLowerInvariant()
                            : null;
                        col.Size=GetDatatypeSize(rdr["type"].ToString());
                        col.Precision=GetDatatypePrecision(rdr["type"].ToString());
                        col.IsNullable=rdr["notnull"].ToString()=="0";
                        col.IsAutoIncrement=false;
                        col.IsPrimaryKey=rdr["pk"].ToString()!="0";
                        if (col.IsPrimaryKey)
                            col.IsAutoIncrement = tbl.SQL.ToUpper().Contains("AUTOINCREMENT");
                        else
                            col.IsAutoIncrement = false;					
                        col.DefaultValue = rdr["dflt_value"] == DBNull.Value ? null : rdr["dflt_value"].ToString();
                        result.Add(col);
                    }
                }
                return result;
            }
        }

			
        List<TableIndex> LoadIndices(string tableName)
        {
            var result=new List<TableIndex>();
		
            using (var cmd1=this._factory.CreateCommand())
            {			
                cmd1.Connection=this._connection;
                cmd1.CommandText=String.Format(INDEX_SQL,tableName);
                using (IDataReader rdr1=cmd1.ExecuteReader())
                {
                    while(rdr1.Read())
                    {			
                        TableIndex indx=new TableIndex();
                        indx.Name=rdr1["name"].ToString();					
                        indx.SQL=rdr1["sql"].ToString();
                        indx.IndexColumns = new List<IndexColumn>();
                        indx.IsUnique = indx.SQL.ToUpper().Contains("UNIQUE");
                        using (var cmd2=this._factory.CreateCommand())
                        {
                            cmd2.Connection=this._connection;
                            cmd2.CommandText=String.Format(INDEX_INFO_SQL,indx.Name);			            
                            using (IDataReader rdr2=cmd2.ExecuteReader())
                            {
                                while(rdr2.Read())
                                {	IndexColumn col = new IndexColumn();								
                                    col.Name = rdr2["name"].ToString();
                                    indx.IndexColumns.Add(col);
                                }
                            }
                        }
                        result.Add(indx);
                    }
                }
            }
            return result;
        }
	
        List<ForeignKey> LoadForeignKeys(string tblName)
        {
            using (var cmd=this._factory.CreateCommand())
            {
                cmd.Connection=this._connection;
                cmd.CommandText=String.Format(FKEY_INFO_SQL,tblName);

                var result=new List<ForeignKey>();
                using (IDataReader rdr=cmd.ExecuteReader())
                {
                    while(rdr.Read())
                    {
                        ForeignKey key=new ForeignKey();
                        key.PrimaryTable=rdr["table"].ToString();
                        key.PrimaryColumns=new [] { rdr["to"].ToString() };
                        key.ForeignColumns=new [] { rdr["from"].ToString() };
                        result.Add(key);
                    }
                }
                return result;
            }
        }
	

	
        DbType? GetPropertyType(string sqlType, string dataScale)
        {
            var sysType=default(DbType?);
            switch (sqlType.ToLower()) 
            {
                case "integer":
                case "int":
                case "tinyint":
                case "smallint":
                case "mediumint":
                case "int2":
                case "int8":
                    sysType = DbType.Int64;
                    break;				
                case "bigint":
                case "unsigned big int":
                    sysType= DbType.UInt64;
                    break;
                case "uniqueidentifier":
                    sysType = DbType.Guid;
                    break;
                case "smalldatetime":
                case "datetime":
                case "date":
                    sysType=  DbType.DateTime;
                    break;				
                case "float":
                case "double precision":
                case "double":
                    sysType = DbType.Double;
                    break;
                case "real":
                case "numeric":
                case "decimal":
                case "number":
                    sysType = DbType.Single;
                    break;
                case "smallmoney":
                case "money":
                    sysType = DbType.Currency;
                    break;
                case "bit":
                    sysType = DbType.Boolean;
                    break;
                case "image":
                case "binary":
                case "varbinary":
                case "timestamp":
                    sysType=  DbType.Binary;
                    break;
            }        
            if (sqlType == "number" && dataScale == "0")
                return DbType.Int64;        
            return sysType;
        }

        const string TABLE_SQL=@"SELECT name, type , sql FROM sqlite_master WHERE type IN ('table','view') and name not in ('sqlite_sequence') ";
        const string COLUMN_SQL=@"pragma table_info({0})";
	
        const string INDEX_SQL=@"SELECT name , sql  FROM sqlite_master WHERE type IN ('index') and lower(tbl_name) = lower('{0}')";
        const string INDEX_INFO_SQL=@"pragma index_info({0})";	
	
        const string FKEY_INFO_SQL=@"pragma foreign_key_list({0})";	
      
    }
}