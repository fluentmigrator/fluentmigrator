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
                tbl.Indexes = this.LoadIndices(tbl.Schema, tbl.Name);
                tbl.ForeignKeys = this.LoadFKeys(tbl.Schema, tbl.Name);
			
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
                        var type = GetPropertyType(rdr["udt_name"].ToString());
                        Column col=new Column();
                        col.Name=rdr["column_name"].ToString();
                        col.PropertyName=CleanUp(col.Name);
                        col.PropertyType = type;
                        col.CustomType = type == null
                            ? rdr["udt_name"].ToString().ToLowerInvariant()
                            : null;
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

        const string ForeignKeySql = @"
        SELECT 
            FK.CONSTRAINT_NAME AS ForeignConstraintName, 
            FK.TABLE_SCHEMA AS ForeignTableSchema, 
            FK.TABLE_NAME AS ForeignTable, 
            FKC.COLUMN_NAME AS ForeignColumn, 
            PK.CONSTRAINT_NAME AS PrimaryConstraintName, 
            PK.TABLE_SCHEMA AS PrimaryTableSchema, 
            PK.TABLE_NAME AS PrimaryTable, 
            PKC.COLUMN_NAME AS PrimaryColumn, 
            RC.UPDATE_RULE AS UpdateRule, 
            RC.DELETE_RULE AS DeleteRule
        FROM         
            INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS FK INNER JOIN
            INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS FKC ON FK.CONSTRAINT_SCHEMA = FKC.CONSTRAINT_SCHEMA AND 
            FK.CONSTRAINT_NAME = FKC.CONSTRAINT_NAME INNER JOIN
            INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS AS RC ON FK.CONSTRAINT_SCHEMA = RC.CONSTRAINT_SCHEMA AND 
            FK.CONSTRAINT_NAME = RC.CONSTRAINT_NAME INNER JOIN
            INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS PK ON RC.UNIQUE_CONSTRAINT_SCHEMA = PK.CONSTRAINT_SCHEMA AND 
            RC.UNIQUE_CONSTRAINT_NAME = PK.CONSTRAINT_NAME INNER JOIN
            INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS PKC ON PK.CONSTRAINT_SCHEMA = PKC.CONSTRAINT_SCHEMA AND 
            PK.CONSTRAINT_NAME = PKC.CONSTRAINT_NAME AND FKC.ORDINAL_POSITION = PKC.ORDINAL_POSITION
        WHERE     
            (FK.CONSTRAINT_TYPE = 'FOREIGN KEY') 
            AND (FK.TABLE_NAME=@tableName) 
            AND (FK.TABLE_SCHEMA=@schemaName)
        ORDER BY ForeignConstraintName, ForeignTableSchema, ForeignTable, FKC.ORDINAL_POSITION";


        List<TableIndex> LoadIndices(string schemaName, string tableName)
        {
            //var result=new List<TableIndex>();
            var indquery = string.Format(@"
select indlst.relname as indname,indlst.oid as indoid,indlst.indkey as indkey,tbllst.oid as tbloid,tbllst.relname as tblname from (select c.relname,c.oid,i.indkey from pg_index as i 
    inner join pg_class as c on i.indexrelid=c.oid
    inner join pg_namespace as ns on c.relnamespace=ns.oid
    where ns.nspname='{0}' and i.indisprimary = false) as indlst
  inner join (select i.indexrelid,c.oid,c.relname from pg_index as i
    inner join pg_class as c on i.indrelid=c.oid
    where c.relname='{1}'
    ) as tbllst on indlst.oid = tbllst.indexrelid
", schemaName, tableName);
            var colquery = @"
select att.attrelid,att.attname,att.attnum from pg_attribute as att
    where att.attnum>0
";
            using(var indTable = new DataTable())
            using(var colTable = new DataTable())
            {
                using (var indcmd = this._factory.CreateCommand())
                using (var colcmd = this._factory.CreateCommand())
                {
                    indcmd.Connection = this._connection;
                    colcmd.Connection = this._connection;
                    indcmd.CommandText = indquery;
                    colcmd.CommandText = colquery;
                    using (var indadp = this._factory.CreateDataAdapter())
                    using (var coladp = this._factory.CreateDataAdapter())
                    {
                        indadp.SelectCommand = indcmd;
                        coladp.SelectCommand = colcmd;
                        indadp.Fill(indTable);
                        coladp.Fill(colTable);
                    }
                }
                var dic = new List<TableIndex>();
                foreach(DataRow indrow in indTable.Rows)
                {
                    var indname = indrow["indname"].ToString();
                    var indcols = indrow["indkey"].ToString().Split(' ');
                    var idx = new TableIndex();
                    idx.IndexColumns = new List<IndexColumn>();
                    idx.Name = indname;
                    idx.SQL = "";
                    idx.IndexColumns.AddRange(
                        colTable.Rows.Cast<DataRow>()
                        .Where(col => indcols.Any(indcolnum => col["attnum"].ToString() == indcolnum && indrow["tbloid"].ToString() == col["attrelid"].ToString()))
                        .Select(col => new IndexColumn() { IsAsc = true, Name = col["attname"].ToString() }))
                        ;
                    dic.Add(idx);
                }
                return dic;
            }
        }

        List<ForeignKey> LoadFKeys(string tblSchema, string tblName)
        {
            using (var cmd = this._factory.CreateCommand())
            {
                cmd.Connection = this._connection;
                cmd.CommandText = ForeignKeySql;

                var p = cmd.CreateParameter();
                p.ParameterName = "@tableName";
                p.Value = tblName;
                cmd.Parameters.Add(p);

                p = cmd.CreateParameter();
                p.ParameterName = "@schemaName";
                p.Value = tblSchema;
                cmd.Parameters.Add(p);

                var keys = cmd.Select(reader => new
                {
                    ForeignConstraintName = reader["ForeignConstraintName"].ToString(),
                    ForeignTableSchema = reader["ForeignTableSchema"].ToString(),
                    ForeignTable = reader["ForeignTable"].ToString(),
                    ForeignColumn = reader["ForeignColumn"].ToString(),
                    PrimaryConstraintName = reader["PrimaryConstraintName"].ToString(),
                    PrimaryTableSchema = reader["PrimaryTableSchema"].ToString(),
                    PrimaryTable = reader["PrimaryTable"].ToString(),
                    PrimaryColumn = reader["PrimaryColumn"].ToString(),
                    UpdateRule = DecodeRule(reader.Get<string>("UpdateRule")),
                    DeleteRule = DecodeRule(reader.Get<string>("DeleteRule")),
                });

                return keys.GroupBy(key => new
                {
                    key.ForeignConstraintName,
                    key.ForeignTableSchema,
                    key.ForeignTable,
                    key.PrimaryTableSchema,
                    key.PrimaryTable,
                    key.UpdateRule,
                    key.DeleteRule
                }).Select(foreignKeyGrouping => new ForeignKey
                {
                    Name = foreignKeyGrouping.Key.ForeignConstraintName,
                    ForeignTable = foreignKeyGrouping.Key.ForeignTable,
                    ForeignTableSchema = foreignKeyGrouping.Key.ForeignTableSchema,
                    ForeignColumns = foreignKeyGrouping.Select(f => f.ForeignColumn).ToList(),
                    PrimaryTable = foreignKeyGrouping.Key.PrimaryTable,
                    PrimaryTableSchema = foreignKeyGrouping.Key.PrimaryTableSchema,
                    PrimaryColumns = foreignKeyGrouping.Select(f => f.PrimaryColumn).ToList(),
                    PrimaryClass = Inflector.MakeSingular(CleanUp(foreignKeyGrouping.Key.PrimaryTable)),
                    UpdateRule = foreignKeyGrouping.Key.UpdateRule,
                    DeleteRule = foreignKeyGrouping.Key.DeleteRule
                }).ToList();
            }
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
    
        static DbType? GetPropertyType(string sqlType)
        {
            switch (sqlType)
            {
                case "int8":
                case "serial8":
                    return DbType.Int64;

                case "bool":
                    return DbType.Boolean;

                case "bytea	":
                    return DbType.Binary;

                case "float8":
                    return DbType.Double;

                case "int4":	
                case "serial4":
                    return DbType.Int32;

                case "money	":
                    return DbType.Currency;

                case "numeric":
                    return DbType.Decimal;

                case "float4":
                    return DbType.Decimal;

                case "int2":	
                    return DbType.Int16;

                case "time":
                case "timetz":
                case "timestamp":
                case "timestamptz":	
                case "date":	
                    return DbType.DateTime;

                default:
                    return DbType.String;
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
        private static System.Data.Rule DecodeRule(string rule)
        {
            return new Switch<string, System.Data.Rule>(rule)
                .Case("CASCADE", System.Data.Rule.Cascade)
                .Case("SET NULL", System.Data.Rule.SetDefault)
                .Case("SET DEFAULT", System.Data.Rule.SetNull)
                .Default(System.Data.Rule.None);
        }
    
    }
}