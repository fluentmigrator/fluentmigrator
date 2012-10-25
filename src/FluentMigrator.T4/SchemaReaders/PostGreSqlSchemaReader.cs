using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System;

namespace FluentMigrator.T4
{
    class PostGreSqlSchemaReader : SchemaReader
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
                        tbl.Name=rdr["table_name"].ToString();
                        tbl.Schema=rdr["table_schema"].ToString();
                        tbl.IsView=String.Compare(rdr["table_type"].ToString(), "View", true)==0;
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
                        col.Name=rdr["column_name"].ToString();
                        col.PropertyName=CleanUp(col.Name);
                        col.PropertyType=this.GetPropertyType(rdr["udt_name"].ToString());
                        col.Size=GetDatatypeSize(rdr["udt_name"].ToString());
                        col.Precision=GetDatatypePrecision(rdr["udt_name"].ToString());
                        col.IsNullable=rdr["is_nullable"].ToString()=="YES";
                        col.IsAutoIncrement = rdr["column_default"].ToString().StartsWith("nextval(");
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

        List<FKey> LoadFKeys(string tblName)
        {
            var result=new List<FKey>();
            return result;		
        }
	
        string GetPK(string table){
        
            string sql=@"SELECT kcu.column_name 
            FROM information_schema.key_column_usage kcu
            JOIN information_schema.table_constraints tc
            ON kcu.constraint_name=tc.constraint_name
            WHERE lower(tc.constraint_type)='primary key'
            AND kcu.table_name=@tablename";

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
            switch (sqlType)
            {
                case "int8":
                case "serial8":	
                    return "long";

                case "bool":	
                    return "bool";

                case "bytea	":	
                    return "byte[]";

                case "float8":	
                    return "double";

                case "int4":	
                case "serial4":	
                    return "int";

                case "money	":	
                    return "decimal";

                case "numeric":	
                    return "decimal";

                case "float4":	
                    return "float";

                case "int2":	
                    return "short";

                case "time":
                case "timetz":
                case "timestamp":
                case "timestamptz":	
                case "date":	
                    return "DateTime";

                default:
                    return "string";
            }
        }



        const string TABLE_SQL=@"
            SELECT table_name, table_schema, table_type
            FROM information_schema.tables 
            WHERE (table_type='BASE TABLE' OR table_type='VIEW')
                AND table_schema NOT IN ('pg_catalog', 'information_schema');
            ";

        const string COLUMN_SQL=@"
            SELECT column_name, is_nullable, udt_name, column_default
            FROM information_schema.columns 
            WHERE table_name=@tableName;
            ";
    
    }
}